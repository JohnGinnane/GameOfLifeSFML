using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GameOfLifeSFML {
    public class GameOfLife {
        private RenderWindow window;

        private DateTime lastUpdate;

        public const float timeStep = 1000f / 100f;
        public float timeScale = 1.0f;

        List<Drawable> shapes = new List<Drawable>();

        public Vector2f lastViewPos;
        public View gridView;

        public View interfaceView;

        public float GridZoom {
            get {
                return gridView.Size.X / Global.ScreenSize.X;
            }
        }
        
        private const float scrollSpeed = 50f;

        public GameOfLife() {
            window = new RenderWindow(new VideoMode((uint)Global.ScreenSize.X, (uint)Global.ScreenSize.Y), "Game Of Life", Styles.Close);
            interfaceView = new View(Global.ScreenSize/2f, Global.ScreenSize);
            gridView = new View(Global.ScreenSize/2f, Global.ScreenSize);
            
            window.SetKeyRepeatEnabled(false);
            window.Closed += window_CloseWindow;
            lastUpdate = DateTime.Now;
            
            lastViewPos = new Vector2f();
            
            makeRandomShapes();
            window.MouseWheelScrolled += mouseWheelScrolled;
        }

        public void window_CloseWindow(object sender, EventArgs e) {
            if (sender == null) { return; }
            window.Close();
        }

        private void mouseWheelScrolled(object sender, MouseWheelScrollEventArgs e) {
            if (e.Delta < 0) {
                gridView.Zoom(1 - 0.001f * e.Delta * scrollSpeed);
            } else
            if (e.Delta > 0) {
                zoomViewToMouse(1 - 0.001f * e.Delta * scrollSpeed);
            }
        }

        public void run() {
            while (window.IsOpen) {
                if (!window.HasFocus()) { continue; }

                if ((float)(DateTime.Now - lastUpdate).TotalMilliseconds >= timeStep) {
                    float delta = timeStep * timeScale;
                    lastUpdate = DateTime.Now;

                    window.DispatchEvents();
                    update(delta);
                }

                draw();
            }
        }

        public void update(float delta) {
            Global.Keyboard.update();
            Global.Mouse.update(window);
            
            if (Global.Keyboard["escape"].isPressed) {
                window.Close();
            }

            handleCamera(delta);
        }

        public void draw() {
            window.Clear(Colour.LightBlue);
            window.SetView(gridView);
            
            foreach(Drawable d in shapes) {
                window.Draw(d);
            }

            window.SetView(interfaceView);

            // // draw cursor position text
            // Text cursorText = new Text(string.Format("{0}, {1}", Global.Mouse.Position.X, Global.Mouse.Position.Y), Fonts.Arial);
            // cursorText.Position = window.MapPixelToCoords(Global.Mouse.Position) + new Vector2f(10, 10);
            // cursorText.FillColor = Color.White;
            // cursorText.CharacterSize = 12;
            // cursorText.OutlineColor = Color.Black;
            // cursorText.OutlineThickness = 1f;
            // window.Draw(cursorText);

            // // draw cursor world position text
            // cursorText.DisplayedString = string.Format("{0}, {1}",
            //                                            window.MapPixelToCoords(Global.Mouse.Position, gridView).X,
            //                                            window.MapPixelToCoords(Global.Mouse.Position, gridView).Y);
            // cursorText.Position = window.MapPixelToCoords(Global.Mouse.Position) + new Vector2f(10, 24);
            // window.Draw(cursorText);

            // // draw view center position text
            // Text viewCenterText = new Text(string.Format("{0}, {1}", gridView.Center.X, gridView.Center.Y), Fonts.Arial);
            // viewCenterText.Position = window.MapPixelToCoords(new Vector2i(10, 10));
            // viewCenterText.FillColor = Color.White;
            // viewCenterText.CharacterSize = 12;
            // viewCenterText.OutlineColor = Color.Black;
            // viewCenterText.OutlineThickness = 1.0f;
            // window.Draw(viewCenterText);
            
            // // draw view size text
            // Text viewSizeText = new Text(string.Format("Grid View Size: {0}, {1}", gridView.Size.X, gridView.Size.Y), Fonts.Arial);
            // viewSizeText.Position = window.MapPixelToCoords(new Vector2i(10, 24));
            // viewSizeText.FillColor = Color.White;
            // viewSizeText.CharacterSize = 12;
            // viewSizeText.OutlineColor = Color.Black;
            // viewSizeText.OutlineThickness = 1.0f;
            // window.Draw(viewSizeText);

            window.Display();
        }

        private void handleCamera(float delta) {
            // Zooming the camera            
            if (Global.Keyboard["q"].isPressed) {
                gridView.Zoom(1 + 0.001f * delta);
            }

            if (Global.Keyboard["e"].isPressed) {
                gridView.Zoom(1 - 0.001f * delta);
            }

            float cameraSprintMulti = 1.0f;

            if (Global.Keyboard["lshift"].isPressed) {
                cameraSprintMulti *= 2f;
            }

            // Panning the camera
            if (Global.Keyboard["w"].isPressed || Global.Keyboard["up"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X, gridView.Center.Y - 1 * delta * GridZoom * cameraSprintMulti);
            }
            
            if (Global.Keyboard["a"].isPressed || Global.Keyboard["left"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X - 1 * delta * GridZoom * cameraSprintMulti, gridView.Center.Y);
            }
            
            if (Global.Keyboard["s"].isPressed || Global.Keyboard["down"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X, gridView.Center.Y + 1 * delta * GridZoom * cameraSprintMulti);
            }
            
            if (Global.Keyboard["d"].isPressed || Global.Keyboard["right"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X + 1 * delta * GridZoom * cameraSprintMulti, gridView.Center.Y);
            }

            if (Global.Mouse["right"].justPressed) {
                lastViewPos = gridView.Center + (Vector2f)Global.Mouse.Position * GridZoom;
            } else
            if (Global.Mouse["right"].isPressed) {
                window.SetMouseCursor(new Cursor(Cursor.CursorType.SizeAll));
                gridView.Center = lastViewPos - (Vector2f)Global.Mouse.Position * GridZoom;
            } else
            if (Global.Mouse["right"].justReleased) {
                window.SetMouseCursor(new Cursor(Cursor.CursorType.Arrow));
            }
        }

        private void zoomViewToMouse(float zoomFactor) {
            gridView.Move(((Vector2f)Global.Mouse.Position - Global.ScreenSize / 2f) / 10f * GridZoom);
            gridView.Zoom(zoomFactor);
        }

        private void makeRandomShapes() {
            int numShapes = 20;
            while (numShapes > 0) {
                switch (util.randint(0, 1)) {
                    case 0:
                        CircleShape cs = new CircleShape(util.randfloat(20, 80));
                        cs.Origin = new Vector2f(cs.Radius, cs.Radius);
                        cs.FillColor = util.hsvtocol(util.randfloat(0, 360), 1, 1);
                        cs.OutlineColor = Color.Black;
                        cs.OutlineThickness = 2f;
                        cs.Position = util.randvec2(-1000, 1000);
                        shapes.Add(cs);
                        break;
                    case 1:
                        RectangleShape rs = new RectangleShape(util.randvec2(20, 80));
                        rs.FillColor = util.hsvtocol(util.randfloat(0, 360), 1, 1);
                        rs.OutlineColor = Color.Black;
                        rs.OutlineThickness = 2f;
                        rs.Position = util.randvec2(-1000, 1000);
                        shapes.Add(rs);
                        break;
                }
                numShapes--;
            }
        }
    }
}