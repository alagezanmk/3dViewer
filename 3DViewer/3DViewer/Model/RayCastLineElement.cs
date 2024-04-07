using System.Drawing;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    class RayCastLineElement
        : SceneElement,
          IRenderable
    {

        public Vertex position1 = new Vertex();
        public Vertex position2 = new Vertex();
        public float width = 3;

        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            //  Push all matrix, attributes, disable lighting and depth testing.
            gl.PushMatrix();
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.DepthFunc(OpenGL.GL_ALWAYS);

            gl.LineWidth(this.width);
            gl.Begin(OpenGL.GL_LINES);

            GLColor color = Color.Aqua;
            gl.Color(color.R, color.G, color.B);

            gl.Vertex(position1.X, position1.Y, position1.Z);
            gl.Vertex(position2.X, position2.Y, position2.Z);
            gl.End();

            gl.PopAttrib();
            gl.PopMatrix();
        }
    }
}
