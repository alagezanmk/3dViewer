using SharpGL;
using SharpGL.SceneGraph;

namespace _3DViewer.Model
{
    public class Ray
    {
        public Vertex origin;
        public Vertex direction;

        public Vertex point;

        public Vertex client;
        public Vertex normClient;
        public float clientWidth;
        public float clientHeight;
    }

    public interface ISelectableElement
    {
        void Transform(OpenGL gl, bool rotateOnly = false);
        void PopTransform(OpenGL gl);

        bool HitTest(OpenGL gl, Ray ray);

        bool Selected
        {
            get;
            set;
        }        
    }
}
