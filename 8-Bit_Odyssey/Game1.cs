using Bit_Odyssey.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Collections;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Header;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace JumpMan
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Player JumpMan;
        private List<Enemy> enemies;
        private List<Block> blocks;
        private int lives = 3;
        private bool isGameOver = false;
        private double gameTimer = 400;
        private bool musicSpedUp = false;
        private SpriteFont font;
        private Texture2D whiteTexture;
        private Camera camera;
        private bool useDemoPlayer = false;
        private List<Coin> coins;
        private List<Rectangle> tileColliders;
        private static TiledMap _tiledMap;
        private static TiledMapRenderer _tiledMapRenderer;
        private DemoPlayer demoPlayer;
        private Music musicManager;
        private List<Texture2D> coinFrames;//moneda

        // Animación del jugador
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
            blocks = new List<Block>
            {
                //new BreakableBlock(new Rectangle(200, 300, 32, 32)),
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

            jumpManTexture = Content.Load<Texture2D>("Personaje/walk1");

            _tiledMap = Content.Load<TiledMap>("Stages/Levels/World_1/Test32x");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

            // Carga las colisiones principales
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
            RegenerarEnemigos();

            //moneda
            coinFrames = new List<Texture2D>();
            for (int i = 1; i <= 9; i++)
            {
                coinFrames.Add(Content.Load<Texture2D>($"coin/goldCoin{i}"));
            }

            coins = new List<Coin>
            {
            new Coin(new Vector2(250, 300), coinFrames),
            new Coin(new Vector2(280, 300), coinFrames)
            };


            // Se crea el player y demoplayer  logica del game over 
            JumpMan = new Player(new Vector2(100, 300), () =>
            {
                lives--;

                gameTimer = 400;

                if (lives <= 0)
                {
                    isGameOver = true;
                }
                else
                {
                    JumpMan.Position = new Vector2(100, 300);
                    JumpMan.Velocity = Vector2.Zero;
                }

                RegenerarEnemigos();
            });
            //carga el archivo de fuentes temporal
            font = Content.Load<SpriteFont>("DefaultFont");
            // Solo activas esto si estás en el menú
            // demoPlayer = new DemoPlayer(new Vector2(100, 369));
        }

        //encargado de leer a los enemigos y cargarlos
        public void RegenerarEnemigos()
        {
            enemies = new List<Enemy>();

            var spawnerLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("EnemySpawner");
            if (spawnerLayer != null)
            {
                foreach (var obj in spawnerLayer.Objects)
                {
                    Vector2 spawnPos = new Vector2(obj.Position.X, obj.Position.Y);

                    if (obj.Properties.TryGetValue("enemyType", out var typeProp))
                    {
                        string type = typeProp.ToString();
                        switch (typeProp)
                        {
                            case "Goomba":
                                enemies.Add(new Goomba(spawnPos));
                                break;

                            case "Koopa":
                                enemies.Add(new Koopa(spawnPos));
                                break;

                            default:
                                //coloca bien los enemigos
                                enemies.Add(new Goomba(spawnPos));
                                break;
                        }
                    }
                }
            }
        }

        private List<float> DemoPlayerCords()
        {
            List<float> puntos = new List<float>();

            var layer = _tiledMap.GetLayer<TiledMapObjectLayer>("DemoJumpPoints"); //capa de objetos que se busca
            if (layer != null)
            {
                foreach (var obj in layer.Objects)
                {
                    puntos.Add(obj.Position.X); //usamos los puntos insertados en el mapa
                }

                puntos.Sort();
            }

            return puntos;
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();

            if (isGameOver)
            {
                // reinicia luego del game over temporal
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    lives = 3;
                    isGameOver = false;
                    gameTimer = 400;
                    musicSpedUp = false;
                    JumpMan.Position = new Vector2(100, 300);
                    JumpMan.Velocity = Vector2.Zero;
                    RegenerarEnemigos();
                }
                return;
            }
            if (!useDemoPlayer && !isGameOver)
            {
                gameTimer -= gameTime.ElapsedGameTime.TotalSeconds;

                if (gameTimer <= 0 && !JumpMan.isRespawning)
                {
                    JumpMan.Die();
                }
            }

            if (gameTimer <= 100 && !musicSpedUp)
            {
                //musica rapida //mete aqui la musica acelerada miguel
                musicSpedUp = true;
            }

            if (keyboard.IsKeyDown(Keys.Tab))
            {
                //descomenta esto jose cuando tengas lo de tiled
                if (keyboard.IsKeyDown(Keys.Tab))
                {
                    useDemoPlayer = true;
                    if (demoPlayer == null)
                    {
                        var puntosDeSalto = DemoPlayerCords();
                        demoPlayer = new DemoPlayer(new Vector2(100, 369), RegenerarEnemigos, puntosDeSalto);
                    }
                }
            }
            else if (keyboard.IsKeyDown(Keys.Enter))
            {
                useDemoPlayer = false;
                demoPlayer = null;
            }

            if (useDemoPlayer && demoPlayer != null)
            {
                demoPlayer.Update(gameTime, tileColliders, blocks, enemies);
                camera.Follow(demoPlayer);
            }
            else
            {
                JumpMan.Update(gameTime, keyboard);
                JumpMan.CheckCollisions(tileColliders, blocks);
                JumpMan.CheckEnemyCollisions(enemies);
                foreach (var coin in coins)
                    coin.Update(JumpMan , gameTime);//coin
                camera.Follow(JumpMan);
            }
            if (!JumpMan.isRespawning)
            {
                foreach (var enemy in enemies)
                    enemy.Update(gameTime, tileColliders);
            }
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if (enemies[i] is Koopa koopa)
                    koopa.HandleShellCollisions(enemies);
            }

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
        //se dibuja al demo player temporalmente como un cubo rosa para pruebas y si no se activa el demo se activa el dibujado del sprite de player 
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(148, 148, 255));
            _spriteBatch.Begin();

            _tiledMapRenderer.Draw(camera.GetViewMatrix());

            // Dibuja al jugador 
            //Texture2D currentTexture = walkFrames[currentFrame];
            //_spriteBatch.Draw(currentTexture, 
            //    new Vector2(JumpMan.Position.X - camera.Position.X, JumpMan.Position.Y),
            //    Color.White);
            if (useDemoPlayer && demoPlayer != null)
            {
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle(
                        (int)(demoPlayer.Position.X - camera.Position.X),
                        (int)demoPlayer.Position.Y,
                        32,
                        32),
                    Color.HotPink);
            }
            else
            {
                Texture2D currentTexture = walkFrames[currentFrame];
                _spriteBatch.Draw(currentTexture,
                    new Vector2(JumpMan.Position.X - camera.Position.X, JumpMan.Position.Y),
                    Color.White);
            }

            // Dibuja enemigos con colores según su estado placeholder temporal
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

            // Dibuja bloques destruibles placeholder temporal
            foreach (var block in blocks)
                block.Draw(_spriteBatch, whiteTexture, camera.Position);
            //dibuja las monedas placeholder temporal
            foreach (var coin in coins)
                coin.Draw(_spriteBatch, camera.Position);//coin
            //dibuja las vidas y tiempo Temporal 
            _spriteBatch.DrawString(font, $"Vidas: {lives}", new Vector2(10, 70), Color.White);
            _spriteBatch.DrawString(font, $"Tiempo: {Math.Ceiling(gameTimer)}", new Vector2(10, 40), Color.White);

            if (isGameOver)
            {
                _spriteBatch.DrawString(font, "GAME OVER", new Vector2(200, 200), Color.Red);
                _spriteBatch.DrawString(font, "Presiona ENTER para reiniciar", new Vector2(180, 230), Color.Yellow);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

