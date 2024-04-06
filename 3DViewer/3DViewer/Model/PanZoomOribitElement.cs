using System;
using System.Windows;
using System.Windows.Input;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.Model
{
    class PanZoomOribitElement 
        : SceneElement
        , IRenderable
        , ISelectableElement
    {
        double scale = 1.0f;
        Vertex translate = new Vertex();
        Vertex angle = new Vertex();

        Point startMousePos = new Point();
        Vertex startAngle = new Vertex();
        Vertex startTranslate = new Vertex();

        bool panMode = false;
        bool rotateMode = false;

        public void Reset()
        {
            this.scale = 1.0f;
            this.translate = new Vertex(0, 0, 0);
            this.angle = new Vertex(0, 0, 0);
        }

        public void SetRotate(float x, float y, float z)
        {
            this.angle.X = x;
            this.angle.Y = y;
            this.angle.Z = z;
        }

        void pan(bool controlPressed, Point pos, double cx, double cy)
        {
            double diffX = (pos.X - this.startMousePos.X) / cx;
            double diffY = (pos.Y - this.startMousePos.Y) / cy;

            if (controlPressed)
                this.translate.Z = (float)(this.startTranslate.Z + diffY * 50);
            else
            {
                const double m = 10;
                this.translate.X = (float)(this.startTranslate.X - diffX * m);
                this.translate.Y = (float)(this.startTranslate.Y + diffY * m);
            }
        }

        void rotate(bool controlPressed, Point pos, double cx, double cy)
        {
            double diffY = (pos.X - this.startMousePos.X) / cx;
            double diffX = (pos.Y - this.startMousePos.Y) / cy;

            if (controlPressed)
                this.angle.Z = (float)((this.startAngle.Z + diffY * 500) % 360);
            else
            {
                const double m = 150;
                this.angle.X = (float)((this.startAngle.X - diffX * m) % 360);
                this.angle.Y = (float)((this.startAngle.Y - diffY * m) % 360);
            }
        }

        public void RotateTransform(OpenGL gl)
        {
            gl.Rotate(this.angle.X, this.angle.Y, this.angle.Z);
        }

        public void TranslateScaleTransform(OpenGL gl)
        {
            gl.Translate(this.translate.X, this.translate.Y, translate.Z);
            gl.Scale(this.scale, this.scale, this.scale);
        }

        #region "ISelectable"
        public virtual void Transform(OpenGL gl)
        {
            this.TranslateScaleTransform(gl);
            this.RotateTransform(gl);
        }

        public virtual void PopTransform(OpenGL gl)
        {}

        public virtual bool HitTest(OpenGL gl, Vertex pos)
        {
            return false;
        }
        bool ISelectableElement.Selected
        {
            get;
            set;
        }
        #endregion "ISelectable"

        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            this.Transform(gl);
        }

        public void OnMouseDown(Point pos, bool leftButton, bool rightButton)
        {
            this.startMousePos = pos;
            this.startTranslate = this.translate;
            this.startAngle = this.angle;

            this.panMode = leftButton;
            this.rotateMode = rightButton;
        }

        public void OnMouseUp(Point pos, bool leftButton, bool rightButton)
        {
            this.panMode = rightButton;
            this.rotateMode = leftButton;
        }

        public void OnMouseMove(Point pos, double cx, double cy)
        {
            bool controlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (this.rotateMode)
                this.rotate(controlPressed, pos, cx, cy);
            else if (this.panMode)
                this.pan(controlPressed, pos, cx, cy);
        }

        public void OnMouseWheel(int delta)
        {
            double scale = this.scale + delta * 0.001;
            this.scale = Math.Max(0.01, scale);
            this.scale = Math.Min(600, this.scale);
        }
    }
}