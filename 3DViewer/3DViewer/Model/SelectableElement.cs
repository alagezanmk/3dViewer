using SharpGL;
using SharpGL.SceneGraph;

namespace _3DViewer.Model
{
    public interface ISelectableElement
    {
        void Transform(OpenGL gl);
        void PopTransform(OpenGL gl);

        bool HitTest(OpenGL gl, Vertex pos);

        bool Selected
        {
            get;
            set;
        }        
    }
}
