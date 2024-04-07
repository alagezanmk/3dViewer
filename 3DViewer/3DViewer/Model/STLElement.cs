using System;

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

            this.selected = false;
            this.triangleSelected = false;
            this.DataReady = true;
        }

        #region "ISelectable"

        public virtual void Transform(OpenGL gl, bool rotateOnly = false)
        {
            if (rotateOnly)
                return;

            gl.PushMatrix();

            // Scale to minimal size
            double m = Math.Abs(this.maxPos.X - this.minPos.X);
            double v = Math.Abs(this.maxPos.Y - this.minPos.Y);
            m = Math.Max(m, v);

            v = Math.Abs(this.maxPos.Z - this.minPos.Z);
            m = Math.Max(m, v);

            double ms = Math.Min(m, 3);
            double _scale = ms / m;

            double scale = Math.Min(1, _scale);
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

        bool bbHitTest(Ray ray, Vertex min, Vertex max)
        {
            bool hit = ray.point.X >= min.X && ray.point.X <= max.X
                    && ray.point.Y >= min.Y && ray.point.Y <= max.Y
                    && ray.point.Z >= min.Z && ray.point.Z <= max.Z;
            return hit;
        }

        public virtual bool HitTest(OpenGL gl, Ray ray)
        {
            if (!this.DataReady)
                return false;

            bool hit = false;
            if (this.TriangleSelectionMode)
            {
                Vertex normalPoint = new Vertex();
                Vertex intersectionPoint = new Vertex();
                hit = this.TriangleHitTest(gl, ray, ref intersectionPoint, ref normalPoint);
            }
            else
                this.triangleSelected = false;

            if (false == hit)
                hit = this.bbHitTest(ray, this.minPos, this.maxPos);

            return hit;
        }

        #region "Triangle Hit Test
        bool triangleSelected = false;
        Vertex[] trianglesHitVertexes = new Vertex[3];

        public bool TriangleHitTest(OpenGL gl, Ray ray, 
                                    ref Vertex intersectionPoint,
                                    ref Vertex normal)
        {
            if (!this.DataReady)
                return false;

            bool hit = false;
            int faceCount = this.vertexes.Length / (3 * 3);
            for (int f = 0; f < faceCount; f++)
            {
                int p = f * 3 * 3;
                _getVertex(ref this.trianglesHitVertexes[0], this.vertexes, ref p);
                _getVertex(ref this.trianglesHitVertexes[1], this.vertexes, ref p);
                _getVertex(ref this.trianglesHitVertexes[2], this.vertexes, ref p);

                hit = rayTriangleIntersect(ray.origin, ray.direction,
                                           this.trianglesHitVertexes[0],
                                           this.trianglesHitVertexes[1],
                                           this.trianglesHitVertexes[2],
                                           ref intersectionPoint);

                if (hit)
                {
                    Vertex[] vs = this.trianglesHitVertexes;
                    Vertex va = vs[0] - vs[1];
                    Vertex vb = vs[1] - vs[2];
                    normal = va.VectorProduct(vb);
                    normal.Normalize();
                    break;
                }
            }
    
            this.triangleSelected = hit;
            return this.triangleSelected;

            #region "Helper methods"
            void _getVertex(ref Vertex vertex, float[] vertexes, ref int p)
            {
                vertex.X = vertexes[p++];
                vertex.Y = vertexes[p++];
                vertex.Z = vertexes[p++];
            }

            bool rayTriangleIntersect(Vertex origin, Vertex direction,
                                      Vertex v0, Vertex v1, Vertex v2,
                                      ref Vertex _intersectionPoint)
            {
                const float kEpsilon = 1e-8f;

                Vertex v0v1 = v1 - v0;
                Vertex v0v2 = v2 - v0;
                Vertex pvec = direction.VectorProduct(v0v2);
                float det = v0v1.ScalarProduct(pvec);
            
                // if the determinant is negative, the triangle is 'back facing'
                // if the determinant is close to 0, the ray misses the triangle
                if (det < kEpsilon)
                    return false;

                float invDet = 1 / det;
                Vertex tvec = origin - v0;
                float u = tvec.ScalarProduct(pvec) * invDet;
                if (u < 0 || u > 1)
                    return false;

                Vertex qvec = tvec.VectorProduct(v0v1);
                float v = direction.ScalarProduct(qvec) * invDet;
                if (v < 0 || u + v > 1)
                    return false;

                float t = v0v2.ScalarProduct(qvec) * invDet;
                _intersectionPoint = origin + direction * t;
                return true;
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
            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);

            System.Drawing.Color color = System.Drawing.Color.LightGray;
            if (this.selected && false == this.triangleSelected)
                color = System.Drawing.Color.Yellow;

            if (this.triangleSelected)
                color = System.Drawing.Color.DimGray;

            gl.Color(color.R / 255f, color.G / 255f, color.B / 255f);

            gl.VertexPointer(3, 0, vertexes);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, normals);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, vertexes.Length / 3);

            gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);

            if (this.triangleSelected)
            {
                color = System.Drawing.Color.Yellow;
                gl.Color(color.R, color.G, color.B);

                Vertex[] v = this.trianglesHitVertexes;
                gl.LineWidth(20);
                gl.Begin(OpenGL.GL_TRIANGLES);
                for(int i = 0; i < 3; i++)
                    gl.Vertex(v[i].X, v[i].Y, v[i].Z);
                gl.End();
            }

            gl.PopAttrib();
            this.PopTransform(gl);
        }        
    }
}
