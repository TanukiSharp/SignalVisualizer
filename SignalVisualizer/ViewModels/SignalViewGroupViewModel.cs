using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Xml;

namespace SignalVisualizer.ViewModels
{
    public class SignalViewGroupViewModel : RootedViewModel, IDisposable
    {
        public SignalViewGroupViewModel(RootViewModel root, Action onRemove)
            : base(root)
        {
            if (onRemove == null)
                throw new ArgumentNullException(nameof(onRemove));

            Containers = new ObservableCollection<SignalViewContainerViewModel>();
            Containers.CollectionChanged += Containers_CollectionChanged;

            RemoveCommand = new AnonymousCommand(onRemove);

            AddView();
        }

        public void Dispose()
        {
            Cleanup();

            Containers.CollectionChanged -= Containers_CollectionChanged;
        }

        private void Containers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ContainerCount = Containers.Count;
        }

        public ObservableCollection<SignalViewContainerViewModel> Containers { get; }

        private int containerCount;
        public int ContainerCount
        {
            get { return containerCount; }
            private set { SetValue(ref containerCount, value); }
        }

        private ICommand addViewCommand;
        public ICommand AddViewCommand
        {
            get
            {
                if (addViewCommand == null)
                    addViewCommand = new AnonymousCommand(() => AddView());
                return addViewCommand;
            }
        }

        public ICommand RemoveCommand { get; }

        public SignalViewContainerViewModel AddView()
        {
            SignalViewContainerViewModel newInstance = null;
            newInstance = new SignalViewContainerViewModel(Root, () =>
            {
                newInstance.Cleanup();
                Containers.Remove(newInstance);
                Root.IsDataModified = true;
            });

            Containers.Add(newInstance);

            return newInstance;
        }

        public void LoadLayout(XmlElement groupNode)
        {
            Containers.Clear();

            foreach (XmlElement containerNode in groupNode.SelectNodes("Container"))
            {
                SignalViewContainerViewModel view = AddView();
                view.LoadLayout(containerNode);
            }
        }

        public void SaveLayout(XmlTextWriter writer)
        {
            writer.WriteStartElement("Group");

            foreach (SignalViewContainerViewModel container in Containers)
                container.SaveLayout(writer);

            writer.WriteEndElement();
        }

        internal void Cleanup()
        {
            foreach (SignalViewContainerViewModel container in Containers)
                container.Cleanup();
        }
    }
}
