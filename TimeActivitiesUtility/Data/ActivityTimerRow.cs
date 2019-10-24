using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeActivitiesUtility.Data
{
    public class ActivityTimerRow
    {
        [CsvHelper.Configuration.Attributes.Name("Activity Text")]
        public string ActivityText { get; set; }

        [CsvHelper.Configuration.Attributes.Name("Total Hours")]
        public double TotalHours { get; set; }
    }
}
