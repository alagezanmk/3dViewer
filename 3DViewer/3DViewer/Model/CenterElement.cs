using _3DViewer.View;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;
using System.Drawing;

namespace _3DViewer.Model
{
    class CenterElement 
        : SceneElement,
          IRenderable
    {
        public CenterElement()
        { }

        public CenterElement(float width, double size)
        {
            this.width = width;
            this.size = size;
        }

        public GLColor color = Color.Green;
        public Vertex position = new Vertex();

        public float width = 3;
        public double size = .5f;

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
            gl.Color(this.color.R, this.color.G, this.color.B);            

            gl.Vertex(position.X - size, position.Y, position.Z);
            gl.Vertex(position.X + size, position.Y, position.Z);

            gl.Vertex(position.X, position.Y - size, position.Z);
            gl.Vertex(position.X, position.Y + size, position.Z);
            gl.End();

            gl.PopAttrib();
            gl.PopMatrix();
        }
    }
}
