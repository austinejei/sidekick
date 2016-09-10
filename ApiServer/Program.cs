using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Api.Common;
using Microsoft.Owin.Hosting;
using NLog;

namespace ApiServer
{
    class Program
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static IDisposable _apiListener;
        static readonly string[] ExitCommands = { "exit", "shutdown", "end", "quit","bye","reboot","clear" };
        private static string _userExitCode;
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
            {
                Log.Fatal("UnhandledException: {0}", e.ExceptionObject.ToString());
            };


            var title = ConfigurationManager.AppSettings["AppName"];

            string consoleArg =
               args.Count(x => x.Equals("-console", StringComparison.OrdinalIgnoreCase)) == 0
                   ? ""
                   : args.Single(x => x.Equals("-console", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(consoleArg))
            {
                var servicesToRun = new ServiceBase[] {
                                                          new SidekickWindowsService(), 
                                                      };
                ServiceBase.Run(servicesToRun);
            }
            else
            {
                Console.Title = title;
                Console.WriteLine("{0}", ConfigurationManager.AppSettings["AppName"]);

                StringBuilder liner = new StringBuilder();
                title.ToCharArray().ToList().ForEach((s) =>
                {
                    liner.Append("-");
                });

                Console.WriteLine(liner);


                StartHttpServer();
                

                ListenForExit();

                
               
            }

            
        }

        private static void ListenForExit()
        {
            _userExitCode = Console.ReadLine();
            while (!ExitCommands.Contains(_userExitCode))
            {
                Console.WriteLine("command not recognised!");
                _userExitCode = Console.ReadLine();
            }

            if ("reboot".Equals(_userExitCode))
            {
                _userExitCode = string.Empty;
                RebootHttpService();
            }
            else if ("clear".Equals(_userExitCode))
            {
                Console.Clear();
                _userExitCode = string.Empty;
                ListenForExit();
            }
            else
            {
                Log.Info("Shutting down http listener :(");
                ShutdownHttpServer();
            }
        }

        private static void RebootHttpService()
        {
            Log.Info("received request to reboot HTTP service");
            
            ShutdownHttpServer();

            Log.Debug("refreshing .config file...."); //simple hack
            ConfigurationManager.RefreshSection("appSettings");
            ConfigurationManager.RefreshSection("configSections");
            ConfigurationManager.RefreshSection("connectionStrings");
            ConfigurationManager.RefreshSection("startup");
            ConfigurationManager.RefreshSection("system.net");
            ConfigurationManager.RefreshSection("authenticationMiddleware");
            ConfigurationManager.RefreshSection("sidekickDelegatingHandlers");
            ConfigurationManager.RefreshSection("sidekickEventHandlers");
            ConfigurationManager.RefreshSection("httpApiHandlersSection");
            ConfigurationManager.RefreshSection("mediaTypeFormatters");
            ConfigurationManager.RefreshSection("runtime");
            ConfigurationManager.RefreshSection("entityFramework");
            Log.Debug("done refreshing .config file!");

            StartHttpServer();
            Log.Info("reboot completed!!");
            ListenForExit();

        }

        private static void StartHttpServer()
        {
              var serverUrl = ConfigurationManager.AppSettings["Core.ApiEndpoint"];
            try
            {
                _apiListener = WebApp.Start<Startup>(serverUrl);

              
                Log.Info("HTTP Listener started at {0}", serverUrl);
                Log.Debug("Waiting for requests...");
            }
            catch (Exception exception)
            {
                Log.Fatal("Could not start HTTP Service: {0}",exception.Message);

            }
           
        }

        private static void ShutdownHttpServer()
        {
            try
            {
                if (_apiListener==null)
                {
                    Log.Warn("cannot shutdown a service that does not exist");
                    return;
                    
                }

                //Startup.EventhandlerModules.ForEach(e => e.Release(SidekickEventsManager.Instance.Events));
                _apiListener.Dispose();
                Log.Info("HTTP listener has been successfully shut down :|");
            }
            catch (Exception exception)
            {
               Log.Fatal(exception.Message);
            }
        }

    }
}
