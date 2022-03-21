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

        public ButtonClickedEventHandler Click;

        public button() {

        }

        public void draw(RenderWindow window) {
            RectangleShape rs = new RectangleShape();
            rs.Position = Position;
            rs.Size = Size;
            rs.FillColor = Color.White;
            rs.OutlineColor = Color.Black;
            rs.OutlineThickness = OutlineThickness;
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
    }
}