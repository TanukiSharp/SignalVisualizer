using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace SignalVisualizer.Contracts
{
    /// <summary>
    /// Represents a base implementation for a signal data source.
    /// </summary>
    public abstract class DataSourceBase : MulticastObservable<double[]>, IDataSource
    {
        /// <summary>
        /// Gets the name of the signal data source.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the version of the signal data source.
        /// </summary>
        public abstract uint Version { get; }

        /// <summary>
        /// Gets the unique identifier of the signal data source.
        /// This value must be constant.
        /// </summary>
        public abstract Guid UniqueIdentifier { get; }

        /// <summary>
        /// Gets the name of each component of the signal data source.
        /// </summary>
        public abstract string[] ComponentNames { get; }

        /// <summary>
        /// Starts the data collection.
        /// Cancels the provided <paramref name="cancellationToken"/> to stop the data collection.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that defines the lifetime of the data collection.</param>
        /// <returns>Returns an awaitable task that will complete when the <paramref name="cancellationToken"/> will be cancelled.</returns>
        public abstract Task Start(CancellationToken cancellationToken);

        /// <summary>
        /// Raised when the configuration is internally changed.
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Raises the <see cref="ConfigurationChanged"/> event.
        /// </summary>
        /// <param name="e">Custom argument.</param>
        protected void OnConfigurationChanged(EventArgs e)
        {
            ConfigurationChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Configures the object.
        /// </summary>
        public virtual void Configure()
        {
        }

        /// <summary>
        /// Loads the object configuration from a XML storage.
        /// </summary>
        /// <param name="element">The XML element that contains the configuration data to load.</param>
        public virtual void Load(XmlElement element)
        {
        }

        /// <summary>
        /// Stores the object configuration to a XML storage.
        /// </summary>
        /// <param name="writer">The XML writer to write the configuration data to.</param>
        public virtual void Save(XmlWriter writer)
        {
        }
    }
}
