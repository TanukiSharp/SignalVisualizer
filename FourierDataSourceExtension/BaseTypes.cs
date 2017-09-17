using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace FourierDataSourceExtension
{
    public abstract class ConfigurationViewModelBase
    {
        private Action configChanged;

        protected ConfigurationViewModelBase(Action configChanged)
        {
            this.configChanged = configChanged;
        }

        protected bool SetValue<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value) == false)
            {
                field = value;
                configChanged();
                return true;
            }

            return false;
        }
    }

    public class AnonymousCommand : ICommand
    {
        private Action<object> action;

        public event EventHandler CanExecuteChanged = delegate { };

        public AnonymousCommand(Action<object> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            this.action = action;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            action(parameter);
        }
    }
}
