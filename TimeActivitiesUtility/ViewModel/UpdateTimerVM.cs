using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeActivitiesUtility.ViewModel
{
    [POCOViewModel(ImplementIDataErrorInfo = true)]
    public class UpdateTimerVM
    {
        protected UpdateTimerVM()
        {
        }

        public static UpdateTimerVM Create()
        {
            return ViewModelSource.Create(() => new UpdateTimerVM());
        }

        protected UpdateTimerVM(TimeSpan theTimer)
        {
            Timer = theTimer;
        }

        public static UpdateTimerVM Create(TimeSpan theTimer)
        {
            return ViewModelSource.Create(() => new UpdateTimerVM(theTimer));
        }

        private TimeSpan initialTimer = TimeSpan.Zero;
        public void InitializeTimer(TimeSpan tmr)
        {
            Timer = tmr;
            initialTimer = tmr;
        }

        public virtual TimeSpan Timer {
            get
            {
                try
                {
                    return TimeSpan.FromHours(Double.Parse(TimerDisplayText));
                }
                catch
                {
                    try
                    {
                        return TimeSpan.Parse(TimerDisplayText);
                    }
                    catch
                    {
                    }
                }
                return initialTimer;
            }
            set
            {
                TimerDisplayText = string.Format("{0:00}:{1:00}", Convert.ToInt32(Math.Floor(value.TotalHours)), Convert.ToInt32(value.Minutes));
            }
        }
        
        public virtual string TimerDisplayText { get; set; }

        #region Bindable commands

        public void Ok() {
            DialogCompleted(true,this);
        }

        public void Cancel()
        {
            DialogCompleted(false, this);
        }

        #endregion

        #region Event commands

        public delegate void DialogCompletedEventHandler(bool ok, UpdateTimerVM updateTimerVM);
        public event DialogCompletedEventHandler DialogCompleted;

        #endregion
    }
}
