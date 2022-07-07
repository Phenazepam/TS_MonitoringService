using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    public static class ConfigurationManager
    {
        //DBUtils db;
        //Configuration config;
        //DefaultConfiguration dconfig;
        //public ConfigurationManager(DBUtils db)
        //{
        //    this.db = db;
        //    config.processOwner = GetConfigFromDb("processOwner");


        //}
        public static Configuration SetConfig(DBUtils db)
        {
            Configuration config = new Configuration
            {
                processOwner = GetConfigFromDb("processOwner", db),
                maxRAMUsage = Convert.ToDouble(GetConfigFromDb("maxRAMUsage", db)),
                maxCPUUsage = Convert.ToDouble(GetConfigFromDb("maxCPUUsage", db)),
                minWorkingTime = Convert.ToDouble(GetConfigFromDb("minWorkingTime", db)),
                updateFrequency = Convert.ToInt32(GetConfigFromDb("updateFrequency", db)),
                highLoadPeriod = Convert.ToInt32(GetConfigFromDb("highLoadPeriod", db)),
                restartDelay = Convert.ToInt32(GetConfigFromDb("restartDelay", db)),
                mailTo = GetConfigFromDb("mailTo", db),

                addressForLogin = GetConfigFromDb("addressForLogin", db),
                endpointForColdStart = GetConfigFromDb("endpointForColdStart", db),
                loginForAuth = GetConfigFromDb("loginForAuth", db),
                passwordForAuth = GetConfigFromDb("passwordForAuth", db),

                smsHubAddress = GetConfigFromDb("smsHubAddress", db),
                XXIidToSendPush = GetConfigFromDb("XXIidToSendPush", db).Split(';'),
                certificateNumber = GetConfigFromDb("certificateNumber", db)
            };
            return config;
        }

        public static string GetConfigFromDb(string name, DBUtils db)
        {
            var value = db.GetConfigParameter(name);
            if (/*value != null &&*/ value != "")
            {
                return value;
            }
            else
            {
                value = DefaultConfiguration.GetValue(name);
                string desc = DefaultConfiguration.GetDescription(name);
                db.SetConfigParameter(name, value, desc);
                return value;
                //return (string)GetType(DefaultConfiguration).GetProperty(name).GetValue(name);
            }
        }
    }
}
