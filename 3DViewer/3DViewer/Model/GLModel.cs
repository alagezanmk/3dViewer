using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DViewer.Model
{
    public class Vector3
    {
        public float x;
        public float y;
        public float z;
    }

    public class Facet
    {
        public Vector3[] normals = new Vector3[3];
        public Vector3[] vertexes = new Vector3[3];

        public Facet()
        {
            for (int i = 0; i < 3; i++)
            {
                this.normals[i] = new Vector3();
                this.vertexes[i] = new Vector3();
            }
        }
    }

    class GLModel
    {
        public string name;
        public List<Facet> facetList = new List<Facet>();

        static void addVectorValues(float[] values, ref int p, Vector3 vector)
        {
            values[p++] = vector.x;
            values[p++] = vector.y;
            values[p++] = vector.z;
        }

        static void addVectorValues(float[] values, ref int p, Vector3[] vectors)
        {
            foreach (Vector3 v in vectors)
                addVectorValues(values, ref p, v);
        }

        public float[] GetNormalPoints()
        {
            int p = 0;
            float[] values = new float[this.facetList.Count * 3 * 3];

            foreach (Facet facet in this.facetList)
                addVectorValues(values, ref p, facet.normals);

            return values;
        }

        public float[] GetVertexValues()
        {
            int p = 0;
            float[] values = new float[this.facetList.Count * 3 * 3];

            foreach (Facet facet in this.facetList)
                addVectorValues(values, ref p, facet.vertexes);

            return values;
        }

        public void GetMinPosition(ref float x, ref float y, ref float z)
        {
            if (this.facetList.Count > 0)
            {
                x = this.facetList[0].vertexes[0].x;
                y = this.facetList[0].vertexes[0].y;
                z = this.facetList[0].vertexes[0].z;
            }

            foreach (Facet facet in this.facetList)
            {
                foreach (Vector3 v in facet.vertexes)
                {
                    x = Math.Min(x, v.x);
                    y = Math.Min(y, v.y);
                    z = Math.Min(z, v.z);
                }
            }
        }

        public void GetMaxPosition(ref float x, ref float y, ref float z)
        {
            //Vector3 maxVec = new Vector3();
            if (this.facetList.Count > 0)
            {
                x = this.facetList[0].vertexes[0].x;
                y = this.facetList[0].vertexes[0].y;
                z = this.facetList[0].vertexes[0].z;
            }

            foreach (Facet facet in this.facetList)
            {
                foreach (Vector3 v in facet.vertexes)
                {
                    x = Math.Max(x, v.x);
                    y = Math.Max(y, v.y);
                    z = Math.Max(z, v.z);
                }
            }
        }
    }
}
