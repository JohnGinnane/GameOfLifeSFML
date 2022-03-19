using System;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GameOfLifeSFML {
    public class GameOfLife {
        private RenderWindow window;

        private DateTime lastUpdate;

        public const float timeStep = 1000f / 100f;
        public float timeScale = 1.0f;

        private CircleShape cs;

        public Vector2f lastViewPos;
        public View view;

        public GameOfLife() {
            window = new RenderWindow(new VideoMode((uint)Global.ScreenSize.X, (uint)Global.ScreenSize.Y), "Game Of Life", Styles.Close);
            view = new View(Global.ScreenSize/2f, Global.ScreenSize);
            window.SetView(view);
            window.SetKeyRepeatEnabled(false);
            window.Closed += window_CloseWindow;
            lastUpdate = DateTime.Now;
            
            window.SetMouseCursor(new Cursor(Cursor.CursorType.SizeAll));
            lastViewPos = new Vector2f();

            cs = new CircleShape(100f);
            cs.Origin = new SFML.System.Vector2f(100f, 100f);
            cs.Position = Global.ScreenSize / 2f;
            cs.OutlineColor = Color.Black;
            cs.OutlineThickness = 2f;
            cs.FillColor = Color.Red;

            Global.InitMouse(window);
        }

        public void window_CloseWindow(object sender, EventArgs e) {
            if (sender == null) { return; }
            window.Close();
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

            if (Global.Mouse["left"].justPressed) {
                lastViewPos = view.Center + (Vector2f)Global.Mouse.Position;
            } else
            if (Global.Mouse["left"].isPressed) {
                view.Center = lastViewPos - (Vector2f)Global.Mouse.Position;
                window.SetView(view);
            }
        }

        public void draw() {
            window.Clear(Colour.LightBlue);
            
            window.Draw(cs);

            // draw cursor position text
            Text cursorText = new Text(string.Format("{0}, {1}", Global.Mouse.Position.X, Global.Mouse.Position.Y), Fonts.Arial);
            cursorText.Position = window.MapPixelToCoords(Global.Mouse.Position) + new Vector2f(10, 10);
            cursorText.FillColor = Color.White;
            cursorText.CharacterSize = 12;
            cursorText.OutlineColor = Color.Black;
            cursorText.OutlineThickness = 1f;
            window.Draw(cursorText);

            // draw view center position text
            Text viewCenterText = new Text(string.Format("{0}, {1}", view.Center.X, view.Center.Y), Fonts.Arial);
            viewCenterText.Position = window.MapPixelToCoords(new Vector2i(10, 10));
            viewCenterText.FillColor = Color.White;
            viewCenterText.CharacterSize = 12;
            viewCenterText.OutlineColor = Color.Black;
            viewCenterText.OutlineThickness = 1.0f;
            window.Draw(viewCenterText);
            
            window.Display();
        }
    }
}