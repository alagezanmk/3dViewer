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
        , ISelectableElement
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

        #region "ISelectable"

        public virtual void Transform(OpenGL gl)
        {
            gl.PushMatrix();

            // Scale rto minimal size
            this.scaleToFit();
            gl.Scale(this.scale, this.scale, this.scale);

            // Move Shape(X,Y-Center, min-Z) to origin
            double cx = (this.maxPos.X + this.minPos.X) / 2f;
            double cy = (this.maxPos.Y + this.minPos.Y) / 2f;
            gl.Translate(-cx, -cy, -this.minPos.Z);

            double[] modelview = new double[16];
            gl.GetDouble(OpenGL.GL_MODELVIEW_MATRIX, modelview);
        }

        public virtual void PopTransform(OpenGL gl)
        {
            gl.PopMatrix();
        }

        public virtual bool HitTest(OpenGL gl, Vertex pos)
        {
            if (!this.dataReady)
                return false;

            bool hit = false;
            hit = this.lineHitTest(gl, pos);
            if (false == hit)
            {
                hit = pos.X >= this.minPos.X && pos.X <= this.maxPos.X
                   && pos.Y >= this.minPos.Y && pos.Y <= this.maxPos.Y
                   && pos.Z >= this.minPos.Z && pos.Z <= this.maxPos.Z;
            }

            return hit;
        }

        #region "Line Hit Test
        bool lineSelected = false;
        Vertex linePos1 = new Vertex();
        Vertex linePos2 = new Vertex();
        public bool lineHitTest(OpenGL gl, Vertex pos)
        {
            if (!this.dataReady)
                return false;

            bool hit = false;
            const float m = .2f;
            int vCount = this.vertexes.Length;
            for (int i = 0; i < vCount; i += 3)
            {
                hit = pos.X >= this.vertexes[i + 0] - m && pos.X <= this.vertexes[i + 0] + m
                   && pos.Y >= this.vertexes[i + 1] - m && pos.Y <= this.vertexes[i + 1] + m
                   && pos.X >= this.vertexes[i + 2] - m && pos.Y <= this.vertexes[i + 2] + m;

                if (hit)
                {
                    int v = i;
                    linePos1.X = this.vertexes[v++];
                    linePos1.Y = this.vertexes[v++];
                    linePos1.Y = this.vertexes[v++];

                    linePos2 = linePos1;
                    break;
                }
            }

            //for (int i = 0; i < vCount; i += 3)
            //{
            //    hit = pos.X >= this.vertexes[i + 0] - m && pos.X <= this.vertexes[i + 3 + 0] + m
            //       && pos.Y >= this.vertexes[i + 1] - m && pos.Y <= this.vertexes[i + 3 + 1] + m
            //       && pos.X >= this.vertexes[i + 2] - m && pos.Y <= this.vertexes[i + 3 + 2] + m;

            //    if (hit)
            //    {
            //        int v = i;
            //        linePos1.X = this.vertexes[v++];
            //        linePos1.Y = this.vertexes[v++];
            //        linePos1.Y = this.vertexes[v++];

            //        linePos2.X = this.vertexes[v++];
            //        linePos2.Y = this.vertexes[v++];
            //        linePos2.Y = this.vertexes[v++];
            //        break;
            //    }
            //}

            this.lineSelected = hit;
            return this.lineSelected;
        }
        #endregion "Line Hit Test

        bool selected = false;
        bool ISelectableElement.Selected
        {
            get => selected;
            set => selected = value;
        }
        #endregion "ISelectable"

        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            if (!this.dataReady)
                return;

            //  Push all attributes, disable lighting and depth testing.
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.DepthFunc(OpenGL.GL_ALWAYS);

            this.Transform(gl);

            /// Drawing ---------------------------------------------
            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);

            Color color = Color.White;
            if (this.selected && false == this.lineSelected)
                color = Color.Yellow;

            gl.Color(color.R / 255f, color.G / 255f, color.B / 255f);

            gl.VertexPointer(3, 0, vertexes);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, normals);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, vertexes.Length / 3);

            gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);

            if (this.lineSelected)
            {
                color = Color.Yellow;
                gl.Color(color.R, color.G, color.B);

                gl.LineWidth(20);
                gl.Begin(OpenGL.GL_LINES);
                gl.Vertex(this.linePos1.X, this.linePos1.Y, this.linePos1.Z);
                gl.Vertex(this.linePos2.X, this.linePos2.Y, this.linePos2.Z);
                gl.End();
            }

            gl.PopAttrib();
            this.PopTransform(gl);
        }        
    }
}
