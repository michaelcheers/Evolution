using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Evolution
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Texture2D[] backgrounds;
        Texture2D[] cellTextures;
        public byte[,] foodGrid;
        public Dictionary<Point, Cell> cells;
        public Dictionary<Point, Cell> toAdd;
        public List<FoodSource> foodSources;
        public HashSet<Point> toRemove;
        Point oldMouse;
        public Point viewPos = new Point(0,0);
        public const int WorldW = 84;
        public const int WorldH = 83;
        public byte[] registers = new byte[256];
        public bool paused = true;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            toAdd = new Dictionary<Point, Cell>();
            toRemove = new HashSet<Point>();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
            backgrounds = new Texture2D[]
            {
                Content.Load<Texture2D>("food0"),
                Content.Load<Texture2D>("food1"),
                Content.Load<Texture2D>("food2"),
                Content.Load<Texture2D>("food3"),
                Content.Load<Texture2D>("food4"),
                Content.Load<Texture2D>("food5"),
                Content.Load<Texture2D>("food6"),
                Content.Load<Texture2D>("food7"),
                Content.Load<Texture2D>("food8"),
                Content.Load<Texture2D>("food9"),
            };

            cellTextures = new Texture2D[]
            {
                Content.Load<Texture2D>("cell_u"),
                Content.Load<Texture2D>("cell_d"),
                Content.Load<Texture2D>("cell_l"),
                Content.Load<Texture2D>("cell_r"),
            };

            Random rand = new Random();

            foodGrid = new byte[WorldW, WorldH];
            for (int Idx = 0; Idx < 10000; ++Idx)
            {
                foodGrid[rand.Next(0, WorldW), rand.Next(0, WorldH)]++;
            }

            for (int Idx = 0; Idx < 100; ++Idx)
            {
                foodGrid[rand.Next(0, WorldW), rand.Next(0, WorldH)] = 255;
            }

            foodSources = new List<FoodSource>();
            for (int n = 0; n < 6; n++)
            {
                FoodSource newSource = new FoodSource();
                newSource.location = new Point(rnd.Next(foodGrid.GetLength(0)), rnd.Next(foodGrid.GetLength(1)));
                newSource.game = this;
                newSource.direction = (Direction)rnd.Next(4);
                foodSources.Add(newSource);
            }

            cells = new Dictionary<Point, Cell>();
            for (int n = 0; n < 100; n++)
            {
                Cell cell = new Cell();
                do
                {
                    cell.location = new Point(rnd.Next(foodGrid.GetLength(0)), rnd.Next(foodGrid.GetLength(1)));
                }
                while (cells.ContainsKey(cell.location));
                cell.program = new InterpreterProgram(this, new byte[]
                {
                    (byte)Instruction.Eat, 0, 0,
                    (byte)Instruction.Move, 0, 0,
                    (byte)Instruction.TurnConstant, 1, 0,
                    (byte)Instruction.SetProgramToRegister, 0, 0,
                    (byte)Instruction.StartBreed, 0, 0,
                    (byte)Instruction.WriteProgramBreed, 0, 0
                }, cell.Eat, cell.Move, cell.Turn, cell.StartBreed, cell.WriteProgramBreed, cell.Die, cell.GetVision);

                cells[cell.location] = cell;
            }
            oldMouse = Mouse.GetState().Position;

            oldFoodCheck = FoodCheck();
            oldEnergyCheck = EnergyCheck();
            Energy = Content.Load<SpriteFont>("Energy");
        }

        public static Random rnd = new Random();

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }


        int oldFoodCheck = 0;
        int oldEnergyCheck = 0;
        int oldNumCells = 0;
        public Queue<Cell> dead = new Queue<Cell>();

        public void Frame ()
        {
            foreach(var foodSource in foodSources)
            {
                foodSource.Update();
            }

            foreach (var item in cells)
            {
                Debug.Assert(item.Key == item.Value.location);
                item.Value.age++;
                if (item.Value.health == 0 || item.Value.energy == 0 || item.Value.age >= 500)
                {
                    item.Value.Die();
                }
                else
                {
                    item.Value.energy--;
                    registers[0] = item.Value.energy < 255 ? (byte)item.Value.energy : (byte)255;
                    registers[1] = item.Value.health;
                    item.Value.program.Run(1000);
                }
            }

            /*foreach (var item in toAdd)
            {
                Debug.Assert(!toRemove.Contains(item.Key));
            }
            foreach (var item in toRemove)
            {
<<<<<<< HEAD
                Debug.Assert(cells.ContainsKey(item));
            }*/
            foreach (var item in toRemove)
            {
                Cell c = cells[item];
                cells.Remove(item);

                if (c.state == State.Dead)
                {
                    //Debug.Assert(!cells.ContainsKey(c.location));
                    dead.Enqueue(c);
                    c.state = State.Recycle;
                }
            }
            foreach (var item in toAdd)
            {
                Debug.Assert(item.Value.state != State.Dead);
                if (item.Value.state == State.Alive && !cells.ContainsKey(item.Key))
                {
                    cells.Add(item.Key, item.Value);
                    Debug.Assert(item.Value.location == item.Key);
                    //item.Value.location = item.Key;
                }
                else
                {
                    Debug.Assert(item.Value.state != State.Dead);
                }
            }

            toAdd.Clear();
            toRemove.Clear();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            int newFoodCheck = FoodCheck();
            int newEnergyCheck = EnergyCheck();
            int newNumCells = cells.Count;
            //Debug.Assert((newFoodCheck+newEnergyCheck) <= (oldFoodCheck+oldEnergyCheck));
            oldFoodCheck = newFoodCheck;
            oldEnergyCheck = newEnergyCheck;
            oldNumCells = newNumCells;

            // TODO: Add your update logic here
            MouseState mouse = Mouse.GetState();
            if(mouse.LeftButton == ButtonState.Pressed)
            {
                viewPos += oldMouse - mouse.Position;
            }
            oldMouse = mouse.Position;
            if (viewPos.X < 0)
                viewPos.X = 0;
            if (viewPos.Y < 0)
                viewPos.Y = 0;


            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                StreamWriter writer = new StreamWriter("result.txt");
                writer.WriteLine("Food amount: " + newFoodCheck);
                writer.WriteLine("Cells amount: " + newNumCells);
                writer.WriteLine("Energy amount: " + newEnergyCheck);
                writer.WriteLine();
                foreach (var item in cells)
                {
                    writer.WriteLine("[" + item.Key.X + ", " + item.Key.Y + "]");
                    writer.WriteLine("Energy: " + item.Value.energy);
                    writer.WriteLine("Health: " + item.Value.health);
                    var program = item.Value.program.program;
                    for (int n = 0; n < program.Length; n += 3)
                    {
                        if (n + 3 > program.Length)
                            break;
                        var inItem = program[n];
                        var byte2 = program[n + 1];
                        var byte3 = program[n + 2];
                        writer.WriteLine(string.Join(" ", ((Instruction)inItem), byte2, byte3));
                    }
                    writer.WriteLine();
                }
                writer.Flush();
                writer.Close();
            }
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                for (int n = 0; n < 100; n++)
                    Frame();
            else if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || !paused10)
                for (int n = 0; n < 10; n++)
                    Frame();
            else if (Keyboard.GetState().IsKeyDown(Keys.Space) || !paused)
                Frame();

            if (Keyboard.GetState().IsKeyDown(Keys.P))
                paused = !paused;

            if (Keyboard.GetState().IsKeyDown(Keys.J))
                paused10 = !paused10;

            base.Update(gameTime);
        }

        public int FoodCheck()
        {
            int total = 0;
            for (int x = 0; x < WorldW; x++)
            {
                for (int y = 0; y < WorldH; y++)
                {
                    total += foodGrid[x, y] * Cell.energyPerFood;
                }
            }
            return total;
        }

        public int EnergyCheck()
        {
            int total = 0;
            foreach (KeyValuePair<Point, Cell> kv in cells)
            {
                total += kv.Value.energy;
            }

            return total;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            // TODO: Add your drawing code here
            int minX = Math.Max(0, viewPos.X/16);
            int minY = Math.Max(0, viewPos.Y/16);
            int maxX = Math.Min( (viewPos.X + Window.ClientBounds.Width)/16 + 1, WorldW);
            int maxY = Math.Min( (viewPos.Y + Window.ClientBounds.Height)/16 + 1, WorldH);

            MouseState mouseState = Mouse.GetState();

            for (int x = minX; x < maxX; x++)
            {
                for(int y = minY; y < maxY; y++)
                {
                    Rectangle rect = new Rectangle(x * 16 - viewPos.X, y * 16 - viewPos.Y, 16, 16);
                    int foodAmount = foodGrid[x, y];
                    if (foodAmount > 9)
                        foodAmount = 9;
                    spriteBatch.Draw(backgrounds[foodAmount], rect, Color.White);

                    Point p = new Point(x, y);
                    if(cells.ContainsKey(p))
                    {
                        Cell c = cells[p];
                        switch(c.direction)
                        {
                            case Direction.Up:
                                spriteBatch.Draw(cellTextures[0], rect, c.color);
                                break;
                            case Direction.Down:
                                spriteBatch.Draw(cellTextures[1], rect, c.color);
                                break;
                            case Direction.Right:
                                spriteBatch.Draw(cellTextures[3], rect, c.color);
                                break;
                            case Direction.Left:
                                spriteBatch.Draw(cellTextures[2], rect, c.color);
                                break;
                        }
                    }
                }
            }

            Point mouseGridPos = new Point((mouseState.Position.X + viewPos.X) / 16, (mouseState.Position.Y + viewPos.Y) / 16);
            if(cells.ContainsKey(mouseGridPos))
            {
                Cell c = cells[mouseGridPos];
                spriteBatch.DrawString(Energy, c.energy.ToString(), new Vector2(mouseGridPos.X * 16 - viewPos.X, mouseGridPos.Y * 16 - viewPos.Y), Color.White);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
        public SpriteFont Energy;
        private bool paused10 = true;
    }
}
