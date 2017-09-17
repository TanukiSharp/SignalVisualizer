using System;
using System.Xml;

namespace SignalVisualizer.Contracts
{
    /// <summary>
    /// Represents a configurable object.
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// Configures the object.
        /// </summary>
        void Configure();

        /// <summary>
        /// Loads the object configuration from a XML storage.
        /// </summary>
        /// <param name="element">The XML element that contains the configuration data to load.</param>
        void Load(XmlElement element);

        /// <summary>
        /// Stores the object configuration to a XML storage.
        /// </summary>
        /// <param name="writer">The XML writer to write the configuration data to.</param>
        void Save(XmlWriter writer);

        /// <summary>
        /// Raised when the configuration is internally changed.
        /// </summary>
        event EventHandler ConfigurationChanged;
    }
}
