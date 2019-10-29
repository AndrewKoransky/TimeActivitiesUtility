using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeActivitiesUtility.Service
{
    public interface IUpdateTimerDialogService
    {
        TimeSpan? ShowDialog(TimeSpan initialValue);
    }
}
