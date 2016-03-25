namespace MonoGameExample
{
    using Codefarts.ContentManager;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using System;

    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        /// <summary>
        /// Holds the html markup to display.
        /// </summary>
        private string html = string.Empty;

        /// <summary>
        /// Holds the google logo texture.
        /// </summary>                                                                                 
        private Texture2D googleTexture;

        /// <summary>
        /// The font use to draw the text.
        /// </summary>
        private SpriteFont font;

        /// <summary>
        /// Holds a reference to a codefarts <see cref="ContentManager{TKey}"/>.
        /// </summary>
        private ContentManager<Uri> manager;


        /// <summary>
        /// Initializes a new instance of the <see cref="Game1"/> class.
        /// </summary>
        public Game1()
        {
            this.graphics = new GraphicsDeviceManager(this);
            this.Content.RootDirectory = "Content";
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
         
            // get singleton instance to the codefarts content manager using a Uri as the key type
            this.manager = ContentManager<Uri>.Instance;

            // register readers
            this.manager.Register(new HtmlReader());
            this.manager.Register(new Texture2DReader() { Game = this });

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            this.spriteBatch = new SpriteBatch(this.GraphicsDevice);

            // Use Xna content manager to load the sprite font
            this.font = this.Content.Load<SpriteFont>("Courier New");

            // asynchronously load google home page html markup
            this.manager.Load<HtmlData>(new Uri("http://www.google.com"), data => this.html = string.IsNullOrWhiteSpace(data.Result.Markup) ? string.Empty : data.Result.Markup.Substring(0, 50));

            // asynchronously load google logo
            this.manager.Load<Texture2D>(new Uri("http://www.google.ca/images/srpr/logo4w.png"), data => this.googleTexture = data.Result);
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                this.Exit();
            }

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            this.spriteBatch.Begin();

            // check if there are still assets being loaded
            if (this.manager.LoadingQueue > 0)
            {
                // draw notice that assets are still loading. Depending on your internet connection speed this message may only appear briefly.
                this.spriteBatch.DrawString(this.font, "Waiting for loading resources ...", new Vector2(10, 10), Color.Red);
            }
            else
            {
                // draw the html
                this.spriteBatch.DrawString(this.font, this.html, new Vector2(10, 10), Color.White);

                // draw the google logo
                if (this.googleTexture != null)
                {
                    this.spriteBatch.Draw(this.googleTexture, new Vector2(10, 50), Color.White);
                }
            }

            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
