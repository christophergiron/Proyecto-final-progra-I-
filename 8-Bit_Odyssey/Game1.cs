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

        private bool wasInDemoMode = false;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            RegenerarEnemigos();
            JumpMan = new Player(new Vector2(100, 300), RegenerarEnemigos);
            demoController = new DemoController();
            demoPlayer = new DemoPlayer(new Vector2(100, 369));

            camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);

            musicManager = new Music();

            base.Initialize();
        }

        public void RegenerarEnemigos()
        {
            enemies = new List<Enemy>
        {
            new Goomba(new Vector2(280, 369)),
            new Koopa(new Vector2(380, 369)),
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
                if (!wasInDemoMode)
                {
                    demoPlayer = new DemoPlayer(new Vector2(100, 369));
                    wasInDemoMode = true;
                }

                demoPlayer.Update(gameTime, tileColliders, enemies);
                camera.Follow(demoPlayer);
            }
            else
            {
                if (wasInDemoMode)
                {
                    JumpMan.Position = demoPlayer.Position;
                    JumpMan.Velocity = demoPlayer.Velocity;

                    typeof(Player).GetField("IsOnGround", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(JumpMan, typeof(DemoPlayer).GetField("IsOnGround", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(demoPlayer));

                    demoController.Reset();
                    wasInDemoMode = false;
                }

                JumpMan.Update(gameTime, keyboard);
                JumpMan.CheckTileCollisions(tileColliders);
                JumpMan.CheckEnemyCollisions(enemies);
                camera.Follow(JumpMan);
            }

            foreach (var enemy in enemies)
            {
                enemy.Update(gameTime, tileColliders); 
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var koopa = enemies[i] as Koopa;
                if (koopa != null && koopa.IsInShell && koopa.IsMovingShell)
                {
                    for (int j = enemies.Count - 1; j >= 0; j--)
                    {
                        if (i == j) continue;
                        var other = enemies[j];

                        if (koopa.Hitbox.Intersects(other.Hitbox))
                        {
                            // Si es un Goomba o un Koopa que no está en caparazón
                            if (other is Goomba || (other is Koopa k && !k.IsInShell))
                            {
                                enemies.RemoveAt(j);
                                break;
                            }
                        }
                    }
                }
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
                    Color.LightCoral);
            }
            else
            {
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle((int)(JumpMan.Position.X - camera.Position.X), (int)JumpMan.Position.Y, 32, 32),
                    Color.Red);
            }

            foreach (var enemy in enemies)
            {
                Color color = Color.Green; // default
                if (enemy is Goomba) color = Color.SaddleBrown;
                else if (enemy is Koopa koopa)
                {
                    color = koopa.IsInShell ? Color.Cyan : Color.ForestGreen;
                }

                _spriteBatch.Draw(whiteTexture,
                    new Rectangle((int)(enemy.Position.X - camera.Position.X), (int)enemy.Position.Y, 32, 32),
                    color);
            }


            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}