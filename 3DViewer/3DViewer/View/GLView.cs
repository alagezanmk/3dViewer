using System.Drawing;
using System.Windows.Controls;

using SharpGL;
using SharpGL.SceneGraph.Primitives;

using _3DViewer.Model;
using SharpGL.SceneGraph;
using System.Windows.Input;

namespace _3DViewer.View
{
    class GLView
    {
        public _3DViewer.Model.Scene scene = new _3DViewer.Model.Scene();
        STLElement stlModel = new STLElement();

        PanZoomOribitElement panZoomOribit = new PanZoomOribitElement();
        CompassElement compass = new CompassElement();        

        public GLView()
        {
            // Compass
            this.scene.SceneContainer.AddChild(this.compass);
            this.compass.origin = this.panZoomOribit;

            // Pan, Zoom, Oribitor
            this.scene.SceneContainer.AddChild(this.panZoomOribit);
            this.ResetRotate();            

            //  Add test primitives.
            var folder = new Folder() { Name = "Design Primitives" };
            folder.AddChild(new SharpGL.SceneGraph.Primitives.Grid());
            folder.AddChild(new Axies());
            this.scene.SceneContainer.AddChild(folder);

            // World Center
            CenterElement worldCenter = new CenterElement(2, .5f);
            worldCenter.color = Color.Red;
            worldCenter.position.Z += 1.5f;
            this.scene.SceneContainer.AddChild(worldCenter);

            #region "Hit Element - Testing"
            // Mouse Click Postion
            this.scene.SceneContainer.AddChild(this.mousePos);
            this.scene.SceneContainer.AddChild(this.rayCastLine);
            #endregion "Hit Element - Testing"

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
            bool controlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if(controlPressed)
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
            if (null != this.selectedElement)
            {
                IDraggableElement draggableElement = this.selectedElement as IDraggableElement;
                if (null != draggableElement)
                {
                    draggableElement.EndDrag(this.scene.CurrentOpenGLContext, pos);
                    return;
                }
            }

            this.panZoomOribit.OnMouseUp(pos, leftButton, rightButton);
        }

        public void OnMouseMove(System.Windows.Point pos, double cx, double cy)
        {
            if(null != this.selectedElement)
            {
                IDraggableElement draggableElement = this.selectedElement as IDraggableElement;
                if (null != draggableElement)
                {
                    draggableElement.Drag(this.scene.CurrentOpenGLContext, pos, cx, cy);
                    return;
                }
            }

            this.panZoomOribit.OnMouseMove(pos, cx, cy);
        }

        public void OnMouseWheel(int delta)
        {
            this.panZoomOribit.OnMouseWheel(delta);
        }

        public bool _OnMouseDown(OpenGL gl, Control view, System.Windows.Point pos, bool leftButton, bool rightButton)
        {
            this.testHit(gl, view, pos, leftButton);
            if (leftButton && this.hitTest(gl, view, pos.X, pos.Y))
                return true;

            return false;
        }

        ISelectableElement selectedElement;
        bool hitTest(OpenGL gl, Control view, double clientX, double clientY)
        {
            var selections = this.scene.HitTest(gl, view, clientX, clientY);
            if (selections.Count > 0)
            {
                this.selectedElement = selections[0];
                IDraggableElement draggableElement = this.selectedElement as IDraggableElement;
                if (null != draggableElement)
                    draggableElement.StartDrag(this.scene.CurrentOpenGLContext, new System.Windows.Point(clientX, clientY));

                return true;
            }

            this.selectedElement = null;
            return false;

        }

        #region "Hit Element - Testing"
        CenterElement mousePos = new CenterElement(2, .05f);
        RayCastLineElement rayCastLine = new RayCastLineElement();
        void testHit(OpenGL gl, Control view, System.Windows.Point pos, bool leftButton)
        {
            if (leftButton)
            {
                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                gl.LoadIdentity();

                this.panZoomOribit.Transform(gl);

                double[] world;
                double clientY = view.ActualHeight - pos.Y;
                unProject(ref this.mousePos.position, .1);
                unProject(ref this.rayCastLine.position1, 0);

                byte[] pixels = new byte[sizeof(float)];
                gl.ReadPixels((int)pos.X, (int)clientY, 1, 1, OpenGL.GL_DEPTH_COMPONENT, OpenGL.GL_FLOAT, pixels);
                float Z = System.BitConverter.ToSingle(pixels, 0);

                unProject(ref this.rayCastLine.position2, Z);

                void unProject(ref Vertex v, double z)
                {
                    world = gl.UnProject(pos.X, clientY, z);
                    v.X = (float)world[0];
                    v.Y = (float)world[1];
                    v.Z = (float)world[2];
                }
            }
        }
        #endregion "Hit Element - Testing"
    }
}