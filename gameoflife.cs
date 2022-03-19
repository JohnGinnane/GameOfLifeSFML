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

        public GameOfLife() {
            window = new RenderWindow(new VideoMode((uint)Global.ScreenSize.X, (uint)Global.ScreenSize.Y), "Game Of Life", Styles.Close);
            View view = new View(Global.ScreenSize/2f, Global.ScreenSize);
            window.SetView(view);
            window.SetKeyRepeatEnabled(false);
            window.Closed += window_CloseWindow;
            lastUpdate = DateTime.Now;
            
            window.SetMouseCursor(new Cursor(Cursor.CursorType.SizeAll));

            cs = new CircleShape(100f);
            cs.Origin = new SFML.System.Vector2f(100f, 100f);
            cs.Position = Global.ScreenSize / 2f;
            cs.OutlineColor = Color.Black;
            cs.OutlineThickness = 2f;
            cs.FillColor = Color.Red;
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
                cs.Position = (Vector2f)Global.Mouse.ClickPosition;
            }
        }

        public void draw() {
            window.Clear(Color.Blue);
            
            window.Draw(cs);

            window.Display();
        }
    }
}