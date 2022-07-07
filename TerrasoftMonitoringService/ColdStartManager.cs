using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TerrasoftMonitoringService
{
    class AuthenticationStatus
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Exception { get; set; }
        public object PasswordChangeUrl { get; set; }
        public object RedirectUrl { get; set; }
    }
    public class ColdStartManager
    {
        public static string SchemaName = "SandBox";
        //public static string Address = "";
        //public static string Login = "";
        //public static string Password = "";
        //public static string CertificateNumber = "";
        //public static string Endpoint = "";
        public int Timeout = 900000;

        private bool _isAuth;
        private CookieContainer _authCookie;
        private X509CertificateCollection _authCert;
        private int _errorCount;
        private bool result;

        public bool ExecuteColdStart(Configuration config)
        {
            result = false;
            try
            {
                if (!_isAuth)
                {
                    _isAuth = TryLogin(config.addressForLogin, config.loginForAuth, config.passwordForAuth, config.certificateNumber, out _authCookie, out _authCert);
                }

                if (_isAuth)
                {
                    result = TryExecuteProcess(config.endpointForColdStart, Timeout, _authCookie, _authCert);
                    _errorCount = 0;
                }
            }
            catch (WebException ex)
                //when (
                //    ex.Message.Contains("401") ||
                //    ex.Message.Contains("403") ||
                //    ex.Message.Contains("503") ||
                //    ex.Message.Contains("524") ||
                //    ex.Message.Contains("500"))
            {
                _isAuth = false;
                _errorCount++;
                Logger.Save("ColdStartManager", "ERROR", $"Try{_errorCount}: " + ex.Message);
                if (_errorCount < 3)
                {
                    ExecuteColdStart(config);
                }
                else
                {
                    _errorCount = 0;
                    return false;
                }
                //Logger.EvLog.WriteEntry($"Конфигурация {_jobConfig.SchemaName}. URI {_jobConfig.Endpoint}. \r\n {ex}", EventLogEntryType.Error);
            }

            return result;
        }

        public bool TryExecuteProcess(string endpoint, int timeout, CookieContainer authCookie, X509CertificateCollection authCert)
        {
            var request = WebRequest.Create(new Uri(endpoint)) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = 0;
            request.Timeout = timeout;
            request.CookieContainer = authCookie;
            request.UseDefaultCredentials = true;

            if (request.RequestUri.Scheme == Uri.UriSchemeHttps)
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
                request.ClientCertificates = authCert;
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public static bool TryLogin(string baseAddress, string login, string password, string certificateNumber,
            out CookieContainer authCookie, out X509CertificateCollection authCert)
        {
            authCookie = new CookieContainer();
            authCert = new X509CertificateCollection();

            var authRequest = WebRequest.Create(new Uri(baseAddress + @"/ServiceModel/AuthService.svc/Login")) as HttpWebRequest;
            authRequest.Method = "POST";
            authRequest.ContentType = "application/json";
            authRequest.Timeout = 60000;
            authRequest.CookieContainer = authCookie;
            authRequest.UseDefaultCredentials = true;

            if (authRequest.RequestUri.Scheme == Uri.UriSchemeHttps)
            {
                ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => true;
                var storeCert = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                storeCert.Open(OpenFlags.ReadOnly);
                authCert = storeCert.Certificates.Find(X509FindType.FindBySerialNumber, certificateNumber, true);
                storeCert.Close();

                if (authCert.Count == 0)
                {
                    throw new AuthenticationException("Сертификат безопасности не найден");
                }

                authRequest.ClientCertificates = authCert;
            }

            using (var requestStream = authRequest.GetRequestStream())
            {
                using (var writer = new StreamWriter(requestStream))
                {
                    writer.Write(@"{""TimeZoneOffset"": -180" + @", ""UserName"":""" + login + @""", ""UserPassword"":""" + password + @"""}");
                }
            }

            AuthenticationStatus status;

            using (var response = (HttpWebResponse)authRequest.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var responseText = reader.ReadToEnd();
                    status = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<AuthenticationStatus>(responseText);
                }
            }

            if (status != null)
            {
                if (status.Code == 0)
                {
                    return true;
                }

                Logger.Save("ColdStartManager Error", "ERROR", status.Message);
                //throw new AuthenticationException(status.Message);
            }

            return false;
        }
    }
}
