using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Api.Common.Configurations;

namespace Api.Common
{
    public class ApiHandlerHelper
    {
        public List<string> GatherRoutesToIgnore()
        {
            var controllersAssemblies = GatherExternalAssembly();

           
            var routesToIgnore = new List<string>();

            var freePassEndpoints = ConfigurationManager.AppSettings["FreePass.Endpoints"].Split(new[] {','},
                StringSplitOptions.RemoveEmptyEntries);


            routesToIgnore.AddRange(freePassEndpoints);

            foreach (var assembly in controllersAssemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.Name.Contains("Controller")
                        && type.IsSubclassOf(typeof(ApiController)))
                    {
                        var  apiController = (ApiController)assembly.CreateInstance(type.FullName);
                        
                        if (apiController!=null)
                        {
                            
                            var routePrefixAttribute =
                                type
                                    .GetCustomAttribute<RoutePrefixAttribute>(false);

                            var allowAnonymousAttribute =
                                type
                                    .GetCustomAttribute<AllowAnonymousAttribute>(false);

                          
                            
                            var controllerName = type.Name.Replace("Controller", string.Empty);
                            if (routePrefixAttribute != null)
                            {
                                controllerName = routePrefixAttribute.Prefix;
                            }

                            if (allowAnonymousAttribute != null)
                            {
                                //add to list
                                routesToIgnore.Add(string.Format("/{0}",controllerName)); //if controller has it..then all the methods are also by default
                                continue;
                            }


                            var methodsWithAllowAnonymousAttribute = type.GetMethods()
                                .Where(m => m.GetCustomAttributes(typeof (AllowAnonymousAttribute), false).Length>0)
                                ;

                            foreach (var methodInfo in methodsWithAllowAnonymousAttribute)
                            {
                                var routeAttribute  = methodInfo.GetCustomAttribute<RouteAttribute>();

                                if (routeAttribute!=null)
                                {
                                    routesToIgnore.Add(string.Format("/{0}/{1}",controllerName,routeAttribute.Template));
                                }
                                else
                                {
                                    routesToIgnore.Add(string.Format("/{0}/{1}", controllerName, methodInfo.Name));
                                }
                            }

                          

                        }
                      

                    }
                }
            }

            return routesToIgnore;
        }

        private List<Assembly> GatherExternalAssembly()
        {
            var assemblies = new List<Assembly>();

            var config = ConfigurationManager.GetSection("httpApiHandlersSection") as HttpApiHandlersConfigurationSection;

            if (config != null)
            {

                // Logger.Debug("found {0} HTTP API handlers", config.HttpHandlerCollection.Count);
                foreach (var moduleElement in config.HttpHandlerCollection)
                {
                    var httpHandlerConfig = (HttpApiHandlersConfig)moduleElement;
                    if (httpHandlerConfig != null)
                    {
                        if (!File.Exists(httpHandlerConfig.AssemblyLocation))
                        {
                            // Logger.Fatal("{0} does not exist!", httpHandlerConfig.AssemblyLocation);
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

                            //Logger.Info("added {0} from {1}", httpHandlerConfig.Name, httpHandlerConfig.AssemblyLocation);
                        }
                    }

                }
            }

            return assemblies.Distinct().ToList();
        }
    }
}