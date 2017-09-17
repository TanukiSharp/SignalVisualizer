using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using SignalVisualizer.Contracts;
using System.Windows.Controls;

namespace SignalVisualizer.ViewModels
{
    public class RootViewModel : ViewModelBase, IDisposable
    {
        public IEnumerable<ISignalFilterFactory> SignalFilterFactories { get; }
        public IEnumerable<IDataSource> DataSources { get; }

        public IEnumerable<DataSourceViewModel> DataSourceViewModels { get; }

        public ObservableCollection<SignalViewGroupViewModel> Groups { get; }

        private Orientation globalLayoutOrientation = Orientation.Vertical;
        public Orientation GlobalLayoutOrientation
        {
            get { return globalLayoutOrientation; }
            set
            {
                if (SetValue(ref globalLayoutOrientation, value))
                    IsDataModified = true;
            }
        }

        private readonly LayoutFileManager layoutFileManager;

        private static readonly string BaseTitle;

        static RootViewModel()
        {
            Version version = Assembly.GetEntryAssembly().GetName().Version;
            BaseTitle = $"Signal Visualizer v{version.Major}.{version.Minor}.{version.Revision}";
        }

        public RootViewModel(
            IEnumerable<IDataSource> dataSources,
            IEnumerable<ISignalFilterFactory> signalFilterFactories
        )
        {
            if (dataSources == null)
                throw new ArgumentNullException(nameof(dataSources));
            if (signalFilterFactories == null)
                throw new ArgumentNullException(nameof(signalFilterFactories));

            UpdateTitle();

            layoutFileManager = new LayoutFileManager(this);
            layoutFileManager.FilenameChanged += LayoutFileManager_FilenameChanged;
            layoutFileManager.IsModifiedChanged += LayoutFileManager_IsModifiedChanged;

            CloseCommand = new AnonymousCommand(App.Current.MainWindow.Close);

            NewLayoutCommand = new AnonymousCommand(NewLayout);
            LoadLayoutCommand = new AnonymousCommand(LoadLayout);
            SaveLayoutCommand = new AnonymousCommand(SaveLayout);
            SaveLayoutAsCommand = new AnonymousCommand(SaveLayoutAs);

            DataSources = new ReadOnlyCollection<IDataSource>(dataSources.ToArray());
            SignalFilterFactories = new ReadOnlyCollection<ISignalFilterFactory>(signalFilterFactories.ToArray());

            DataSourceViewModels = DataSources.Select(s => new DataSourceViewModel(this, s)).ToArray();

            Groups = new ObservableCollection<SignalViewGroupViewModel>();

            System.Windows.Window window = App.Current.MainWindow;
            window.Closing += MainWindow_Closing;
            window.Closed += Window_Closed;

            window.CommandBindings.Add(CreateCommandBinding(ApplicationCommands.New, NewLayoutCommand));
            window.CommandBindings.Add(CreateCommandBinding(ApplicationCommands.Open, LoadLayoutCommand));
            window.CommandBindings.Add(CreateCommandBinding(ApplicationCommands.Save, SaveLayoutCommand));
            window.CommandBindings.Add(CreateCommandBinding(ApplicationCommands.SaveAs, SaveLayoutAsCommand));
            window.CommandBindings.Add(CreateCommandBinding(ApplicationCommands.Close, CloseCommand));
        }

        public void Cleanup()
        {
            foreach (SignalViewGroupViewModel x in Groups)
            {
                RemoveGroup(x, false);
                x.Dispose();
            }
            Groups.Clear();

            foreach (DataSourceViewModel dataSource in DataSourceViewModels)
                dataSource.Stop();
        }

        public void Dispose()
        {
            foreach (SignalViewGroupViewModel x in Groups)
            {
                RemoveGroup(x, false);
                x.Dispose();
            }
            Groups.Clear();

            foreach (DataSourceViewModel x in DataSourceViewModels)
                x.Dispose();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            layoutFileManager.FilenameChanged -= LayoutFileManager_FilenameChanged;
            layoutFileManager.IsModifiedChanged -= LayoutFileManager_IsModifiedChanged;
        }

        private CommandBinding CreateCommandBinding(ICommand source, ICommand target)
        {
            return new CommandBinding(source, (s, e) =>
            {
                if (target != null && target.CanExecute(e.Parameter))
                    target.Execute(e.Parameter);
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool isCancelled;

            layoutFileManager.ApplicationClose(out isCancelled);

            if (isCancelled)
                e.Cancel = true;
        }

        private void LayoutFileManager_IsModifiedChanged(object sender, EventArgs e)
        {
            UpdateTitle();
        }

        private void LayoutFileManager_FilenameChanged(object sender, EventArgs e)
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var sb = new StringBuilder(BaseTitle);

            if (layoutFileManager != null)
            {
                if (layoutFileManager.Filename != null || layoutFileManager.IsModified)
                {
                    sb.Append(" - ");

                    if (layoutFileManager.Filename != null)
                        sb.Append($"{layoutFileManager.Filename} ");

                    if (layoutFileManager.IsModified)
                        sb.Append("*");
                }
            }

            App.Current.MainWindow.Title = sb.ToString();
        }

        public bool IsDataModified
        {
            get { return layoutFileManager.IsModified; }
            set { layoutFileManager.IsModified = value; }
        }

        public ICommand CloseCommand { get; }

        public ICommand NewLayoutCommand { get; }
        public ICommand LoadLayoutCommand { get; }
        public ICommand SaveLayoutCommand { get; }
        public ICommand SaveLayoutAsCommand { get; }

        public ICommand changeGlobalLayoutOrientationCommand;
        public ICommand ChangeGlobalLayoutOrientationCommand
        {
            get
            {
                if (changeGlobalLayoutOrientationCommand == null)
                    changeGlobalLayoutOrientationCommand = new AnonymousCommand<string>(OnChangeGlobalLayoutOrientationCommand);
                return changeGlobalLayoutOrientationCommand;
            }
        }

        private void OnChangeGlobalLayoutOrientationCommand(string orientation)
        {
            if (orientation == "Vertical")
                GlobalLayoutOrientation = Orientation.Vertical;
            else
                GlobalLayoutOrientation = Orientation.Horizontal;
        }

        public void NewLayout()
        {
            layoutFileManager.Close();
        }

        public void LoadLayout()
        {
            layoutFileManager.Open();
        }

        public void SaveLayout()
        {
            layoutFileManager.Save();
        }

        public void SaveLayoutAs()
        {
            layoutFileManager.SaveAs();
        }

        private ICommand addGroupCommand;
        public ICommand AddGroupCommand
        {
            get
            {
                if (addGroupCommand == null)
                    addGroupCommand = new AnonymousCommand(() => AddGroup());
                return addGroupCommand;
            }
        }

        public SignalViewGroupViewModel AddGroup()
        {
            SignalViewGroupViewModel newInstance = null;
            newInstance = new SignalViewGroupViewModel(this, () => RemoveGroup(newInstance, true));

            Groups.Add(newInstance);

            return newInstance;
        }

        private void RemoveGroup(SignalViewGroupViewModel group, bool removeFromCollection)
        {
            group.Dispose();

            if (removeFromCollection)
                Groups.Remove(group);
        }
    }

    public abstract class RootedViewModel : ViewModelBase
    {
        public RootViewModel Root { get; }

        public RootedViewModel(RootViewModel root)
        {
            if (root == null)
                throw new ArgumentNullException(nameof(root));

            Root = root;
        }

        protected bool SetDataValue<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (SetValue(ref field, value, propertyName))
            {
                Root.IsDataModified = true;
                return true;
            }

            return false;
        }
    }
}
