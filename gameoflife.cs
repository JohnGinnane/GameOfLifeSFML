using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GameOfLifeSFML {
    public class GameOfLife {
#region "Properties"
        private RenderWindow window;

        private DateTime lastUpdate;

        public const float timeStep = 1000f / 100f;
        public float timeScale = 1.0f;

        public Vector2f lastViewPos;
        public View gridView;

        public View interfaceView;

        public float GridZoom {
            get {
                return gridView.Size.X / Global.ScreenSize.X;
            }
        }

        private int rows = 60;
        private int cols = 60;
        
        private const float scrollSpeed = 50f;

        private cell[][] cells;

        private bool wrapScreen = false;

        private DateTime lastSimulation;
        private float lastSimulationSpeed = 0f;
        private float simulationSpeed = 30f;

        // when we click and drag are we setting the affected cells alive or dead?
        private int mouseSettingState = 0;
        private View mouseLockedToView;

        private button PlayPauseButton;
        private List<button> buttons;
#endregion

        public GameOfLife() {
            window = new RenderWindow(new VideoMode((uint)Global.ScreenSize.X, (uint)Global.ScreenSize.Y), "Game Of Life", Styles.Close);
            interfaceView = new View(Global.ScreenSize/2f, Global.ScreenSize);
            gridView = new View(Global.ScreenSize/2f, Global.ScreenSize);
            
            window.SetKeyRepeatEnabled(false);
            window.Closed += window_CloseWindow;
            lastUpdate = DateTime.Now;
            
            lastViewPos = new Vector2f();
            
            generateGrid();
            window.MouseWheelScrolled += mouseWheelScrolled;

            lastSimulation = DateTime.Now;

            buttons = new List<button>();

            PlayPauseButton = new button();
            PlayPauseButton.Size = new Vector2f(150, 30);
            PlayPauseButton.Position = new Vector2f(Global.ScreenSize.X / 2f - PlayPauseButton.Size.X / 2f, Global.ScreenSize.Y - PlayPauseButton.Size.Y * 1.5f);
            PlayPauseButton.Text = "Pause";
            PlayPauseButton.IsToggle = true;
            PlayPauseButton.CharacterSize = 16;
            PlayPauseButton.Click += PlayPauseButton_Click;
            buttons.Add(PlayPauseButton);
            playPauseSimulation();

            button ResetGridButton = new button();
            ResetGridButton.Use(b => {
                b.Size = new Vector2f(100, 30);
                b.Position = PlayPauseButton.Position + new Vector2f(160, 0);
                b.Text = "Clear Grid [C]";
                b.CharacterSize = 16;
                b.Click += ClearGridButton_Click;
                buttons.Add(b);
            });

            button RandomiseGridButton = new button();
            RandomiseGridButton.Use(b => {
                b.Size = new Vector2f(120, 30);
                b.Position = PlayPauseButton.Position + new Vector2f(-130, 0);
                b.Text = "Randomise [R]";
                b.CharacterSize = 16;
                b.Click += RandomiseGridButton_Click;
                buttons.Add(b);
            });
        }

#region "Events"
        public void window_CloseWindow(object sender, EventArgs e) {
            if (sender == null) { return; }
            window.Close();
        }

        public void PlayPauseButton_Click(object sender, EventArgs e) {
            playPauseSimulation();
        }

        private void ClearGridButton_Click(object sender, EventArgs e) {
            clearGrid();
        }

        private void RandomiseGridButton_Click(object sender, EventArgs e) {
            generateGrid();
        }

        private void mouseWheelScrolled(object sender, MouseWheelScrollEventArgs e) {
            if (buttonUnderMouse() != null) { return; }

            if (e.Delta < 0) {
                gridView.Zoom(1 - 0.001f * e.Delta * scrollSpeed);
            } else
            if (e.Delta > 0) {
                zoomViewToMouse(1 - 0.001f * e.Delta * scrollSpeed);
            }
        }
#endregion

#region "Main"
        public void run() {
            while (window.IsOpen) {
                if (!window.HasFocus()) { continue; }

                if ((float)(DateTime.Now - lastUpdate).TotalMilliseconds >= timeStep) {
                    float delta = timeStep * timeScale;
                    lastUpdate = DateTime.Now;

                    window.DispatchEvents();
                    update(delta);
                }

                draw();
            }
        }

        public void update(float delta) {
            Global.Keyboard.update();
            Global.Mouse.update(window);
            
            if (Global.Keyboard["escape"].isPressed) {
                window.Close();
            }

            if (Global.Keyboard["space"].justPressed) {
                playPauseSimulation();
            }

            if (Global.Keyboard["r"].justPressed) {
                generateGrid();
            }

            if (Global.Keyboard["c"].justPressed) {
                clearGrid();
            }

            if (Global.Mouse["left"].justReleased || Global.Mouse["right"].justReleased) {
                mouseLockedToView = null;
            }

            // check if the mouse is hovering over the UI or the grid
            button buttonMouse = buttonUnderMouse();
            if (buttonMouse != null && mouseLockedToView != gridView) {
                if ((Global.Mouse["left"].isPressed || Global.Mouse["right"].isPressed) && mouseLockedToView == null) {
                    mouseLockedToView = interfaceView;
                }

                // handle buttons and stuff
                if (Global.Mouse["left"].justReleased) {
                    buttonMouse.Click?.Invoke(buttonMouse, null);
                }
            } else
            if (buttonMouse == null && mouseLockedToView != interfaceView) {
                if ((Global.Mouse["left"].isPressed || Global.Mouse["right"].isPressed) && mouseLockedToView == null) {
                    mouseLockedToView = gridView;
                }

                // handle grid stuff
                if (Global.Mouse["left"].isPressed) {
                    cell cellUnderMouse = findCellUnderMouse();
                    
                    if (cellUnderMouse != null) {
                        if (mouseSettingState == 0) {
                            if (cellUnderMouse.State) { mouseSettingState = -1; } else { mouseSettingState = 1; }
                        }

                        if (mouseSettingState > 0) { cellUnderMouse.State = true; }
                        if (mouseSettingState < 0) { cellUnderMouse.State = false; }
                    }
                } else
                if (Global.Mouse["left"].justReleased) {
                    mouseSettingState = 0;
                }

                handleCamera(delta);
            }

            if (simulationSpeed > 0) {
                if (DateTime.Now > lastSimulation.AddSeconds(1 / simulationSpeed)) {
                    lastSimulation = DateTime.Now;

                    // prepare new states array
                    bool[][] newStates = new bool[rows+2][];
                    for (int i = 0; i < newStates.Length; i++) {
                        newStates[i] = new bool[cols+2];
                        for (int j = 0; j < newStates[i].Length; j++) {
                            newStates[i][j] = false;
                        }
                    }

                    // iterate over all the cells and determine next generation
                    for (int row = 1; row < cells.Length - 1; row++) {
                        for (int col = 1; col < cells[row].Length - 1; col++) {
                            cell c = cells[row][col];
                            c.update(delta);

                            int livingNeighbours = 0;
                            List<Vector2i> neighbourIndices = getNeighbourIndices(row, col);
                            foreach (Vector2i v in neighbourIndices) {
                                if (cells[v.Y][v.X] == null) { continue; }
                                if (cells[v.Y][v.X].State) {
                                    livingNeighbours++;
                                }
                            }

                            newStates[row][col] = c.checkRules(livingNeighbours);
                        }
                    }

                    // iterate over all cells again and set their new states
                    for (int row = 1; row < cells.Length - 1; row++) {
                        for (int col = 1; col < cells[row].Length - 1; col++) {
                            cells[row][col].State = newStates[row][col];
                        }
                    }
                }
            }
        }

        public void draw() {
            window.Clear(Colour.LightBlue);
            window.SetView(gridView);

            cell cellUnderMouse = findCellUnderMouse();
            for (int row = 1; row < cells.Length - 1; row++) {
                for (int col = 1; col < cells[row].Length - 1; col++) {
                    cell thisCell = cells[row][col];

                    RectangleShape rs = new RectangleShape(new Vector2f(cell.Width, cell.Height));
                    rs.FillColor = thisCell.Temperature;
                    rs.OutlineThickness = cell.OutlineThickness;
                    rs.Position = new Vector2f(col * (cell.Width  + cell.OutlineThickness * 2 + cell.Spacing),
                                               row * (cell.Height + cell.OutlineThickness * 2 + cell.Spacing));

                    if (thisCell.State) {
                        if (cellUnderMouse == thisCell) {
                            rs.OutlineColor = Color.Green;
                        } else {
                            rs.OutlineColor = Color.Black;
                        }
                    } else {
                        if (cellUnderMouse == thisCell) {
                            rs.OutlineColor = Colour.DarkGreen;
                        } else {
                            rs.OutlineColor = Color.White;
                        }
                    }

                    window.Draw(rs);
                }
            }
            
            window.SetView(interfaceView);

            foreach (button b in buttons) {
                b.draw(window);
            }

            window.Display();
        }
#endregion

#region "Functions"
        private void playPauseSimulation() {
            if (simulationSpeed > 0) {
                lastSimulationSpeed = simulationSpeed;
                simulationSpeed = 0f;
                PlayPauseButton.Text = "Play [spacebar]";
            } else {
                simulationSpeed = lastSimulationSpeed;
                PlayPauseButton.Text = "Pause [spacebar]";
            }
        }

        private button buttonUnderMouse() {
            if (mouseLockedToView == gridView) { return null; }
            button bUnderMouse = null;

            foreach (button b in buttons) {
                b.MouseHovering = false;
                b.MousePressing = false;

                if (bUnderMouse == null) {
                    if (b.Dimensions.Contains(Global.Mouse.Position.X, Global.Mouse.Position.Y)) {
                        b.MouseHovering = true;
                        b.MousePressing = Global.Mouse["left"].isPressed;
                        bUnderMouse = b;
                    }
                }
            }

            return bUnderMouse;
        }

        private cell findCellUnderMouse() {
            foreach (button b in buttons) {
                if (b.Dimensions.Contains(Global.Mouse.Position.X, Global.Mouse.Position.Y)) {
                    return null;
                }
            }

            cell cellUnderMouse = null;

            for (int row = 1; row < cells.Length - 1; row++) {
                if (cellUnderMouse != null) { break; }
                for (int col = 1; col < cells[row].Length - 1; col++) {
                    Vector2f cellPosition = new Vector2f(col * (cell.Width  + cell.OutlineThickness * 2 + cell.Spacing),
                                                            row * (cell.Height + cell.OutlineThickness * 2 + cell.Spacing));
                    if (intersection.pointInsideRectangle(window.MapPixelToCoords(Global.Mouse.Position, gridView),
                                                            new FloatRect(cellPosition.X, cellPosition.Y, cell.Width, cell.Height))) {
                        cellUnderMouse = cells[row][col];
                        break;
                    }
                }
            }

            return cellUnderMouse;
        }

        private void handleCamera(float delta) {
            // Zooming the camera            
            if (Global.Keyboard["q"].isPressed) {
                gridView.Zoom(1 + 0.001f * delta);
            }

            if (Global.Keyboard["e"].isPressed) {
                gridView.Zoom(1 - 0.001f * delta);
            }

            float cameraSprintMulti = 1.0f;

            if (Global.Keyboard["lshift"].isPressed) {
                cameraSprintMulti *= 2f;
            }

            // Panning the camera
            if (Global.Keyboard["w"].isPressed || Global.Keyboard["up"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X, gridView.Center.Y - 1 * delta * GridZoom * cameraSprintMulti);
            }
            
            if (Global.Keyboard["a"].isPressed || Global.Keyboard["left"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X - 1 * delta * GridZoom * cameraSprintMulti, gridView.Center.Y);
            }
            
            if (Global.Keyboard["s"].isPressed || Global.Keyboard["down"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X, gridView.Center.Y + 1 * delta * GridZoom * cameraSprintMulti);
            }
            
            if (Global.Keyboard["d"].isPressed || Global.Keyboard["right"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X + 1 * delta * GridZoom * cameraSprintMulti, gridView.Center.Y);
            }

            if (Global.Mouse["right"].justPressed) {
                lastViewPos = gridView.Center + (Vector2f)Global.Mouse.Position * GridZoom;
            } else
            if (Global.Mouse["right"].isPressed) {
                window.SetMouseCursor(new Cursor(Cursor.CursorType.SizeAll));
                gridView.Center = lastViewPos - (Vector2f)Global.Mouse.Position * GridZoom;
            } else
            if (Global.Mouse["right"].justReleased) {
                window.SetMouseCursor(new Cursor(Cursor.CursorType.Arrow));
            }
        }

        private void zoomViewToMouse(float zoomFactor) {
            gridView.Move(((Vector2f)Global.Mouse.Position - Global.ScreenSize / 2f) / 10f * GridZoom);
            gridView.Zoom(zoomFactor);
        }

        private List<Vector2i> getNeighbourIndices(int row, int col) {
            List<Vector2i> neighbours = new List<Vector2i>();
            neighbours.Add(new Vector2i(col - 1, row - 1));
            neighbours.Add(new Vector2i(col,     row - 1));
            neighbours.Add(new Vector2i(col + 1, row - 1));

            neighbours.Add(new Vector2i(col - 1, row));
            neighbours.Add(new Vector2i(col + 1, row));

            neighbours.Add(new Vector2i(col - 1, row + 1));
            neighbours.Add(new Vector2i(col,     row + 1));
            neighbours.Add(new Vector2i(col + 1, row + 1));

            if (wrapScreen) {
                for (int i = 0; i < neighbours.Count; i++) {
                    if (neighbours[i].X < 1)     { neighbours[i] = new Vector2i(cols - 2, neighbours[i].Y); }
                    if (neighbours[i].X >= rows) { neighbours[i] = new Vector2i(1,        neighbours[i].Y); }
                    if (neighbours[i].Y < 1)     { neighbours[i] = new Vector2i(neighbours[i].X, rows - 2); }
                    if (neighbours[i].Y >= cols) { neighbours[i] = new Vector2i(neighbours[i].X, 1       ); }
                }
            }

            return neighbours;
        }

        private void clearGrid() {
            cells = null;

            cells = new cell[rows+2][];
            for (int row = 0; row < cells.Length; row++) {
                cells[row] = new cell[cols+2];
            }

            for (int row = 1; row < cells.Length-1; row++) {
                for (int col = 1; col < cells[row].Length-1; col++) {
                    cells[row][col] = new cell(false);
                }
            }
        }

        private void generateGrid() {
            cells = null;

            cells = new cell[rows+2][];
            for (int row = 0; row < cells.Length; row++) {
                cells[row] = new cell[cols+2];
            }

            for (int row = 1; row < cells.Length-1; row++) {
                for (int col = 1; col < cells[row].Length-1; col++) {
                    cells[row][col] = new cell(util.randbit());
                }
            }
        }
#endregion
    }
}