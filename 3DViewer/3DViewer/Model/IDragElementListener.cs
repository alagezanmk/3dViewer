using System.Windows;
using System.Windows.Controls;

using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    public enum DragState
    {
        Started,
        Dragging,
        Finished
    }

    public interface IDragElementListener
    {
        bool OnDragElement(Control view, Point clientPos, SceneElement element, DragState state);
    }
}
