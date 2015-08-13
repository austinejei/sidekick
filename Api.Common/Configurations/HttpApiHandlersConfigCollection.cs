using System.Configuration;

namespace Api.Common.Configurations
{
    public class HttpApiHandlersConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new HttpApiHandlersConfig();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {

            return ((HttpApiHandlersConfig)element).Name;
        }
    }
}