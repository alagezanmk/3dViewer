using SharpGL;
using SharpGL.SceneGraph;
using System.Drawing;

using SharpGL.SceneGraph.Cameras;

using System.Windows.Controls;
using System;

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
            this.InitilizeCamera();
        }

        public void InitilizeCamera()
        {
            /// Perspective
            this.perspective.FieldOfView = 45;
            this.perspective.Near = 1;
            this.perspective.Far = 100;
            this.perspective.Position = new Vertex(0f, .00001f, 25f);

            /// Oothogonal 
            this.orthogonal.Near = -2000;
            this.orthogonal.Far = 2000;
            this.orthogonal.Position = new Vertex(0f, 0f, 0f);
        }

        public void Resize(OpenGL gl, Control view)
        {
            gl.Viewport(0, 0, (int)view.ActualWidth, (int)view.ActualHeight);
            this.AspectRatio = view.ActualWidth / Math.Max(1, view.ActualHeight);

            /// Orthagraphic Camera
            double sceneSize = 10;
            this.orthogonal.Left = sceneSize * this.AspectRatio;
            this.orthogonal.Right = -sceneSize * this.AspectRatio;
            this.orthogonal.Top = -sceneSize;
            this.orthogonal.Bottom = sceneSize;

            this.InitializeProjection();
        }

        public void InitializeProjection()
        {
            OpenGL gl = this.CurrentOpenGLContext;
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            this.perspective.Position = new Vertex(0f, .00001f, 24.15f);

            if (this.Perspective)
            {
                var p = this.perspective;
                gl.Perspective(p.FieldOfView, this.AspectRatio, p.Near, p.Far);

                gl.LookAt(p.Position.X, p.Position.Y, p.Position.Z,
                          0, 0, 0,
                          0, 0 , 1);
            }
            else
            {
                var o = this.orthogonal;
                gl.Translate(o.Position.X, o.Position.Y, o.Position.Z);
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
            float[] light_1 = new float[]
            {
                0.2f * color.R / 255.0f,
                0.2f * color.G / 255.0f,
                0.2f * color.B / 255.0f,
                1.0f
            };

            float[] light_2 = new float[]
            {
                10.0f * color.R / 255.0f,
                10.0f * color.G / 255.0f,
                10.0f * color.B / 255.0f,
                1.0f
            };

            float[] specref = new float[]
            {
                0.2f * color.R / 255.0f,
                0.2f * color.G / 255.0f,
                0.2f * color.B / 255.0f,
                1.0f
            };

            gl.Enable(OpenGL.GL_LIGHTING);

            float[] specular_0 = new float[] { -1.0f, -1.0f, 1.0f, 1.0f };
            float[] lightPos_0 = new float[] { 1000f, 1000f, -200.0f, 0.0f };
            setLight(OpenGL.GL_LIGHT0, light_1, light_2, specular_0, lightPos_0);

            float[] specular_1 = new float[] { 1.0f, -1.0f, 1.0f, 1.0f };
            float[] lightPos_1 = new float[] { -1000f, 1000f, -200.0f, 0.0f };
            setLight(OpenGL.GL_LIGHT1, light_1, light_2, specular_1, lightPos_1);

            void setLight(uint light, float[] light1, float[] light2, float[] specular, float[] lightPos)
            {
                gl.Light(light, OpenGL.GL_AMBIENT, light1);
                gl.Light(light, OpenGL.GL_DIFFUSE, light2);
                gl.Light(light, OpenGL.GL_SPECULAR, specular);
                gl.Light(light, OpenGL.GL_POSITION, lightPos);
                gl.Enable(light);
            }

            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, specref);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, 10);
            gl.Enable(OpenGL.GL_NORMALIZE);
        }

        public void Draw(OpenGL gl, Camera camera = null)
        {
            this.InitializeLighting(gl);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            base.Draw(camera);
        }

        ///////////////////////////////////////////////////////////////////////////
        class CameraInfo
        {
            public Vertex Position { get; set; }

            public double FieldOfView { get; set; }

            public double Near { get; set; }
            public double Far { get; set; }
        }

        //------------------------------------
        class OrthoganalCameraInfo : CameraInfo
        {
            public double Left { get; set; }
            public double Right { get; set; }

            public double Top { get; set; }
            public double Bottom { get; set; }
        }
    }
}
