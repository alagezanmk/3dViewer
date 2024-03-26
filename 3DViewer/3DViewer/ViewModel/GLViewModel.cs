using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace _3DViewer.ViewModel
{
    class GLViewModel
    {
        public Window uiMainWindow = null;
        public View.GLView glView = new View.GLView();

        Model.GLModel model = new Model.GLModel();

        public GLViewModel()
        {}

        void setModel(Model.GLModel model)
        {
            this.model = model;
            this.glView.updateModel(model);
        }

        #region "FileOpenCommand"
        private ICommand fileOpenCommand;
        public ICommand FileOpenCommand
        {
            get
            {
                if (fileOpenCommand == null)
                    fileOpenCommand = new Command.ModelCommand(param => this.FileOpen(), null);

                return fileOpenCommand;
            }
        }

        private void FileOpen()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "STL Files|*.stl;*.txt;";

            if (false == dialog.ShowDialog())
                return;

            File.STLFileReader reader = new File.STLFileReader();
            if (!reader.Read(dialog.FileName))
            {
                MessageBox.Show(reader.ProcessError, "STL File Read Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.setModel(reader.model);
            MessageBox.Show("File has been Successfully read", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        #endregion "FileOpenCommand"

        #region "FileSaveCommand"
        private ICommand fileSaveCommand;
        public ICommand FileSaveCommand
        {
            get
            {
                if (fileSaveCommand == null)
                    fileSaveCommand = new Command.ModelCommand(param => this.FileSave(), null);

                return fileSaveCommand;
            }
        }

        private void FileSave()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "STL Files|*.stl;*.txt;";

            if (false == dialog.ShowDialog())
                return;

            File.STLFileWriter writer = new File.STLFileWriter();
            writer.model = this.model;
            if (!writer.Write(dialog.FileName))
            {
                MessageBox.Show(writer.ProcessError, "STL File Write Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("File has been Successfully written", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion "FileSaveCommand"

        #region "XmlExportCommand"
        private ICommand xmlExport;
        public ICommand XmlExportCommand
        {
            get
            {
                if (xmlExport == null)
                    xmlExport = new Command.ModelCommand(param => this.XmlExport(), null);

                return xmlExport;
            }
        }

        private void XmlExport()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "STL Files|*.xml";

            if (false == dialog.ShowDialog())
                return;

            File.STLXmlExport xmlExport = new File.STLXmlExport();
            xmlExport.model = this.model;

            if (!xmlExport.Write(dialog.FileName))
            {
                MessageBox.Show(xmlExport.ProcessError, "Write STL XML File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("XML File has been Successfully exported", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion "XmlExportCommand"

        #region "XmlImportCommand"
        private ICommand xmlImport;
        public ICommand XmlImportCommand
        {
            get
            {
                if (xmlImport == null)
                    xmlImport = new Command.ModelCommand(param => this.XmlImport(), null);

                return xmlImport;
            }
        }

        private void XmlImport()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "STL Files|*.xml";

            if (false == dialog.ShowDialog())
                return;

            File.STLXmlImport xmlImport = new File.STLXmlImport();
            if (!xmlImport.Read(dialog.FileName))
            {
                MessageBox.Show(xmlImport.ProcessError, "Read STL XML File Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.setModel(xmlImport.model);
            MessageBox.Show("XML File has been Successfully imported", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion "XmlImpotCommand"

        #region "TogglePerspectiveCommand"
        private ICommand togglePerspective;
        public ICommand TogglePerspectiveCommand
        {
            get
            {
                if (togglePerspective == null)
                    togglePerspective = new Command.ModelCommand(param => this.TogglePerspective(), null);

                return togglePerspective;
            }
        }

        private void TogglePerspective()
        {
            this.glView.Perspective = !this.glView.Perspective;

            if (null != this.uiMainWindow)
            {
                MainWindow view = this.uiMainWindow as MainWindow;
                var imageName = "OrthogonalImage";
                var labelName = "Orthogonal";
                if (this.glView.Perspective)
                {
                    imageName = "PerspectiveImage";
                    labelName = "Perspective";
                }

                view.Projection.LargeImageSource = (ImageSource)view.FindResource(imageName);
                view.Projection.Label = labelName;
                this.glView.Invalidate();
            }
        }
        #endregion "TogglePerspectiveCommand"

        #region "CloseAppCommand"
        private ICommand closeApp;
        public ICommand CloseAppCommand
        {
            get
            {
                if (closeApp == null)
                    closeApp = new Command.ModelCommand(param => this.CloseApp(), null);

                return closeApp;
            }
        }

        private void CloseApp()
        {
            Application.Current.MainWindow.Close();
        }
        #endregion "CloseAppCommand"
    }
}
