using System;

namespace SignalVisualizer.ViewModels
{
    public class ExceptionViewModel
    {
        private Exception exception;

        public ExceptionViewModel(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            exception = ex;
        }

        public string TypeName
        {
            get { return exception.GetType().FullName; }
        }

        public string Message
        {
            get { return exception.Message; }
        }

        public string StackTrace
        {
            get { return exception.StackTrace; }
        }
    }
}
