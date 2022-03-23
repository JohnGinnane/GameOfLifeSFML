using System;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace GameOfLifeSFML {
    public class slider : control {
        public delegate void SliderValueChangedEventHandler(object sender, EventArgs e);
        public SliderValueChangedEventHandler SliderValueChanged;
        public override FloatRect Dimensions {
            get { return dimensions; }
            set {
                dimensions = value;
                findSliderMinMax();
            }
        }
        
        public override Vector2f Position {
            get { return new Vector2f(dimensions.Left, dimensions.Top); }
            set {
                dimensions.Left = value.X;
                dimensions.Top = value.Y;
                findSliderMinMax();
            }
        }

        public override Vector2f Size {
            get { return new Vector2f(dimensions.Width, dimensions.Height); }
            set {
                dimensions.Width = value.X;
                dimensions.Height = value.Y;
                findSliderMinMax();
            }
        }

        protected FloatRect sliderControllerDimensions;
        public FloatRect SliderControllerDimensions {
            get { return new FloatRect(sliderMinValueX + (sliderMaxValueX - sliderMinValueX) * Value,
                                       Position.Y + Size.Y / 2f - sliderControllerDimensions.Height / 2f,
                                       sliderControllerDimensions.Width,
                                       sliderControllerDimensions.Height); }
            set {
                // Need to calculate the X position of the start and end over the slider
                sliderControllerDimensions = value;
                findSliderMinMax();
            }
        }

        protected bool mouseHoveringOverSlider = false;
        public bool MouseHoveringOverSlider => mouseHoveringOverSlider;

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

        private float sliderMinValueX;
        private float sliderMaxValueX;

        public slider() {
            SliderControllerDimensions = new FloatRect(0, 0, 8, 20);
        }

        public override void draw(RenderWindow window) {
            
            RectangleShape rs = new RectangleShape();
            rs.Position = Position;
            rs.Size = Size;
            rs.OutlineColor = Color.Black;
            rs.OutlineThickness = OutlineThickness;

            if (MouseHoveringOverSlider) {
                rs.OutlineThickness *= 2f;
            }

            window.Draw(rs);
            // the slider bar
            RectangleShape sliderInner = new RectangleShape();
            sliderInner.Position = Position + new Vector2f(Size.X * 0.05f, Size.Y / 2f - 1);
            sliderInner.Size = new Vector2f(Size.X * 0.9f, 2);
            sliderInner.FillColor = Colour.Grey;
            window.Draw(sliderInner);

            // the slider control
            RectangleShape sliderController = new RectangleShape();
            sliderController.Size = new Vector2f(SliderControllerDimensions.Width, SliderControllerDimensions.Height);
            sliderController.Position = new Vector2f(SliderControllerDimensions.Left, SliderControllerDimensions.Top);
            //sliderController.FillColor = Colour.DarkGrey;
            sliderController.FillColor = new Color(200, 200, 200, 200);
            window.Draw(sliderController);
        }

        // If the control dimensions change then we need to recalculate
        // the X coordinate of the start of the slider bar, and
        // the X coordinate of the end of the slider bar
        private void findSliderMinMax() {
            sliderMinValueX = Position.X + Size.X * 0.05f - SliderControllerDimensions.Width / 2f;
            sliderMaxValueX = Position.X + Size.X * 0.95f - SliderControllerDimensions.Width / 2f;
        }

        public override void Control_MouseMoved(object sender, MouseMoveEventArgs e) {
            base.Control_MouseMoved(sender, e);

            if (SliderControllerDimensions.Contains(e.X, e.Y)) {
                mouseHoveringOverSlider = true;
            } else {
                mouseHoveringOverSlider = false;
            }
            
            if (mousePressing) {
                float newValue = (e.X - sliderMinValueX - SliderControllerDimensions.Width / 2f) / (sliderMaxValueX - sliderMinValueX);
                newValue = Math.Clamp(newValue, 0, 1);

                if (newValue != Value) {
                    Value = newValue;
                    this.SliderValueChanged?.Invoke(this, null);
                }
            }
        }
        
        public override void Control_MouseButtonPressed(object sender, MouseButtonEventArgs e) {
            if (MouseHoveringOverSlider) {
                mousePressing = true;
            }
        }

        public override void Control_MouseButtonReleased(object sender, MouseButtonEventArgs e) {
            mousePressing = false;
        }
    }
}