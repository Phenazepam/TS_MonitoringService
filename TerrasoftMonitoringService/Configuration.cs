using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{

    public static class DefaultConfiguration
    {
        public static string[] processOwner { get; set; } = { "KUBANKREDIT", "Наименование пула процесса" };
        public static string[] maxRAMUsage { get; set; } = { "80", "Максимальная нагрузка на оперативную память (в %)" };
        public static string[] maxCPUUsage { get; set; } = { "80", "Максимальная нагрузка на процессор (в %)" };
        public static string[] minWorkingTime { get; set; } = { "240", "Время непрерывной работы процесса w3wp (в сек.)" };
        public static string[] updateFrequency { get; set; } = { "3", "Частота обновления (в сек.)" };
        public static string[] highLoadPeriod { get; set; } = { "120", "Время работы под максимальной нагрузкой (в сек.)" };
        public static string[] restartDelay { get; set; } = { "30", "Задержка перед перезагрузкой после срабатывания тревоги (в сек.)" };
        public static string[] mailTo { get; set; } = { "aamusienko@lotus.bank.srv", "Адрес почты для отправки оповещений" };
        public static string[] addressForLogin { get; set; } = { "http://localhost", "Адрес для входа в систему и открытия сессии" };
        public static string[] endpointForColdStart { get; set; } = { "http://localhost/0/Nui/ViewModule.aspx", "Адрес для запуска холодного старта" };
        public static string[] loginForAuth { get; set; } = { "SDH", "Логин для входа и запуска ХС" };
        public static string[] passwordForAuth { get; set; } = { "w3tX*p#gH38M3@}$TM", "Пароль для входа и запуска ХС" };
        public static string[] smsHubAddress { get; set; } = { "NULL", "Адрес шины СМС хаба" }; //https://bus-sms.bank.srv:9443/rest/sendsms/
        public static string[] XXIidToSendPush { get; set; } = { "NULL", "Клиентские XxiId сотрудников для отправки PUSH (для нескольких разделитель - ;)" };
        public static string[] certificateNumber { get; set; } = { "NULL", "Серийный номер сертификата для обращения на шину СМС хаба" };

        public static string GetValue(string propertyName)
        {
            Type DConf = typeof(DefaultConfiguration);
            PropertyInfo property = DConf.GetProperty(propertyName);
            string[] value = (string[])property.GetValue(propertyName);
            return value[0];
        }

        public static string GetDescription(string propertyName)
        {
            Type DConf = typeof(DefaultConfiguration);
            PropertyInfo property = DConf.GetProperty(propertyName);
            string[] value = (string[])property.GetValue(propertyName);
            return value[1];
        }
    }


    public class Configuration
    {
        //public static Dictionary<string, string> defaultConfigArray = new Dictionary<string, string>()
        //{
        //    {"defaultProcessOwner",  "KUBANKREDIT"},
        //    {"defaultMaxRAMUsage",  "80"},
        //    {"defaultmaxCPUUsage",  "80"},
        //    {"defaultMinWorkingTime",  "240"},
        //    {"defaultUpdateFrequency",  "3"},
        //    {"defaultHighLoadPeriod",  "120"},
        //    {"defaultMailTo",  "aamusienko@lotus.bank.srv"},
        //    {"defaultSmsHubAddress",  "https://bus-sms.bank.srv:9443/rest/sendsms/"},
        //    {"defaultXXIidToSendPush",  ""},
        //    {"defaultCertificateNumber",  ""},
        //};

        public string processOwner { get; set; }
        public string serverName { get; set; }
        public double maxRAMUsage { get; set; }
        public double maxCPUUsage { get; set; }
        public double minWorkingTime { get; set; }
        public int updateFrequency { get; set; }
        public int highLoadPeriod { get; set; }
        public int restartDelay { get; set; }
        public string mailTo { get; set; }
        public string addressForLogin { get; set; }
        public string endpointForColdStart { get; set; }
        public string loginForAuth { get; set; }
        public string passwordForAuth { get; set; }
        public string smsHubAddress { get; set; }
        public string[] XXIidToSendPush { get; set; }
        public string certificateNumber { get; set; }
        public DbConfiguration db { get; set; }

        public Configuration()
        {
            this.db = new DbConfiguration();
            //processOwner = System.Configuration.ConfigurationSettings.AppSettings["processOwner"];
            serverName = System.Configuration.ConfigurationSettings.AppSettings["serverName"];
            //maxRAMUsage = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["maxRAMUsage"]);
            //maxCPUUsage = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["maxCPUUsage"]);
            //minWorkingTime = Convert.ToDouble(System.Configuration.ConfigurationSettings.AppSettings["minWorkingTime"]);
            //updateFrequency = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["updateFrequency"]);
            //highLoadPeriod = Convert.ToInt32(System.Configuration.ConfigurationSettings.AppSettings["highLoadPeriod"]);
            //mailTo = System.Configuration.ConfigurationSettings.AppSettings["mailTo"];
            //smsHubAddress = System.Configuration.ConfigurationSettings.AppSettings["smsHubAddress"];
            //XXIidToSendPush = System.Configuration.ConfigurationSettings.AppSettings["XXIidToSendPush"].Split(';');
            //certificateNumber = System.Configuration.ConfigurationSettings.AppSettings["certificateNumber"];
        }

    }

    public class DbConfiguration
    {
        public string dataSource { get; set; }
        public string dbUser { get; set; }
        public string dbPassword { get; set; }
        public DbConfiguration()
        {
            dataSource = System.Configuration.ConfigurationSettings.AppSettings["dataSource"];
            dbUser = System.Configuration.ConfigurationSettings.AppSettings["dbUser"];
            dbPassword = System.Configuration.ConfigurationSettings.AppSettings["dbPassword"];
        }
    }
}
