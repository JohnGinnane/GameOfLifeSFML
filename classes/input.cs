using SFML.System;
using SFML.Window;
using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace GameOfLifeSFML {
    public class key {
        public string name;
        public int code;
        public bool isPressed;
        public DateTime timePressed;
        public DateTime timeReleased;

        public bool justPressed;
        public bool justReleased;

        public key(string name, int code) {
            this.name = name;
            this.code = code;
        }
    }

    public class button {
        public string name;
        public int code;
        public bool isPressed;
        public DateTime timePressed;
        public DateTime timeReleased;

        public bool justPressed;
        public bool justReleased;

        public button(string name, int code) {
            this.name = name;
            this.code = code;
        }
    }

    public class mouse {
        /*
            Left,
            Right,
            Middle,
            XButton1,
            XButton2,
            ButtonCount (5)
        */
        private List<button> buttons;
        private Vector2i position;
        public Vector2i Position {
            get { return this.position; }
            set {
                Mouse.SetPosition((Vector2i)value);
                this.position = value;
            }
        }

        private Vector2i clickPosition;
        public Vector2i ClickPosition {
            get { return clickPosition; }
            set { clickPosition = value; }
        }

        private float scroll;

        public bool JustScrolledUp => scroll > 0;
        public bool JustScrolledDown => scroll < 0;

        public mouse(RenderWindow window) {
            buttons = new List<button>();

            for (int k = (int)Mouse.Button.Left; k < (int)Mouse.Button.ButtonCount; k++) {
                button v = new button(((Mouse.Button)k).ToString(), k);
                buttons.Add(v);
            }

            window.MouseWheelScrolled += mouseWheelScrolled;
        }

        private void mouseWheelScrolled(object sender, MouseWheelScrollEventArgs e) {
            if (e.Wheel == Mouse.Wheel.VerticalWheel) {
                scroll += e.Delta;
            }
        }

        public void update(RenderWindow window) {
            for (int k = (int)Mouse.Button.Left; k < (int)Mouse.Button.ButtonCount; k++) {
                
                bool lastPressed = buttons[k].isPressed;

                buttons[k].justPressed = false;
                buttons[k].justReleased = false;

                buttons[k].isPressed = Mouse.IsButtonPressed((Mouse.Button)k);

                if (!lastPressed && buttons[k].isPressed) {
                    buttons[k].justPressed = true;
                    clickPosition = Position;
                }

                if (lastPressed && !buttons[k].isPressed) {
                    buttons[k].justReleased = true;
                }
            }

            this.position = Mouse.GetPosition(window);

            if (scroll != 0) {
                if (Math.Abs(scroll) == 1) {
                    scroll -= scroll / 2f;
                } else {
                    scroll -= scroll;
                }
            }
        }

        public button this[string name] => FindKeyIndex(name);

        private button FindKeyIndex(string name) {
            button output;

            output = buttons.Find(x => x.name.ToLower() == name.ToLower());

            if (output == null) {
                return new button("Unknown", -1);
            }

            return output;
        }
    }

    public class keyboard {
        private List<key> keys;

        public keyboard() {
            keys = new List<key>();

            for (int k = (int)Keyboard.Key.A; k < (int)Keyboard.Key.KeyCount; k++) {
                key v = new key(((Keyboard.Key)k).ToString(), k);
                keys.Add(v);
            }
        }

        public void update() {
            for (int k = (int)Keyboard.Key.A; k < (int)Keyboard.Key.KeyCount; k++) {
                
                bool lastPressed = keys[k].isPressed;

                keys[k].justPressed = false;
                keys[k].justReleased = false;
                
                keys[k].isPressed = Keyboard.IsKeyPressed((Keyboard.Key)k);

                if (!lastPressed && keys[k].isPressed) {
                    keys[k].justPressed = true;
                }

                if (lastPressed && !keys[k].isPressed) {
                    keys[k].justReleased = true;
                }
            }
        }

        public key this[string name] => FindKeyIndex(name);

        private key FindKeyIndex(string name) {
            key output;

            output = keys.Find(x => x.name.ToLower() == name.ToLower());

            if (output == null) {
                return new key("Unknown", -1);
            }

            return output;
        }
    }
}