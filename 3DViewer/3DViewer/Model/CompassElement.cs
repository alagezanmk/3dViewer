using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    class CompassElement
        : SceneElement
        , IRenderable
        , ISelectableElement
        , IDraggableElement
    { 
        public class SnapInfo
        {
            public bool Enabled = false;
            public ISelectableElement Element;

            public Vertex Vertex;
            public Vertex Normal;

            public Vertex Object2ScreenPoint(OpenGL gl)
            {
                Vertex screenPoint = gl.Project(this.Vertex);
                return screenPoint;
            }

            public void Transform(OpenGL gl, bool rotateOnly = false)
            {
                this.Transform(gl, (SceneElement)this.Element, rotateOnly);
            }

            public void Transform(OpenGL gl, SceneElement element, bool rotateOnly = false)
            {
                if (null != element.Parent)
                    this.Transform(gl, element.Parent, rotateOnly);

                ISelectableElement se = element as ISelectableElement;
                if (null != se)
                    se.Transform(gl, rotateOnly);
            }
        }

        public SnapInfo Snap = new SnapInfo();

        #region "ISelectable"
        const int margin = 80;
        public Point TopRightMargin = new Point( margin, margin );
        public virtual void Transform(OpenGL gl, bool rotateOnly = false)
        {
            gl.PushMatrix();

            // Start with identify Transform
            gl.LoadIdentity();

            Vertex screenSnapPoint = new Vertex();
            if (this.Snap.Enabled)
            {
                // Transform top Parent to Snapped Element
                this.Snap.Transform(gl);

                // Map Element Snap point to Screen point
                screenSnapPoint = this.Snap.Object2ScreenPoint(gl);
                gl.LoadIdentity();

                // Map Screen point with current compass Transform
                Vertex position = Geometry.UnProject(gl, screenSnapPoint.X, screenSnapPoint.Y, .9f);
                gl.Translate(position.X, position.Y, position.Z);
            }
            else
            {
                gl.Translate(0, 0, -10f); // Move origin to some depth

                // Compass Position Top, Right of View
                var viewport = new int[4];
                gl.GetInteger(OpenGL.GL_VIEWPORT, viewport);

                double screenX = viewport[2] - this.TopRightMargin.X;
                double screenY = viewport[3] - this.TopRightMargin.Y;
                Vertex position = Geometry.UnProject(gl, screenX, screenY, .9f);
                gl.Translate(position.X, position.Y, position.Z);
            }

            // Render size correction for Orthogonal Project
            double scale = 1;
            Scene scene = (Scene)(this.Parent as SceneContainer).ParentScene;
            if (false == scene.Perspective)
                scale = 3.5;

            gl.Scale(scale, scale, scale);

            // Rotate at fixed compass position along origin
            this.Snap.Transform(gl, true);

            if (this.Snap.Enabled)
            {
                // Orient to Normal of selected triangle
                Vertex n = this.Snap.Normal;
                gl.Rotate(90 * n.X, 90 * n.Y, 90 * n.Z);
            }
        }

        public virtual void PopTransform(OpenGL gl)
        {
            gl.PopMatrix();
        }

        const float off = .1f;
        const float len = .5f;
        const float vertLen = .8f;

        public virtual bool HitTest(OpenGL gl, RayCast rayCast)
        {
            bool hit = rayCast.point.X >= -CompassElement.off && rayCast.point.X <= -CompassElement.off + CompassElement.len
                    && rayCast.point.Y >= -CompassElement.off && rayCast.point.Y <= -CompassElement.off + CompassElement.len
                    && rayCast.point.Z >= 0 && rayCast.point.Z <= CompassElement.vertLen;

            return hit;
        }

        bool selected = false;
        bool ISelectableElement.Selected
        {
            get => selected;
            set => selected = value;
        }
        #endregion "ISelectable"

        #region "IDraggable"
        public IDragElementListener DragListener;

        bool dragMode = false;
        Point dragOffSet;

        public void _StartDrag(Control view, Point screenPos)
        {
            double originX = view.ActualWidth - this.TopRightMargin.X;          // OriginX from margin
            this.dragOffSet = new Point(screenPos.X - originX,                  //  offset from mouse pos to origin
                                        screenPos.Y - this.TopRightMargin.Y);    
        }

        public virtual void StartDrag(OpenGL gl, Control view, Point screenPos)
        {
            this._StartDrag(view, screenPos);
            this.dragMode = true;

            this.DragListener?.OnDragElement(view, screenPos, this, DragState.Started);
        }

        public virtual bool Drag(OpenGL gl, Control view, System.Windows.Point screenPos, double cx, double cy)
        {
            if (false == this.dragMode)
                return false;

            if (false == this.DragListener?.OnDragElement(view, screenPos, this, DragState.Dragging))
            {
                this.TopRightMargin.X = view.ActualWidth - screenPos.X + this.dragOffSet.X;
                this.TopRightMargin.Y = screenPos.Y - this.dragOffSet.Y;
            }

            return true;
        }

        public virtual void EndDrag(OpenGL gl, Control view, System.Windows.Point pos)
        {
            this.dragMode = false;
            this.DragListener?.OnDragElement(view, pos, this, DragState.Finished);
        }
        #endregion "IDraggable"

        #region "IRenderable"
        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            //  Push all matrix, attributes, disable lighting and depth testing.
            gl.PushMatrix();
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT
                          | OpenGL.GL_LINE_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);

            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.DepthFunc(OpenGL.GL_ALWAYS);

            this.Transform(gl);

            // Drawing ////////////////////////////////////////////
            // Draw Base arc 
            GLColor lineColor = System.Drawing.Color.White;
            void drawBase()
            {
                this.drawArcXY(gl, -CompassElement.off, -CompassElement.off, 0, CompassElement.len, 0, 1.5708);
                gl.Vertex(-CompassElement.off, -CompassElement.off, 0);
            }

            GLColor fillColor = new GLColor(0f, 128 / 255f, 128 / 255f, 1);
            if (false == this.dragMode)
            {
                if (this.selected)
                    fillColor = System.Drawing.Color.BlueViolet;

                if(this.Snap.Enabled)
                    fillColor = System.Drawing.Color.Aquamarine;

                gl.Color(fillColor);

                gl.LineWidth(1.0f);
                gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                gl.Begin(OpenGL.GL_POLYGON);
                drawBase();
                gl.End();
            }

            gl.Color(lineColor);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
            gl.Begin(OpenGL.GL_POLYGON);
            drawBase();
            gl.End();
            // End - Draw Base arc --------------------

            if (false == this.dragMode)
            { 
                // Draw L Wing; ---------------------------
                float o = .23f, l = .38f;
                void drawLWing()
                {
                    this.drawArcXZ(gl, 0, 0, o, l, 0, 1.5708);
                    gl.Vertex(0, 0, o);
                    this.drawArcYZ(gl, 0, 0, o, l, 0, 1.5708);
                    gl.Vertex(0, 0, o);
                }

                gl.Color(fillColor);            

                gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
                gl.Begin(OpenGL.GL_POLYGON);
                drawLWing();
                gl.End();

                gl.Color(lineColor);
                gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_LINE);
                gl.Begin(OpenGL.GL_POLYGON);
                drawLWing();
                gl.End();
                // End - Draw L Wing; ---------------------------
            }

            // Vertical line
            gl.Color(lineColor);
            gl.Begin(OpenGL.GL_LINES);
            gl.Vertex(0f, 0f, 0f);
            gl.Vertex(0, 0f, CompassElement.vertLen);
            gl.End();

            // Draw Points
            gl.PointSize(5.0f);
            gl.Begin(OpenGL.GL_POINTS);

            // Vertical Top WHITE point
            gl.Vertex(0f, 0f, CompassElement.vertLen);

            // Vertical Bottom RED point
            gl.Color(1f, 0f, 0f);
            gl.Vertex(0f, 0f, 0f);
            gl.End();

            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);

            gl.PopAttrib();
            this.PopTransform(gl);
        }
        #endregion "IRenderable"

        #region "Helper methods"
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
        #endregion "Helper methods"
    }
}
