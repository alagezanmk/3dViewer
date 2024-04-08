using SharpGL;
using SharpGL.SceneGraph;
using System.Windows.Controls;

namespace _3DViewer.Model
{
    public class RayCast
    {
        public Vertex origin;        // Object ray start point with z 0
        public Vertex point;         // Object hit point
        public Vertex direction;

        public Vertex client;
        public Vertex normClient;

        public float clientWidth;
        public float clientHeight;

        public static RayCast Create(Control view, double clientX, double clientY)
        {
            RayCast ray = new RayCast();
            ray.client = ray.normClient = new Vertex((float)clientX, (float)clientY, 0f);
            ray.normClient.Y = (float)(view.ActualHeight - clientY);

            ray.clientWidth = (float)view.ActualWidth;
            ray.clientHeight = (float)view.ActualHeight;
            return ray;
        }

        public void UpdateClientToObjectPoint(OpenGL gl)
        {
            this.point = Geometry.UnProjectPixelHitZ(gl, this.normClient.X, this.normClient.Y);
            this.origin = Geometry.UnProject(gl, this.normClient.X, this.normClient.Y, 0);
            this.direction = this.point - this.origin;
        }

    }
}
