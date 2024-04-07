using SharpGL;
using System.Windows.Controls;

namespace _3DViewer.Model
{
    public interface IDraggableElement
    {
        void StartDrag(OpenGL gl, Control view, System.Windows.Point pos);
        bool Drag(OpenGL gl, Control view, System.Windows.Point pos, double cx, double cy);
        void EndDrag(OpenGL gl, Control view, System.Windows.Point pos);
    }
}
