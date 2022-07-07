using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    static class Logger
    {
        public static DBUtils db { get; set; }

        public static bool Save(string Event, string level, string message)
        {
            return (db.ToLog(Event, level, message) == 1) ? true : false;
        }
    }

}
