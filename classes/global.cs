using SFML.Graphics;
using SFML.System;

namespace GameOfLifeSFML {
    public static class Global {            
        private static Vector2f screenSize;
        public static Vector2f ScreenSize {
            get { return screenSize; }
            set { screenSize = value; }
        }

        private static keyboard kb = new keyboard();
        public static keyboard Keyboard {
            get { return kb; }
        }

        private static mouse mouse = new mouse();
        public static mouse Mouse  {
            get { return mouse; }
        }
    }
}