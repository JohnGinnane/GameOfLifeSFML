using System;
using SFML.System;
using SFML.Graphics;

namespace GameOfLifeSFML {
    public class slider : control {
        private float minimumValue = 0f;
        public float MinimumValue {
            get { return minimumValue; }
            set { minimumValue = value; }
        }

        private float value;
        public float Value {
            get { return value; }
            set { this.value = value; }
        }

        private float maximumValue = 1f;
        public float MaximumValue {
            get { return maximumValue; }
            set { maximumValue = value; }
        }

        public slider() {
            dimensions = new FloatRect(0, 0, 50, 10);
        }

        public override void draw(RenderWindow window) {
            base.draw(window);

            // the slider bar
            RectangleShape sliderInner = new RectangleShape();
            sliderInner.Position = Position + new Vector2f(Size.X * 0.05f, Size.Y / 2f - 1);
            sliderInner.Size = new Vector2f(Size.X * 0.9f, 2);
            sliderInner.FillColor = Colour.Grey;
            window.Draw(sliderInner);

            // the slider control
            RectangleShape sliderController = new RectangleShape();
            sliderController.Size = new Vector2f(8, 20);
            sliderController.Position = sliderInner.Position - sliderController.Size / 2f;
            sliderController.FillColor = Colour.DarkGrey;
            window.Draw(sliderController);
        }
    }
}