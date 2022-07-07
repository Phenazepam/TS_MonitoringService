using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    public partial class TerrasoftMonitoringService : ServiceBase
    {
        Monitoring monitoring;
        public TerrasoftMonitoringService()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            Configuration config = new Configuration();

            DBUtils db = new DBUtils(config);

            config = ConfigurationManager.SetConfig(db);

            Capacity capacity = new Capacity(config);

            //PushUtils utils = new PushUtils(config);

            monitoring = new Monitoring(config, capacity, db);

            //db.SendMail("osfo@lotus.bank.srv", "Проверка связи", "Здорова, мужики, проверка связи. Раз-раз");

            Logger.Save("Service Start", "INFO", $"Monitoring service started on {DateTime.Now}");

            Thread monitoringThread = new Thread(new ThreadStart(monitoring.Start));
            monitoringThread.Start();
        }

        protected override void OnStop()
        { 
            Logger.Save("Service Stop", "INFO", $"Monitoring service stopped on {DateTime.Now}");
            monitoring.Stop();
            Thread.Sleep(2000);
        }
    }
}
