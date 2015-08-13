using System.Configuration;

namespace Api.Common.Configurations
{
    public class HttpApiHandlersConfig : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)this["name"];
            }
            set
            { this["name"] = value; }
        }

        [ConfigurationProperty("assemblyLocation", IsRequired = true)]
        public string AssemblyLocation
        {
            get
            {
                return (string)this["assemblyLocation"];
            }
            set
            { this["assemblyLocation"] = value; }
        }

        [ConfigurationProperty("assemblyIsLocal", IsRequired = true)]
        public bool AssemblyIsLocal
        {
            get
            {
                return (bool)this["assemblyIsLocal"];
            }
            set
            { this["assemblyIsLocal"] = value; }
        }
    }
}