using Bit_Odyssey.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Tiled.Renderers;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Reflection;

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

        private List<Rectangle> breakableBlocks;
        private bool wasInDemoMode = false;

        // Animación JumpMan
        private Texture2D[] walkFrames;
        private int currentFrame;
        private double animationTimer;
        private double frameDuration = 0.1;
        private Texture2D jumpManTexture;
     

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            breakableBlocks = new List<Rectangle>
            {
                new Rectangle(200, 300, 32, 32),
            };

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
                new Goomba(new Vector2(380, 369)),
                new Koopa(new Vector2(680, 369)),
            };
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });
            jumpManTexture = Content.Load<Texture2D>("Personaje/walk1");

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

            walkFrames = new Texture2D[6];
            for (int i = 0; i < 6; i++)
            {
                walkFrames[i] = Content.Load<Texture2D>($"Personaje/walk{i + 1}");
            }

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

                    typeof(Player).GetField("IsOnGround", BindingFlags.NonPublic | BindingFlags.Instance)
                        ?.SetValue(JumpMan, typeof(DemoPlayer).GetField("IsOnGround", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(demoPlayer));

                    demoController.Reset();
                    wasInDemoMode = false;
                }

                JumpMan.Update(gameTime, keyboard);
                JumpMan.CheckTileCollisions(tileColliders);
                JumpMan.CheckBreakableBlocks(breakableBlocks);
                JumpMan.CheckEnemyCollisions(enemies);
                camera.Follow(JumpMan);
            }

            foreach (var enemy in enemies)
            {
                enemy.Update(gameTime, tileColliders);
            }

            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i] is Koopa koopa)
                {
                    koopa.HandleShellCollisions(enemies);
                }
            }

            // Actualizar animación de JumpMan
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (animationTimer >= frameDuration)
            {
                currentFrame = (currentFrame + 1) % walkFrames.Length;
                animationTimer = 0;
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
                Vector2 origin = new Vector2(0, 32); // origen en la esquina inferior izquierda

                _spriteBatch.Draw(jumpManTexture,
                     new Vector2(JumpMan.Position.X - camera.Position.X, JumpMan.Position.Y),
                      null,
                      Color.White,
                      0f,
                      origin,
                      1f,
                      SpriteEffects.None,
                      0f);
            }
            else
            {
                Texture2D currentTexture = walkFrames[currentFrame];
                _spriteBatch.Draw(currentTexture,
                    new Vector2(JumpMan.Position.X - camera.Position.X, JumpMan.Position.Y),
                    Color.White);
            }

            foreach (var enemy in enemies)
            {
                Color color = Color.Green;
                if (enemy is Goomba) color = Color.SaddleBrown;
                else if (enemy is Koopa koopa)
                    color = koopa.IsInShell ? (koopa.IsMovingShell ? Color.Orange : Color.Cyan) : Color.ForestGreen;

                _spriteBatch.Draw(whiteTexture,
                    new Rectangle((int)(enemy.Position.X - camera.Position.X), (int)enemy.Position.Y, 32, 32),
                    color);
            }

            foreach (var block in breakableBlocks)
            {
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle(block.X - (int)camera.Position.X, block.Y, block.Width, block.Height),
                    Color.Yellow);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
