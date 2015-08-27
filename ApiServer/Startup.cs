using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Api.Common.Configurations;
using Api.Events;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Host.HttpListener;
using Microsoft.Owin.Security.Jwt;
using NLog;
using Owin;

namespace ApiServer
{
    public class Startup
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static List<IEventhandlerModule> EventhandlerModules;

        public void Configuration(IAppBuilder app)
        {
            if (app.Properties.ContainsKey("Microsoft.Owin.Host.HttpListener.OwinHttpListener"))
            {
                Logger.Info("configuring server environment");

                OwinHttpListener listener =
                (OwinHttpListener)app.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"];

                int maxAccepts, maxRequests;
                listener.GetRequestProcessingLimits(out maxAccepts, out maxRequests);


                listener.SetRequestQueueLimit(int.Parse(ConfigurationManager.AppSettings["Owin.RequestQueueLimit"]));

                listener.SetRequestProcessingLimits(int.Parse(ConfigurationManager.AppSettings["Owin.MaxAccepts"]),
                    int.Parse(ConfigurationManager.AppSettings["Owin.MaxRequests"]));


                app.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"] = listener;
            }
            else
            {
                Logger.Warn("key {0} not found. Will revert to default values.", "Microsoft.Owin.Host.HttpListener.OwinHttpListener");
            }



            app.AttachAuthenticationModules();


            var config = new HttpConfiguration();

            config.AttachDelegatingHandlers();
            config.AttachMediaFormatters();
          
            config.Services.Replace(typeof (IAssembliesResolver), new SidekickHttpAssemblyResolver());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );

            config.Formatters.XmlFormatter.UseXmlSerializer = true;

            config.MapHttpAttributeRoutes();

            config.EnsureInitialized();

            app.UseCors(CorsOptions.AllowAll);

            SidekickEventsManager.Start();
            
            app.MapSignalR();

            EventhandlerModules = new List<IEventhandlerModule>();

            LoadEventHandlers();


            app.UseWebApi(config);



        }


        private void LoadEventHandlers()
        {

            var config = ConfigurationManager.GetSection("sidekickEventHandlers") as EventHandlersConfigurationSection;

            if (config != null)
            {
                Logger.Info("Identified {0} event handler modules", config.Modules.Count);
                EventhandlerModules.Clear();
                Logger.Debug("loading event handler modules...");
                foreach (ProviderSettings moduleElement in config.Modules)
                {
                    if (moduleElement.Type != null)
                    {
                        IEventhandlerModule eventhandlerModule =
                            Activator.CreateInstance(Type.GetType(moduleElement.Type)) as IEventhandlerModule;
                        if (eventhandlerModule != null)
                        {
                            eventhandlerModule.Initialize(SidekickEventsManager.Instance.Events);
                            
                            EventhandlerModules.Add(eventhandlerModule);
                            Logger.Debug("{0} is loaded and initialized.", moduleElement.Name);
                        }

                    }

                }
            }
            else
            {
                Logger.Warn("no event handler modules have been configured!!");
            }

        }


    

    }

 

  

    
}