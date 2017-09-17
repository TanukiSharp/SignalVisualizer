using System;
using System.Collections.Generic;
using System.Linq;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public class SignalViewSelectorViewModel : RootedViewModel
    {
        private readonly SignalViewContainerViewModel parent;

        public SignalViewSelectorViewModel(RootViewModel root, SignalViewContainerViewModel parent)
            : base(root)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            this.parent = parent;

            AvailableDataSources = root.DataSources
                .Select(ds => new DataSourceInstanceViewModel(root, this, ds))
                .ToArray();
        }

        public IEnumerable<DataSourceInstanceViewModel> AvailableDataSources { get; }

        private bool isValidable;
        public bool IsValidable
        {
            get { return isValidable; }
            set { SetValue(ref isValidable, value); }
        }

        private DataSourceInstanceViewModel selectedDataSource;
        public DataSourceInstanceViewModel SelectedDataSource
        {
            get { return selectedDataSource; }
            set
            {
                // fucking hack to avoid WPF to screw SelectedDataSource.SelectedComponentIndex up
                SetValue(ref selectedDataSource, null);

                if (SetValue(ref selectedDataSource, value))
                    IsValidable = SelectedDataSource != null && SelectedDataSource.SelectedComponentIndex > -1;
            }
        }
    }
}
