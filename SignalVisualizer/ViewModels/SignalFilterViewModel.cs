using System;
using System.Windows.Input;
using System.Xml;
using SignalVisualizer.Contracts;

namespace SignalVisualizer.ViewModels
{
    public class SignalFilterViewModel : RootedViewModel, IReorderable
    {
        public string Name { get { return signalFilter.Name; } }

        private readonly ISignalFilter signalFilter;

        public SignalFilterViewModel(RootViewModel root, ISignalFilter signalFilter)
            : base(root)
        {
            if (signalFilter == null)
                throw new ArgumentNullException(nameof(signalFilter));

            this.signalFilter = signalFilter;
            this.signalFilter.ConfigurationChanged += SignalFilter_ConfigurationChanged;
        }

        private void SignalFilter_ConfigurationChanged(object sender, EventArgs e)
        {
            Root.IsDataModified = true;
        }

        public double ProcessValue(double time, double value)
        {
            return signalFilter.ProcessValue(time, value);
        }

        public void Configure()
        {
            signalFilter.Configure();
        }

        #region IReorderable

        private bool isFirst;
        public bool IsFirst
        {
            get { return isFirst; }
            set { SetValue(ref isFirst, value); }
        }

        private bool isLast;
        public bool IsLast
        {
            get { return isLast; }
            set { SetValue(ref isLast, value); }
        }

        private bool canMoveUp;
        public bool CanMoveUp
        {
            get { return canMoveUp; }
            set { if (SetValue(ref canMoveUp, value)) CanMove = CanMoveUp || CanMoveDown; }
        }

        private bool canMoveDown;
        public bool CanMoveDown
        {
            get { return canMoveDown; }
            set { if (SetValue(ref canMoveDown, value)) CanMove = CanMoveUp || CanMoveDown; }
        }

        private bool canMove;
        public bool CanMove
        {
            get { return canMove; }
            private set { SetValue(ref canMove, value); }
        }

        public IReorderer Reorderer { get; set; }

        private ICommand moveUpCommand;
        public ICommand MoveUpCommand
        {
            get
            {
                if (moveUpCommand == null)
                    moveUpCommand = new AnonymousCommand(MoveUp);
                return moveUpCommand;
            }
        }

        private ICommand moveDownCommand;
        public ICommand MoveDownCommand
        {
            get
            {
                if (moveDownCommand == null)
                    moveDownCommand = new AnonymousCommand(MoveDown);
                return moveDownCommand;
            }
        }

        private void MoveUp()
        {
            if (Reorderer != null)
            {
                lock (Reorderer)
                    Reorderer.MoveUp(this);
            }
        }

        private void MoveDown()
        {
            if (Reorderer != null)
            {
                lock (Reorderer)
                    Reorderer.MoveDown(this);
            }
        }

        #endregion // IReorderable

        public void SaveLayout(XmlTextWriter writer)
        {
            writer.WriteStartElement("Filter");

            writer.WriteAttributeString("Guid", XmlConvert.ToString(signalFilter.SignalFilterFactory.UniqueIdentifier));
            try
            {
                signalFilter.Save(writer);
            }
            catch { }

            writer.WriteEndElement();
        }

        internal void Cleanup()
        {
            signalFilter.ConfigurationChanged -= SignalFilter_ConfigurationChanged;

            if (signalFilter is IDisposable d)
                d.Dispose();
        }
    }
}
