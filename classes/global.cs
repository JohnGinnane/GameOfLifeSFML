using SFML.Graphics;
using SFML.System;

namespace GameOfLifeSFML {
    public static class Global {            
        private static Vector2f screenSize;
        public static Vector2f ScreenSize {
            get { return screenSize; }
            set { screenSize = value; }
        }
    }
}