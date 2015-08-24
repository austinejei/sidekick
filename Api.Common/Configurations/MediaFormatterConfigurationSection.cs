using System.Configuration;

namespace Api.Common.Configurations
{
    /// <summary>
    /// Helps add media formatter to the HttpConfiguration at startup
    /// </summary>
    public class MediaFormatterConfigurationSection : ConfigurationSection
    {
        /// <summary>
        /// List of media formatters
        /// </summary>
        [ConfigurationProperty("formatters", IsRequired = true)]
        public ProviderSettingsCollection Formatters
        {
            get { return (ProviderSettingsCollection) base["formatters"]; }
            set { base["formatters"] = value; }
        }

    }
}