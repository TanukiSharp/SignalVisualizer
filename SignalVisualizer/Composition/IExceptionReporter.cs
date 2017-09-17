using System;

namespace SignalVisualizer.Composition
{
    public interface IExceptionReporter
    {
        void Report(Exception error);
    }
}
