using Bit_Odyssey.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using Microsoft.VisualBasic.ApplicationServices;
using MonoGame.Extended.ECS;
using static System.Net.Mime.MediaTypeNames;


namespace JumpMan
{

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player JumpMan;
        private List<Enemy> enemies;
        private Texture2D whiteTexture;
        private Camera camera;
        private List<Rectangle> tileColliders;
        private Music musicManager;
        private static TiledMap _tiledMap;
        private static TiledMapRenderer _tiledMapRenderer;
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
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            _tiledMap = Content.Load<TiledMap>("Stages/Levels/World_1/Test32x");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);
            tileColliders = new List<Rectangle>();
            var collisionLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");

            for (int y = 0; y < collisionLayer.Height; y++)
            {
                for (int x = 0; x < collisionLayer.Width; x++)
                {
                    var tile = collisionLayer.GetTile((ushort)x, (ushort)y);
                    if (!tile.IsBlank)
                    {
                        tileColliders.Add(new Rectangle(
                            x * _tiledMap.TileWidth,
                            y * _tiledMap.TileHeight,
                            _tiledMap.TileWidth,
                            _tiledMap.TileHeight
                        ));
                    }
                }
            }
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
                demoPlayer.Update(gameTime, tileColliders, enemies);
                camera.Follow(demoPlayer);

            }
            else
            {
                JumpMan.Update(gameTime, keyboard);
                JumpMan.CheckTileCollisions(tileColliders);
                JumpMan.CheckEnemyCollisions(enemies);
                camera.Follow(JumpMan);

            }
            _tiledMapRenderer.Update(gameTime);

            base.Update(gameTime);
            Music.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(148, 148, 255));
            _spriteBatch.Begin();

            _tiledMapRenderer.Draw(camera.GetViewMatrix());

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
            foreach (var enemy in enemies)
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle((int)(enemy.Position.X - camera.Position.X), (int)enemy.Position.Y, 32, 32),
                    Color.Green);

            _spriteBatch.End();
            base.Draw(gameTime);
        }

    }
}