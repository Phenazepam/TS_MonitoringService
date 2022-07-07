using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    public static class ProcessManager
    {
        public static bool ProcessStop(int processUid)
        {
            try
            {
                if (processUid != -1)
                {
                    Process process = Process.GetProcessById(processUid);
                    process.Kill();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Save("ProcessManager Error", "ERROR", ex.Message);
                return false;
            }
        }
    }
}
