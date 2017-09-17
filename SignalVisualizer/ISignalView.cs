using System;
using SignalVisualizer.Contracts;

namespace SignalVisualizer
{
    /// <summary>
    /// Represents the view of a single component of a multi-components data source.
    /// </summary>
    public interface ISignalView : IObserver<double[]>, IObservable<double>
    {
        /// <summary>
        /// Gets the name of the component.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the index of the component in its data source.
        /// </summary>
        int ComponentIndex { get; }
    }

    public class SignalView : MulticastObservable<double>, ISignalView
    {
        public virtual string Name { get; }

        public virtual int ComponentIndex { get; }

        public SignalView(string name, int index)
        {
            Name = name;
            ComponentIndex = index;
        }

        public virtual void OnNext(double[] value)
        {
            NotifyNext(value[ComponentIndex]);
        }

        public virtual void OnCompleted()
        {
            NotifyCompleted();
        }

        public virtual void OnError(Exception error)
        {
            NotifyError(error);
        }
    }
}
