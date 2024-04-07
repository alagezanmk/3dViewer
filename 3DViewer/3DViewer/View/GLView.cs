using System.Drawing;
using System.Windows.Controls;
using System.Windows.Input;

using SharpGL;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph;

using _3DViewer.Model;
using SharpGL.SceneGraph.Core;

namespace _3DViewer.View
{
    class GLView : IDragElementListener
    {
        public _3DViewer.Model.Scene scene = new _3DViewer.Model.Scene();
        STLElement stlModelElement = new STLElement();

        PanZoomOribitElement panZoomOribitElement = new PanZoomOribitElement();
        CompassElement compassElement = new CompassElement();        

        public GLView()
        {
            // Pan, Zoom, Oribitor
            this.scene.SceneContainer.AddChild(this.panZoomOribitElement);
            this.ResetRotate();

            //  Add Grid primitives.
            var folder = new Folder() { Name = "Design Primitives" };
            folder.AddChild(new SharpGL.SceneGraph.Primitives.Grid());
            folder.AddChild(new Axies());
            this.panZoomOribitElement.AddChild(folder);

            #region "Hit Element - Testing"
            // ***For testing mouse click point and ray Cast origin and direction
            // World Center 
            CenterElement worldCenter = new CenterElement(2, .5f);
            worldCenter.color = Color.Red;
            worldCenter.position.Z += 1.5f;
            //this.panZoomOribitElement.AddChild(worldCenter);

            // Mouse Click Postion - 
            //this.panZoomOribitElement.AddChild(this.mousePos);     
            //this.panZoomOribitElement.AddChild(this.rayCastLine);  
            #endregion "Hit Element - Testing"

            // STL Model
            this.panZoomOribitElement.AddChild(this.stlModelElement);

            // Compass
            this.scene.SceneContainer.AddChild(this.compassElement);
            this.compassElement.originElement = this.panZoomOribitElement;
            this.compassElement.DragListener = this;
        }

        public void SetModelData(Model.STLData data)
        {
            this.stlModelElement.SetData(data);
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

        public bool ToggleSelectionMode()
        {
            this.stlModelElement.TriangleSelectionMode = !this.stlModelElement.TriangleSelectionMode;
            return this.stlModelElement.TriangleSelectionMode;
        }

        public bool TogglePanMouseMode()
        {
            this.panZoomOribitElement.PanMouseMode = !this.panZoomOribitElement.PanMouseMode;
            return this.panZoomOribitElement.PanMouseMode;
        }

        public void ResetProjection(string direction = "")
        {
            bool controlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            if(controlPressed)
                this.panZoomOribitElement.Reset();

            switch (direction)
            {
            default:
                this.ResetRotate();
                break;

            case "x":
                this.panZoomOribitElement.SetRotate(90, 0, 0);
                break;

            case "y":
                this.panZoomOribitElement.SetRotate(0, 90, 0);
                break;

            case "z":
                this.panZoomOribitElement.SetRotate(0, 0, 90);
                break;
            }
        }

        public void ResetRotate()
        {
            this.panZoomOribitElement.SetRotate(60, 0, 45);
        }

        public void OnMouseDown(OpenGL gl, Control view, System.Windows.Point pos,  bool leftButton, bool rightButton)
        {
            if (this._OnMouseDown(gl, view, pos, leftButton, rightButton))
                return;

            this.panZoomOribitElement.OnMouseDown(pos, leftButton, rightButton);
        }      

        public void OnMouseUp(Control view, System.Windows.Point pos, bool leftButton, bool rightButton)
        {
            if (null != this.selectedElement)
            {
                IDraggableElement draggableElement = this.selectedElement as IDraggableElement;
                if (null != draggableElement)
                {
                    draggableElement.EndDrag(this.scene.CurrentOpenGLContext, view, pos);
                    return;
                }
            }

            this.panZoomOribitElement.OnMouseUp(pos, leftButton, rightButton);
        }

        public void OnMouseMove(Control view, System.Windows.Point pos, double cx, double cy)
        {
            if(null != this.selectedElement)
            {
                IDraggableElement draggableElement = this.selectedElement as IDraggableElement;
                if (null != draggableElement)
                {
                    if(draggableElement.Drag(this.scene.CurrentOpenGLContext, view, pos, cx, cy))
                        return;
                }
            }

            this.panZoomOribitElement.OnMouseMove(pos, cx, cy);
        }

        public void OnMouseWheel(int delta)
        {
            this.panZoomOribitElement.OnMouseWheel(delta);
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
                // Unselect back elements
                for (int i = 0; i < selections.Count - 1; i++)
                    selections[i].Selected = false;

                // Select last element
                this.selectedElement = selections[selections.Count - 1];

                // Start Draggable element
                IDraggableElement draggableElement = this.selectedElement as IDraggableElement;
                if (null != draggableElement)
                    draggableElement.StartDrag(this.scene.CurrentOpenGLContext, view, 
                                               new System.Windows.Point(clientX, clientY));

                return true;
            }

            this.selectedElement = null;
            return false;

        }

        #region "IDragListenElement" 
        public virtual bool OnDragElement(Control view, System.Windows.Point clientPos, 
                                          SceneElement element, DragState state)
        {
            if (false == this.stlModelElement.DataReady)
                return false;

            if (state != DragState.Dragging)
                return false;

            OpenGL gl = this.scene.CurrentOpenGLContext;
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            this.panZoomOribitElement.Transform(gl);
            this.stlModelElement.Transform(gl);

            Ray ray = Geometry.CreateRay(view, clientPos.X, clientPos.Y);
            Geometry.MapRayPointsToModel(ray, gl);

            Vertex normalIntersectionPoint = new Vertex();
            Vertex intersectionPoint = new Vertex();
            bool hit = this.stlModelElement.TriangleHitTest(gl, ray, ref intersectionPoint, ref normalIntersectionPoint);
            if (hit)
            {
                this.compassElement.Snap.Enabled = true;
                this.compassElement.Snap.ParentElements.Clear();
                this.compassElement.Snap.ParentElements.Add(this.panZoomOribitElement);
                this.compassElement.Snap.ParentElements.Add(this.stlModelElement);

                this.compassElement.Snap.Vertex = intersectionPoint;
                this.compassElement.Snap.Normal = normalIntersectionPoint;
            }
            else
            {
                if (this.compassElement.Snap.Enabled)
                {
                    this.compassElement.TopRightMargin.X = view.ActualWidth - clientPos.X;
                    this.compassElement.TopRightMargin.Y = view.ActualHeight - clientPos.Y;
                    this.compassElement.Snap.Enabled = false;
                    this.compassElement.Snap.ParentElements.Clear();
                }
            }

            return false;
        }
        #endregion "IDragListenElement"

        #region "Hit Element - Testing"
        CenterElement mousePos = new CenterElement(2, .05f);
        RayCastLineElement rayCastLine = new RayCastLineElement();
        void testHit(OpenGL gl, Control view, System.Windows.Point pos, bool leftButton)
        {
            if (leftButton)
            {
                gl.MatrixMode(OpenGL.GL_MODELVIEW);
                gl.LoadIdentity();

                this.panZoomOribitElement.Transform(gl);

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