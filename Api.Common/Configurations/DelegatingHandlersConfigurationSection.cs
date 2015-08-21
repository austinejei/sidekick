using System.Configuration;

namespace Api.Common.Configurations
{
    public class DelegatingHandlersConfigurationSection : ConfigurationSection
    {
       
        [ConfigurationProperty("modules", IsRequired = true)]
        public ProviderSettingsCollection DelegatingHandlers
        {
            get { return (ProviderSettingsCollection)base["modules"]; }
            set { base["modules"] = value; }
        }


    }
}