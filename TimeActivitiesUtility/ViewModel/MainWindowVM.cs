using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace TimeActivitiesUtility.ViewModel
{
    [POCOViewModel]
    public class MainWindowVM
    {
        protected Service.IUpdateTimerDialogService dlgSvc;

        protected MainWindowVM(Service.IUpdateTimerDialogService dlgSvc)
        {
            this.dlgSvc = dlgSvc;

            TimerCollection = new ObservableCollection<ActivityTimerVM>();

            TimerCollection.CollectionChanged += TimerCollection_CollectionChanged;

            FilteredTimerCollection = new ObservableCollection<ActivityTimerVM>();

            ReadData();

            StartMasterTimer();
        }

        public static MainWindowVM Create(Service.IUpdateTimerDialogService dlgSvc)
        {
            return ViewModelSource.Create(() => new MainWindowVM(dlgSvc));
        }

        #region Bindable properties

        public virtual string TotalTime { get; protected set; }

        string _searchText = string.Empty;
        public virtual string SearchText
        {
            get
            {
                if (IsFilteredToActiveOnly)
                {
                    return string.Empty;
                }
                else
                {
                    return _searchText;
                }
            }
            set
            {
                _searchText = value;
            }
        }

        private bool _isFilteredToActiveOnly = false;
        public virtual bool IsFilteredToActiveOnly
        {
            get
            {
                return _isFilteredToActiveOnly;
            }
            set
            {
                _isFilteredToActiveOnly = value;
                UpdateFilteredTimers();
                this.RaisePropertyChanged(x => x.IsSearchTextEnabled);
                this.RaisePropertyChanged(x => x.SearchText);
            }
        }

        public virtual bool IsSearchTextEnabled
        {
            get
            {
                return !IsFilteredToActiveOnly;
            }
        }

        #endregion

        #region Bindable commands

        public virtual void SearchTextChanged(string txt)
        {
            if (IsFilteredToActiveOnly)
            {
                SearchText = string.Empty;
            }
            else
            {
                SearchText = txt;
            }
            UpdateFilteredTimers();
        }

        public void UpdateFilteredTimers()
        {
            if (FilteredTimerCollection == null) return;
            FilteredTimerCollection.Clear();
            foreach (ActivityTimerVM timerVM in TimerCollection)
            {
                if (IsFilteredToActiveOnly)
                {
                    if (timerVM.Timer.TotalMilliseconds > 0)
                    {
                        FilteredTimerCollection.Add(timerVM);
                    }
                }
                else if (timerVM.ActivityDescription.IndexOf(SearchText, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    FilteredTimerCollection.Add(timerVM);
                }
            }
        }

        public void ClearFilter()
        {
            SearchText = string.Empty;
            UpdateFilteredTimers();
        }

        public virtual void AddNewTimer() 
        {
            ClearFilter();
            AddTimer(ActivityTimerVM.Create());
        }

        public virtual bool CanResetAllTimers() {
            return (TimerCollection.Count > 0) &&
                (TimerCollection.Any(t=>t.Timer>TimeSpan.Zero));
        }

        public virtual void ResetAllTimers()
        {
            ClearFilter();
            foreach (ActivityTimerVM timerVM in TimerCollection)
            {
                timerVM.Reset(); 
            }
        }

        public virtual bool CanDeleteAllTimers()
        {
            return (TimerCollection.Count > 0);
        }

        public virtual void DeleteAllTimers()
        {
            ClearFilter();
            List<ActivityTimerVM> timerList = new List<ActivityTimerVM>(TimerCollection);
            foreach (ActivityTimerVM timerVM in timerList)
            {
                DeleteTimer(timerVM);
            }
        }

        #endregion

        #region TimerCollection and Management

        protected void AddTimer(ActivityTimerVM timerVm)
        {
            timerVm.StopOrResetRequested += WriteData;
            timerVm.DeleteRequested += DeleteTimer;
            (timerVm as System.ComponentModel.INotifyPropertyChanged).PropertyChanged += TimerVM_PropertyChanged;
            TimerCollection.Add(timerVm);   
        }

        private void TimerVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if ("Timer".Equals(e.PropertyName))
            {
                UpdateTotalTime();
            }
        }

        private void TimerCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTotalTime();
            UpdateFilteredTimers();
        }


        protected void DeleteTimer(ActivityTimerVM timerVm)
        {
            timerVm.DeleteRequested -= DeleteTimer;
            timerVm.StopOrResetRequested -= WriteData;
            (timerVm as System.ComponentModel.INotifyPropertyChanged).PropertyChanged -= TimerVM_PropertyChanged;
            TimerCollection.Remove(timerVm);
            WriteData();
        }

        public virtual ObservableCollection<ActivityTimerVM> TimerCollection { get; set; }
        public virtual ObservableCollection<ActivityTimerVM> FilteredTimerCollection { get; set; }


        private TimeSpan totalTimeSpan = TimeSpan.Zero;
        private void UpdateTotalTime()
        {
            totalTimeSpan = TimeSpan.Zero;
            foreach (ActivityTimerVM timerVM in TimerCollection)
            {
                totalTimeSpan = totalTimeSpan.Add(timerVM.Timer);
            }
            if (totalTimeSpan.Seconds % 2 == 0)
            {
                TotalTime = string.Format("{0:00}:{1:00}", Convert.ToInt32(Math.Floor(totalTimeSpan.TotalHours)), Convert.ToInt32(totalTimeSpan.Minutes));
            }
            else
            {
                TotalTime = string.Format("{0:00} {1:00}", Convert.ToInt32(Math.Floor(totalTimeSpan.TotalHours)), Convert.ToInt32(totalTimeSpan.Minutes));
            }
        }
        #endregion

        #region Master Timer

        private void StartMasterTimer()
        {
            DispatcherTimer MasterTimer = new DispatcherTimer()
            {
                Interval = new TimeSpan(0, 0, 1)
            };
            MasterTimer.Tick += MasterTimer_Tick;
            MasterTimer.Start();
        }

        private void MasterTimer_Tick(object sender, EventArgs e)
        {
            foreach (ActivityTimerVM timerVM in TimerCollection)
            {
                if (timerVM.IsTimerEnabled)
                {
                    timerVM.Tick();
                }
            }
        }

        #endregion

        #region Persistence

        private string strFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + System.IO.Path.DirectorySeparatorChar + "TimeActivitiesUtility.csv";

        bool isIOInProgress = false;
        public bool ReadData()
        {
            if (isIOInProgress) return false;

            try
            {
                isIOInProgress = true;
                if (System.IO.File.Exists(strFilePath))
                {
                    TimerCollection.Clear();

                    // read the model...
                    IEnumerable<Data.ActivityTimerRow> rows;
                    using (System.IO.StreamReader sReader = new System.IO.StreamReader(strFilePath))
                    using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(sReader))
                    {
                        rows = csvReader.GetRecords<Data.ActivityTimerRow>();
                        foreach (Data.ActivityTimerRow row in rows)
                        {
                            // and update the ViewModel from the Model...
                            AddTimer(ActivityTimerVM.Create(TimeSpan.FromHours(row.TotalHours), row.ActivityText));
                        }
                    }
                    return true;
                }
                return false;
            }
            finally
            {
                isIOInProgress = false;
            }
        }

        public void WriteData()
        {
            if (isIOInProgress) return;

            try
            {
                isIOInProgress = true;

                // convert ActivityTimer View Model to Model...
                List<Data.ActivityTimerRow> rows = new List<Data.ActivityTimerRow>();
                foreach (ActivityTimerVM timerVM in TimerCollection)
                {
                    rows.Add(timerVM.GetModel());
                }

                // write the model out...
                using (var sWriter = new System.IO.StreamWriter(strFilePath))
                using (var csvWriter = new CsvHelper.CsvWriter(sWriter))
                {
                    csvWriter.WriteRecords(rows);
                }
            }
            finally
            {
                isIOInProgress = false;
            }
        }

        #endregion

    }
}
