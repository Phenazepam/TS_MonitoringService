using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    public class DBUtils
    {
        OracleConnection con;
        string serverName;
        string CSIGuid;
        string SCGuid;

        public DBUtils(Configuration config)
        {
            CreateConnection(config);
            serverName = config.serverName;
            Logger.db = this;

            CSIGuid = GetGuid("ColdStartInfo");
            Console.WriteLine(CSIGuid);
            SCGuid = GetGuid("ServerCapacity");
            Console.WriteLine(SCGuid);
        }



        public int StopUsing()
        {
            try
            {
                string s = String.Format("UPDATE \"ColdStartInfo\"" +
                                            " SET \"InUse\" = 0" +
                                            " WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int StartUsing()
        {
            try
            {
                string s = String.Format("UPDATE \"ColdStartInfo\"" +
                                            " SET \"InUse\" = 1" +
                                            " WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int SetInColdStart()
        {
            try
            {
                string s = String.Format("UPDATE \"ColdStartInfo\"" +
                                            " SET \"InColdStart\" = 1" +
                                            " WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int SetNotInColdStart()
        {
            try
            {
                string s = String.Format("UPDATE \"ColdStartInfo\"" +
                                            " SET \"InColdStart\" = 0" +
                                            " WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int SetNotExecuteRestart()
        {
            try
            {
                string s = String.Format("UPDATE \"ColdStartInfo\"" +
                                            " SET \"ExecuteRestart\" = 0" +
                                            " WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void UpdateCapacity(Capacity capacity)
        {
            UpdateCapacityParam("CPUusage", capacity.CPULoad.ToString());
            UpdateCapacityParam("RAMusage", capacity.RAMLoad.ToString());
            UpdateCapacityParam("ProcessWorkingTime", capacity.processTimeWorking.ToString());
            SaveCapacity(capacity);
        }

        int UpdateCapacityParam(string param, string value)
        {
            try
            {
                string s = String.Format("UPDATE \"ServerCapacity\"" +
                                            " SET \"{0}\" = '{1}'" +
                                            " WHERE \"Id\" = '{2}'", param, value, SCGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        int SaveCapacity(Capacity capacity)
        {
            try
            {
                string s = String.Format("INSERT INTO \"ServerCapacityGraph\" (\"CreatedOn\", \"ModifiedBy\", \"ModifiedOn\", \"ColdStartInfoId\",\"ServerName\", " +
                    " \"CPUusage\", \"RAMusage\", \"ProcessWorkingTime\")" +
                    " VALUES ('{0}', 'SERVICE', '{0}', '{1}', '{2}', '{3}', '{4}', '{5}')", DateTime.Now, CSIGuid, serverName, capacity.CPULoad, capacity.RAMLoad, capacity.processTimeWorking);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        protected void CreateConnection(Configuration config)
        {
            con = new OracleConnection();
            OracleConnectionStringBuilder ocsb = new OracleConnectionStringBuilder
            {
                Password = config.db.dbPassword,
                UserID = config.db.dbUser,
                DataSource = config.db.dataSource
            };
            try
            {
                con.ConnectionString = ocsb.ConnectionString;
                con.Open();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        protected string GetGuid(string table)
        {
            try
            {
                string s = String.Format("SELECT \"Id\" FROM \"{0}\" WHERE \"ServerName\" = '{1}'", table, serverName);
                OracleCommand cmd = new OracleCommand(s, con);
                object guid = cmd.ExecuteScalar();
                if (guid == null)
                {
                    NewRecord(table);
                    Console.WriteLine($"GUID for table {table} for {serverName} is not found! New record added.");
                    Logger.Save("NoGuid", "INFO", $"GUID for table {table} for {serverName} is not found! New record added.");
                    return GetGuid(table);
                }
                return Convert.ToString(guid);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        int NewRecord(string table)
        {
            try
            {
                string s = String.Format("INSERT INTO \"{2}\"(\"CreatedOn\", \"ModifiedBy\", \"ModifiedOn\", \"ServerName\") " +
                                          "VALUES('{0}', 'SERVICE', '{0}', '{1}')", DateTime.Now, serverName, table);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int GetInUseValue()
        {
            try
            {
                string s = String.Format("SELECT \"InUse\" FROM \"ColdStartInfo\" WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                object obj = cmd.ExecuteScalar();
                try
                {
                    return Convert.ToInt16(obj);
                }
                catch (Exception)
                {
                    return 0;
                }
                //return Convert.ToInt16(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int GetInAutomaticModeValue()
        {
            try
            {
                string s = String.Format("SELECT \"InAutomaticMode\" FROM \"ColdStartInfo\" WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                object obj = cmd.ExecuteScalar();
                try
                {
                    return Convert.ToInt16(obj);
                }
                catch (Exception)
                {
                    return 0;
                }
                //return Convert.ToInt16(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int GetExecuteRestartValue()
        {
            try
            {
                string s = String.Format("SELECT \"ExecuteRestart\" FROM \"ColdStartInfo\" WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                object obj = cmd.ExecuteScalar();
                try
                {
                    return Convert.ToInt16(obj);
                }
                catch (Exception)
                {
                    return 0;
                }
                //return Convert.ToInt16(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int GetInColdStartValue()
        {
            try
            {
                string s = String.Format("SELECT \"InColdStart\" FROM \"ColdStartInfo\" WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                object obj = cmd.ExecuteScalar();
                try
                {
                    return Convert.ToInt16(obj);
                }
                catch (Exception)
                {
                    return 0;
                }
                //return Convert.ToInt16(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int GetRenewConfigValue()
        {
            try
            {
                string s = String.Format("SELECT \"RenewConfig\" FROM \"ColdStartInfo\" WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                object obj = cmd.ExecuteScalar();
                try
                {
                    return Convert.ToInt16(obj);
                }
                catch (Exception)
                {
                    return 0;
                }
                //return Convert.ToInt16(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int SetNotRenewConfig()
        {
            try
            {
                string s = String.Format("UPDATE \"ColdStartInfo\"" +
                                            " SET \"RenewConfig\" = 0" +
                                            " WHERE \"Id\" = '{0}'", CSIGuid);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int ToLog(string Event, string level, string message)
        {
            try
            {
                string s = String.Format("INSERT INTO \"ColdStartLog\" (\"EventDate\", \"ServerName\", \"User\", \"Event\", \"Level\",\"Message\") " +
                                          "VALUES('{0}', '{1}', 'SERVICE', '{2}', '{3}','{4}')", DateTime.Now, serverName, Event, level, message);
                OracleCommand cmd = new OracleCommand(s, con);
                return cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public int SendMail(string mailTo, string title, string body)
        {
            try
            {
                //string s = String.Format("EXEC MAIL_PKG.SEND('{0}','{1}','{2}');", mailTo, title, body);
                //string s = "EXEC MAIL_PKG.SEND('aamusienko@lotus.bank.srv','Test','Test');";
                //OracleCommand cmd = new OracleCommand(s, con);
                //return cmd.ExecuteNonQuery();


                using (OracleCommand cmd = new OracleCommand("BPM.MAIL_PKG.SEND", con))
                {
                    cmd.BindByName = true;
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "BPM.MAIL_PKG.SEND";
                    cmd.Parameters.Add("mailto", OracleDbType.NVarchar2).Value = mailTo;
                    cmd.Parameters.Add("subject", OracleDbType.NVarchar2).Value = title;
                    cmd.Parameters.Add("message", OracleDbType.Clob).Value = body;
                    return cmd.ExecuteNonQuery();
                }

            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string GetConfigParameter(string name)
        {
            try
            {
                string s = String.Format("SELECT \"Value\" FROM \"ColdStartConfig\" WHERE \"ServerName\" = '{0}' AND \"Name\" = '{1}'", serverName, name);
                OracleCommand cmd = new OracleCommand(s, con);
                object obj = cmd.ExecuteScalar();
                try
                {
                    return Convert.ToString(obj);
                }
                catch (Exception)
                {
                    return null;
                }
                //return Convert.ToInt16(cmd.ExecuteScalar());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int SetConfigParameter(string name, string value, string description)
        {
            try
            {
                string s = String.Format("SELECT \"Id\" FROM \"ColdStartConfig\" WHERE \"ServerName\" = '{0}' AND \"Name\" = '{1}'", serverName, name);
                OracleCommand cmd = new OracleCommand(s, con);
                object obj = cmd.ExecuteScalar();
                if (Convert.ToString(obj) == null || Convert.ToString(obj) == "")
                {
                    s = String.Format("INSERT INTO \"ColdStartConfig\"(\"CreatedOn\", \"ModifiedBy\", \"ModifiedOn\", \"ServerName\", \"Name\", \"Value\", \"Description\") " +
                                              "VALUES('{0}', 'SERVICE', '{0}', '{1}', '{2}', '{3}', '{4}')", DateTime.Now, serverName, name, value, description);
                    cmd = new OracleCommand(s, con);
                    return cmd.ExecuteNonQuery();
                }
                else
                {
                    s = String.Format("UPDATE \"ServerCapacity\"" +
                                            " SET \"Value\" = '{0}'," +
                                            "\"Description\" = '{1}'" +
                                            " WHERE \"Id\" = '{2}'", value, description, Convert.ToString(obj));
                    cmd = new OracleCommand(s, con);
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
