using System;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public abstract class SignalViewViewModelBase : RootedViewModel, IDisposable
    {
        public SignalViewContainerViewModel Parent { get; }

        public IDataSource DataSource { get; }
        public ISignalView SignalView { get; }

        private IDisposable sourceSubscription;
        private IDisposable viewSubscription;

        public static double ViewHeight { get { return 250.0; } }

        public string Name { get { return string.Format("{0} - {1}", DataSource.Name, SignalView.Name); } }

        public SignalViewViewModelBase(RootViewModel root, SignalViewContainerViewModel parent, IDataSource dataSource, ISignalView signalView)
            : base(root)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (signalView == null)
                throw new ArgumentNullException(nameof(signalView));

            Parent = parent;
            DataSource = dataSource;
            SignalView = signalView;

            viewSubscription = OnSignalViewSubscription(SignalView);

            sourceSubscription = dataSource.Subscribe(new AnonymousObserver<double[]>(OnDataSourceValue));
        }

        public void Dispose()
        {
            Cleanup();

            sourceSubscription = null;
            viewSubscription = null;
        }

        protected abstract IDisposable OnSignalViewSubscription(ISignalView signalView);

        private void OnDataSourceValue(double[] value)
        {
            try
            {
                SignalView.OnNext(value);
            }
            catch (Exception ex)
            {
                Error = ex;
                if (sourceSubscription != null)
                {
                    sourceSubscription.Dispose();
                    sourceSubscription = null;
                }
            }
        }

        public void Remove()
        {
            Cleanup();
        }

        internal void Cleanup()
        {
            if (sourceSubscription != null)
                sourceSubscription.Dispose();

            if (viewSubscription != null)
                viewSubscription.Dispose();
        }

        private bool isError;
        public bool IsError
        {
            get { return isError; }
            private set { SetValue(ref isError, value); }
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

        public void LoadLayout(XmlElement viewNode)
        {
            OnLoadLayout(viewNode);
        }

        public void SaveLayout(XmlTextWriter writer)
        {
            writer.WriteStartElement("View");

            writer.WriteAttributeString("Source", XmlConvert.ToString(DataSource.UniqueIdentifier));
            writer.WriteAttributeString("ComponentIndex", XmlConvert.ToString(SignalView.ComponentIndex));

            OnSaveLayout(writer);

            writer.WriteEndElement();
        }

        protected abstract void OnLoadLayout(XmlElement viewNode);
        protected abstract void OnSaveLayout(XmlTextWriter writer);
    }
}
