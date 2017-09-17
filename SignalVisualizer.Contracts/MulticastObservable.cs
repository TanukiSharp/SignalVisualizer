using System;
using System.Collections.Generic;

namespace SignalVisualizer.Contracts
{
    /// <summary>
    /// A provider that can dispatch data to serveral observers.
    /// This component tries to enforce as many observer-pattern rules as possible.
    /// </summary>
    /// <typeparam name="T">The type of transmitted data.</typeparam>
    public class MulticastObservable<T> : IObservable<T>
    {
        private readonly object syncRoot = new object();
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();
        private readonly List<IObserver<T>> unsubscriptionList = new List<IObserver<T>>();
        private bool isDone;

        /// <summary>
        /// Subscribes an observer that will receive notifications as they are pushed to this provider.
        /// </summary>
        /// <param name="observer">The observer to subscribe.</param>
        /// <returns>Returns a 'subscription' instance. Dispose it to unsubscribe.
        /// See <see cref="RequestUnsubscription"/> for more information about unsubscription.</returns>
        /// <seealso cref="RequestUnsubscription"/>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (syncRoot)
            {
                if (isDone)
                    return NullDisposer.Shared;

                OnSubscribed(observer);

                observers.Add(observer);

                return new Disposer(observer, this);
            }
        }

        /// <summary>
        /// Requests to unsubscribe an observer.
        /// Unsubscription is not immediate and may happen at a later time.
        /// It is however guaranted that the observer will never receive any event (Next, Completed or Error) anymore.
        /// </summary>
        /// <param name="observer">The observer to request the unsubscription for.</param>
        public void RequestUnsubscription(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            lock (unsubscriptionList)
            {
                if (isDone == false)
                    unsubscriptionList.Add(observer);
            }
        }

        private void ProcessUnsubscriptions()
        {
            lock (unsubscriptionList)
            {
                foreach (IObserver<T> observer in unsubscriptionList)
                    observers.Remove(observer);
                unsubscriptionList.Clear();
            }
        }

        /// <summary>
        /// Called when an observer is subscribed.
        /// </summary>
        /// <param name="observer">The newly subscribed observer.</param>
        protected virtual void OnSubscribed(IObserver<T> observer)
        {
            Subscribed?.Invoke(this, new ObserverEventArgs<T>(observer));
        }

        /// <summary>
        /// Raised when an observer is subscribed.
        /// </summary>
        public event EventHandler<ObserverEventArgs<T>> Subscribed;

        /// <summary>
        /// Perform an action on all subscribed observers.
        /// </summary>
        /// <param name="action">The action to perform on all subscribed observers.</param>
        protected void ForEachObserver(Action<IObserver<T>> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            lock (syncRoot)
            {
                ProcessUnsubscriptions();

                foreach (IObserver<T> observer in observers)
                    action(observer);
            }
        }

        /// <summary>
        /// Notifies all subscribed observers that data has been received.
        /// </summary>
        /// <param name="value">The received value to pass to all subscribed observers.</param>
        protected void NotifyNext(T value)
        {
            lock (syncRoot)
            {
                if (isDone)
                    return;

                ProcessUnsubscriptions();

                foreach (IObserver<T> observer in observers)
                    observer.OnNext(value);
            }
        }

        /// <summary>
        /// Notifies all subscribed observers that that data stream has stopped.
        /// </summary>
        protected void NotifyCompleted()
        {
            lock (syncRoot)
            {
                if (isDone)
                    return;

                ProcessUnsubscriptions();

                foreach (IObserver<T> observer in observers)
                    observer.OnCompleted();

                observers.Clear();
                isDone = true;
            }
        }

        /// <summary>
        /// Notifies all subscribed observers that an error occured.
        /// </summary>
        /// <param name="error">The error that occured.</param>
        protected void NotifyError(Exception error)
        {
            lock (syncRoot)
            {
                if (isDone)
                    return;

                ProcessUnsubscriptions();

                foreach (IObserver<T> observer in observers)
                    observer.OnError(error);

                observers.Clear();
                isDone = true;
            }
        }

        private class Disposer : IDisposable
        {
            private readonly IObserver<T> observer;
            private readonly MulticastObservable<T> observable;
            private readonly object syncRoot = new object();
            private bool isDisposed;

            public Disposer(IObserver<T> observer, MulticastObservable<T> observable)
            {
                if (observer == null)
                    throw new ArgumentNullException(nameof(observer));
                if (observable == null)
                    throw new ArgumentNullException(nameof(observable));

                this.observer = observer;
                this.observable = observable;
            }

            public void Dispose()
            {
                if (isDisposed)
                    return;

                lock (syncRoot)
                {
                    if (isDisposed)
                        return;

                    isDisposed = true;
                }

                observable.RequestUnsubscription(observer);
            }
        }

        private class NullDisposer : IDisposable
        {
            public static readonly IDisposable Shared = new NullDisposer();

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// An observer related event argument.
    /// </summary>
    /// <typeparam name="T">The type of data received by the observer.</typeparam>
    public class ObserverEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the observer.
        /// </summary>
        public IObserver<T> Observer { get; }

        /// <summary>
        /// Initializes the <see cref="ObserverEventArgs{T}"/> instance.
        /// </summary>
        /// <param name="observer">The observer related to this event.</param>
        public ObserverEventArgs(IObserver<T> observer)
        {
            if (observer == null)
                throw new ArgumentNullException(nameof(observer));

            Observer = observer;
        }
    }
}
