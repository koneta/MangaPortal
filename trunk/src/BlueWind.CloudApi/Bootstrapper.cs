using BlueWind.CloudApi.Controllers;
using BlueWind.CloudApi.Utility;
using BlueWind.Crawler.Core;
using BlueWind.Crawler.Manga.Domain;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using System;
using System.IO;
using System.Security.Cryptography;
using System.ServiceModel;
using System.Text;
using System.Web.Http;
using System.Web.Http.SelfHost;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Mvc;
using BlueWind.Common;
using BlueWind.Crawler.Manga;
using BlueWind.WebCore.Unity;

namespace BlueWind.CloudApi
{
    public static class Bootstrapper
    {
        private static string GlobalPassword = "454bb2978055ef3cd02aff23c891ed54";
        
        public static string LogPath;
        
        private static UnityContainer container;
        
        public static void Initialise(bool isSelfHost)
        {
            AppDomain.CurrentDomain.SetData("SQLServerCompactEditionUnderWebHosting", true);
            MangaSettings.Default["ConnectionString"] = "data source=.\\SQLEXPRESS;database=Manga;multipleactiveresultsets=True;Integrated Security=true";
            LogPath = String.Format("C:\\BlueWindCloud\\ApplicationSessionLog_{0:yyyyMMddHHmmss}.log", DateTime.Now);
            Logger.LogAction = LogCallback;
            container = new UnityContainer();
            var unityServiceLocator = new UnityServiceLocator(container);
            ServiceLocator.SetLocatorProvider(() => unityServiceLocator);
            container.RegisterType<SitesContext>("SitesContext");
            MangaController.Initialize();
            HttpSelfHostServer server;
            HttpConfiguration apiHttpConfiguration;


            if (isSelfHost)
            {

                apiHttpConfiguration = new HttpSelfHostConfiguration("http://0.0.0.0:80") { ClientCredentialType = HttpClientCredentialType.None};
                apiHttpConfiguration.DependencyResolver = new UnityDependencyResolver(container);
                apiHttpConfiguration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                WebApiConfig.Register(apiHttpConfiguration);
                server = new HttpSelfHostServer((HttpSelfHostConfiguration)apiHttpConfiguration);
                server.OpenAsync().Wait();
            }
            else
            {
                apiHttpConfiguration = GlobalConfiguration.Configuration;
                apiHttpConfiguration.DependencyResolver = new UnityDependencyResolver(container);
                apiHttpConfiguration.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

                WebApiConfig.Register(apiHttpConfiguration);
                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
            }

        }

        private static string GetHash(string input)
        {
            byte[] data = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append((data[i]).ToString("x2"));
            }
            return sBuilder.ToString();
        }

        public static bool CheckPassword(string password)
        {
            return GetHash(password) == GlobalPassword;
        }
        
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Write("Unhandled Exception", LogLevel.Error, (Exception)e.ExceptionObject);
        }
        
        private static void LogCallback(string message, LogLevel level, Exception ex)
        {
            try
            {
                if (!Directory.Exists((new FileInfo(LogPath)).DirectoryName))
                {
                    Directory.CreateDirectory((new FileInfo(LogPath)).DirectoryName);
                }
                File.AppendAllText(LogPath, "\r\n" + String.Format("{0} [{1}] {2}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"), level, message));
                File.AppendAllText(LogPath, GetExceptionText(ex));
            }
            catch
            {
            }
        }
        
        private static string GetExceptionText(Exception ex, int count = 0)
        {
            StringBuilder builder = new StringBuilder();
            if (ex != null)
            {
                var lineHead = "".PadLeft(count, '\t');
                builder.Append("\r\n" + lineHead + "== Exception");
                builder.Append("\r\n" + lineHead + "==== Message:\t" + ex.Message);
                builder.Append("\r\n" + lineHead + "==== Trace:\t" + ex.StackTrace.Replace("\n", "\n" + lineHead));
                if (ex.InnerException != null)
                {
                    builder.Append(GetExceptionText(ex.InnerException, count + 1));
                }
            }
            return builder.ToString();
        }
    }
}