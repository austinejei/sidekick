using System.Configuration;

namespace Api.Common.Configurations
{
    public class AuthenticationModulesConfigurationSection: ConfigurationSection
    {
        [ConfigurationProperty("modules", IsRequired = true)]
        public ProviderSettingsCollection Modules
        {
            get { return (ProviderSettingsCollection)base["modules"]; }
            set { base["modules"] = value; }
        }

        [ConfigurationProperty("delegatingHandlers", IsRequired = true)]
        public ProviderSettingsCollection DelegatingHandlers
        {
            get { return (ProviderSettingsCollection)base["delegatingHandlers"]; }
            set { base["delegatingHandlers"] = value; }
        }

        [ConfigurationProperty("oauth", IsRequired = true)]
        public ProviderSettingsCollection OAuth
        {
            get { return (ProviderSettingsCollection)base["oauth"]; }
            set { base["oauth"] = value; }
        }

        //[ConfigurationProperty("bearerAuth", IsRequired = true)]
        //public ProviderSettingsCollection BearerAuth
        //{
        //    get { return (ProviderSettingsCollection)base["bearerAuth"]; }
        //    set { base["bearerAuth"] = value; }
        //}
    }
}
