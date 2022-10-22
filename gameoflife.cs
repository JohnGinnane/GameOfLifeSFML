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
        public const float timeStep = 1000f / 200f;
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

        private DateTime lastSimulation;

        // when we click and drag are we setting the affected cells alive or dead?
        private FloatRect mouseSelectionBox;
        public bool SelectingCells {
            get {
                return Math.Abs(mouseSelectionBox.Width) > 0.5 && Math.Abs(mouseSelectionBox.Height) > 0.5;
            }
        }
        private View mouseLockedToView;

        private button PlayPauseButton;
        private slider SimulationSpeedSlider;
        private button ToggleWrapScreenButton;

        private Text actionText;

        private List<control> controls;

        // debugging info
        panel debugPanel;

        float updateTime;
        float renderTime;
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
            mouseSelectionBox = new FloatRect();

            lastSimulation = DateTime.Now;
            
            actionText = new Text();
            actionText.Font = Fonts.Arial;
            actionText.FillColor = Color.Black;
            actionText.CharacterSize = 16;

            controls = new List<control>();

            // debugging info
            debugPanel = new panel();
            debugPanel.FillColour = new Color(200, 200, 200, 150);
            label labelDebugInfo = new label();
            labelDebugInfo.Font = Fonts.Arial;
            labelDebugInfo.CharacterSize = 16;
            labelDebugInfo.Position = debugPanel.Position + new Vector2f(10, 10);
            labelDebugInfo.FillColour = Color.White;
            labelDebugInfo.OutlineColour = Color.Black;
            debugPanel.add(labelDebugInfo);

            controls.Add(debugPanel);

            // interface
            PlayPauseButton = new button();
            PlayPauseButton.Size = new Vector2f(150, 30);
            PlayPauseButton.Position = new Vector2f(Global.ScreenSize.X / 2f - PlayPauseButton.Size.X / 2f + 200, Global.ScreenSize.Y - PlayPauseButton.Size.Y * 1.5f);
            PlayPauseButton.ToggleOnText = "Play [Spacebar]";
            PlayPauseButton.ToggleOffText = "Pause [Spacebar]";
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

            ToggleWrapScreenButton = new button();
            ToggleWrapScreenButton.Size = new Vector2f(140, 30);
            ToggleWrapScreenButton.Position = SimulationSpeedSlider.Position - new Vector2f(150, 0);
            ToggleWrapScreenButton.ToggleOnText = "Wrap Screen";
            ToggleWrapScreenButton.ToggleOffText = "Don't Wrap Screen";
            ToggleWrapScreenButton.IsToggle = true;
            ToggleWrapScreenButton.CharacterSize = 16;
            controls.Add(ToggleWrapScreenButton);

            // iterate over all controls and add mouse events
            foreach (control c in controls) {
                window.MouseMoved += c.Control_MouseMoved;
                window.MouseButtonPressed += c.Control_MouseButtonPressed;
                window.MouseButtonReleased += c.Control_MouseButtonReleased;
            }
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
                ((label)debugPanel.Children[0]).Text = String.Format("Update: {0:n4}ms\nRender: {1:n4}ms", 
                                                                     updateTime,
                                                                     renderTime);

                if (DateTime.Now > lastUpdate.AddMilliseconds(timeStep)) {
                    float delta = timeStep * timeScale;
                    window.DispatchEvents();
                    DateTime beforeUpdate = DateTime.Now;
                    update(delta);
                    lastUpdate = DateTime.Now;
                    updateTime = (float)(lastUpdate - beforeUpdate).TotalMilliseconds;
                }

                if (DateTime.Now > lastFrame.AddMilliseconds(frameRate)) {
                    DateTime beforeDraw = DateTime.Now;
                    draw();
                    lastFrame = DateTime.Now;
                    renderTime = (float)(lastFrame - beforeDraw).TotalMilliseconds;
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
            
            // check if the mouse is hovering over the UI or the grid
            control ctrlUnderMouse = controlUnderMouse();
            
            // if you start a click and you're hovering over a control
            // then "lock" the mouse so it only interacts with controls...
            if (ctrlUnderMouse != null && mouseLockedToView != gridView) {
                if ((Input.Mouse["left"].isPressed ||
                     Input.Mouse["right"].isPressed ||
                     Input.Mouse["middle"].isPressed) && mouseLockedToView == null) {
                    mouseLockedToView = interfaceView;
                }
            } else
            
            // ...or lock it to the grid so you don't accidentally
            // interact with the interface
            if (ctrlUnderMouse == null && mouseLockedToView != interfaceView) {
                if ((Input.Mouse["left"].isPressed ||
                     Input.Mouse["right"].isPressed ||
                     Input.Mouse["middle"].isPressed) && mouseLockedToView == null) {
                    mouseLockedToView = gridView;
                }

                if (Input.Mouse["left"].justPressed) {
                    actionText.DisplayedString = "Draw";
                    FloatRect textLocalBounds = actionText.GetLocalBounds();
                    actionText.Origin = new Vector2f(textLocalBounds.Width, textLocalBounds.Height) / 2f + new Vector2f(0, actionText.CharacterSize / 4f);
                } else
                if (Input.Mouse["right"].justPressed) {
                    actionText.DisplayedString = "Clear";
                    FloatRect textLocalBounds = actionText.GetLocalBounds();
                    actionText.Origin = new Vector2f(textLocalBounds.Width, textLocalBounds.Height) / 2f + new Vector2f(0, actionText.CharacterSize / 4f);
                }

                if (Input.Mouse["left"].isPressed || Input.Mouse["right"].isPressed) {
                    Vector2i mousePosRelativeToClick = Input.Mouse.Position - Input.Mouse.ClickPosition;

                    if (Input.Keyboard["lshift"].isPressed) {
                        // holding down shift lets you draw a perfect square
                        int shortestSide = 0;
                        shortestSide = Math.Abs(mousePosRelativeToClick.X);
                        if (Math.Abs(mousePosRelativeToClick.Y) < shortestSide) {
                            shortestSide = Math.Abs(mousePosRelativeToClick.Y);
                        }

                        int width = shortestSide;
                        int height = shortestSide;

                        if (mousePosRelativeToClick.X < 0) {
                            width *= -1;
                        }

                        if (mousePosRelativeToClick.Y < 0) {
                            height *= -1;
                        }

                        mouseSelectionBox = new FloatRect(Input.Mouse.ClickPosition.X,
                                                          Input.Mouse.ClickPosition.Y,
                                                          width,
                                                          height);
                    } else {
                        mouseSelectionBox = new FloatRect(Input.Mouse.ClickPosition.X,
                                                        Input.Mouse.ClickPosition.Y,
                                                        mousePosRelativeToClick.X,
                                                        mousePosRelativeToClick.Y);
                    }
                }

                if (Input.Mouse["left"].justReleased || Input.Mouse["right"].justReleased) {
                    if (SelectingCells) {
                        Vector2f mouseStartPos = window.MapPixelToCoords((Vector2i)util.Position(mouseSelectionBox), gridView);
                        Vector2f mouseEndPos = window.MapPixelToCoords((Vector2i)(util.Position(mouseSelectionBox) + util.Size(mouseSelectionBox)), gridView);

                        setCellsInBox(util.PointsToFloatRect(mouseStartPos, mouseEndPos),
                                    Input.Mouse["left"].justReleased);
                    } else {
                        cell c = findCellUnderMouse();
                        if (c != null) { c.State = !c.State; }
                    }
                }

                handleCamera(delta);
            }

            // if the mouse was released then we release it from the view
            if (Input.Mouse["left"].justReleased ||
                Input.Mouse["right"].justReleased ||
                Input.Mouse["middle"].justReleased) {
                mouseLockedToView = null;
                mouseSelectionBox = new FloatRect();
            }

            // do the actual game of life
            if (SimulationSpeedSlider.Value > 0 && PlayPauseButton.ToggleState) {
                float simulationSpeed = SimulationSpeedSlider.Value * (1000f / timeStep);
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

            ///////////////
            // GRID VIEW //
            ///////////////

            // draw a border around the grid if the screen is set not to wrap
            if (!ToggleWrapScreenButton.ToggleState) {
                RectangleShape screenEdge = new RectangleShape();
                screenEdge.Position = new Vector2f(cell.Width + cell.OutlineThickness + cell.Spacing,
                                                   cell.Height + cell.OutlineThickness + cell.Spacing);
                screenEdge.Size = new Vector2f(cols * (cell.Width + cell.OutlineThickness * 2 + cell.Spacing),
                                               rows * (cell.Height + cell.OutlineThickness * 2 + cell.Spacing));
                screenEdge.OutlineColor = Color.Black;
                screenEdge.OutlineThickness = 10f;
                window.Draw(screenEdge);
            }

            cell cellUnderMouse = findCellUnderMouse();
            for (int row = 1; row < cells.Length - 1; row++) {
                for (int col = 1; col < cells[row].Length - 1; col++) {
                    cell thisCell = cells[row][col];
                    
                    // dont render cells outside the view
                    Vector2f cellPos = new Vector2f(col * (cell.Width  + cell.OutlineThickness * 2 + cell.Spacing),
                                                    row * (cell.Height + cell.OutlineThickness * 2 + cell.Spacing));
                    
                    RectangleShape rs = new RectangleShape(new Vector2f(cell.Width, cell.Height));
                    rs.FillColor = thisCell.Temperature;
                    rs.OutlineThickness = cell.OutlineThickness;
                    rs.Position = cellPos;

                    if (thisCell.State) {
                        if (cellUnderMouse == thisCell) {
                            rs.OutlineColor = Colour.DarkGreen;
                        } else {
                            rs.OutlineColor = Color.Black;
                        }
                    } else {
                        if (cellUnderMouse == thisCell) {
                            rs.OutlineColor = Color.Green;
                        } else {
                            rs.OutlineColor = Colour.LightGrey;
                        }
                    }

                    window.Draw(rs);
                }
            }
            
            window.SetView(interfaceView);

            ////////////////////
            // INTERFACE VIEW //
            ////////////////////
            if (SelectingCells) {
                RectangleShape selectBox = new RectangleShape();
                selectBox.Position = util.Position(mouseSelectionBox);
                selectBox.Size = util.Size(mouseSelectionBox);
                selectBox.FillColor = Colour.VeryOpaque;
                selectBox.OutlineThickness = 2f;
                selectBox.OutlineColor = Color.Black;
                window.Draw(selectBox);

                // tell the user what sort of box we are doing                
                if (Math.Abs(util.Size(mouseSelectionBox).X) > 50 &&
                    Math.Abs(util.Size(mouseSelectionBox).Y) > actionText.CharacterSize * 1.5) {
                    actionText.Position = util.Position(mouseSelectionBox) + util.Size(mouseSelectionBox) / 2f;
                    window.Draw(actionText);
                }
            }
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

        private void setCellsInBox(FloatRect box, bool newState) {
            if (controlUnderMouse() != null) { return; }

            // the more efficient way to do this is
            // find the row and column for the top left 
            // most corner and bottom right corner
            // then iterate only over those rows and 
            // columns, saving going over every single cell
            for (int row = 1; row < cells.Length - 1; row++) {
                for (int col = 1; col < cells[row].Length - 1; col++) {
                    Vector2f cellPosition = new Vector2f(col * (cell.Width  + cell.OutlineThickness * 2 + cell.Spacing),
                                                         row * (cell.Height + cell.OutlineThickness * 2 + cell.Spacing));
                    if (intersection.rectangleInsideRectangle(box,
                                                              new FloatRect(cellPosition.X, cellPosition.Y, cell.Width, cell.Height))) {
                        cells[row][col].State = newState;
                    }
                }
            }
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

            if (mouseLockedToView == gridView) {
                if (Input.Mouse["middle"].justPressed) {
                    lastViewPos = gridView.Center + (Vector2f)Input.Mouse.Position * GridZoom;
                } else
                if (Input.Mouse["middle"].isPressed) {
                    window.SetMouseCursor(new Cursor(Cursor.CursorType.SizeAll));
                    gridView.Center = lastViewPos - (Vector2f)Input.Mouse.Position * GridZoom;
                } else
                if (Input.Mouse["middle"].justReleased) {
                    window.SetMouseCursor(new Cursor(Cursor.CursorType.Arrow));
                }
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

            if (ToggleWrapScreenButton.ToggleState) {
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