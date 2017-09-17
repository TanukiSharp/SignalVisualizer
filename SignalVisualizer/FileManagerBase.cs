using System;
using System.Windows;
using Microsoft.Win32;

namespace SignalVisualizer
{
    public abstract class FileManagerBase
    {
        protected abstract string CloseDialogMessage { get; }
        protected abstract string CloseDialogTitle { get; }
        protected abstract void OnClose();

        protected abstract string DialogBoxFilters { get; }
        protected abstract string OpenDialogTitle { get; }
        protected abstract string SaveDialogTitle { get; }

        protected abstract void OnLoad(string filename);
        protected abstract void OnSave(string filename);

        private bool isModified;
        public bool IsModified
        {
            get { return isModified; }
            set
            {
                if (isModified == value)
                    return;

                isModified = value;
                OnIsModifiedChanged();
            }
        }

        public event EventHandler IsModifiedChanged;

        protected virtual void OnIsModifiedChanged()
        {
            IsModifiedChanged?.Invoke(this, EventArgs.Empty);
        }

        private string filename;
        public string Filename
        {
            get { return filename; }
            private set
            {
                if (filename != value)
                {
                    filename = value;
                    OnFilenameChanged();
                }
            }
        }

        public event EventHandler FilenameChanged;

        protected virtual void OnFilenameChanged()
        {
            FilenameChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool InternalClose()
        {
            var result = true;

            if (IsModified)
            {
                var dlgResult = MessageBox.Show(CloseDialogMessage, CloseDialogTitle,
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (dlgResult == MessageBoxResult.Cancel)
                    return false;

                if (dlgResult == MessageBoxResult.Yes)
                    result = Filename != null ? InternalSave() : InternalSaveAs();
            }

            if (result)
            {
                OnClose();
                IsModified = false;
                Filename = null;

                Closed?.Invoke(this, EventArgs.Empty);
            }

            return result;
        }

        private bool InternalOpen()
        {
            if (IsModified)
            {
                if (InternalClose() == false)
                    return false;
            }

            var dlg = new OpenFileDialog
            {
                Title = OpenDialogTitle,
                Filter = DialogBoxFilters,
                Multiselect = false,
            };

            if (dlg.ShowDialog() != true)
                return false;

            InternalOpenWithoutConfirm(dlg.FileName);

            return true;
        }

        private bool InternalOpen(string fileName)
        {
            if (IsModified)
            {
                if (InternalClose() == false)
                    return false;
            }

            InternalOpenWithoutConfirm(fileName);

            return true;
        }

        private void InternalOpenWithoutConfirm(string fileName)
        {
            OnLoad(fileName);

            Filename = fileName;
            IsModified = false;

            Opened?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Closed;
        public event EventHandler Opened;
        public event EventHandler Saved;

        private bool InternalSave()
        {
            if (IsModified == false)
                return true;

            if (Filename == null)
                return InternalSaveAs();

            OnSave(Filename);

            IsModified = false;

            Saved?.Invoke(this, EventArgs.Empty);

            return true;
        }

        private bool InternalSaveAs()
        {
            var dlg = new SaveFileDialog
            {
                Title = SaveDialogTitle,
                Filter = DialogBoxFilters,
            };

            if (dlg.ShowDialog() != true)
                return false;

            OnSave(dlg.FileName);

            Filename = dlg.FileName;
            IsModified = false;

            Saved?.Invoke(this, EventArgs.Empty);

            return true;
        }

        private bool InternalApplicationClose()
        {
            return InternalClose();
        }

        // ================================================================================

        public void Close()
        {
            InternalClose();
        }

        public void Open()
        {
            InternalOpen();
        }

        public void Open(string fileName)
        {
            InternalOpen(fileName);
        }

        public void Save()
        {
            InternalSave();
        }

        public void SaveAs()
        {
            InternalSaveAs();
        }

        public virtual void ApplicationClose(out bool cancel)
        {
            cancel = true;
            if (InternalApplicationClose())
                cancel = false;
        }
    }
}
