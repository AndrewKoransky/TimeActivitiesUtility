using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace TimeActivitiesUtility.ViewModel
{
    [POCOViewModel]
    public class MainWindowVM
    {
        protected MainWindowVM()
        {
            TimerCollection = new ObservableCollection<ActivityTimerVM>();

            ReadData();

            StartMasterTimer();
        }

        public static MainWindowVM Create()
        {
            return ViewModelSource.Create(() => new MainWindowVM());
        }

        #region Bindable properties

        public virtual string TotalTime { get; protected set; }

        #endregion

        #region Bindable commands

        public virtual void AddNewTimer() {
            AddTimer(ActivityTimerVM.Create());
        }

        public virtual bool CanResetAllTimers() {
            return (TimerCollection.Count > 0) &&
                (TimerCollection.Any(t=>t.Timer>TimeSpan.Zero));
        }

        public virtual void ResetAllTimers()
        {
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
            timerVm.DeleteRequested += DeleteTimer;
            timerVm.TimerManuallyUpdated += TimerManuallyUpdated;
            TimerCollection.Add(timerVm);
            // Calling UpdateTotalTime() is not needed here... 
            // The timer will either be zero (adding from the UI), 
            // or it is being read on ReadData() in which case the total will be calculated upon completion.
        }

        protected void DeleteTimer(ActivityTimerVM timerVm)
        {
            timerVm.DeleteRequested -= DeleteTimer;
            timerVm.TimerManuallyUpdated -= TimerManuallyUpdated;
            TimerCollection.Remove(timerVm);
            UpdateTotalTime();
        }

        public virtual ObservableCollection<ActivityTimerVM> TimerCollection { get; set; }

        void TimerManuallyUpdated(ActivityTimerVM timerVM, TimeSpan elapsed)
        {
            UpdateTotalTime();
        }

        private TimeSpan totalTimeSpan = TimeSpan.Zero;
        private void UpdateTotalTime()
        {
            TimeSpan prevTotalTimeSpan = totalTimeSpan;
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

            // if the visible total time has changed, save the data...
            if ((prevTotalTimeSpan.TotalHours != totalTimeSpan.TotalHours) || (prevTotalTimeSpan.Minutes != totalTimeSpan.Minutes))
            {
                WriteData();
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

            UpdateTotalTime();
        }

        #endregion

        #region Persistence

        private string strFilePath = System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + System.IO.Path.DirectorySeparatorChar + "TimeActivitiesUtility2.csv";

        public bool ReadData()
        {
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
                UpdateTotalTime();
                return true;
            }
            return false;
        }

        public void WriteData()
        {
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

        #endregion

    }
}
