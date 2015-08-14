using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using NLog;

namespace ApiServer
{
    class Program
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static IDisposable _cashBaggApiListener;
        static readonly string[] exitCommands = { "exit", "shutdown", "end", "quit" };

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
                                                          {
                                                              Log.Fatal("UnhandledException: {0}", e.ExceptionObject.ToString());
                                                          };

       

            Console.Title = ConfigurationManager.AppSettings["AppName"];
            Console.WriteLine("{0}", ConfigurationManager.AppSettings["AppName"]);
            Console.WriteLine("-------------------------");


           Task.Factory.StartNew(StartHttpServer);

            var userExitCode = Console.ReadLine();
           while (!exitCommands.Contains(userExitCode))
           {
               Console.WriteLine("command not recognised!");
               userExitCode = Console.ReadLine();
           }

            
           Log.Info("Shutting down http listener :( ...");
            ShutdownHttpServer();
        }

        private static void StartHttpServer()
        {
              var serverUrl = ConfigurationManager.AppSettings["Core.ApiEndpoint"];
            try
            {
                _cashBaggApiListener = WebApp.Start<Startup>(serverUrl);

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
                if (_cashBaggApiListener==null)
                {
                    Log.Warn("cannot shutdown a service that does not exist");
                    return;
                    
                }

                //Startup.EventhandlerModules.ForEach(e => e.Release(SidekickEventsManager.Instance.Events));
                _cashBaggApiListener.Dispose();
                Log.Info("HTTP listener has been successfully shut down :|");
            }
            catch (Exception exception)
            {
               Log.Fatal(exception.Message);
            }
        }

    }
}
