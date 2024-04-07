﻿using System.Drawing;
using System.Windows.Controls;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    class Geometry
    {
        public static bool BBHitTest(Ray ray, Vertex min, Vertex max)
        {
            bool hit = ray.point.X >= min.X && ray.point.X <= max.X
                    && ray.point.Y >= min.Y && ray.point.Y <= max.Y
                    && ray.point.Z >= min.Z && ray.point.Z <= max.Z;
            return hit;
        }

        public static bool TriangleIntersect(Ray ray,Vertex v0, Vertex v1, Vertex v2,
                                             ref Vertex intersectionPoint)
        {
            const float kEpsilon = 1e-8f;

            Vertex v0v1 = v1 - v0;
            Vertex v0v2 = v2 - v0;
            Vertex pvec = ray.direction.VectorProduct(v0v2);
            float det = v0v1.ScalarProduct(pvec);

            // if the determinant is negative, the triangle is 'back facing'
            // if the determinant is close to 0, the ray misses the triangle
            if (det < kEpsilon)
                return false;

            float invDet = 1 / det;
            Vertex tvec = ray.origin - v0;
            float u = tvec.ScalarProduct(pvec) * invDet;
            if (u < 0 || u > 1)
                return false;

            Vertex qvec = tvec.VectorProduct(v0v1);
            float v = ray.direction.ScalarProduct(qvec) * invDet;
            if (v < 0 || u + v > 1)
                return false;

            float t = v0v2.ScalarProduct(qvec) * invDet;
            intersectionPoint = ray.origin + ray.direction * t;
            return true;
        }

        public static Ray CreateRayCast(Control view, double clientX, double clientY)
        {
            Ray ray = new Ray();
            ray.client = ray.normClient = new Vertex((float)clientX, (float)clientY, 0f);
            ray.normClient.Y = (float)(view.ActualHeight - clientY);

            ray.clientWidth = (float)view.ActualWidth;
            ray.clientHeight = (float)view.ActualHeight;
            return ray;
        }

        public static void RayCastPointsToModel(Ray ray, OpenGL gl)
        {
            ray.point = Geometry.UnProjectPixelHitZ(gl, ray.normClient.X, ray.normClient.Y);
            ray.origin = Geometry.UnProject(gl, ray.normClient.X, ray.normClient.Y, 0);
            ray.direction = ray.point - ray.origin;
        }

        public static Vertex UnProject(OpenGL gl, double clientX, double clientY, float Z)
        {
            double[] vertexes = gl.UnProject((double)clientX, clientY, Z);
            Vertex modelPoint = new Vertex((float)vertexes[0], (float)vertexes[1], (float)vertexes[2]);
            return modelPoint;
        }

        public static Vertex UnProjectPixelHitZ(OpenGL gl, double clientX, double clientY)
        {
            float Z = Geometry.ReadPixelHitZ(gl, (int)clientX, (int)clientY);
            return UnProject(gl, clientX, clientY, Z);
        }

        public static float ReadPixelHitZ(OpenGL gl, int x, int y)
        {
            byte[] pixels = new byte[sizeof(float)];
            gl.ReadPixels(x, y, 1, 1, OpenGL.GL_DEPTH_COMPONENT, OpenGL.GL_FLOAT, pixels);
            float z = System.BitConverter.ToSingle(pixels, 0);
            return z;
        }
    }
}
