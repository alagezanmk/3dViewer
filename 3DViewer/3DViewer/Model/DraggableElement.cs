using System;
using System.Drawing;
using System.Windows.Controls;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    public interface IDraggableElement
    {
        void StartDrag(OpenGL gl, System.Windows.Point pos);
        void Drag(OpenGL gl, System.Windows.Point pos, double cx, double cy);
        void EndDrag(OpenGL gl, System.Windows.Point pos);
    }
}
