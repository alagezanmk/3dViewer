using SharpGL;
using SharpGL.SceneGraph;

namespace _3DViewer.Model
{
    public interface ISelectableElement
    {
        void Transform(OpenGL gl, bool rotateOnly = false);

        void PopTransform(OpenGL gl);

        bool HitTest(OpenGL gl, RayCast rayCast);

        bool Selected
        {
            get;
            set;
        }        
    }
}
