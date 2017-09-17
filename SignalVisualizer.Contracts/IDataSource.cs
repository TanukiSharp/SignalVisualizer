using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalVisualizer.Contracts
{
    /// <summary>
    /// Represents a source that produces a multi-components signal.
    /// </summary>
    public interface IDataSource : IObservable<double[]>, IConfigurable
    {
        /// <summary>
        /// Gets the name of the signal data source.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the version of the signal data source.
        /// </summary>
        uint Version { get; }

        /// <summary>
        /// Gets the unique identifier of the data source.
        /// This value must be constant.
        /// </summary>
        Guid UniqueIdentifier { get; }

        /// <summary>
        /// Gets the name of each component of the signal data source.
        /// </summary>
        string[] ComponentNames { get; }

        /// <summary>
        /// Starts the data collection.
        /// Cancels the provided <paramref name="cancellationToken"/> to stop the data collection.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that defines the lifetime of the data collection.</param>
        /// <returns>Returns an awaitable task that will complete when the <paramref name="cancellationToken"/> will be cancelled.</returns>
        Task Start(CancellationToken cancellationToken);
    }
}
