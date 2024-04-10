using System;
using System.Windows.Controls;
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
        public bool DataReady = false;
        public bool TriangleSelectionMode = true;

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

            this.DataReady = true;
            this.selected = false;
            this.triangleSelected = false;
        }

        #region "ISelectable"
        public virtual void Transform(OpenGL gl, bool rotateOnly = false)
        {
            if (rotateOnly)
                return;

            gl.PushMatrix();

            // Scale the model to a size brtween min to max
            const double minSize = 3, maxSize = 7;
            double maxModelSize = Math.Abs(this.maxPos.X - this.minPos.X);
            double size = Math.Abs(this.maxPos.Y - this.minPos.Y);
            maxModelSize = Math.Max(maxModelSize, size);

            size = Math.Abs(this.maxPos.Z - this.minPos.Z);
            maxModelSize = Math.Max(maxModelSize, size);

            double rangeSize = Math.Min(maxModelSize, maxSize);
            rangeSize = Math.Max(rangeSize, minSize);

            double scale = rangeSize / maxModelSize;
            gl.Scale(scale, scale, scale);

            // Move Shape(X,Y-Center, min-Z) to origin
            double cx = (this.maxPos.X + this.minPos.X) / 2f;
            double cy = (this.maxPos.Y + this.minPos.Y) / 2f;
            gl.Translate(-cx, -cy, -this.minPos.Z);
        }

        public virtual void PopTransform(OpenGL gl)
        {
            gl.PopMatrix();
        }

        public virtual bool HitTest(OpenGL gl, RayCast rayCast)
        {
            if (!this.DataReady)
                return false;

            bool hit = false;
            if (this.TriangleSelectionMode)
            {
                Vertex normalPoint = new Vertex();
                Vertex intersectionPoint = new Vertex();
                hit = this.TriangleHitTest(gl, rayCast, ref intersectionPoint, ref normalPoint);
            }
            else
                this.triangleSelected = false;

            if (false == hit)
                hit = Geometry.BBHitTest(rayCast, this.minPos, this.maxPos);

            return hit;
        }

        #region "Triangle Hit Test
        int triangleHitIndex = 0;
        bool triangleSelected = false;

        public bool TriangleHitTest(OpenGL gl, RayCast ray, 
                                    ref Vertex intersectionPoint,
                                    ref Vertex normal)
        {
            if (!this.DataReady)
                return false;

            this.triangleSelected = false;
            double minDistance = double.MaxValue;

            Vertex[] vs = new Vertex[3];
            bool hit = false;
            int faceCount = this.vertexes.Length / (3 * 3);
            for (int f = faceCount - 1; f >= 0; f--)
            {
                int p = f * 3 * 3;
                getVertexes(vs, this.vertexes, ref p);
                hit = Geometry.TriangleIntersect(ray, vs[0], vs[1], vs[2], ref intersectionPoint);
                if (hit)
                {
                    double distance = Geometry.Distance(ray.point, intersectionPoint);
                    if (distance < minDistance)
                    {
                        this.triangleSelected = true;
                        minDistance = distance;
                        this.triangleHitIndex = f;
                        normal = Geometry.Normal(vs[0], vs[1], vs[2]);
                    }
                }
            }
    
            return this.triangleSelected;

            #region "Helper methods"
            void getVertex(ref Vertex vertex, float[] vertexes, ref int p)
            {
                vertex.X = vertexes[p++];
                vertex.Y = vertexes[p++];
                vertex.Z = vertexes[p++];
            }

            void getVertexes(Vertex[] vs1, float[] vertexes, ref int p)
            {
                getVertex(ref vs1[0], vertexes, ref p);
                getVertex(ref vs1[1], vertexes, ref p);
                getVertex(ref vs1[2], vertexes, ref p);
            }
            #endregion "Helper methods"
        }

        #endregion "Triangle Hit Test

        bool selected = false;
        bool ISelectableElement.Selected
        {
            get => selected;
            set => selected = value;
        }
        #endregion "ISelectable"

        #region "IRenderable"
        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            if (!this.DataReady)
                return;

            //  Push all attributes, disable lighting and depth testing.
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.DepthFunc(OpenGL.GL_ALWAYS);

            this.Transform(gl);

            /// Drawing ---------------------------------------------
            System.Drawing.Color color = System.Drawing.Color.LightGray;
            if (this.selected && false == this.triangleSelected)
                color = System.Drawing.Color.Yellow;

            if (this.triangleSelected)
                color = System.Drawing.Color.DimGray;

            gl.Color(color.R, color.G, color.B, (byte)100);

            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.VertexPointer(3, 0, vertexes);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, normals);
            if (this.triangleSelected)
            {
                if (this.triangleHitIndex > 0)
                    gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, this.triangleHitIndex * 3);

                // Draw selected triangle
                System.Drawing.Color oldColor = color;
                color = System.Drawing.Color.Aqua;
                gl.Color(color.R, color.G, color.B, (byte)255);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, this.triangleHitIndex * 3, 3);

                int triangleCount = this.vertexes.Length / (3 * 3);
                int lastStartIndex = this.triangleHitIndex + 1;
                if (lastStartIndex < triangleCount)
                {
                    color = oldColor;
                    gl.Color(color.R, color.G, color.B, (byte)100);
                    int lastCount = triangleCount - lastStartIndex;
                    gl.DrawArrays(OpenGL.GL_TRIANGLES, lastStartIndex * 3, lastCount * 3);
                }

                color = System.Drawing.Color.Yellow;
                gl.Color(color.R, color.G, color.B, (byte)200);
                gl.DrawArrays(OpenGL.GL_TRIANGLES, this.triangleHitIndex * 3, 3);
            }
            else
                gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, this.vertexes.Length / 3);

            gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);

            gl.PopAttrib();
            this.PopTransform(gl);
        }
        #endregion "IRenderable"
    }
}
