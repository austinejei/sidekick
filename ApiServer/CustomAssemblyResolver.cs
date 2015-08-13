using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http.Dispatcher;
using Api.Common.Configurations;
using NLog;

namespace ApiServer
{
    public class CustomAssemblyResolver : DefaultAssembliesResolver
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public override ICollection<Assembly> GetAssemblies()
        {
            var baseAssemblies = base.GetAssemblies().ToList();
            var assemblies = new List<Assembly>(baseAssemblies);

            var externalAssembly = GatherExternalAssembly();
 
            assemblies.AddRange(externalAssembly);
            return assemblies;
        }

        private List<Assembly> GatherExternalAssembly()
        {
            var assemblies = new List<Assembly>();

            var config = ConfigurationManager.GetSection("httpApiHandlersSection") as HttpApiHandlersConfigurationSection;

            if (config != null)
            {
    
                Logger.Debug("found {0} HTTP API handlers",config.HttpHandlerCollection.Count);
                foreach (var moduleElement in config.HttpHandlerCollection)
                {
                    var httpHandlerConfig = (HttpApiHandlersConfig) moduleElement;
                    if (httpHandlerConfig != null)
                    {
                        if (!File.Exists(httpHandlerConfig.AssemblyLocation))
                        {
                            Logger.Fatal("{0} does not exist!", httpHandlerConfig.AssemblyLocation);
                        }
                        else
                        {
                            if (httpHandlerConfig.AssemblyIsLocal)
                            {
                                assemblies.Add(
                                    Assembly.LoadFile(AppDomain.CurrentDomain.BaseDirectory +
                                                      httpHandlerConfig.AssemblyLocation));
                            }
                            else
                            {
                                assemblies.Add(Assembly.LoadFile(httpHandlerConfig.AssemblyLocation));
                            }

                            Logger.Info("added {0} HTTP handler from {1}",httpHandlerConfig.Name,httpHandlerConfig.AssemblyLocation);
                        }
                    }

                }
            }
            else
            {
                Logger.Fatal("no HTTP API handlers config section found. Please configure a section for handling HTTP requests...");
                //throw new Exception("no HTTP API handlers config section found. Please configure a section for handling HTTP requests...");
            }

            return assemblies.Distinct().ToList();
        }
    }
}