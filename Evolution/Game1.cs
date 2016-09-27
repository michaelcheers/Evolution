using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

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
        public int[,] foodGrid;
        public Dictionary<Point, Cell> cells;
        public Dictionary<Point, Cell> toAdd;
        public List<Point> toRemove;
        Point oldMouse;
        Point viewPos = new Point(0,0);
        public const int WorldW = 84;
        public const int WorldH = 83;
        public byte[] registers = new byte[256];

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1000;
            graphics.PreferredBackBufferHeight = 768;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            toAdd = new Dictionary<Point, Cell>();
            toRemove = new List<Point>();
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
            foodGrid = new int[WorldW, WorldH];
            for(int Idx = 0; Idx < 12000; ++Idx)
            {
                foodGrid[rand.Next(0, WorldW), rand.Next(0, WorldH)]++;
            }

            cells = new Dictionary<Point, Cell>();
            for (int n = 0; n < 10; n++)
            {
                Cell cell = new Cell();
                cell.location = new Point(rnd.Next(foodGrid.GetLength(0)), rnd.Next(foodGrid.GetLength(1)));

                cell.program = new InterpreterProgram(this, new byte[]
                {
                    (byte)Instruction.Eat, 0, 0,
                    (byte)Instruction.Move, 0, 0,
                    (byte)Instruction.Turn, 1, 0,
                    (byte)Instruction.StartBreed, 0, 0,
                    (byte)Instruction.SetProgramToRegister, 0, 0,
                    (byte)Instruction.WriteProgramBreed, 0, 0
                }, cell.Eat, cell.Move, cell.Turn, cell.StartBreed, cell.WriteProgramBreed, cell.Die);

                cells[cell.location] = cell;
            }
            oldMouse = Mouse.GetState().Position;
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

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here
            MouseState mouse = Mouse.GetState();
            if(mouse.LeftButton == ButtonState.Pressed)
            {
                viewPos += oldMouse - mouse.Position;
            }
            oldMouse = mouse.Position;
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                foreach (var item in cells)
                {
                    registers[0] = item.Value.energy;
                    registers[1] = item.Value.health;
                    if (item.Value.health == 0 || item.Value.energy == 0)
                        item.Value.Die();
                    item.Value.energy--;
                    item.Value.program.Run(10);
                }
                foreach (var item in toAdd)
                {
                    if(!cells.ContainsKey(item.Key))
                        cells.Add(item.Key, item.Value);
                }
                foreach (var item in toRemove)
                {
                    cells.Remove(item);
                }

                toAdd.Clear();
                toRemove.Clear();
            }

            base.Update(gameTime);
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
                                spriteBatch.Draw(cellTextures[0], rect, Color.White);
                                break;
                            case Direction.Down:
                                spriteBatch.Draw(cellTextures[1], rect, Color.White);
                                break;
                            case Direction.Right:
                                spriteBatch.Draw(cellTextures[3], rect, Color.White);
                                break;
                            case Direction.Left:
                                spriteBatch.Draw(cellTextures[2], rect, Color.White);
                                break;
                        }
                    }
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
