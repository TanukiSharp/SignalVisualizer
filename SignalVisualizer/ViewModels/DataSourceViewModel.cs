using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public class DataSourceViewModel : RootedViewModel, IDisposable
    {
        public IDataSource DataSource { get; }

        public DataSourceViewModel(RootViewModel root, IDataSource dataSource)
            : base(root)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            DataSource = dataSource;
            DataSource.ConfigurationChanged += DataSource_ConfigurationChanged;
        }

        public void Dispose()
        {
            Stop();

            DataSource.ConfigurationChanged -= DataSource_ConfigurationChanged;
        }

        private void DataSource_ConfigurationChanged(object sender, EventArgs e)
        {
            Root.IsDataModified = true;
        }

        public string Name
        {
            get { return DataSource.Name; }
        }

        public uint Version
        {
            get { return DataSource.Version; }
        }

        private ICommand startCommand;
        public ICommand StartCommand
        {
            get
            {
                if (startCommand == null)
                    startCommand = new AnonymousCommand(Start);
                return startCommand;
            }
        }

        private ICommand stopCommand;
        public ICommand StopCommand
        {
            get
            {
                if (stopCommand == null)
                    stopCommand = new AnonymousCommand(Stop);
                return stopCommand;
            }
        }

        private ICommand configureCommand;
        public ICommand ConfigureCommand
        {
            get
            {
                if (configureCommand == null)
                    configureCommand = new AnonymousCommand(Configure);
                return configureCommand;
            }
        }

        private bool isRunning;
        public bool IsRunning
        {
            get { return isRunning; }
            set { SetValue(ref isRunning, value); }
        }

        private Exception error;
        public Exception Error
        {
            get { return error; }
            set
            {
                if (SetValue(ref error, value))
                    IsError = Error != null;
            }
        }

        private bool isError;
        public bool IsError
        {
            get { return isError; }
            private set { SetValue(ref isError, value); }
        }

        private CancellationTokenSource cancellationTokenSource;

        public async void Start()
        {
            if (IsRunning)
                return;

            cancellationTokenSource = new CancellationTokenSource();

            Error = null;
            IsRunning = true;

            bool isDataModified = Root.IsDataModified;
            Root.IsDataModified = true;

            try
            {
                await DataSource.Start(cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Root.IsDataModified = isDataModified;
                Error = ex;
            }

            Root.IsDataModified = true;

            IsRunning = false;
        }

        public void Stop()
        {
            if (IsRunning == false)
                return;

            cancellationTokenSource.Cancel();
        }

        public void Configure()
        {
            DataSource.Configure();
        }
    }
}
