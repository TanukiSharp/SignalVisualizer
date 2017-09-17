using System;
using System.Windows.Input;

namespace SignalVisualizer
{
    public class AnonymousCommand : AnonymousCommand<object>
    {
        public AnonymousCommand(Action action)
            : base(_ => action())
        {
        }
    }

    public class AnonymousCommand<T> : ICommand
    {
        private Action<T> action;

        public AnonymousCommand(Action<T> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            this.action = action;
        }

        private bool isEnabled = true;
        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled == value)
                    return;

                isEnabled = value;

                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return isEnabled;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter is T)
                action((T)parameter);
            else
                action(default(T));
        }
    }
}
