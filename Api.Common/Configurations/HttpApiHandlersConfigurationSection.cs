using System.Configuration;

namespace Api.Common.Configurations
{
    public class HttpApiHandlersConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("handlers", IsRequired = true,IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(HttpApiHandlersConfigCollection),
        AddItemName = "add",
        ClearItemsName = "clear",
        RemoveItemName = "remove")]
        public HttpApiHandlersConfigCollection HttpHandlerCollection
        {
            get { return (HttpApiHandlersConfigCollection)base["handlers"]; }
         
        }
    }
}