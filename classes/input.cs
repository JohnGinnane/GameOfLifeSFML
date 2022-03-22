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

    public class mouseButton {
        public string name;
        public int code;
        public bool isPressed;
        public DateTime timePressed;
        public DateTime timeReleased;

        public bool justPressed;
        public bool justReleased;

        public mouseButton(string name, int code) {
            this.name = name;
            this.code = code;
        }
    }

    public class mouse {
        /*
            Left,
            Right,
            Middle,
            XmouseButton1,
            XmouseButton2,
            mouseButtonCount (5)
        */
        private List<mouseButton> mouseButtons;
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

        public mouse() {
            mouseButtons = new List<mouseButton>();

            for (int k = (int)Mouse.Button.Left; k < (int)Mouse.Button.ButtonCount; k++) {
                mouseButton v = new mouseButton(((Mouse.Button)k).ToString(), k);
                mouseButtons.Add(v);
            }
        }

        public void update(RenderWindow window) {
            for (int k = (int)Mouse.Button.Left; k < (int)Mouse.Button.ButtonCount; k++) {                
                bool lastPressed = mouseButtons[k].isPressed;

                mouseButtons[k].justPressed = false;
                mouseButtons[k].justReleased = false;

                mouseButtons[k].isPressed = Mouse.IsButtonPressed((Mouse.Button)k);

                if (!lastPressed && mouseButtons[k].isPressed) {
                    mouseButtons[k].justPressed = true;
                    clickPosition = Position;
                }

                if (lastPressed && !mouseButtons[k].isPressed) {
                    mouseButtons[k].justReleased = true;
                }
            }

            this.position = Mouse.GetPosition(window);
        }

        public mouseButton this[string name] => FindKeyIndex(name);

        private mouseButton FindKeyIndex(string name) {
            mouseButton output;

            output = mouseButtons.Find(x => x.name.ToLower() == name.ToLower());

            if (output == null) {
                return new mouseButton("Unknown", -1);
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

    public static class Input {
        private static keyboard keyboard = new keyboard();
        public static keyboard Keyboard => keyboard;

        private static mouse mouse = new mouse();
        public static mouse Mouse => mouse;
    }
}