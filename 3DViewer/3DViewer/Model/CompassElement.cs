using System;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    class CompassElement
        : SceneElement
        , IRenderable
    {
        public PanZoomOribitElement origin;
        public double viewWidth = 100;
        public double viewHeight = 100;

        Vertex position = new Vertex();

        const int compassId = 1;
        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            gl.LoadName(compassId);

            //  Push all matrix, attributes, disable lighting and depth testing.
            gl.PushMatrix();
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.DepthFunc(OpenGL.GL_ALWAYS);

            // Start with identify Transform
            gl.LoadIdentity();

            gl.Translate(0, 0, -10f); // Move origin to some depth

            // Compass Position Top, Right of View
            var viewport = new int[4];
            gl.GetInteger(OpenGL.GL_VIEWPORT, viewport);

            const int offset = 80;
            int viewWidth = viewport[2];
            int viewHeight = viewport[3];
            double[] w = gl.UnProject(viewWidth - offset, viewHeight - offset, .9);

            this.position.X = (float)w[0];
            this.position.Y = (float)w[1];
            this.position.Z = (float)w[2];
            gl.Translate(this.position.X, this.position.Y, this.position.Z);

            // Render size correction for Orthogonal Project
            double scale = 1;
            Scene scene = (Scene)(this.Parent as SceneContainer).ParentScene;
            if (false == scene.Perspective)
                scale = 3.5;

            gl.Scale(scale, scale, scale);

            // Match with origin Transform
            this.origin?.RotateTransform(gl); // Rotate at fixed compass position along origin

            // Drawing ////////////////////////////////////////////
            // Draw Base arc 
            float o = .1f;
            float l = .5f;
            void drawBase()
            {
                this.drawArcXY(gl, -o, -o, 0, l, 0, 1.5708);
                gl.Vertex(-o, -o, 0);
            }

            gl.LineWidth(1.0f);
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
            // End - Draw Base arc --------------------

            // Draw L Wing; ---------------------------
            o = .23f; l = .38f;
            void drawLWing()
            {
                this.drawArcXZ(gl, 0, 0, o, l, 0, 1.5708);
                gl.Vertex(0, 0, o);
                this.drawArcYZ(gl, 0, 0, o, l, 0, 1.5708);
                gl.Vertex(0, 0, o);               
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
            // End - Draw L Wing; ---------------------------

            // Vertical line
            const float vertLen = .8f;
            gl.Color(1f, 1f, 1f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(0f, 0f, 0f);
            gl.Vertex(0, 0f, vertLen);
            gl.End();

            // Draw Points
            gl.PointSize(5.0f);
            gl.Begin(OpenGL.GL_POINTS);

            // Vertical Top WHITE point
            gl.Vertex(0f, 0f, vertLen);

            // Vertical Bottom RED point
            gl.Color(1f, 0f, 0f);
            gl.Vertex(0f, 0f, 0f);
            gl.End();

            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);

            gl.PopAttrib();
            gl.PopMatrix();
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
                     float r, double startAngle, double arcAngle)
        {
            double x, y;
            double end_angle = startAngle + arcAngle;
            double angle_increment = Math.PI / 1000;
            for (double theta = startAngle; theta < end_angle; theta += angle_increment)
            {
                x = r * Math.Cos(theta);
                y = r * Math.Sin(theta);

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
    }
}
