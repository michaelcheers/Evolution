using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

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
        Texture2D[] animals;
        int[,] foodGrid;
        Point oldMouse;
        Point viewPos = new Point(0,0);
        public const int WorldW = 128;
        public const int WorldH = 32;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
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

            animals = new Texture2D[]
            {
                Content.Load<Texture2D>("cell_u"),
                Content.Load<Texture2D>("cell_d"),
                Content.Load<Texture2D>("cell_l"),
                Content.Load<Texture2D>("cell_r"),
            };

            Random rand = new Random();
            foodGrid = new int[WorldW, WorldH];
            for(int Idx = 0; Idx < 200; ++Idx)
            {
                foodGrid[rand.Next(0, WorldW), rand.Next(0, WorldH)]++;
            }

            oldMouse = Mouse.GetState().Position;
        }

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
                    spriteBatch.Draw(backgrounds[foodGrid[x, y]], new Rectangle(x * 16 - viewPos.X, y * 16 - viewPos.Y, 16, 16), Color.White);
                }
            }
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
