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

        private bool state;
        public bool State {
            get { return state; }
            set { state = value; }
        }

        private int row;
        public int Row => row;

        private int col;
        public int Col => col;

        private List<cell> neighbours = new List<cell>();
        public List<cell> Neighbours => neighbours;

        public cell(bool state = false) {
            this.state = state;
        }

        public void draw(RenderWindow window) {
            RectangleShape rs = new RectangleShape(new Vector2f(Width, Height));
            rs.Position = new Vector2f(Row * (Width + OutlineThickness * 2 + Spacing), Col * (Height + OutlineThickness * 2 + Spacing));
            rs.OutlineThickness = OutlineThickness;
            rs.OutlineColor = OutlineColour;

            if (State) {
                rs.FillColor = Color.White;
            } else {
                rs.FillColor = Colour.Grey;
            }

            window.Draw(rs);
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

        public List<Vector2i> getNeighbours(int rows, int cols, bool wrapScreen) {
            List<Vector2i> neighbours = new List<Vector2i>();
            neighbours.Add(new Vector2i(Row,     Col - 1));
            neighbours.Add(new Vector2i(Row + 1, Col - 1));
            neighbours.Add(new Vector2i(Row + 1, Col));
            neighbours.Add(new Vector2i(Row + 1, Col + 1));
            neighbours.Add(new Vector2i(Row,     Col + 1));
            neighbours.Add(new Vector2i(Row - 1, Col + 1));
            neighbours.Add(new Vector2i(Row - 1, Col));
            neighbours.Add(new Vector2i(Row - 1, Col - 1));

            if (wrapScreen) {
                for (int i = 0; i < neighbours.Count; i++) {
                    if (neighbours[i].X <  0)    { neighbours[i] = new Vector2i(rows - 1, neighbours[i].Y); }
                    if (neighbours[i].X >= rows) { neighbours[i] = new Vector2i(0,        neighbours[i].Y); }
                    if (neighbours[i].Y <  0)    { neighbours[i] = new Vector2i(neighbours[i].X, cols - 1); }
                    if (neighbours[i].Y >= cols) { neighbours[i] = new Vector2i(neighbours[i].X, 0); }
                }
            }

            return neighbours;
        }
    }
}