using System;
using SFML.System;
using SFML.Graphics;

namespace GameOfLifeSFML {
    public delegate void ButtonClickedEventHandler(object sender, EventArgs e);

    public class button {
        private FloatRect dimensions = new FloatRect(0, 0, 50, 10);
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

        private bool isToggle = false;
        public bool IsToggle {
            get { return isToggle; }
            set { isToggle = value; }
        }

        private bool toggleState = false;
        public bool ToggleState => toggleState;

        private string text = "Sample Text";
        public string Text {
            get { return text; }
            set { text = value; }
        }

        private uint characterSize = 12;
        public uint CharacterSize {
            get { return characterSize; }
            set { characterSize = value; }
        }

        private float outlineThickness = 1f;
        public float OutlineThickness {
            get { return outlineThickness; }
            set { outlineThickness = value; }
        }

#region "Colours"
        // The colour of a standard button
        private Color fillColour;
        public Color FillColour {
            get { return fillColour; }
            set { fillColour = value; }
        }

        // The colour of a toggle button whose state is off
        private Color toggleOffColour;
        public Color ToggleOffColour {
            get { return toggleOffColour; }
            set { toggleOffColour = value; }
        }

        // The colour of a toggle button whose state is on
        private Color toggleOnColour;
        public Color ToggleOnColour {
            get { return toggleOnColour; }
            set { toggleOnColour = value;}
        }

        // The colour of a standard button when hovering over it
        private Color hoverColour;
        public Color HoverColour {
            get { return hoverColour; }
            set { hoverColour = value; }
        }

        // The colour when pressing the standard button
        private Color pressColour;
        public Color PressColour {
            get { return pressColour; }
            set { pressColour = value; }
        }

        // The colour when pressing a toggle button whose state will go on
        private Color pressToggleOnColour;
        public Color PressToggleOnColour {
            get { return pressToggleOnColour; }
            set { pressToggleOnColour = value; }
        }

        // The colour when pressing a toggle button whose state will go off
        private Color pressToggleOffColour;
        public Color PressToggleOffColour {
            get { return pressToggleOffColour; }
            set { pressToggleOffColour = value; }
        }
#endregion

        private bool mouseHovering = false;
        public bool MouseHovering {
            get { return mouseHovering; }
            set { mouseHovering = value; }
        }

        private bool mousePressing = false;
        public bool MousePressing {
            get { return mousePressing; }
            set { mousePressing = value; }
        }
        public ButtonClickedEventHandler Click;

        public button() {
            this.Click += handleToggle;
            FillColour = Color.White;
            HoverColour = Colour.LightYellow;
            ToggleOnColour = Colour.LightGreen;
            ToggleOffColour = Colour.LightRed;
            PressToggleOnColour = Colour.Red;
            PressToggleOffColour = Colour.Green;
            PressColour = Colour.Yellow;
        }

        public void draw(RenderWindow window) {
            RectangleShape rs = new RectangleShape();
            rs.Position = Position;
            rs.Size = Size;
            rs.OutlineColor = Color.Black;
            rs.OutlineThickness = OutlineThickness;

            if (MouseHovering) { rs.OutlineThickness *= 2f; }

            if (IsToggle) {
                if (MousePressing && ToggleState) {
                    rs.FillColor = PressToggleOffColour;
                } else
                if (MousePressing && !ToggleState) {
                    rs.FillColor = PressToggleOnColour;
                }else
                if (ToggleState) {
                    rs.FillColor = ToggleOnColour;
                } else {
                    rs.FillColor = ToggleOffColour;
                }
            } else {
                if (MouseHovering) {
                    if (MousePressing) {
                        rs.FillColor = PressColour;
                    } else {
                        rs.FillColor = HoverColour;
                    }
                } else {
                    rs.FillColor = FillColour;
                }
            }

            window.Draw(rs);

            Text t = new Text();
            t.CharacterSize = CharacterSize;
            t.FillColor = Color.Black;
            t.Font = Fonts.Arial;
            t.DisplayedString = text;
            FloatRect textLocalBounds = t.GetLocalBounds();
            t.Origin = new Vector2f(textLocalBounds.Width, textLocalBounds.Height) / 2f + new Vector2f(0, CharacterSize / 4f);
            t.Position = Size / 2f + rs.Position; // - new Vector2f(textLocalBounds.Width, 0);
            window.Draw(t);
        }

        public void handleToggle(object sender, EventArgs e) {
            if (!IsToggle) { return; }
            toggleState = !toggleState;
        }
    }
}