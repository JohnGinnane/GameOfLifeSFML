using System;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GameOfLifeSFML {
    public class label : control {
        private string text;
        public string Text {
            get { return text; }
            set { text = value; }
        }

        private Font font;
        public Font Font {
            get { return font; }
            set { font = value; }
        }

        private uint characterSize = 12;
        public uint CharacterSize {
            get { return characterSize; }
            set { characterSize = value; }
        }

        private Color outlineColour;
        public Color OutlineColour {
            get { return outlineColour; }
            set { outlineColour = value; }
        }

        public label() {
            text = "Sample Text";
            FillColour = Color.Black;
        }

        public override void draw(RenderWindow window)
        {
            Text t = new Text();
            t.CharacterSize = CharacterSize;
            t.FillColor = FillColour;
            t.OutlineThickness = OutlineThickness;
            t.OutlineColor = OutlineColour;
            t.Font = Font;
            t.Position = Position;
            t.DisplayedString = Text;

            window.Draw(t);
        }
    }
}