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

        public Vertex screen;
        public Vertex normScreen;

        public float screenWidth;
        public float screenHeight;

        public static RayCast Create(Control view, double screenX, double screenY)
        {
            RayCast ray = new RayCast();
            ray.screen = ray.normScreen = new Vertex((float)screenX, (float)screenY, 0f);
            ray.normScreen.Y = (float)(view.ActualHeight - screenY);

            ray.screenWidth = (float)view.ActualWidth;
            ray.screenHeight = (float)view.ActualHeight;
            return ray;
        }

        public void UpdateObjectPoint(OpenGL gl)
        {
            this.point = Geometry.UnProjectPixelHitZ(gl, this.normScreen.X, this.normScreen.Y);
            this.origin = Geometry.UnProject(gl, this.normScreen.X, this.normScreen.Y, 0);
            this.direction = this.point - this.origin;
        }

    }
}
