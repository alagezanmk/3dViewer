using Microsoft.Win32;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

using _3DViewer.Model;

namespace _3DViewer.ViewModel
{
    class GLViewModel
    {
        public Window uiMainWindow = null;
        public View.GLView glView = new View.GLView();

        STLData stlData = new STLData();

        public GLViewModel()
        {}

        void setData(STLData data)
        {
            this.stlData = data;
            this.glView.SetModelData(data);
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

            this.setData(reader.stlData);
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
                    fileSaveCommand = new Command.ModelCommand(param => this.FileSave(), 
                                                               canExecute => false == this.stlData.IsEmpty());

                return fileSaveCommand;
            }
        }

        private void FileSave()
        {
            if (this.stlData.IsEmpty())
                return;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "STL Files|*.stl;*.txt;";

            if (false == dialog.ShowDialog())
                return;

            File.STLFileWriter writer = new File.STLFileWriter();
            writer.stlData = this.stlData;
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
                    xmlExport = new Command.ModelCommand(param => this.XmlExport(), 
                                                         canExecute => false == this.stlData.IsEmpty());

                return xmlExport;
            }
        }

        private void XmlExport()
        {
            if (this.stlData.IsEmpty())
                return;

            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "STL Files|*.xml";

            if (false == dialog.ShowDialog())
                return;

            File.STLXmlExport xmlExport = new File.STLXmlExport();
            xmlExport.stlData = this.stlData;

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

            this.setData(xmlImport.stlData);
            MessageBox.Show("XML File has been Successfully imported", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion "XmlImpotCommand"

        #region "ResetProjectionCommand"
        private ICommand resetProjection;
        public ICommand ResetProjectionCommand
        {
            get
            {
                if (resetProjection == null)
                    resetProjection = new Command.ModelCommand(param => this.glView.ResetProjection(), 
                                                               null);

                return resetProjection;
            }
        }
        #endregion "ResetProjectionCommand"

        #region "XProjectionCommand"
        private ICommand xProjection;
        public ICommand XProjectionCommand
        {
            get
            {
                if (xProjection == null)
                    xProjection = new Command.ModelCommand(param => this.glView.ResetProjection("x"),
                                                           null);

                return xProjection;
            }
        }
        #endregion "XProjectionCommand"

        #region "YProjectionCommand"
        private ICommand yProjection;
        public ICommand YProjectionCommand
        {
            get
            {
                if (yProjection == null)
                    yProjection = new Command.ModelCommand(param => this.glView.ResetProjection("y"),
                                                           null);

                return yProjection;
            }
        }
        #endregion "YProjectionCommand"

        #region "ZProjectionCommand"
        private ICommand zProjection;
        public ICommand ZProjectionCommand
        {
            get
            {
                if (zProjection == null)
                    zProjection = new Command.ModelCommand(param => this.glView.ResetProjection("z"),
                                                           null);

                return zProjection;
            }
        }
        #endregion "ZProjectionCommand"

        #region "ToggleProjectionCommand"
        private ICommand toggleProjection;
        public ICommand ToggleProjectionCommand
        {
            get
            {
                if (toggleProjection == null)
                    toggleProjection = new Command.ModelCommand(param => this.ToggleProjection(), null);

                return toggleProjection;
            }
        }

        private void ToggleProjection()
        {
            if (null == this.uiMainWindow)
                return;

            var perspective = this.glView.ToggleProjection();
            var name = perspective ? "Perspective" :"Orthogonal";

            MainWindow view = this.uiMainWindow as MainWindow;
            view.Projection.LargeImageSource = (ImageSource)view.FindResource(name + "Image");
            view.Projection.Label = name;
        }
        #endregion "ToggleProjectionCommand"

        #region "SelectionModeCommand"
        private ICommand selectionModeCommand;
        public ICommand SelectionModeCommand
        {
            get
            {
                if (selectionModeCommand == null)
                    selectionModeCommand = new Command.ModelCommand(param => this.SelectionMode(), null);

                return selectionModeCommand;
            }
        }

        private void SelectionMode()
        {
            if (null == this.uiMainWindow)
                return;

            var triangle = this.glView.ToggleSelectionMode();
            var name = triangle ? "Triangle" : "Object";

            MainWindow view = this.uiMainWindow as MainWindow;
            view.SelectionMode.LargeImageSource = (ImageSource)view.FindResource(name + "Image");
            view.SelectionMode.Label = name;
        }
        #endregion "SelectionModeCommand"

        #region "MouseModeCommand"
        private ICommand mouseModeCommand;
        public ICommand MouseModeCommand
        {
            get
            {
                if (mouseModeCommand == null)
                    mouseModeCommand = new Command.ModelCommand(param => this.MouseMode(), null);

                return mouseModeCommand;
            }
        }

        private void MouseMode()
        {
            if (null == this.uiMainWindow)
                return;

            var panMouseMode = this.glView.TogglePanMouseMode();
            var name = panMouseMode ? "Pan" : "Orbit";

            MainWindow view = this.uiMainWindow as MainWindow;
            view.MouseMode.LargeImageSource = (ImageSource)view.FindResource(name + "Image");
            view.MouseMode.Label = name;
        }
        #endregion "MouseModeCommand"


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
