using System.Drawing;
using System.Windows.Controls;

using SharpGL;
using SharpGL.SceneGraph.Primitives;

using _3DViewer.Model;

namespace _3DViewer.View
{
    class GLView
    {
        public _3DViewer.Model.Scene scene = new _3DViewer.Model.Scene();
        STLElement stlModel = new STLElement();

        PanZoomOribitElement panZoomOribit = new PanZoomOribitElement();

        CompassElement compass = new CompassElement();        
        CenterElement mousePos = new CenterElement(2, .05f);

        public GLView()
        {
            // Compass
            this.scene.SceneContainer.AddChild(this.compass);

            // Pan, Zoom, Oribitor
            this.compass.origin = this.panZoomOribit;

            this.scene.SceneContainer.AddChild(this.panZoomOribit);
            this.ResetRotate();            

            //  Add test primitives.
            var folder = new Folder() { Name = "Design Primitives" };
            folder.AddChild(new SharpGL.SceneGraph.Primitives.Grid());
            folder.AddChild(new Axies());
            this.scene.SceneContainer.AddChild(folder);

            // World Center
            CenterElement worldCenter = new CenterElement(2, 1f);
            worldCenter.color = Color.Red;
            worldCenter.position.Z += .5f;
            this.scene.SceneContainer.AddChild(worldCenter);

            // Mouse Click Postion
            this.scene.SceneContainer.AddChild(this.mousePos);

            // STL Model
            this.scene.SceneContainer.AddChild(this.stlModel);
        }   
     
        public void SetModelData(Model.STLData data)
        {
            this.stlModel.SetData(data);
        }

        public void InitializeGL(OpenGL gl, Control view)
        {
            this.scene.InitializeGL(gl, view);
        }

        public void OnResize(OpenGL gl, Control view)
        {
            this.scene.Resize(gl, view);
        }

        public void OnDraw(OpenGL gl, Control view)
        {
            this.scene.Draw(gl, null);
        }

        public bool ToggleProjection()
        {
            this.scene.Perspective = !this.scene.Perspective;
            this.scene.InitializeProjection();
            return this.scene.Perspective;
        }

        public void ResetProjection(string direction = "")
        {
            this.panZoomOribit.Reset();
            switch (direction)
            {
            default:
                this.ResetRotate();
                break;

            case "x":
                this.panZoomOribit.SetRotate(90, 0, 0);
                break;

            case "y":
                this.panZoomOribit.SetRotate(0, 90, 0);
                break;

            case "z":
                this.panZoomOribit.SetRotate(0, 0, 90);
                break;
            }
        }

        public void ResetRotate()
        {
            this.panZoomOribit.SetRotate(60, 0, 45);
        }

        public void OnMouseDown(OpenGL gl, Control view, System.Windows.Point pos,  bool leftButton, bool rightButton)
        {
            if (this._OnMouseDown(gl, view, pos, leftButton, rightButton))
                return;

            this.panZoomOribit.OnMouseDown(pos, leftButton, rightButton);
        }      

        public void OnMouseUp(System.Windows.Point pos, bool leftButton, bool rightButton)
        {
            this.panZoomOribit.OnMouseUp(pos, leftButton, rightButton);
        }

        public void OnMouseMove(System.Windows.Point pos, double cx, double cy)
        {
            this.panZoomOribit.OnMouseMove(pos, cx, cy);
        }

        public void OnMouseWheel(int delta)
        {
            this.panZoomOribit.OnMouseWheel(delta);
        }

        public bool _OnMouseDown(OpenGL gl, Control view, System.Windows.Point pos, bool leftButton, bool rightButton)
        {
            if (leftButton)
            {
                gl.LoadIdentity();
                this.panZoomOribit.Transform(gl);

                double[] s = gl.UnProject(pos.X, view.ActualHeight - pos.Y, .1);
                this.mousePos.position.X = (float)s[0];
                this.mousePos.position.Y = (float)s[1];
                this.mousePos.position.Z = (float)s[2];

                //if (this.hitTest(view, gl, pos.X, pos.Y))
                //   return;
            }

            var hitItems = this.scene.DoHitTest((int)pos.X, (int)pos.Y);
            foreach (var item in hitItems)
            {
                return true;
            }

            return false;
        }

        bool hitTest(Control view, OpenGL gl, double x, double y)
        {
            //return false;
            const int BUFFER_LENGTH = 64;
            uint[] selectBuff = new uint[BUFFER_LENGTH];

            int hits;
            int[] viewport = new int[4];

            bool success = false;
            do
            {
                gl.SelectBuffer(BUFFER_LENGTH, selectBuff);
                gl.GetInteger(OpenGL.GL_VIEWPORT, viewport);

                gl.RenderMode(OpenGL.GL_SELECT);
                gl.MatrixMode(OpenGL.GL_PROJECTION);
                gl.PushMatrix();
                gl.LoadIdentity();

                // Change render mode
                gl.InitNames();
                gl.PushName(0);

                gl.PickMatrix(x, viewport[3] - y, .2, .2, viewport);
                this.scene.InitializeProjection();

                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                this.OnDraw(gl, view);

                hits = gl.RenderMode(OpenGL.GL_RENDER);
                this.scene.InitializeProjection();
                if (hits > 0)
                {
                    uint count = selectBuff[0];
                    if (count > 0)
                    {
                        uint id = selectBuff[3];
                        success = true;
                    }
                }

            } while (false);

            return success;
        }
    }
}