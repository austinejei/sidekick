using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using NLog;

namespace ApiServer
{
    partial class SidekickWindowsService : ServiceBase
    {

        private static IDisposable _apiListener;
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();
        public SidekickWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            StartHttpServer();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
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
                Log.Fatal("Could not start HTTP Service: {0}", exception.Message);

            }

        }

        private static void ShutdownHttpServer()
        {
            try
            {
                if (_apiListener == null)
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
