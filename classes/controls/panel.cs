using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GameOfLifeSFML {
    public class panel : control {
        private List<control> children = new List<control>();
        public List<control> Children => children;

        private bool draggable = true;
        public bool Draggable {
            get { return draggable; }
            set { draggable = value; }
        }

        private Vector2f mouseClickOffset;

        public panel() {
            FillColour = new Color(100, 100, 100, 100);
            Position = new Vector2f(10, 10);
            Size = new Vector2f(200, 100);
        }

        public override void draw(RenderWindow window)
        {
            base.draw(window);

            foreach (control c in children) {
                Vector2f ogPos = c.Position;
                c.Position = Position + c.Position;
                c.draw(window);
                c.Position = ogPos;
            }
        }

        public override void Control_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            base.Control_MouseButtonPressed(sender, e);

            if (MouseHovering && e.Button == Mouse.Button.Left) {
                mouseClickOffset = Position - new Vector2f(e.X, e.Y);
            }
        }

        public override void Control_MouseMoved(object sender, MouseMoveEventArgs e)
        {
            base.Control_MouseMoved(sender, e);
            
            if (Draggable && mousePressing) {
                if (/*Dimensions.Contains(e.X, e.Y) &&*/ mousePressing) {
                    Position = mouseClickOffset + new Vector2f(e.X, e.Y);
                }
            }
        }

        // Adds the control to this panel
        // converts the position automatically
        public void add(control c) {
            children.Add(c);
            // make child relative to this panel
            c.Position = c.Position - Position;
        }
    }
}