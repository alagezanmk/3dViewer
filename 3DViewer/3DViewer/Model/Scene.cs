using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Cameras;

namespace _3DViewer.Model
{
    class Scene : SharpGL.SceneGraph.Scene
    {
        public bool Perspective { get; set; } = true;
        public double AspectRatio { get; set; } = 1;

        CameraInfo perspective = new CameraInfo();
        OrthoganalCameraInfo orthogonal = new OrthoganalCameraInfo();

        public Scene()
        {
            this.perspective.Set(1, 100, 45);
            this.perspective.SetPosition(new Vertex(0f, 1e-20f, 25f));

            this.orthogonal.Set(-2000, 2000);
        }

        public void Draw(OpenGL gl, Camera camera = null)
        {
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            this.InitializeLighting(gl);

            base.Draw(camera);
        }

        public void Resize(OpenGL gl, Control view)
        {
            gl.Viewport(0, 0, (int)view.ActualWidth, (int)view.ActualHeight);
            this.AspectRatio = view.ActualWidth / Math.Max(1, view.ActualHeight);

            /// Orthagraphic Camera
            double sceneSize = 10;
            this.orthogonal.SetClip( sceneSize * this.AspectRatio,
                                    -sceneSize * this.AspectRatio,
                                    -sceneSize, sceneSize);

            this.InitializeProjection();
        }

        public void InitializeProjection()
        {
            OpenGL gl = this.CurrentOpenGLContext;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            if (this.Perspective)
            {
                var p = this.perspective;
                gl.Perspective(p.FieldOfView, this.AspectRatio, p.Near, p.Far);

                gl.LookAt(p.Position.X, p.Position.Y, p.Position.Z,
                          0, 0, 0,
                          0, 0, 1);
            }
            else
            {
                var o = this.orthogonal;
                gl.Ortho(o.Left, o.Right, o.Bottom, o.Top, o.Near, o.Far);
            }
        }
        public void InitializeGL(OpenGL gl, Control view)
        {
            gl.Enable(OpenGL.GL_LIGHTING);
            gl.Enable(OpenGL.GL_LIGHT0);
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.Enable(OpenGL.GL_DEPTH_TEST);

            this.CreateInContext(gl);
            this.Resize(gl, view);
        }

        void InitializeLighting(OpenGL gl)
        {
            Color color = Color.White;
            float[] lightColor1 = new float[]
            {
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                .1f
            };

            color = Color.White;
            float[] lightColor2 = new float[]
            {
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                .1f
            };

            color = Color.LightGray;
            float[] specref = new float[]
            {
                color.R / 255f,
                color.G / 255f,
                color.B / 255f,
                .1f
            };

            gl.Enable(OpenGL.GL_LIGHTING);

            float p = 100000f;
            float[] specular0 = new float[] { -p, -p, p, p };
            float[] lightPos0 = new float[] { 1000f, 1000f, -200f, 0f };
            setLight(OpenGL.GL_LIGHT0, lightColor1, lightColor2, specular0, lightPos0);

            float[] specular1 = new float[] { p, -p, p, p };
            float[] lightPos1 = new float[] { -1000f, 1000f, -200f, 0f };
            setLight(OpenGL.GL_LIGHT1, lightColor1, lightColor2, specular1, lightPos1);

            void setLight(uint lightIndex, float[] _lightColor1, float[] _lightColor2, float[] specular, float[] lightPos)
            {
                gl.Light(lightIndex, OpenGL.GL_AMBIENT, _lightColor1);
                gl.Light(lightIndex, OpenGL.GL_DIFFUSE, _lightColor2);
                gl.Light(lightIndex, OpenGL.GL_SPECULAR, specular);
                gl.Light(lightIndex, OpenGL.GL_POSITION, lightPos);
                gl.Enable(lightIndex);
            }

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, specref);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, 10);
            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        public static Vertex UnProject(OpenGL gl, double clientX, double clientY)
        {
            byte[] pixels = new byte[sizeof(float)];
            gl.ReadPixels((int)clientX, (int)clientY, 1, 1, OpenGL.GL_DEPTH_COMPONENT, OpenGL.GL_FLOAT, pixels);
            float Z = System.BitConverter.ToSingle(pixels, 0);

            double[] world = gl.UnProject((double)clientX, clientY, Z);
            Vertex pos = new Vertex((float)world[0], (float)world[1], (float)world[2]);
            return pos;
        }

        public List<ISelectableElement> HitTest(OpenGL gl, Control view, double clientX, double clientY)
        {
            double normClientY = view.ActualHeight - clientY;

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            Vertex world;
            List<ISelectableElement> selections = new List<ISelectableElement>();
            foreach (ISelectableElement se in this.SceneContainer.Traverse(se => se is ISelectableElement))
            {
                se.Transform(gl);
                world = UnProject(gl, clientX, normClientY);
                se.Selected = se.HitTest(gl, world);
                if (se.Selected)
                    selections.Add(se);

                se.PopTransform(gl);
            }
            
            return selections;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    public class CameraInfo
    {
        public void Set(double near, double far, double fieldOfView = 45)
        {
            this.Near = near;
            this.Far = far;
            this.FieldOfView = fieldOfView;
        }

        public void SetPosition(Vertex position)
        {
            this.Position = position;
        }

        public Vertex Position { get; set; }

        public double FieldOfView { get; set; }

        public double Near { get; set; }
        public double Far { get; set; }
    }

    //------------------------------------
    public class OrthoganalCameraInfo : CameraInfo
    {
        public void SetClip(double left, double right, double top, double bottom)
        {
            this.Left = left;
            this.Right = right;
            this.Top = top;
            this.Bottom = bottom;
        }

        public double Left { get; set; }
        public double Right { get; set; }

        public double Top { get; set; }
        public double Bottom { get; set; }
    }

}
