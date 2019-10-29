using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeActivitiesUtility.Service
{
    public class UpdateTimerDialogService : IUpdateTimerDialogService
    {
        private UpdateTimer dialogUI;

        public TimeSpan? ShowDialog(TimeSpan initialValue)
        {
            dialogUI = new UpdateTimer();
            dialogUI.Owner = System.Windows.Application.Current.MainWindow;
            var vm = dialogUI.DataContext as ViewModel.UpdateTimerVM;
            vm.DialogCompleted += DialogCompleted;
            vm.Timer = initialValue;
            if (true==dialogUI.ShowDialog())
            {
                return vm.Timer;
            }

            return null;
        }

        public void DialogCompleted(bool ok, ViewModel.UpdateTimerVM updateTimerVM)
        {
            if (ok)
            {
                dialogUI.DialogResult = true;
            }
            else
            {
                dialogUI.DialogResult = false;
            }
        }
    }
}
