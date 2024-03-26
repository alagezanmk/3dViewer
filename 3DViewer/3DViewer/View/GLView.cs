using _3DViewer.Model;
using SharpGL;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace _3DViewer.View
{
    class GLView
    {
        View.GLCamera camera = new View.GLCamera();

        bool modelReady = false;
        Model.Vector3 minPos = new Model.Vector3();
        Model.Vector3 maxPos = new Model.Vector3();
        float[] normals { get; set; }
        float[] vertexes { get; set; }

        public bool Perspective = true;
        bool propertyChanged = true;
        Vector3 compassPosition = new Vector3();

        public GLView()
        {
            this.compassPosition.x = 5;
            this.compassPosition.y = 3;
            this.compassPosition.z = 0;
        }

        public void updateModel(Model.GLModel model)
        {
            this.normals = model.GetNormalPoints();
            this.vertexes = model.GetVertexValues();

            model.GetMinPosition(ref minPos.x, ref minPos.y, ref minPos.z);
            model.GetMaxPosition(ref maxPos.x, ref maxPos.y, ref maxPos.z);

            this.modelReady = true;
        }        

        public void Invalidate()
        {
            this.propertyChanged = true;
        }

        double viewWidth, viewHeight;
        double sizeRatio = 1;
        public void initializeGL(OpenGL gl, Control view)
        {
            //if (false == this.propertyChanged)
              //  return;

            this.propertyChanged = false;

            this.viewWidth = view.Width;
            this.viewHeight = view.Height;
            if (0 == this.viewHeight)
                this.viewHeight = 1;

            this.sizeRatio = this.viewWidth / this.viewHeight;
            gl.Viewport(0, 0, (int)this.viewWidth, (int)this.viewHeight);

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();

            if (this.Perspective)
                gl.Perspective(45, this.sizeRatio, .1, 100);
            else
                gl.Ortho(-view.Width / 2, view.Width / 2, -view.Height / 2, view.Height / 2, -20000, +20000);                    

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();
            gl.ClearColor(0, 0, 0, 0);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            gl.ClearColor(0, 0, 0, 0);

            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.ClearDepth(1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);

            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);
            gl.Enable(OpenGL.GL_LINE_SMOOTH);
            gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);

            this.matrix();
        }

        GlmNet.mat4 projectionViewMatrix = new GlmNet.mat4();
        GlmNet.mat4 cameraViewMatrix = new GlmNet.mat4();
        GlmNet.mat4 modelMatrix = new GlmNet.mat4();
        GlmNet.mat4 pvmMatrixO2S = new GlmNet.mat4(); // Object to Screen
        GlmNet.mat4 pvMatrixO2S = new GlmNet.mat4(); // view Object to screen

        //GlmNet.mat4 mvpMatrixO2S = new GlmNet.mat4(); // Object to Screen

        void matrix()
        {
            // Perspective Projection
            this.projectionViewMatrix = GlmNet.glm.perspective(GlmNet.glm.radians(45.0f),
                                                     (float)this.sizeRatio, .1f, 100f);

            // Camera matrix
            this.cameraViewMatrix = GlmNet.glm.lookAt(new GlmNet.vec3(4, 3, 3),   // Camera is at (4,3,3), in World Space
                                                      new GlmNet.vec3(0, 0, 0),   // and looks at the origin
                                                      new GlmNet.vec3(0, 1, 0));  // Head is up (set to 0,-1,0 to look upside-down)

            // Model matrix: an identity matrix (model will be at the origin)
            this.modelMatrix = new GlmNet.mat4(1.0f);

            // Vec_screen = Mat_projection x Mat_view x Mat_model x Vec_local/Object
            this.pvmMatrixO2S = this.projectionViewMatrix * this.cameraViewMatrix * this.modelMatrix;
            this.pvMatrixO2S = this.projectionViewMatrix * this.cameraViewMatrix;
        }

        public void initializeLighting(OpenGL gl)
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

        public void drawCube(OpenGL gl, float size)
        {
            gl.PushMatrix();
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

            gl.PopMatrix();
        }

        public void drawAxes(OpenGL gl, float size = 1000.0f)
        {
            float wcsSize = 1000.0f;

            gl.PushAttrib(SharpGL.Enumerations.AttributeMask.ColorBuffer);
            gl.LineWidth(5.0f);
            gl.Begin(OpenGL.GL_LINES);

            gl.Color(1.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(wcsSize, 0.0f, 0.0f);

            gl.Color(0.0f, 1.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, wcsSize, 0.0f);

            gl.Color(0.0f, 0.0f, 1.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, wcsSize);
            gl.End();
            gl.PushAttrib(SharpGL.Enumerations.AttributeMask.ColorBuffer);
        }

        void drawArcXZ(OpenGL gl, double cx, double cy, double cz, 
                     float r, double start_angle, double arc_angle)
        {
            double x, y;
            double end_angle = start_angle + arc_angle;
            double angle_increment = Math.PI / 1000;
            double theta1 = 0;
            for (theta1 = start_angle; theta1 < end_angle; theta1 += angle_increment)
            {
                x = r * Math.Cos(theta1);
                y = r * Math.Sin(theta1);

                gl.Vertex(cx + x, cy, cz + y);
            }
        }

        void drawArcXY(OpenGL gl, double cx, double cy, double cz,
                     float r, double start_angle, double arc_angle)
        {
            double x, y;
            double end_angle = start_angle + arc_angle;
            double angle_increment = Math.PI / 1000;
            double theta1 = 0;
            for (theta1 = start_angle; theta1 < end_angle; theta1 += angle_increment)
            {
                x = r * Math.Cos(theta1);
                y = r * Math.Sin(theta1);

                gl.Vertex(cx + x, cy + y, cz);
            }
        }

        void drawArcYZ(OpenGL gl, double cx, double cy, double cz,
                     float r, double start_angle, double arc_angle)
        {
            double x, y;
            double end_angle = start_angle + arc_angle;
            double angle_increment = Math.PI / 1000;
            double theta1 = 0;
            for (theta1 = start_angle; theta1 < end_angle; theta1 += angle_increment)
            {
                x = r * Math.Cos(theta1);
                y = r * Math.Sin(theta1);

                gl.Vertex(cx, cy + x, cz + y);
            }
        }

        void test(OpenGL gl)
        {
            GlmNet.mat4 m = this.getModelMatrix(gl);
            //GlmNet.mat4 m = this.getModelMatrix(gl, OpenGL.GL_);
            GlmNet.mat4 o2s = this.pvMatrixO2S * m;
            //o2s = m2;
            GlmNet.mat4 s2o = GlmNet.glm.inverse(o2s);
            GlmNet.vec4 sv = new GlmNet.vec4((float)((this.viewWidth - 10) / this.viewWidth), (float)(10 / this.viewHeight), 0, 1);

            GlmNet.vec4 ov = s2o * sv;
            GlmNet.vec4 ov1 = o2s * sv;
        }

        const int compassId = 1;
        public void drawCompass(OpenGL gl)
        {
            gl.LoadName(compassId);

            gl.PushMatrix();
            gl.PushAttrib(SharpGL.Enumerations.AttributeMask.ColorBuffer);
            gl.LineWidth(1.0f);

            this.test(gl);
            this.compassPosition.x = 5;
            this.compassPosition.y = .5f;
            this.compassPosition.z = 0;
            gl.Translate(this.compassPosition.x, this.compassPosition.y, this.compassPosition.z);
            this.camera.RotateTransform(gl); // Rotate at fixed compass position along camera origin

            // Draw Base arc
            float o = .2f;
            float l = .5f;
            void drawBase()
            {
                this.drawArcXZ(gl, -o, 0, -o, l, 0, 1.565);
                gl.Vertex(-o, 0f, -o);
                gl.Vertex(-o + l, 0f, -o);
            }

            gl.Color(0f, 128 / 255f, 128 / 255f);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            gl.Begin(OpenGL.GL_POLYGON);
                drawBase();
            gl.End();

            gl.Color(1f, 1f, 1f);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            gl.Begin(OpenGL.GL_POLYGON);
            drawBase();
            gl.End();

            // draw L Wing;
            o = .23f; l = .38f;
            float x = -.01f;
            void drawLWing()
            {
                this.drawArcYZ(gl, x, o, 0, l, 0, 1.565);
                gl.Vertex(x, o, 0);
                gl.Vertex(x + l, o, 0);
                this.drawArcXY(gl, x, o, 0, l, 0, 1.565);
            }

            gl.Color(0f, 128 / 255f, 128 / 255f);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
            gl.Begin(OpenGL.GL_POLYGON);
            drawLWing();
            gl.End();

            gl.Color(1f, 1f, 1f);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            gl.Begin(OpenGL.GL_POLYGON);
            drawLWing();
            gl.End();

            // Vertical line
            const float vertLen = .8f;
            gl.Color(1f, 1f, 1f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(0f, 0f, 0f);
            gl.Vertex(0, vertLen, 0f);
            gl.End();

            // Draw Points
            gl.PointSize(5.0f);
            gl.Begin(OpenGL.GL_POINTS);

            // Vertical Top WHITE point
            gl.Vertex(0f, vertLen, 0f);

            // Vertical Bottom RED point
            gl.Color(1f, 0f, 0f);
            gl.Vertex(0f, 0f, 0f);
            gl.End();

            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);

            gl.PopAttrib();
            gl.PopMatrix();
        }

        const int StlId = 1;
        public void drawModel(OpenGL gl, float scale)
        {
            if (!this.modelReady)
                return;

            gl.LoadName(StlId);
            gl.PushMatrix();
            gl.Scale(scale, scale, scale);
            gl.Translate(-this.minPos.x, -this.minPos.y, -this.minPos.z);
            gl.Translate(   -(this.maxPos.x - this.minPos.x) / 2.0f, 
                            -(this.maxPos.y - this.minPos.y) / 2.0f, 
                            -(this.maxPos.z - this.minPos.z) / 2.0f);

            gl.EnableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.EnableClientState(OpenGL.GL_NORMAL_ARRAY);

            Color modelColor = Color.Beige;
            gl.VertexPointer(3, 0, vertexes);
            gl.Color(modelColor.R / 255.0, modelColor.G / 255.0, modelColor.B / 255.0);
            gl.NormalPointer(OpenGL.GL_FLOAT, 0, normals);
            gl.DrawArrays(OpenGL.GL_TRIANGLES, 0, vertexes.Length / 3);

            gl.DisableClientState(OpenGL.GL_NORMAL_ARRAY);
            gl.DisableClientState(OpenGL.GL_VERTEX_ARRAY);
            gl.PopMatrix();            
        }

        GlmNet.mat4 getModelMatrix(OpenGL gl, uint type = OpenGL.GL_MODELVIEW_MATRIX)
        {
            float[] _m = new float[16];
            gl.GetFloat(type, _m);

            GlmNet.mat4 m = new GlmNet.mat4(new GlmNet.vec4(_m[0], _m[1], _m[2], _m[3]),
                                            new GlmNet.vec4(_m[4], _m[5], _m[6], _m[7]),
                                            new GlmNet.vec4(_m[8], _m[9], _m[10], _m[11]),
                                            new GlmNet.vec4(_m[12], _m[13], _m[14], _m[15]));

            return m;
        }

        float rotation = 0;
        public void Draw(Control view, OpenGL gl)
        {
            this.initializeGL(gl, view);
            this.initializeLighting(gl);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.InitNames();
            gl.PushName(0);

            // Clear The Screen And The Depth Buffer
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Translate(0.0f, 0.0f, -10.0f);
            gl.Rotate(rotation, 0.0f, 1.0f, 0.0f);

            gl.Color(1.0f, 0.0f, 0.0f);
            //Teapot tp = new Teapot();
            //tp.Draw(gl, 5, .75, OpenGL.GL_FILL);

            this.drawCompass(gl);
            this.camera.Transform(gl);

            // Matrix test ----------
            if (this.bHitTest)
            {
                double[] matrix1 = new double[16];
                gl.GetDouble(OpenGL.GL_PROJECTION_MATRIX, matrix1);

                gl.LoadIdentity();
                int i = 0;
                GlmNet.vec4[] vecs = new GlmNet.vec4[4];
                for (int v = 0; v < 4; v++)
                {
                    vecs[v].x = (float)matrix1[i++];
                    vecs[v].y = (float)matrix1[i++];
                    vecs[v].z = (float)matrix1[i++];
                    vecs[v].w = (float)matrix1[i++];
                }

                GlmNet.mat4 mat4 = new GlmNet.mat4(vecs);
                GlmNet.vec4 vPos = new GlmNet.vec4(this.hx, this.hy, 0, 0);
                GlmNet.vec4 glPos = mat4 * vPos;
            }/// ------------------

            this.drawModel(gl, .08f);

            gl.Translate(-10f, -5f, -5f);
            this.drawAxes(gl);
            this.drawCube(gl, .2f);

            //rotation += 3.0f;
        }

        bool bHitTest = false;
        float hx, hy;
        bool hitTest(Control view, OpenGL gl, double x, double y)
        {
            this.bHitTest = true;
            this.hx = (float)x;
            this.hy = (float)y;
            const int BUFFER_LENGTH = 64;
            uint[] selectBuff = new uint[BUFFER_LENGTH];

            int hits;
            int[] viewport = new int[4];

            bool success = false;
            do
            {
                this.initializeGL(gl, view);
                gl.SelectBuffer(BUFFER_LENGTH, selectBuff);
                gl.GetInteger(OpenGL.GL_VIEWPORT, viewport);

                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.PushMatrix();

                // Change render mode
                hits = gl.RenderMode(OpenGL.GL_SELECT);

                gl.PickMatrix(x, viewport[3] - y, 2, 2, viewport);
                gl.Perspective(45, this.sizeRatio, .1, 100);
                this.Draw(view, gl);

                gl.RenderMode(OpenGL.GL_RENDER);
                if (hits > 0)
                {
                    uint count = selectBuff[0];
                    if (count > 0)
                    {
                        uint id = selectBuff[3];
                        success = true;
                    }
                }

                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.PopMatrix();

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
            } while (false);

            this.bHitTest = false;
            return success;
        }

        public void OnMouseDown(Control view, OpenGL gl, System.Windows.Point pos, bool leftButton, bool rightButton)
        {
            if (this.hitTest(view, gl, pos.X, pos.Y))
                return;

            this.camera.OnMouseDown(pos, leftButton, rightButton);
        }

        public void OnMouseUp(System.Windows.Point pos, bool leftButton, bool rightButton)
        {
            this.camera.OnMouseUp(pos, leftButton, rightButton);
        }

        public void OnMouseMove(System.Windows.Point pos, double cx, double cy)
        {
            this.camera.OnMouseMove(pos, cx, cy);
        }

        public void OnMouseWheel(int delta)
        {
            this.camera.OnMouseWheel(delta);
        }
    }
}
