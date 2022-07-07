using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    class Monitoring
    {
        //protected double maxRAMUsage;
        //protected double maxCPUUsage;
        //protected TimeSpan minWorkTime;
        //protected int updateFrequency;
        //protected int highLoadPeriod;
        //protected string mailTo;
        protected string errorText;
        protected string errorTitle;
        //protected string serverName;

        protected bool enabled;

        Capacity capacity;
        DBUtils db;
        //PushUtils pushUtils;
        CheckRAM checkRAM;
        CheckCPU checkCPU;
        Configuration config;
        public Monitoring(Configuration config, Capacity capacity, DBUtils db)
        {

            //pushUtils = new PushUtils(config);
            this.db = db;
            this.config = config;
            //maxRAMUsage = config.maxRAMUsage;
            //maxCPUUsage = config.maxCPUUsage;
            //minWorkTime = TimeSpan.FromSeconds(config.minWorkingTime);
            //updateFrequency = config.updateFrequency;
            //highLoadPeriod = config.highLoadPeriod;
            //mailTo = config.mailTo;
            //serverName = config.serverName;

            enabled = true;

            this.capacity = capacity;
            checkRAM = new CheckRAM(config.updateFrequency, config.highLoadPeriod);
            checkCPU = new CheckCPU(config.updateFrequency, config.highLoadPeriod);
        }

        public void Start()
        {
            while (enabled)
            {
                if (IsActiveMode() && !IsInColdStart())
                {
                    if (IsOkay())
                    {
                        Console.WriteLine("Active");
                        db.UpdateCapacity(capacity);
                        capacity.Reload();
                    }
                    else
                    {
                        Alarm();
                        if (IsInAutomaticMode())
                        {
                            Thread restartThread = new Thread(this.ExecuteRestart);
                            Thread.Sleep(config.restartDelay * 1000);
                            restartThread.Start();
                            //ExecuteRestart();
                            capacity.Reload();
                        }

                        //break;
                    }
                }
                else
                {
                    Console.WriteLine("Passive");
                    if (IsRenewConfig())
                    {
                        Console.WriteLine("Обновляем конфиг");
                        config = ConfigurationManager.SetConfig(db);
                        db.SetNotRenewConfig();
                        Logger.Save("Config Updated", "INFO", "Config was updated on " + DateTime.Now);
                    }
                    if (IsExecuteRestart())
                    {
                        Thread restartThread = new Thread(this.ExecuteRestart);
                        restartThread.Start();
                    }
                    db.UpdateCapacity(capacity);
                    capacity.Reload();
                }
                Thread.Sleep(config.updateFrequency * 1000);
            }
        }

        public void Stop()
        {
            this.enabled = false;
        }
        bool IsOkay()
        {
            bool res = true;
            if (!checkRAM.IsOkay(capacity.RAMLoad, config.maxRAMUsage))
            {
                errorText = $"RAM usage is more {config.maxRAMUsage}% for {config.highLoadPeriod} seconds";
                errorTitle = "RAMoverflow";
                //Logger.Save("RAMoverflow", errorText);
                //pushUtils.SendPushToXXIids("RAMoverflow", errorText);
                //Console.WriteLine($"RAM usage is more {maxRAMUsage} for {highLoadPeriod} seconds");
                return false;
            }
            if (!checkCPU.IsOkay(capacity.CPULoad, config.maxCPUUsage))
            {
                errorText = $"CPU usage is more {config.maxCPUUsage}% for {config.highLoadPeriod} seconds";
                errorTitle = "CPUoverflow";
                //Logger.Save("CPUoverflow", errorText);
                //pushUtils.SendPushToXXIids("CPUoverflow", errorText);
                return false;
            }
            if (!capacity.isW3WPAlive)
            {
                errorText = $"Process w3wp.exe is not found";
                errorTitle = "ProcessDrop";
                //Logger.Save("ProcessDrop", errorText);
                //pushUtils.SendPushToXXIids("ProcessDrop", errorText);
                return false;
            }
            if (capacity.processTimeWorking < TimeSpan.FromSeconds(config.minWorkingTime))
            {
                errorText = $"Process w3wp.exe works less then {TimeSpan.FromSeconds(config.minWorkingTime)} seconds";
                errorTitle = "ProcessDrop";
                //Logger.Save("ProcessDrop", errorText);
                //pushUtils.SendPushToXXIids("ProcessDrop", errorText);
                return false;
            }

            return res;
        }

        void Alarm()
        {
            Console.WriteLine("ALARM!!!!!");
            db.StopUsing();
            Logger.Save("Alarm", "INFO", $"Using of application server stopped");
            db.SendMail(config.mailTo, config.serverName + " - ALARM!", $"Нагрузка с {config.serverName} снята в {DateTime.Now} по причине: {errorText}");
            Logger.Save(errorTitle, "INFO", errorText);
            try
            {
                PushUtils.SendPushToXXIids(errorTitle, errorText, config);
            }
            catch (Exception e)
            {
                Logger.Save("Monitoring Error", "ERROR", e.Message);
            }

        }

        bool IsActiveMode()
        {
            return (db.GetInUseValue() == 1) ? true : false;
        }

        bool IsInAutomaticMode()
        {
            return (db.GetInAutomaticModeValue() == 1) ? true : false;
        }
        bool IsExecuteRestart()
        {
            return (db.GetExecuteRestartValue() == 1) ? true : false;
        }
        bool IsInColdStart()
        {
            return (db.GetInColdStartValue() == 1) ? true : false;
        }
        bool IsRenewConfig()
        {
            return (db.GetRenewConfigValue() == 1) ? true : false;
        }

        void ExecuteRestart()
        {
            try
            {

                if (!IsActiveMode() && !IsInColdStart())
                {
                    ProcessManager.ProcessStop(capacity.GetProcessId());
                    Console.WriteLine("Начинаем перезапуск");
                    Logger.Save("Restart Executed", "INFO", "Restart was executed on " + DateTime.Now);
                    db.SendMail(config.mailTo, config.serverName + ": Restart.", $"Перезапуск {config.serverName} начат в {DateTime.Now}.");
                    db.SetInColdStart();
                    ColdStartManager cs = new ColdStartManager();
                    if (cs.ExecuteColdStart(config))
                    {
                        db.SetNotInColdStart();
                        Console.WriteLine("Перезапуск завершен");
                        Logger.Save("Restart Ended", "INFO", "Restart ended on " + DateTime.Now);
                        db.StartUsing();
                        db.SetNotExecuteRestart();
                        db.SendMail(config.mailTo, config.serverName + ": Restart finished.", $"Перезапуск {config.serverName} закончен в {DateTime.Now}.");
                        PushUtils.SendPushToXXIids(config.serverName + ": Restart finished.", "Restart on " + config.serverName + " finished on " + DateTime.Now, config);
                    }
                    else
                    {
                        db.SetNotInColdStart();
                        db.SetNotExecuteRestart();
                        Console.WriteLine("Ошибка перезапуска");
                        Logger.Save("Restart Error", "ERROR", "Restart was not ended. Check log.");
                        db.SendMail(config.mailTo, config.serverName + ": Restart Error.", $"Перезапуск {config.serverName} завершился с ошибкой в {DateTime.Now}. Необходимо проверить лог.");
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Save("Monitoring Error. ExecuteRestart", "ERROR", ex.Message);
            }
        }
    }

    class CheckRAM
    {
        int deathClock;
        int max;
        public CheckRAM(int updFreq, int highLoadPeriod)
        {
            max = highLoadPeriod / updFreq;
        }

        public bool IsOkay(double RAMusage, double maxRAMUsage)
        {
            deathClock = (RAMusage > maxRAMUsage) ? deathClock + 1 : 0;
            return (deathClock >= max) ? false : true;
        }
    }

    class CheckCPU
    {
        int deathClock;
        int max;
        public CheckCPU(int updFreq, int highLoadPeriod)
        {
            max = highLoadPeriod / updFreq;
        }

        public bool IsOkay(double CPUusage, double maxCPUUsage)
        {
            deathClock = (CPUusage > maxCPUUsage) ? deathClock + 1 : 0;
            return (deathClock >= max) ? false : true;
        }
    }
}
