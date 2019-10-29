using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeActivitiesUtility.ViewModel
{
    [POCOViewModel]
    public class ActivityTimerVM
    {
        protected ActivityTimerVM() { }
        public static ActivityTimerVM Create()
        {
            return ViewModelSource.Create(() => new ActivityTimerVM());
        }

        protected ActivityTimerVM(TimeSpan timer, string text)
        {
            Timer = timer;
            ActivityDescription = text;
        }
        public static ActivityTimerVM Create(TimeSpan timer, string text)
        {
            return ViewModelSource.Create(() => new ActivityTimerVM(timer,text));
        }

        #region Bindable Properties

        public virtual bool IsTimerEnabled { get; protected set; }

        #region Model Interface
        public virtual string ActivityDescription { get; set; }

        private TimeSpan _theTimer = TimeSpan.Zero;
        public TimeSpan Timer {
            get
            {
                return _theTimer;
            }
            set {
                _theTimer = value;
                if (_theTimer.Seconds % 2 == 0)
                {
                    TimerDisplayText = string.Format("{0:00}:{1:00}", Convert.ToInt32(Math.Floor(_theTimer.TotalHours)), Convert.ToInt32(_theTimer.Minutes));
                }
                else
                {
                    TimerDisplayText = string.Format("{0:00} {1:00}", Convert.ToInt32(Math.Floor(_theTimer.TotalHours)), Convert.ToInt32(_theTimer.Minutes));
                }
            }
        }

        public virtual string TimerDisplayText { get; set; } = "00:00";

        public Data.ActivityTimerRow GetModel()
        {
            return new Data.ActivityTimerRow() { ActivityText = ActivityDescription, TotalHours = Timer.TotalHours };
        }
        #endregion

        #endregion

        #region Bindable Commands

        public void Delete()
        {
            DeleteRequested(this);
        }

        public bool CanReset()
        {
            return (Timer != TimeSpan.Zero);
        }

        public void Reset()
        {
            IsTimerEnabled = false;
            Timer = TimeSpan.Zero;
            TimerManuallyUpdated(this,Timer);
        }

        public void StartStop()
        {
            IsTimerEnabled = !IsTimerEnabled;
        }

        public void EditTimer()
        {
            EditTimerRequested(this);
        }

        #endregion

        #region Events
        public delegate void TimerManuallyUpdatedEventHandler(ActivityTimerVM activityTimer, TimeSpan elapsed);
        public event TimerManuallyUpdatedEventHandler TimerManuallyUpdated;

        public delegate void DeleteRequestedEventHandler(ActivityTimerVM activityTimer);
        public event DeleteRequestedEventHandler DeleteRequested;

        public delegate void EditTimerEventHandler(ActivityTimerVM activityTimer);
        public event EditTimerEventHandler EditTimerRequested;
        #endregion

        public void Tick()
        {
            Timer = Timer.Add(TimeSpan.FromSeconds(1));
            this.RaiseCanExecuteChanged(c=>c.Reset());
        }
    }
}
