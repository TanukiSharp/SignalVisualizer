using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public class SignalFiltersPipelineViewModel : RootedViewModel
    {
        public IEnumerable<ISignalFilterFactory> AvailableSignalFilterFactories { get; }
        public ReorderableObservableCollection<SignalFilterViewModel> SignalFilters { get; }

        private SignalViewSelectorViewModel signalViewSelector;

        public SignalFiltersPipelineViewModel(RootViewModel root, SignalViewSelectorViewModel signalViewSelector)
            : base(root)
        {
            if (signalViewSelector == null)
                throw new ArgumentNullException(nameof(signalViewSelector));

            this.signalViewSelector = signalViewSelector;

            AvailableSignalFilterFactories = root.SignalFilterFactories;

            SignalFilters = new ReorderableObservableCollection<SignalFilterViewModel>();
            SignalFilters.ChildReordered += SignalFilters_ChildReordered;
        }

        private void SignalFilters_ChildReordered(object sender, ReorderingEventArgs e)
        {
            Root.IsDataModified = true;
        }

        private ISignalFilterFactory selectedSignalFilterFactory;
        public ISignalFilterFactory SelectedSignalFilterFactory
        {
            get { return selectedSignalFilterFactory; }
            set
            {
                SetValue(ref selectedSignalFilterFactory, value);
                ((AnonymousCommand)AddSelectedFilterCommand).IsEnabled = SelectedSignalFilterFactory != null;
            }
        }

        private SignalFilterViewModel selectedSignalFilter;
        public SignalFilterViewModel SelectedSignalFilter
        {
            get { return selectedSignalFilter; }
            set
            {
                SignalFilterViewModel storeSelectedSignalFilter = selectedSignalFilter;

                SetValue(ref selectedSignalFilter, value);
                ((AnonymousCommand)RemoveSelectedFilterCommand).IsEnabled = SelectedSignalFilter != null;
                ((AnonymousCommand)ConfigureSelectedFilterCommand).IsEnabled = SelectedSignalFilter != null;

                if (SignalFilters.IsReordering)
                {
                    // fixes the visual deselection issue when reordering
                    Dispatcher.BeginInvoke((Action)delegate
                    {
                        SetValue(ref selectedSignalFilter, storeSelectedSignalFilter);
                    });
                }
            }
        }

        private ICommand addSelectedFilterCommand;
        public ICommand AddSelectedFilterCommand
        {
            get
            {
                if (addSelectedFilterCommand == null)
                {
                    addSelectedFilterCommand = new AnonymousCommand(AddSelectedFilter);
                    ((AnonymousCommand)addSelectedFilterCommand).IsEnabled = false;
                }
                return addSelectedFilterCommand;
            }
        }

        private void AddSelectedFilter()
        {
            if (SelectedSignalFilterFactory == null)
                return;

            ISignalFilter signalFilter = SelectedSignalFilterFactory.ProduceSignalFilter();

            var vm = new SignalFilterViewModel(Root, signalFilter)
            {
                Reorderer = SignalFilters,
            };

            SignalFilters.Add(vm);
            Root.IsDataModified = true;
        }

        private ICommand removeSelectedFilterCommand;
        public ICommand RemoveSelectedFilterCommand
        {
            get
            {
                if (removeSelectedFilterCommand == null)
                    removeSelectedFilterCommand = new AnonymousCommand(RemoveSelectedFilter) { IsEnabled = false };
                return removeSelectedFilterCommand;
            }
        }

        private void RemoveSelectedFilter()
        {
            if (SelectedSignalFilter == null)
                return;

            SelectedSignalFilter.Cleanup();
            SignalFilters.Remove(SelectedSignalFilter);
            Root.IsDataModified = true;
        }

        private ICommand configureSelectedFilterCommand;
        public ICommand ConfigureSelectedFilterCommand
        {
            get
            {
                if (configureSelectedFilterCommand == null)
                    configureSelectedFilterCommand = new AnonymousCommand(ConfigureSelectedFilter) { IsEnabled = false };
                return configureSelectedFilterCommand;
            }
        }

        private void ConfigureSelectedFilter()
        {
            if (SelectedSignalFilter == null)
                return;

            SelectedSignalFilter.Configure();
        }

        internal void Cleanup()
        {
            SignalFilters.ChildReordered -= SignalFilters_ChildReordered;

            foreach (SignalFilterViewModel signalFilter in SignalFilters)
                signalFilter.Cleanup();
            
            SignalFilters.Clear();
        }
    }
}
