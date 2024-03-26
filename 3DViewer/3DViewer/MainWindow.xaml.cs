using _3DViewer.ViewModel;
using SharpGL;
using SharpGL.WPF;
using System.Windows;
using System.Windows.Input;

namespace _3DViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GLViewModel viewModel = null;
        public MainWindow()
        {
            InitializeComponent();

            this.viewModel = this.DataContext as GLViewModel;
            this.viewModel.uiMainWindow = this;
        }

        private void ogl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            this.gl = args.OpenGL;
            this.viewModel.glView.Draw(this, this.gl);
        }

        OpenGL gl;
        private void ogl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            this.gl = args.OpenGL;
            this.viewModel.glView.initializeGL(this, this.gl);
        }

        private void ogl_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            this.viewModel.glView.OnMouseWheel(e.Delta);
        }

        private void ogl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point pos = e.GetPosition(this);
            this.viewModel.glView.OnMouseMove(pos, this.ogl.ActualWidth, this.ogl.ActualHeight);
        }

        private void ogl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            this.viewModel.glView.OnMouseDown(this, this.gl, pos,
                MouseButtonState.Pressed == e.LeftButton,
                MouseButtonState.Pressed == e.RightButton);
        }

        private void ogl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            this.viewModel.glView.OnMouseUp(pos, 
                MouseButtonState.Pressed == e.LeftButton, 
                MouseButtonState.Pressed == e.RightButton);
        }
    }
}
