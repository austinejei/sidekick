using System;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Description;
using Api.Common;
using Api.Common.Configurations;
using Microsoft.Owin.Security.Jwt;
using NLog;
using Owin;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace ApiServer
{
    public static class SidekickWebStackExtensions
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static void AttachAuthenticationModules(this IAppBuilder app)
        {
            var config = ConfigurationManager.GetSection("authenticationMiddleware") as AuthenticationModulesConfigurationSection;

            if (config != null)
            {
                Logger.Info("Loaded {0} authentication modules", config.Modules.Count);
                foreach (ProviderSettings moduleElement in config.Modules)
                {
                    if (moduleElement.Type != null)
                    {
                        JwtBearerAuthenticationOptions jwtBearerAuthenticationOptions = null;

                        try
                        {
                            jwtBearerAuthenticationOptions =
                                Activator.CreateInstance(Type.GetType(moduleElement.Type)) as JwtBearerAuthenticationOptions;

                           
                        }
                        catch (Exception exception)
                        {

                        }

                        if (jwtBearerAuthenticationOptions != null)
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

        public static void AttachDelegatingHandlers(this HttpConfiguration httpConfiguration)
        {
            var config = ConfigurationManager.GetSection("sidekickDelegatingHandlers") as DelegatingHandlersConfigurationSection;

            if (config != null)
            {
                Logger.Info("Loaded {0} delegating handlers", config.DelegatingHandlers.Count);
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

        public static void AttachMediaFormatters(this HttpConfiguration configuration)
        {
            var config = ConfigurationManager.GetSection("mediaTypeFormatters") as MediaFormatterConfigurationSection;
            if (config != null)
            {
                Logger.Info("Loading {0} Media Type Formatters...", config.Formatters.Count);
                foreach (ProviderSettings formatter in config.Formatters)
                {
                    if (formatter.Type == null) continue;
                    MediaTypeFormatter mediaTypeFormatter = Activator.CreateInstance(Type.GetType(formatter.Type)) as MediaTypeFormatter;
                    configuration.Formatters.Add(mediaTypeFormatter);
                    Logger.Info("Module {0} added", formatter.Name);
                }
            }
            else Logger.Warn("No MediaFormatters found");
        }

        public static void AttachSwagger(this HttpConfiguration config)
        {

            var thisAssembly = typeof(SidekickWebStackExtensions).Assembly;
            config
                .EnableSwagger(c =>
                {
                    // c.BasicAuth("basic").Description("LUSSD Basic Authentication");
                    //  c.OperationFilter<AddAuthorizationHeaderParameterOperationFilter>();

                    c.RootUrl(r => ConfigurationManager.AppSettings["Core.ApiEndpoint"]);

                    c.IncludeXmlComments("UssdAppHandler.XML");
                    c.IncludeXmlComments("SmsShortcodeHandler.XML");
                    c.IncludeXmlComments("ApiHandlers.XML");
              

                    c.DocumentFilter<HideInDocsFilter>();
                    // c.OperationFilter<LussdSpecialParameterOperationFilter>();
                    c.IgnoreObsoleteActions();
                    c.SingleApiVersion("v1", "Sidekick API Documentation")
                        .Description("Sample description.")
                        .Contact(cc =>
                        {
                            cc.Url("https://github.com/austinejei/sidekick")
                                .Email("sidekick@sidekick.com")
                                .Name("Sidekick");
                        })
                        ;



                })
                .EnableSwaggerUi(s =>
                {
                    s.InjectJavaScript(thisAssembly, "ApiServer.CustomContent.sidekick-auth.js");
                })
                ;

        }

    }

    public class HideInDocsFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            foreach (var apiDescription in apiExplorer.ApiDescriptions)
            {
                if (
                    !apiDescription.ActionDescriptor.ControllerDescriptor.GetCustomAttributes<SwHideInDocsAttribute>()
                        .Any() && !apiDescription.ActionDescriptor.GetCustomAttributes<SwHideInDocsAttribute>().Any())
                {
                    continue;
                }
                var route = "/" + apiDescription.Route.RouteTemplate.TrimEnd('/');
                swaggerDoc.paths.Remove(route);


            }
        }
    }
}