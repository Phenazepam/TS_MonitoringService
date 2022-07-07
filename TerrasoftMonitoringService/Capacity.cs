using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    public class Capacity
    {
        protected List<float> RAMLoadList = new List<float>();
        protected List<float> processLoadList = new List<float>();
        protected string processOwner;
        protected int processUID;
        protected int updateFrequency;

        public double CPULoad;
        public double RAMLoad;
        public bool isW3WPAlive;
        public TimeSpan processTimeWorking;

        public Capacity(Configuration config)
        {
            processOwner = config.processOwner;
            updateFrequency = config.updateFrequency;
            Init();

            GetProcessorLoad();
            GetRAMLoad();
            IsW3WPAlive();
            GetProcessTimeWorking();
        }

        protected void Init()
        {
            GetProcessUID();
        }


        public void Reload()
        {
            Init();

            GetProcessorLoad();
            GetRAMLoad();
            IsW3WPAlive();
            GetProcessTimeWorking();
        }


        protected void GetProcessorLoad()
        {
            ObjectQuery objQuery = new ObjectQuery("SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
            ManagementObjectSearcher mngObjSearch = new ManagementObjectSearcher(@"\root\CIMV2", "SELECT * FROM Win32_PerfFormattedData_PerfOS_Processor WHERE Name=\"_Total\"");
            ManagementObjectCollection mngObjColl = mngObjSearch.Get();
            uint cpu_usage = 0;
            if (mngObjColl.Count > 0)
            {
                foreach (ManagementObject mngObject in mngObjColl)
                {
                    try
                    {
                        cpu_usage = 100 - Convert.ToUInt32(mngObject["PercentIdleTime"]);
                        if (this.processLoadList.Count < (60 / updateFrequency))
                        {
                            processLoadList.Add(cpu_usage);
                        }
                        else
                        {
                            processLoadList.RemoveAt(0);
                            processLoadList.Add(cpu_usage);
                        }
                        //CPULoad = Math.Round(processLoadList.Sum() / processLoadList.Count);
                        CPULoad = Math.Round(Convert.ToDouble(cpu_usage), 2);
                        break;
                    }
                    catch (Exception ex)
                    {
                        break;
                        throw ex;
                    }
                }
            }

        }

        protected void GetRAMLoad()
        {
            PerformanceCounter memcounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            float tmp = memcounter.NextValue();

            //if (this.RAMLoadList.Count < 60)
            //{
            //    RAMLoadList.Add(tmp);
            //}
            //else
            //{
            //    RAMLoadList.RemoveAt(0);
            //    RAMLoadList.Add(tmp);
            //}
            //RAMLoad = Math.Round(RAMLoadList.Sum() / RAMLoadList.Count, 2);
            RAMLoad = Math.Round(tmp, 2);
            Console.WriteLine(RAMLoad);
        }

        protected void IsW3WPAlive()
        {
            try
            {
                Process proc = Process.GetProcessById(processUID);
                isW3WPAlive = true;
            }
            catch
            {
                isW3WPAlive = false;
            }
            //DateTime procStart = proc.StartTime;
            //Console.WriteLine(procStart.ToShortTimeString());

            //Process[] processes = Process.GetProcessesByName("w3wp");
            //Console.WriteLine(processes[0].StartTime);
            //processes[0].Kill();
        }

        protected void GetProcessTimeWorking()
        {
            try
            {
                Process proc = Process.GetProcessById(processUID);
                processTimeWorking = DateTime.Now.Subtract(proc.StartTime);
            }
            catch
            {
                isW3WPAlive = false;
                processTimeWorking = TimeSpan.Zero;
            }
        }
        protected void GetProcessUID()
        {
            Process[] proc = Process.GetProcessesByName("w3wp");
            foreach (Process pr in proc)
            {
                if (GetProcessOwner(pr.Id) == this.processOwner)
                {

                    processUID = pr.Id;
                    break;
                }
                else
                {
                    processUID = -1;
                }
            }
        }

        protected string GetProcessOwner(int processId)
        {
            string query = "Select * from Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            foreach (ManagementObject obj in processList)
            {
                try
                {
                    string[] argList = new string[] { string.Empty, string.Empty };
                    int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                    if (returnVal == 0)
                    {
                        // return DOMAIN\user
                        string owner = /*argList[1] + "\\" +*/ argList[0];
                        return owner;
                    }
                }
                catch
                {

                }
            }

            return "NO OWNER";
        }

        public int GetProcessId()
        {
            GetProcessUID();
            return processUID;
        }
    }
}

