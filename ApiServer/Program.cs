using System;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using NLog;

namespace ApiServer
{
    class Program
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static IDisposable _apiListener;
        static readonly string[] ExitCommands = { "exit", "shutdown", "end", "quit","bye" };

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (o, e) =>
                                                          {
                                                              Log.Fatal("UnhandledException: {0}", e.ExceptionObject.ToString());
                                                          };


            var title = ConfigurationManager.AppSettings["AppName"];
            Console.Title = title;
            Console.WriteLine("{0}", ConfigurationManager.AppSettings["AppName"]);

            StringBuilder liner = new StringBuilder();
            title.ToCharArray().ToList().ForEach((s) =>
                                                 {
                                                     liner.Append("-");
                                                 });

            Console.WriteLine(liner);


           Task.Factory.StartNew(StartHttpServer);

            var userExitCode = Console.ReadLine();
           while (!ExitCommands.Contains(userExitCode))
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
