using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    class PushResponse
    {
        public int Code { get; set; }
        public Response Response { get; set; }
        public object Exception { get; set; }
    }

    class Response
    {
        public string idsms;
    }

    static class PushUtils
    {
        //private static X509CertificateCollection authCert;
        //private static string smsHubAddress;
        //private static string[] XXIidToSendPush;
        //private static string certNumber;
        //private static string serverName; 

        //public PushUtils(Configuration config)
        //{
        //    certNumber = config.certificateNumber;
        //    smsHubAddress = config.smsHubAddress;
        //    XXIidToSendPush = config.XXIidToSendPush;
        //    serverName = config.serverName;
        //    GetCertificate(certNumber);

        //    //TrySendPush(smsHubAddress, authCert);
        //}


        public static void SendPushToXXIids(string title, string body, Configuration config)
        {
            string logBody = "Push was sent on " + config.smsHubAddress + '\n';

            string pushTitle = "Alarm on " + config.serverName;
            string pushBody = config.serverName + ": " + title + '-' + body;
            string result = "";
            foreach (string item in config.XXIidToSendPush)
            {
                try
                {
                    result = TrySendPush(item, pushTitle, pushBody, "TerrasoftMonitoringService", config);
                    if (result != null)
                    {
                        logBody += item + " (idsms: " + result + ")" + '\n';
                    }
                    logBody += "With text: " + pushTitle + '-' + pushBody;

                    Logger.Save("SendPush", "INFO", logBody);
                }
                catch (Exception ex)
                {
                    Logger.Save("PushUtils Error", "ERROR", ex.Message);
                }
            }
            
        }

        private static X509CertificateCollection GetCertificate(string certNumber)
        {
            try
            {
                var storeCert = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                storeCert.Open(OpenFlags.ReadOnly);
                X509CertificateCollection authCert = storeCert.Certificates.Find(X509FindType.FindBySerialNumber, certNumber, true);
                storeCert.Close();
                return authCert;
            }
            catch (Exception ex)
            {
                Logger.Save("PushUtils Error", "ERROR", ex.Message);
                return null;
            }

        }

        public static string TrySendPush(string XXIid, string title, string body, string from, Configuration config)
        {
            var Request = WebRequest.Create(new Uri(config.smsHubAddress)) as HttpWebRequest;
            Request.Method = "POST";
            Request.ContentType = "application/json";
            Request.Timeout = 60000;
            //Request.CookieContainer = authCookie;

            if (Request.RequestUri.Scheme == Uri.UriSchemeHttps)
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
                Request.ClientCertificates = GetCertificate(config.certificateNumber);
            }

            using (var requestStream = Request.GetRequestStream())
            {
                using (var writer = new StreamWriter(requestStream))
                {
                    //string str = @"{""SmsRequest"": {""clientId"":""" + XXIid + @""", ""msgTitle"":""" + title + @""", ""msgBody"":""" + body + @""", ""typemessage"": ""PUSHONLY"", ""from"": """ + from + @"""}}";
                    writer.Write(@"{""SmsRequest"": {""clientId"":""" + XXIid + @""", ""msgTitle"":""" + title + @""", ""msgBody"":""" + body + @""", ""typemessage"": ""PUSHONLY"", ""from"": """ + from + @"""}}");
                }
            }

            PushResponse res;

            using (var response = (HttpWebResponse)Request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseText = reader.ReadToEnd();
                    res = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<PushResponse>(responseText);
                }
            }

            if (res != null)
            {
                if (res.Code == 0 && res.Response.idsms != null)
                {
                    return res.Response.idsms;
                }

                throw new Exception(Convert.ToString(res.Exception));
            }


            return null;

        }

    }
}
