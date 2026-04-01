using System;

namespace Romarr.Common.Messaging
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class LifecycleEventAttribute : Attribute
    {
    }
}
