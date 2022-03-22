using System;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace GameOfLifeSFML {
    public abstract class control {
        public delegate void ClickedEventHandler(object sender, EventArgs e);
        public ClickedEventHandler Click;

        protected FloatRect dimensions = new FloatRect(0, 0, 50, 10);
        public FloatRect Dimensions {
            get { return dimensions; }
            set { dimensions = value; }
        }
        
        public Vector2f Position {
            get { return new Vector2f(dimensions.Left, dimensions.Top); }
            set {
                dimensions.Left = value.X;
                dimensions.Top = value.Y;
            }
        }

        public Vector2f Size {
            get { return new Vector2f(dimensions.Width, dimensions.Height); }
            set {
                dimensions.Width = value.X;
                dimensions.Height = value.Y;
            }
        }

        protected float outlineThickness = 1f;
        public float OutlineThickness {
            get { return outlineThickness; }
            set { outlineThickness = value; }
        }

        // The colour of a standard button
        protected Color fillColour;
        public Color FillColour {
            get { return fillColour; }
            set { fillColour = value; }
        }

        protected bool mouseHovering = false;
        public bool MouseHovering => mouseHovering;

        protected bool mousePressing = false;
        public bool MousePressing => mousePressing;

        public virtual void draw(RenderWindow window) {            
            RectangleShape rs = new RectangleShape();
            rs.Position = Position;
            rs.Size = Size;
            rs.OutlineColor = Color.Black;
            rs.OutlineThickness = OutlineThickness;

            if (MouseHovering) { rs.OutlineThickness *= 2f; }

            window.Draw(rs);
        }

        public virtual void Control_MouseMoved(object sender, MouseMoveEventArgs e) {
            if (Dimensions.Contains(e.X, e.Y)) {
                mouseHovering = true;
            } else {
                mouseHovering = false;
            }
        }

        public virtual void Control_MouseButtonPressed(object sender, MouseButtonEventArgs e) {
            if (MouseHovering) {
                mousePressing = true;
            }
        }

        public virtual void Control_MouseButtonReleased(object sender, MouseButtonEventArgs e) {
            // only register a "click" if we started the click on this control
            if (MouseHovering && MousePressing) {
                this.Click?.Invoke(sender, e);
            }
            
            mousePressing = false;
        }
    }
}