using _3DViewer.Model;
using SharpGL;
using System;
using System.Windows;
using System.Windows.Input;

namespace _3DViewer.View
{
    class GLCamera
    {
        public double scale = 1.0f;
        public float translateX = 0, translateY = 0, translateZ = 0;
        public float angleX = 0, angleY = 0, angleZ = 0;

        public void RotateTransform(OpenGL gl)
        {
            gl.Rotate(this.angleX, 1f, 0f, 0f);
            gl.Rotate(this.angleY, 0f, 1f, 0f);
            gl.Rotate(this.angleZ, 0f, 0f, 1f);
        }

        public void Transform(OpenGL gl)
        {
            const float ScaleFactor = 1.0f;
            gl.Translate(this.translateX, this.translateY, translateZ);

            this.RotateTransform(gl);
            gl.Scale( this.scale * ScaleFactor,
                      this.scale * ScaleFactor,
                      this.scale * ScaleFactor);
        }

        Vector3 mouseStartPos = new Vector3();

        bool pan = false;
        Vector3 startTranslate = new Vector3();

        bool rotate = false;
        Vector3 startRotation = new Vector3();

        public void OnMouseDown(Point pos, bool leftButton, bool rightButton)
        {
            
            this.mouseStartPos.x = (float)pos.X;
            this.mouseStartPos.y = (float)pos.Y;

            this.pan = leftButton;
            this.startTranslate.x = this.translateX;
            this.startTranslate.y = this.translateY;
            this.startTranslate.z = this.translateZ;

            this.rotate = rightButton;
            this.startRotation.x = this.angleX;
            this.startRotation.y = this.angleY;
            this.startRotation.z = this.angleZ;            
        }

        public void OnMouseUp(Point pos, bool leftButton, bool rightButton)
        {
            this.pan = leftButton;
            this.rotate = rightButton;
        }

        public void OnMouseMove(Point pos, double cx, double cy)
        {
            bool controlKey = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if (this.rotate)
            {
                double diffY = pos.X - this.mouseStartPos.x;
                double diffX = pos.Y - this.mouseStartPos.y;

                if (controlKey)
                    this.angleZ = (float)((this.startRotation.z + diffY + 720) % 360);
                else
                {
                    this.angleX = (float)((this.startRotation.x + diffX + 720) % 360);
                    this.angleY = (float)((this.startRotation.y + diffY + 720) % 360);
                }
            }
            else if (this.pan)
            {
                double diffX = (pos.X - this.mouseStartPos.x) / cx;
                double diffY = (pos.Y - this.mouseStartPos.y) / cy;

                const double m = 8;
                if (controlKey)
                    this.translateZ = (float)(this.startTranslate.z - diffY * m * 5);
                else
                {
                    this.translateY = (float)(this.startTranslate.y - diffY * m);
                    this.translateX = (float)(this.startTranslate.x + diffX * m);
                }
            }
        }

        public void OnMouseWheel(int delta)
        {
            double scale = this.scale + delta * 0.001;
            this.scale = Math.Max(0.1, scale);
            this.scale = Math.Min(10, this.scale);
        }
    }
}
