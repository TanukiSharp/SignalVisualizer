using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public class DataSourceInstanceViewModel : RootedViewModel
    {
        private readonly SignalViewSelectorViewModel parent;

        public IDataSource DataSource { get; }

        public string Name { get; }
        public int ComponentCount { get; }
        public string[] Components { get; }

        public DataSourceInstanceViewModel(RootViewModel root, SignalViewSelectorViewModel parent, IDataSource dataSource)
            : base(root)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));
            if (dataSource == null)
                throw new ArgumentNullException(nameof(dataSource));

            this.parent = parent;

            DataSource = dataSource;

            Name = dataSource.Name;
            ComponentCount = dataSource.ComponentNames.Length;
            Components = dataSource.ComponentNames;
        }

        private int selectedComponentIndex = -1;
        public int SelectedComponentIndex
        {
            get { return selectedComponentIndex; }
            set
            {
                if (SetValue(ref selectedComponentIndex, value))
                    parent.IsValidable = parent.SelectedDataSource != null && SelectedComponentIndex > -1;
            }
        }
    }
}
