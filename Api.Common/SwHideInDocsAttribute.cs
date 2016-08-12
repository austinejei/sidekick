using System;

namespace Api.Common
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SwHideInDocsAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SwBuildOrUpdateAttribute : Attribute
    {
    }
}