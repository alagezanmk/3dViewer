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

        OpenGL gl;

        private void ogl_OpenGLInitialized(object sender, OpenGLRoutedEventArgs args)
        {
            this.gl = args.OpenGL;
            this.viewModel.glView.InitializeGL(this.gl, this.ogl);
        }

        private void ogl_Resized(object sender, OpenGLRoutedEventArgs args)
        {
            this.gl = args.OpenGL;
            this.viewModel.glView.OnResize(this.gl, this.ogl);
        }

        private void ogl_OpenGLDraw(object sender, OpenGLRoutedEventArgs args)
        {
            this.viewModel.glView.OnDraw(args.OpenGL, this.ogl);
        }

        void onMouse_Down_Up(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this.ogl);
            this.viewModel.glView.OnMouseDown(this.gl, this.ogl, pos,
                MouseButtonState.Pressed == e.LeftButton,
                MouseButtonState.Pressed == e.RightButton);
            e.Handled = true;
        }

        private void ogl_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            Point pos = e.GetPosition(this.ogl);
            this.viewModel.glView.OnMouseMove(pos, this.ogl.ActualWidth, this.ogl.ActualHeight);
            e.Handled = true;
        }

        private void ogl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.viewModel.glView.OnMouseWheel(e.Delta);
            e.Handled = true;
        }
    }
}
