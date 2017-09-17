using System;
using System.Xml;

namespace SignalVisualizer.Contracts
{
    /// <summary>
    /// Represents an object that can produce filters.
    /// </summary>
    public interface ISignalFilterFactory
    {
        /// <summary>
        /// Gets the name of the produced filters.
        /// </summary>
        string FilterName { get; }

        /// <summary>
        /// Gets the unique identifier of signal filter factory.
        /// This value must be constant.
        /// </summary>
        Guid UniqueIdentifier { get; }

        /// <summary>
        /// Produces a signal filter.
        /// </summary>
        /// <returns>Returns a newly created signal filter.</returns>
        ISignalFilter ProduceSignalFilter();
    }

    /// <summary>
    /// Represents a filter that can process an signal in order to transform it.
    /// </summary>
    public interface ISignalFilter : IConfigurable
    {
        /// <summary>
        /// Gets the name of the signal filter.
        /// This is usually the same as the <see cref="ISignalFilterFactory.FilterName"/> property of the signal filter factory that produced this signal filter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the signal filter factory that produced this signal filter.
        /// </summary>
        ISignalFilterFactory SignalFilterFactory { get; }

        /// <summary>
        /// Processes the input signal value, and produces a new ouptut signal value.
        /// </summary>
        /// <param name="time">An absolute elapsed time, in milliseconds.</param>
        /// <param name="value">The signal instant value at the given time.</param>
        /// <returns>Returns a new signal instant value.</returns>
        double ProcessValue(double time, double value);
    }

    /// <summary>
    /// Represents a base implementation for a signal filter.
    /// </summary>
    public abstract class SignalFilterBase : ISignalFilter
    {
        /// <summary>
        /// Gets the value of the <see cref="ISignalFilterFactory.FilterName"/> property of the signal filter factory passed at the constructor.
        /// </summary>
        public virtual string Name => signalFilterFactory.FilterName;

        /// <summary>
        /// Gets the signal filter factory passed at the constructor.
        /// </summary>
        public ISignalFilterFactory SignalFilterFactory => signalFilterFactory;

        private ISignalFilterFactory signalFilterFactory;

        /// <summary>
        /// Initializes the <see cref="SignalFilterBase"/> instance.
        /// </summary>
        /// <param name="signalFilterFactory">The signal filter factory that produced this signal filter.</param>
        public SignalFilterBase(ISignalFilterFactory signalFilterFactory)
        {
            if (signalFilterFactory == null)
                throw new ArgumentNullException(nameof(signalFilterFactory));

            this.signalFilterFactory = signalFilterFactory;
        }

        /// <summary>
        /// Processes the input signal value, and produces a new ouptut signal value.
        /// </summary>
        /// <param name="time">An absolute elapsed time, in milliseconds.</param>
        /// <param name="value">The signal instant value at the given time.</param>
        /// <returns>Returns a new signal instant value.</returns>
        public abstract double ProcessValue(double time, double value);

        /// <summary>
        /// Raised when the configuration of the signal filter changed.
        /// </summary>
        public virtual event EventHandler ConfigurationChanged = delegate { };

        /// <summary>
        /// Configures the signal filter.
        /// </summary>
        public virtual void Configure() { }

        /// <summary>
        /// Loads the signal filter configuration from a previously stored data.
        /// </summary>
        /// <param name="element">The XML element that contains the data to restore.</param>
        public virtual void Load(XmlElement element) { }

        /// <summary>
        /// Saves the signal filter configuration to an XML storage.
        /// </summary>
        /// <param name="writer">The XML writer where to write the signal filter configuration.</param>
        public virtual void Save(XmlWriter writer) { }
    }
}
