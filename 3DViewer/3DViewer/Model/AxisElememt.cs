using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;
using System.Drawing;

namespace _3DViewer.Model
{
    class AxisElememt 
        : SceneElement
        , IRenderable
    {
        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            gl.Translate(-1f, -1f, -1f);
            this.drawAxes(gl);
            this.drawCube(gl, .02f);
        }

        public void drawCube(OpenGL gl, float size)
        {
            //  Push all matrix, attributes, disable lighting and depth testing.
            gl.PushMatrix();
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Scale(size, size, size);

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.Crimson.R / 255.0, Color.Crimson.G / 255.0, Color.Crimson.B / 255.0);
            gl.Vertex(0.5f, 0.5f, -0.5f);
            gl.Vertex(-0.5f, 0.5f, -0.5f);
            gl.Vertex(-0.5f, -0.5f, -0.5f);
            gl.Vertex(0.5f, -0.5f, -0.5f);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.Yellow.R / 255.0, Color.Yellow.G / 255.0, Color.Yellow.B / 255.0);
            gl.Vertex(0.5f, 0.5f, 0.5f);
            gl.Vertex(-0.5f, 0.5f, 0.5f);
            gl.Vertex(-0.5f, -0.5f, 0.5f);
            gl.Vertex(0.5f, -0.5f, 0.5f);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.DarkOrange.R / 255.0, Color.DarkOrange.G / 255.0, Color.DarkOrange.B / 255.0);
            gl.Vertex(0.5f, 0.5f, 0.5f);
            gl.Vertex(0.5f, -0.5f, 0.5f);
            gl.Vertex(0.5f, -0.5f, -0.5f);
            gl.Vertex(0.5f, 0.5f, -0.5f);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.BlueViolet.R / 255.0, Color.BlueViolet.G / 255.0, Color.BlueViolet.B / 255.0);
            gl.Vertex(-0.5f, 0.5f, 0.5f);
            gl.Vertex(-0.5f, -0.5f, 0.5f);
            gl.Vertex(-0.5f, -0.5f, -0.5f);
            gl.Vertex(-0.5f, 0.5f, -0.5f);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.Purple.R / 255.0, Color.Purple.G / 255.0, Color.Purple.B / 255.0);
            gl.Vertex(0.5f, 0.5f, 0.5f);
            gl.Vertex(-0.5f, 0.5f, 0.5f);
            gl.Vertex(-0.5f, 0.5f, -0.5f);
            gl.Vertex(0.5f, 0.5f, -0.5f);
            gl.End();

            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.LightSeaGreen.R / 255.0, Color.LightSeaGreen.G / 255.0, Color.LightSeaGreen.B / 255.0);
            gl.Vertex(0.5f, -0.5f, 0.5f);
            gl.Vertex(-0.5f, -0.5f, 0.5f);
            gl.Vertex(-0.5f, -0.5f, -0.5f);
            gl.Vertex(0.5f, -0.5f, -0.5f);
            gl.End();

            gl.PopAttrib();
            gl.PopMatrix();
        }
        public void drawAxes(OpenGL gl)
        {
            float size = 5f;

            //  Push all matrix, attributes, disable lighting and depth testing.
            gl.PushMatrix();
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.DepthFunc(OpenGL.GL_ALWAYS);

            gl.LineWidth(5.0f);
            gl.Begin(OpenGL.GL_LINES);

            // X Axis - Red
            Vertex v = new Vertex(size, 0f, 0f);
            gl.Color(1f, 0f, 0f);
            gl.Vertex(0f, 0f, 0f);
            gl.Vertex(v);

            v.X += 10;
            Vertex d = gl.Project(v);
            gl.DrawText((int)d.X, (int)d.Y, 1f, 1f, 1f, "Courier New", 12.0f, "x");

            // Y Axis - Green
            v = new Vertex(0f, size, 0f);
            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(0f, 0f, 0f);
            gl.Vertex(v);

            // Z Axis - Blue
            v = new Vertex(0f, 0, size);
            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(0f, 0f, 0f);
            gl.Vertex(v);
            gl.End();

            gl.PopAttrib();
            gl.PopMatrix();;
        }
    }
}
