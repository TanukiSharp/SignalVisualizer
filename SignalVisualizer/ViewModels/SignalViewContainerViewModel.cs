using System;
using System.Linq;
using System.Windows.Input;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public enum PaneType
    {
        SignalViewSelector,
        SignalView,
        FilterPipeline,
    }

    public class SignalViewContainerViewModel : RootedViewModel
    {
        private readonly Action onRemove;

        public SignalViewContainerViewModel(RootViewModel root, Action onRemove)
            : base(root)
        {
            if (onRemove == null)
                throw new ArgumentNullException(nameof(onRemove));

            this.onRemove = onRemove;

            SignalViewSelector = new SignalViewSelectorViewModel(root, this);
            SignalFiltersPipeline = new SignalFiltersPipelineViewModel(root, SignalViewSelector);
        }

        public void SetWorkingSignalView(IDataSource dataSource, int dataSourceComponentIndex)
        {
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));
            if (dataSourceComponentIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(dataSourceComponentIndex));

            if (SignalView != null)
            {
                if (SignalView.DataSource != dataSource || SignalView.SignalView.ComponentIndex != dataSourceComponentIndex)
                {
                    SignalView.Dispose();
                    SignalView = null;
                }
            }

            if (SignalView == null)
            {
                int index = dataSourceComponentIndex;
                SignalView = CreateSignalView(dataSource, new SignalView(dataSource.ComponentNames[index], index));
            }

            CurrentPane = PaneType.SignalView;
        }

        private SignalViewViewModelBase CreateSignalView(IDataSource dataSource, ISignalView signalView)
        {
            return new SignalViewViewModel(Root, this, dataSource, signalView);
        }

        private PaneType currentPane;
        public PaneType CurrentPane
        {
            get { return currentPane; }
            set { SetValue(ref currentPane, value); }
        }

        public SignalViewSelectorViewModel SignalViewSelector { get; }
        public SignalFiltersPipelineViewModel SignalFiltersPipeline { get; }

        private SignalViewViewModelBase signalView;
        public SignalViewViewModelBase SignalView
        {
            get { return signalView; }
            private set { SetValue(ref signalView, value); }
        }

        #region Remove Container

        private ICommand removeCommand;
        public ICommand RemoveCommand
        {
            get
            {
                if (removeCommand == null)
                    removeCommand = new AnonymousCommand(Remove);
                return removeCommand;
            }
        }

        private void Remove()
        {
            onRemove();

            if (SignalView != null)
                SignalView.Remove();
        }

        #endregion

        #region View Selection

        private ICommand changeViewCommand;
        public ICommand ChangeViewCommand
        {
            get
            {
                if (changeViewCommand == null)
                    changeViewCommand = new AnonymousCommand(() => CurrentPane = PaneType.SignalViewSelector);
                return changeViewCommand;
            }
        }

        private ICommand acceptViewCommand;
        public ICommand AcceptViewCommand
        {
            get
            {
                if (acceptViewCommand == null)
                    acceptViewCommand = new AnonymousCommand(AcceptView);
                return acceptViewCommand;
            }
        }

        private void AcceptView()
        {
            if (SignalViewSelector.SelectedDataSource == null)
                return;

            DataSourceInstanceViewModel selectedDataSource = SignalViewSelector.SelectedDataSource;

            SetWorkingSignalView(selectedDataSource.DataSource, selectedDataSource.SelectedComponentIndex);

            Root.IsDataModified = true;
        }

        #endregion

        private ICommand filtersCommand;
        public ICommand FiltersCommand
        {
            get
            {
                if (filtersCommand == null)
                    filtersCommand = new AnonymousCommand(() => CurrentPane = PaneType.FilterPipeline);
                return filtersCommand;
            }
        }

        private ICommand returnToViewCommand;
        public ICommand ReturnToViewCommand
        {
            get
            {
                if (returnToViewCommand == null)
                    returnToViewCommand = new AnonymousCommand(() => CurrentPane = PaneType.SignalView);
                return returnToViewCommand;
            }
        }

        public void LoadLayout(XmlElement containerNode)
        {
            if (containerNode.SelectSingleNode("View") is XmlElement viewNode)
                LoadSignalView(viewNode);

            foreach (XmlElement filterNode in containerNode.SelectNodes("Filters/Filter"))
                LoadFilter(filterNode);
        }

        private void LoadSignalView(XmlElement viewNode)
        {
            XmlAttribute attr = viewNode.Attributes["Source"];
            if (attr == null)
                return;

            Guid sourceGuid = XmlConvert.ToGuid(attr.Value);

            IDataSource dataSource = Root.DataSources.FirstOrDefault(s => s.UniqueIdentifier == sourceGuid);
            if (dataSource == null)
                return;

            attr = viewNode.Attributes["ComponentIndex"];
            if (attr == null)
                return;

            int componentIndex;
            if (int.TryParse(attr.Value, out componentIndex) == false)
                return;

            SetWorkingSignalView(dataSource, componentIndex);

            SignalViewSelector.SelectedDataSource =  SignalViewSelector.AvailableDataSources.FirstOrDefault(vm => vm.DataSource.UniqueIdentifier == sourceGuid);
            if (SignalViewSelector.SelectedDataSource != null)
                SignalViewSelector.SelectedDataSource.SelectedComponentIndex = componentIndex;

            SignalView.LoadLayout(viewNode);
        }

        private void LoadFilter(XmlElement filterNode)
        {
            XmlAttribute attr = filterNode.Attributes["Guid"];
            if (attr == null)
                return;

            Guid guid = XmlConvert.ToGuid(attr.Value);

            ISignalFilterFactory factory = Root.SignalFilterFactories
                .FirstOrDefault(f => f.UniqueIdentifier == guid);

            if (factory == null)
                return;

            DataSourceInstanceViewModel selectedDataSource = SignalViewSelector.SelectedDataSource;

            ISignalFilter filter = factory.ProduceSignalFilter();

            try
            {
                filter.Load(filterNode);
                SignalFiltersPipeline.SignalFilters.Add(new SignalFilterViewModel(Root, filter));
            }
            catch { }
        }

        public void SaveLayout(XmlTextWriter writer)
        {
            writer.WriteStartElement("Container");

            if (SignalView != null)
            {
                SignalView.SaveLayout(writer);

                writer.WriteStartElement("Filters");
                foreach (SignalFilterViewModel filter in SignalFiltersPipeline.SignalFilters)
                    filter.SaveLayout(writer);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        internal void Cleanup()
        {
            if (SignalFiltersPipeline != null)
                SignalFiltersPipeline.Cleanup();

            if (SignalView != null)
                SignalView.Cleanup();
        }
    }
}
