using System;
using System.Collections.Generic;

using SharpGL.SceneGraph;

namespace _3DViewer.Model
{
    class STLData
    {
        public string name;
        public List<Facet> facetList = new List<Facet>();

        public bool IsEmpty()
        {
            return 0 == this.facetList.Count;
        }

        public float[] GetNormalPoints()
        {
            int p = 0;
            float[] values = new float[this.facetList.Count * 3 * 3];

            foreach (Facet facet in this.facetList)
                addVertexValues(values, ref p, facet.normals);

            return values;
        }

        public float[] GetVertexValues()
        {
            int p = 0;
            float[] values = new float[this.facetList.Count * 3 * 3];

            foreach (Facet facet in this.facetList)
                addVertexValues(values, ref p, facet.vertexes);

            return values;
        }

        public void GetMinPosition(ref Vertex minPos)
        {
            minPos.Set(0, 0, 0);
            if (this.facetList.Count > 0)
            {
                minPos.X = this.facetList[0].vertexes[0].X;
                minPos.Y = this.facetList[0].vertexes[0].Y;
                minPos.Z = this.facetList[0].vertexes[0].Z;
            }

            foreach (Facet facet in this.facetList)
            {
                foreach (Vertex v in facet.vertexes)
                {
                    minPos.X = Math.Min(minPos.X, v.X);
                    minPos.Y = Math.Min(minPos.Y, v.Y);
                    minPos.Z = Math.Min(minPos.Z, v.Z);
                }
            }
        }

        public void GetMaxPosition(ref Vertex maxPos)
        {
            maxPos.Set(0, 0, 0);
            if (this.facetList.Count > 0)
            {
                maxPos.X = this.facetList[0].vertexes[0].X;
                maxPos.Y = this.facetList[0].vertexes[0].Y;
                maxPos.Z = this.facetList[0].vertexes[0].Z;
            }

            foreach (Facet facet in this.facetList)
            {
                foreach (Vertex v in facet.vertexes)
                {
                    maxPos.X = Math.Max(maxPos.X, v.X);
                    maxPos.Y = Math.Max(maxPos.Y, v.Y);
                    maxPos.Z = Math.Max(maxPos.Z, v.Z);
                }
            }
        }

        static void addVertexValues(float[] values, ref int p, Vertex vector)
        {
            values[p++] = vector.X;
            values[p++] = vector.Y;
            values[p++] = vector.Z;
        }

        static void addVertexValues(float[] values, ref int p, Vertex[] vectors)
        {
            foreach (Vertex v in vectors)
                addVertexValues(values, ref p, v);
        }

        ////////////////////////////////////////////////////////////////////////////
        public class Facet
        {
            public Vertex[] normals = new Vertex[3];
            public Vertex[] vertexes = new Vertex[3];

            public Facet()
            {
                for (int i = 0; i < 3; i++)
                {
                    this.normals[i] = new Vertex();
                    this.vertexes[i] = new Vertex();
                }
            }
        }
    }
}
