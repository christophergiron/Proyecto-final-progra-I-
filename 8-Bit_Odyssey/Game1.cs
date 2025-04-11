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
        private double timeSinceLastSpawn = 0;
        private double spawnInterval = 3;
        private Music musicManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            JumpMan = new Player(new Vector2(100, 300));
            platforms = new List<Rectangle>
            {
                new Rectangle(50, 400, 800, 20),
                new Rectangle(300, 300, 200, 20),
                new Rectangle(250, 200, 20, 40),
                new Rectangle(100, 100, 20, 40)
            };
            enemies = new List<Enemy>
            {
                new Enemy(new Vector2(250, 380))
            };

           camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            musicManager = new Music();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });

            musicManager.Load(Content);
            musicManager.PlayMusic();
        }

        protected override void Update(GameTime gameTime)
        {
            timeSinceLastSpawn += gameTime.ElapsedGameTime.TotalSeconds;

            if (timeSinceLastSpawn >= spawnInterval)
            {
                SpawnEnemy();
                timeSinceLastSpawn = 0;
            }

            KeyboardState keyboard = Keyboard.GetState();
            JumpMan.Update(gameTime, keyboard);
            JumpMan.CheckCollisions(platforms);
            JumpMan.CheckEnemyCollisions(enemies);
            camera.Follow(JumpMan);

            base.Update(gameTime);
        }
        public void SpawnEnemy()
        {
            Random rand = new Random();
            Vector2 spawnPosition = new Vector2(rand.Next(50, 750), rand.Next(50, 350));
            enemies.Add(new Enemy(spawnPosition));
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(148, 148, 255));
            _spriteBatch.Begin();

            _spriteBatch.Draw(whiteTexture,
                new Rectangle((int)(JumpMan.Position.X - camera.Position.X), (int)JumpMan.Position.Y, 32, 32),
                Color.Red);

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