using System;

namespace SignalVisualizer
{
    public class AnonymousObserver<T> : IObserver<T>, IDisposable
    {
        private Action<T> onNext;
        private Action onCompleted;
        private Action<Exception> onError;

        private bool isDisposed;

        public AnonymousObserver(Action<T> onNext)
            : this(onNext, null, null)
        {
        }

        public AnonymousObserver(Action<T> onNext, Action onCompleted)
            : this(onNext, onCompleted, null)
        {
        }

        public AnonymousObserver(Action<T> onNext, Action<Exception> onError)
            : this(onNext, null, onError)
        {
        }

        public AnonymousObserver(Action<T> onNext, Action onCompleted, Action<Exception> onError)
        {
            if (onNext == null)
                throw new ArgumentNullException(nameof(onNext));

            this.onNext = onNext;
            this.onCompleted = onCompleted;
            this.onError = onError;
        }

        public void OnNext(T value)
        {
            onNext(value);
        }

        public void OnCompleted()
        {
            onCompleted?.Invoke();
        }

        public void OnError(Exception error)
        {
            onError?.Invoke(error);
        }

        public void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;

            onNext = null;
            onCompleted = null;
            onError = null;
        }
    }
}
