using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using SignalVisualizer.Contracts;
using SignalVisualizer.ViewModels;

namespace SignalVisualizer
{
    public class LayoutFileManager : FileManagerBase
    {
        private RootViewModel rootViewModel;

        public LayoutFileManager(RootViewModel rootViewModel)
        {
            if (rootViewModel == null)
                throw new ArgumentNullException(nameof(rootViewModel));

            this.rootViewModel = rootViewModel;
        }

        protected override string CloseDialogMessage
        {
            get { return "Do you want to save the last layout modifications ?"; }
        }

        protected override string CloseDialogTitle
        {
            get { return "Save modification ?"; }
        }

        protected override string DialogBoxFilters
        {
            get { return "Xml File (*.xml)|*.xml|All Files (*.*)|*.*"; }
        }

        protected override string OpenDialogTitle
        {
            get { return "Load layout"; }
        }

        protected override string SaveDialogTitle
        {
            get { return "Save layout"; }
        }

        protected override void OnClose()
        {
            rootViewModel.Cleanup();

            IsModified = false;
        }

        protected override void OnLoad(string filename)
        {
            if (File.Exists(filename) == false)
                return;

            var doc = new XmlDocument();
            doc.Load(filename);

            foreach (XmlElement dataSourceNode in doc.SelectNodes("SignalVisualizer/DataSources/DataSource"))
            {
                try
                {
                    var isRunning = XmlConvert.ToBoolean(dataSourceNode.Attributes["IsRunning"].Value);
                    var guid = XmlConvert.ToGuid(dataSourceNode.Attributes["Guid"].Value);

                    DataSourceViewModel vm = rootViewModel.DataSourceViewModels.FirstOrDefault(svm => svm.DataSource.UniqueIdentifier == guid);
                    if (vm != null)
                    {
                        vm.DataSource.Load(dataSourceNode.ChildNodes.OfType<XmlElement>().FirstOrDefault());

                        if (isRunning)
                            vm.Start();
                    }
                }
                catch { }
            }

            rootViewModel.Groups.Clear();
            foreach (XmlElement groupNode in doc.SelectNodes("SignalVisualizer/Groups/Group"))
            {
                var group = rootViewModel.AddGroup();
                group.LoadLayout(groupNode);
            }
        }

        protected override void OnSave(string filename)
        {
            using (var writer = new XmlTextWriter(filename, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.Indentation = 2;
                writer.IndentChar = ' ';

                writer.WriteStartDocument();
                writer.WriteStartElement("SignalVisualizer");

                if (rootViewModel.DataSourceViewModels.Any())
                {
                    writer.WriteStartElement("DataSources");
                    foreach (DataSourceViewModel vm in rootViewModel.DataSourceViewModels)
                    {
                        writer.WriteStartElement("DataSource");
                        writer.WriteAttributeString("Guid", XmlConvert.ToString(vm.DataSource.UniqueIdentifier));
                        writer.WriteAttributeString("IsRunning", XmlConvert.ToString(vm.IsRunning));
                        vm.DataSource.Save(writer);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                if (rootViewModel.Groups.Count > 0)
                {
                    writer.WriteStartElement("Groups");
                    foreach (var group in rootViewModel.Groups)
                        group.SaveLayout(writer);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
