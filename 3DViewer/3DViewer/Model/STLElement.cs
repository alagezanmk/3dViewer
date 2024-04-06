using System;
using System.Drawing;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    class STLElement
        : SceneElement
        , IRenderable
    {
        bool dataReady = false;

        Vertex minPos = new Vertex();
        Vertex maxPos = new Vertex();

        float[] normals { get; set; }
        float[] vertexes { get; set; }

        public void SetData(STLData data)
        {
            data.GetMinPosition(ref this.minPos);
            data.GetMaxPosition(ref this.maxPos);

            this.normals = data.GetNormalPoints();
            this.vertexes = data.GetVertexValues();

            this.dataReady = true;
        }

        const int StlId = 2;
        double scale = 1;

        void scaleToFit()
        {
            double m = Math.Abs(this.maxPos.X - this.minPos.X);
            double v = Math.Abs(this.maxPos.Y - this.minPos.Y);
            m = Math.Max(m, v);

            v = Math.Abs(this.maxPos.Z - this.minPos.Z);
            m = Math.Max(m, v);
            double ms = Math.Min(m, 3);

            double _scale = ms / m;
            this.scale = Math.Min(1, _scale);
        }

        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            if (!this.dataReady)
                return;

            gl.LoadName(StlId);

            //  Push all matrix, attributes, disable lighting and depth testing.
            gl.PushMatrix();
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.DepthFunc(OpenGL.GL_ALWAYS);

            // Scale rto minimal size
            this.scaleToFit();
            gl.Scale(this.scale, this.scale, this.scale);

            // Move Shape(X,Y-Center, min-Z) to origin
            double cx = (this.maxPos.X + this.minPos.X) / 2.0f;
            double cy = (this.maxPos.Y + this.minPos.Y) / 2.0f;
            gl.Translate(-cx, -cy, -this.minPos.Z);

            /// Drawing ---------------------------------------------
            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);

            Color modelColor = Color.Beige;
            gl.VertexPointer(3, 0, vertexes);
            gl.Color(modelColor.R / 255.0, modelColor.G / 255.0, modelColor.B / 255.0);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, normals);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, vertexes.Length / 3);

            gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);

            gl.PopAttrib();
            gl.PopMatrix();
        }
    }
}
