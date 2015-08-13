using System.Configuration;

namespace Api.Common.Configurations
{
    public class EventHandlersConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("modules", IsRequired = true)]
        public ProviderSettingsCollection Modules
        {
            get { return (ProviderSettingsCollection)base["modules"]; }
            set { base["modules"] = value; }
        }
    }
}