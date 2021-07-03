using System;

namespace Microsoft.AspNet.Hosting
{
    /// <summary>
    /// Used to mark which class in an assembly should be the startup type to be used by the web host.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class WebHostStartupAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebHostStartupAttribute"/> class
        /// </summary>
        /// <param name="startupType">The type of the startup class.</param>
        public WebHostStartupAttribute(Type startupType)
        {
            StartupType = startupType;
        }

        /// <summary>
        /// Gets the type of the startup class.
        /// </summary>
        /// <value>The type of the startup class.</value>
        public Type StartupType { get; }
    }
}
