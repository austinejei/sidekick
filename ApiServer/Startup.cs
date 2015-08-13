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
            OwinHttpListener listener =
                (OwinHttpListener) app.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"];

            int maxAccepts, maxRequests;
            listener.GetRequestProcessingLimits(out maxAccepts, out maxRequests);


            listener.SetRequestQueueLimit(int.Parse(ConfigurationManager.AppSettings["Owin.RequestQueueLimit"]));

            listener.SetRequestProcessingLimits(int.Parse(ConfigurationManager.AppSettings["Owin.MaxAccepts"]),
                int.Parse(ConfigurationManager.AppSettings["Owin.MaxRequests"]));


            app.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"] = listener;




            AttachAuthenticationModules(app);

            var config = new HttpConfiguration();

            AttachDelegatingHandlers(config);
          
            config.Services.Replace(typeof (IAssembliesResolver), new CustomAssemblyResolver());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new {id = RouteParameter.Optional}
                );

            config.Formatters.XmlFormatter.UseXmlSerializer = true;

            config.MapHttpAttributeRoutes();

            config.EnsureInitialized();

            app.UseCors(CorsOptions.AllowAll);

            CashBaggEventsManager.Start();
            
            app.MapSignalR();

            EventhandlerModules = new List<IEventhandlerModule>();

            LoadEventHandlers();


            app.UseWebApi(config);



        }


        private void LoadEventHandlers()
        {
            Logger.Debug("loading event handler modules...");
            var config = ConfigurationManager.GetSection("sidekickventHandlers") as EventHandlersConfigurationSection;

            if (config != null)
            {
                Logger.Info("Loaded {0} event handler modules", config.Modules.Count);
                foreach (ProviderSettings moduleElement in config.Modules)
                {
                    if (moduleElement.Type != null)
                    {
                        IEventhandlerModule eventhandlerModule =
                            Activator.CreateInstance(Type.GetType(moduleElement.Type)) as IEventhandlerModule;
                        if (eventhandlerModule != null)
                        {
                            eventhandlerModule.Initialize(CashBaggEventsManager.Instance.Events);
                            
                            EventhandlerModules.Add(eventhandlerModule);
                            Logger.Debug("{0} is initialized.", moduleElement.Name);
                        }

                    }

                }
            }
            else
            {
                Logger.Warn("no event handler modules have been configured!!");
            }

        }


        private void AttachAuthenticationModules(IAppBuilder app)
        {
            var config = ConfigurationManager.GetSection("authenticationMiddleware") as AuthenticationModulesConfigurationSection;

            if (config != null)
            {
                Logger.Info("Loaded {0} authentication modules", config.Modules.Count);
                foreach (ProviderSettings moduleElement in config.Modules)
                {
                    if (moduleElement.Type != null)
                    {
                        JwtBearerAuthenticationOptions jwtBearerAuthenticationOptions =null;

                        try
                        {
                            jwtBearerAuthenticationOptions =
                                Activator.CreateInstance(Type.GetType(moduleElement.Type)) as JwtBearerAuthenticationOptions;

                        }
                        catch (Exception exception)
                        {
                            
                        }

                        if (jwtBearerAuthenticationOptions!=null)
                        {
                             app.UseJwtBearerAuthentication(jwtBearerAuthenticationOptions);
                        }
                        else
                        {
                             app.Use(Type.GetType(moduleElement.Type));
                        }

                       
                        Logger.Debug("added {0}", moduleElement.Name);
                    }

                }
            }
            else
            {
                throw new Exception("no authentication modules found");
            }
        }

        private void AttachDelegatingHandlers(HttpConfiguration httpConfiguration)
        {
            var config = ConfigurationManager.GetSection("authenticationMiddleware") as AuthenticationModulesConfigurationSection;

            if (config != null)
            {
                Logger.Info("Loaded {0} request throttling handlers", config.DelegatingHandlers.Count);
                foreach (ProviderSettings element in config.DelegatingHandlers)
                {
                    if (element.Type != null)
                    {
                        DelegatingHandler delegatingHandler =
                            Activator.CreateInstance(Type.GetType(element.Type)) as DelegatingHandler;

                        
                        httpConfiguration.MessageHandlers.Add(delegatingHandler);
                        Logger.Debug("added {0}", element.Name);
                    }

                }
            }
            else
            {
                Logger.Warn("DelegatingHandlers handler not found");
            }
        }

    }

 

  

    
}