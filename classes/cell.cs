using System;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GameOfLifeSFML {
    public class cell {
        public static float Height = 35f;
        public static float Width  = 35f;
        public static float OutlineThickness = 3f;
        public static float Spacing = 0f;
        public static Color OutlineColour = Color.Black;

        private float temperature = 0f;
        private float temperatureWarmUpSpeed = 1 / 100f;
        private float temperatureCoolDownSpeed = 1 / 200f;
        public Color Temperature {
            get { return util.hsvtocol(0, temperature, 1); }
        }

        private bool state;
        public bool State {
            get { return state; }
            set { state = value; }
        }

        public cell(bool state = false) {
            this.state = state;
        }

        public void update(float delta) {
            if (State && temperature < 1f) { temperature = Math.Min(1, temperature + temperatureWarmUpSpeed * delta); }
            if (!State && temperature > 0f) { temperature = Math.Max(0, temperature - temperatureCoolDownSpeed * delta); }
        }

        public bool checkRules(int livingNeighbours) {
            if (State && livingNeighbours >= 2 && livingNeighbours <= 3) {
                return true;
            }

            if (!State && livingNeighbours == 3) {
                return true;
            }

            return false;
        }
    }
}