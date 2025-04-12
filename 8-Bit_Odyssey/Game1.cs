using Bit_Odyssey.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;


namespace JumpMan
{

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player JumpMan;
        private List<Rectangle> platforms;
        private List<Enemy> enemies;
        private Texture2D whiteTexture;
        private Camera camera;
        private Music musicManager;
        private DemoController demoController;
        private DemoPlayer demoPlayer;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            RegenerarEnemigos();
            JumpMan = new Player(new Vector2(100, 369));
            demoController = new DemoController();
            demoPlayer = new DemoPlayer(new Vector2(100, 369));

            platforms = new List<Rectangle>
            {
                new Rectangle(50, 400, 800, 20),
                new Rectangle(300, 300, 200, 20),
                new Rectangle(550, 250, 200, 20),
                new Rectangle(250, 200, 20, 40),
                new Rectangle(100, 100, 20, 40)
            };
            JumpMan = new Player(new Vector2(100, 300), RegenerarEnemigos);

            camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            musicManager = new Music();

            base.Initialize();
        }
        public void RegenerarEnemigos()
        {
            enemies = new List<Enemy>
            {
                new Enemy(new Vector2(280, 369)),
                new Enemy(new Vector2(380, 369)),
                new Enemy(new Vector2(560, 220)),
                new Enemy(new Vector2(670, 220)),
                new Enemy(new Vector2(770, 220)),
                new Enemy(new Vector2(870, 220)),
                new Enemy(new Vector2(970, 220)),
                new Enemy(new Vector2(1100, 220))
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });
            Music.Load(Content);
            Music.PlayMusicOverWorld();
        }

        protected override void Update(GameTime gameTime)
        {

            KeyboardState keyboard = Keyboard.GetState();
            demoController.Update(gameTime, keyboard);

            if (demoController.InDemoMode)
            {
                demoPlayer.Update(gameTime, platforms, enemies);
                camera.Follow(demoPlayer);

            }
            else
            {
                JumpMan.Update(gameTime, keyboard);
                JumpMan.CheckCollisions(platforms);
                JumpMan.CheckEnemyCollisions(enemies);
                camera.Follow(JumpMan);

            }

            base.Update(gameTime);
            Music.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(148, 148, 255));
            _spriteBatch.Begin();

            if (demoController.InDemoMode)
            {
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle((int)(demoPlayer.Position.X - camera.Position.X), (int)demoPlayer.Position.Y, 32, 32),
                    Color.LightCoral); // Diferente color para diferenciar demo
            }
            else
            {
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle((int)(JumpMan.Position.X - camera.Position.X), (int)JumpMan.Position.Y, 32, 32),
                    Color.Red);
            }


            foreach (var platform in platforms)
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle(platform.X - (int)camera.Position.X, platform.Y, platform.Width, platform.Height),
                    Color.Gray);

            foreach (var enemy in enemies)
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle((int)(enemy.Position.X - camera.Position.X), (int)enemy.Position.Y, 32, 32),
                    Color.Green);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

    }
}