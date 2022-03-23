using System;
using System.Collections.Generic;
using SFML.Window;
using SFML.Graphics;
using SFML.System;

namespace GameOfLifeSFML {
    public class GameOfLife {
#region "Properties"
        private RenderWindow window;

        private DateTime lastFrame;
        public const float frameRate = 1000f / 60f;

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

        private int rows = 10;
        private int cols = 10;
        
        private const float scrollSpeed = 50f;

        private cell[][] cells;

        private bool wrapScreen = true;

        private DateTime lastSimulation;

        // when we click and drag are we setting the affected cells alive or dead?
        private int mouseSettingState = 0;
        private View mouseLockedToView;

        private button PlayPauseButton;
        private slider SimulationSpeedSlider;
        private List<control> controls;

        private DateTime testTime;
        private int testAcc = 0;
#endregion

        public GameOfLife() {
            window = new RenderWindow(new VideoMode((uint)Global.ScreenSize.X, (uint)Global.ScreenSize.Y), "Game Of Life", Styles.Close);
            interfaceView = new View(Global.ScreenSize/2f, Global.ScreenSize);
            gridView = new View(Global.ScreenSize/2f, Global.ScreenSize);
            
            window.SetKeyRepeatEnabled(false);
            window.Closed += window_CloseWindow;
            lastUpdate = DateTime.Now;
            lastFrame = DateTime.Now;
            
            lastViewPos = new Vector2f();
            
            generateGrid();
            window.MouseWheelScrolled += mouseWheelScrolled;

            lastSimulation = DateTime.Now;

            controls = new List<control>();

            PlayPauseButton = new button();
            PlayPauseButton.Size = new Vector2f(150, 30);
            PlayPauseButton.Position = new Vector2f(Global.ScreenSize.X / 2f - PlayPauseButton.Size.X / 2f + 100, Global.ScreenSize.Y - PlayPauseButton.Size.Y * 1.5f);
            PlayPauseButton.ToggleOnText = "Pause [Spacebar]";
            PlayPauseButton.ToggleOffText = "Play [Spacebar]";
            PlayPauseButton.IsToggle = true;
            PlayPauseButton.CharacterSize = 16;
            controls.Add(PlayPauseButton);

            button ResetGridButton = new button();
            ResetGridButton.Use(b => {
                b.Size = new Vector2f(100, 30);
                b.Position = PlayPauseButton.Position + new Vector2f(160, 0);
                b.Text = "Clear Grid [C]";
                b.CharacterSize = 16;
                b.Click += ClearGridButton_Click;
                controls.Add(b);
            });

            button RandomiseGridButton = new button();
            RandomiseGridButton.Use(b => {
                b.Size = new Vector2f(120, 30);
                b.Position = PlayPauseButton.Position + new Vector2f(-130, 0);
                b.Text = "Randomise [R]";
                b.CharacterSize = 16;
                b.Click += RandomiseGridButton_Click;
                controls.Add(b);
            });

            SimulationSpeedSlider = new slider();
            SimulationSpeedSlider.Size = new Vector2f(200, 30);
            SimulationSpeedSlider.Position = RandomiseGridButton.Position - new Vector2f(210, 0);
            SimulationSpeedSlider.Value = 0.2f;
            controls.Add(SimulationSpeedSlider);

            // iterate over all controls and add mouse events
            foreach (control c in controls) {
                window.MouseMoved += c.Control_MouseMoved;
                window.MouseButtonPressed += c.Control_MouseButtonPressed;
                window.MouseButtonReleased += c.Control_MouseButtonReleased;
            }

            testTime = DateTime.Now;
        }

#region "Events"
        public void window_CloseWindow(object sender, EventArgs e) {
            if (sender == null) { return; }
            window.Close();
        }

        private void ClearGridButton_Click(object sender, EventArgs e) {
            clearGrid();
        }

        private void RandomiseGridButton_Click(object sender, EventArgs e) {
            generateGrid();
        }

        private void mouseWheelScrolled(object sender, MouseWheelScrollEventArgs e) {
            if (controlUnderMouse() != null) { return; }

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

                if ((float)(DateTime.Now - lastFrame).TotalMilliseconds >= frameRate) {
                    lastFrame = DateTime.Now;
                    draw();
                }
            }
        }

        public void update(float delta) {
            Input.Keyboard.update();
            Input.Mouse.update(window);
            
            if (Input.Keyboard["escape"].isPressed) {
                window.Close();
            }

            if (Input.Keyboard["space"].justPressed) {
                PlayPauseButton.handleToggle(null, null);
            }

            if (Input.Keyboard["r"].justPressed) {
                generateGrid();
            }

            if (Input.Keyboard["c"].justPressed) {
                clearGrid();
            }

            // TO DO:
            // Rather than using the update code to handle the UI
            // I should add events to all controls for whenever the mouse
            // does anything like move or press a button
            // this way each control can be aware of the mouse and
            // respond more immediately
            // not sure what the downsides are yet though    

            // check if the mouse is hovering over the UI or the grid
            control ctrlUnderMouse = controlUnderMouse();
            
            // if you start a click and you're hovering over a control
            // then "lock" the mouse so it only interacts with controls...
            if (ctrlUnderMouse != null && mouseLockedToView != gridView) {
                if ((Input.Mouse["left"].isPressed || Input.Mouse["right"].isPressed) && mouseLockedToView == null) {
                    mouseLockedToView = interfaceView;
                }
            } else
            
            // ...or lock it to the grid so you don't accidentally
            // interact with the interface
            if (ctrlUnderMouse == null && mouseLockedToView != interfaceView) {
                if ((Input.Mouse["left"].isPressed || Input.Mouse["right"].isPressed) && mouseLockedToView == null) {
                    mouseLockedToView = gridView;
                }

                if (Input.Mouse["left"].isPressed) {
                    cell cellUnderMouse = findCellUnderMouse();
                    
                    if (cellUnderMouse != null) {
                        if (mouseSettingState == 0) {
                            // if we clicked on a living cell then any new cells to hover over
                            // will be set to dead, and vice versa
                            if (cellUnderMouse.State) { mouseSettingState = -1; } else { mouseSettingState = 1; }
                        }

                        if (mouseSettingState > 0) { cellUnderMouse.State = true; }
                        if (mouseSettingState < 0) { cellUnderMouse.State = false; }
                    }
                } else
                if (Input.Mouse["left"].justReleased) {
                    mouseSettingState = 0;
                }

                handleCamera(delta);
            }

            // if the mouse was released then we release it from the view
            if (Input.Mouse["left"].justReleased || Input.Mouse["right"].justReleased) {
                mouseLockedToView = null;
            }

            // do the actual game of life
            if (SimulationSpeedSlider.Value > 0 && PlayPauseButton.ToggleState) {
                float simulationSpeed = SimulationSpeedSlider.Value * (1000f / timeStep);
                if (DateTime.Now > lastSimulation.AddSeconds(1 / simulationSpeed)) {
                    lastSimulation = DateTime.Now;

                    testAcc++;

                    if (DateTime.Now > testTime.AddSeconds(1)) {
                        testTime = DateTime.Now;
                        window.SetTitle(String.Format("Game Of Life (ups: {0})", testAcc));
                        testAcc = 0;
                    }

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
                            rs.OutlineColor = Colour.LightGrey;
                        }
                    }

                    window.Draw(rs);
                }
            }
            
            window.SetView(interfaceView);

            foreach (control c in controls) {
                c.draw(window);
            }

            window.Display();
        }
#endregion

#region "Functions"
        private control controlUnderMouse() {
            if (mouseLockedToView == gridView) { return null; }

            foreach (control c in controls) {
                if (c.MouseHovering) {
                    return c;
                }
            }

            return null;
        }

        private cell findCellUnderMouse() {
            if (controlUnderMouse() != null) { return null; }

            cell cellUnderMouse = null;

            for (int row = 1; row < cells.Length - 1; row++) {
                if (cellUnderMouse != null) { break; }
                for (int col = 1; col < cells[row].Length - 1; col++) {
                    Vector2f cellPosition = new Vector2f(col * (cell.Width  + cell.OutlineThickness * 2 + cell.Spacing),
                                                            row * (cell.Height + cell.OutlineThickness * 2 + cell.Spacing));
                    if (intersection.pointInsideRectangle(window.MapPixelToCoords(Input.Mouse.Position, gridView),
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
            if (Input.Keyboard["q"].isPressed) {
                gridView.Zoom(1 + 0.001f * delta);
            }

            if (Input.Keyboard["e"].isPressed) {
                gridView.Zoom(1 - 0.001f * delta);
            }

            float cameraSprintMulti = 1.0f;

            if (Input.Keyboard["lshift"].isPressed) {
                cameraSprintMulti *= 2f;
            }

            // Panning the camera
            if (Input.Keyboard["w"].isPressed || Input.Keyboard["up"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X, gridView.Center.Y - 1 * delta * GridZoom * cameraSprintMulti);
            }
            
            if (Input.Keyboard["a"].isPressed || Input.Keyboard["left"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X - 1 * delta * GridZoom * cameraSprintMulti, gridView.Center.Y);
            }
            
            if (Input.Keyboard["s"].isPressed || Input.Keyboard["down"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X, gridView.Center.Y + 1 * delta * GridZoom * cameraSprintMulti);
            }
            
            if (Input.Keyboard["d"].isPressed || Input.Keyboard["right"].isPressed) {
                gridView.Center = new Vector2f(gridView.Center.X + 1 * delta * GridZoom * cameraSprintMulti, gridView.Center.Y);
            }

            if (Input.Mouse["right"].justPressed) {
                lastViewPos = gridView.Center + (Vector2f)Input.Mouse.Position * GridZoom;
            } else
            if (Input.Mouse["right"].isPressed) {
                window.SetMouseCursor(new Cursor(Cursor.CursorType.SizeAll));
                gridView.Center = lastViewPos - (Vector2f)Input.Mouse.Position * GridZoom;
            } else
            if (Input.Mouse["right"].justReleased) {
                window.SetMouseCursor(new Cursor(Cursor.CursorType.Arrow));
            }
        }

        private void zoomViewToMouse(float zoomFactor) {
            gridView.Move(((Vector2f)Input.Mouse.Position - Global.ScreenSize / 2f) / 10f * GridZoom);
            gridView.Zoom(zoomFactor);
        }

        private List<Vector2i> getNeighbourIndices(int row, int col) {
            List<Vector2i> n = new List<Vector2i>();
            n.Add(new Vector2i(col - 1, row - 1));
            n.Add(new Vector2i(col,     row - 1));
            n.Add(new Vector2i(col + 1, row - 1));

            n.Add(new Vector2i(col - 1, row));
            n.Add(new Vector2i(col + 1, row));

            n.Add(new Vector2i(col - 1, row + 1));
            n.Add(new Vector2i(col,     row + 1));
            n.Add(new Vector2i(col + 1, row + 1));

            if (wrapScreen) {
                for (int i = 0; i < n.Count; i++) {
                    if (n[i].X < 1)        { n[i] = new Vector2i(cols,   n[i].Y); }
                    if (n[i].X > rows) { n[i] = new Vector2i(1,      n[i].Y); }
                    if (n[i].Y < 1)        { n[i] = new Vector2i(n[i].X, rows); }
                    if (n[i].Y > cols) { n[i] = new Vector2i(n[i].X, 1); }
                }
            }

            return n;
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