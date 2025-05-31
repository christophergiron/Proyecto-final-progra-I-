using Bit_Odyssey.Scripts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace JumpMan
{

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Player JumpMan;
        private List<Enemy> enemies;
        private List<Block> blocks;
        private List<Coin> coins;
        private List<TiledMapObject> warpZones;
        private List<Rectangle> tileColliders;
        private List<Goal> goals;

        private int lives = 3;
        private bool isGameOver = false;
        private double gameTimer = 300;
        private bool musicSpedUp = false;
        private bool isPaused = false;
        private KeyboardState previousKeyboard;

        private SpriteFont font;
        private Texture2D whiteTexture;

        private Camera camera;
        private DemoPlayer demoPlayer;
        private bool useDemoPlayer = false;

        private static TiledMap _tiledMap;
        private static TiledMapRenderer _tiledMapRenderer;

        private Music musicManager;

        // Animaciones jugador
        private Texture2D[] walkRightFrames;
        private Texture2D[] walkLeftFrames;
        private Texture2D[] idleFrames;
        private int currentFrame = 3;
        private double animationTimer;
        private double frameDuration = 0.1;

        private enum PlayerState { Idle, WalkingRight, WalkingLeft }
        private PlayerState currentState = PlayerState.Idle;

        private List<Texture2D> coinFrames;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            goals = new List<Goal>();
            blocks = new List<Block>();
            camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            musicManager = new Music();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            //goals.Add(new Goal(new Vector2(250, 300)));
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });

            // Carga mapa principal
            _tiledMap = Content.Load<TiledMap>("Stages/Levels/World_1/Test32x");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

            // Cargar colisiones
            tileColliders = new List<Rectangle>();
            var collisionLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");
            for (int y = 0; y < collisionLayer.Height; y++)
                for (int x = 0; x < collisionLayer.Width; x++)
                {
                    var tile = collisionLayer.GetTile((ushort)x, (ushort)y);
                    if (!tile.IsBlank)
                        tileColliders.Add(new Rectangle(x * _tiledMap.TileWidth, y * _tiledMap.TileHeight, _tiledMap.TileWidth, _tiledMap.TileHeight));
                }

            // Warp zones
            var warpLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("Warps");
            warpZones = warpLayer?.Objects?.ToList() ?? new List<TiledMapObject>();

            // Animaciones jugador
            walkRightFrames = new Texture2D[6];
            walkLeftFrames = new Texture2D[6];
            idleFrames = new Texture2D[6];
            for (int i = 0; i < 6; i++)
            {
                walkRightFrames[i] = Content.Load<Texture2D>($"Personaje/walk{i + 1}");
                walkLeftFrames[i] = Content.Load<Texture2D>($"Personaje/walk{i + 1}l");
                idleFrames[i] = Content.Load<Texture2D>($"Personaje/idle{i + 1}");
            }

            // Animaciones monedas
            coinFrames = new List<Texture2D>();
            for (int i = 1; i <= 9; i++)
                coinFrames.Add(Content.Load<Texture2D>($"coin/goldCoin{i}"));

            // Música
            Music.Load(Content);
            Music.PlayMusicOverWorld();

            RegenerarObjetos();

            JumpMan = new Player(playerSpawnPoint(), () => 
            {
                lives--;
                gameTimer = 400;
                if (lives <= 0)
                {
                    isGameOver = true;
                    Music.PlayGameover();
                }
                else
                {
                    JumpMan.Position = playerSpawnPoint();
                    JumpMan.Velocity = Vector2.Zero;
                }
                RegenerarObjetos();
            });

            font = Content.Load<SpriteFont>("DefaultFont");
        }

        private Vector2 playerSpawnPoint()
        {
            var spawnerLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("ObjectSpawner");
            if (spawnerLayer != null)
            {
                foreach (var obj in spawnerLayer.Objects)
                {
                    if (obj.Properties.TryGetValue("spawn", out var propValue) &&
                        bool.TryParse(propValue.ToString(), out bool isSpawn) &&
                        isSpawn)
                    {
                        return new Vector2(obj.Position.X, obj.Position.Y);
                    }
                }
            }
            return new Vector2(100, 300);
        }
        public void RegenerarObjetos()
        {
            enemies = new List<Enemy>();
            blocks = new List<Block>();
            coins = new List<Coin>();

            var spawnerLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("ObjectSpawner");
            if (spawnerLayer != null)
            {
                foreach (var obj in spawnerLayer.Objects)
                {
                    Vector2 spawnPos = new Vector2(obj.Position.X, obj.Position.Y);

                    if (obj.Properties.TryGetValue("objectType", out var typeProp))
                    {
                        switch (typeProp)
                        {
                            case "Goomba":
                                enemies.Add(new Goomba(spawnPos));
                                break;

                            case "Koopa":
                                enemies.Add(new Koopa(spawnPos));
                                break;

                            case "Bloque_destruible":
                                Rectangle rect = new Rectangle(
                                    (int)obj.Position.X,
                                    (int)(obj.Position.Y - obj.Size.Height),
                                    (int)obj.Size.Width,
                                    (int)obj.Size.Height);
                                blocks.Add(new BreakableBlock(rect));
                                break;

                            case "Coin":
                                coins.Add(new Coin(spawnPos, coinFrames));
                                break;
                            
                            case "Meta":
                                goals.Add(new Goal(spawnPos));
                                break;

                            default:
                                enemies.Add(new Goomba(spawnPos));
                                break;
                        }
                    }
                }
            }

            // Otro que se cae por la fuerza de gravedad
            // Otro más por si sobrevive de casualidad
            // Refuta mi tesis, cabrón, hoy te vamos a dar catequesis
            // No he metido un gol y tengo cristianos orándole a Messi

        }

        private List<float> DemoPlayerCords()
        {
            List<float> puntos = new List<float>();

            var layer = _tiledMap.GetLayer<TiledMapObjectLayer>("DemoJumpPoints");
            if (layer != null)
            {
                foreach (var obj in layer.Objects)
                {
                    puntos.Add(obj.Position.X);
                }
                puntos.Sort();
            }
            return puntos;
        }


        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.P) && previousKeyboard.IsKeyUp(Keys.P))
            {
                isPaused = !isPaused;
            }
            previousKeyboard = keyboard;

            if (isGameOver)
            {
                if (keyboard.IsKeyDown(Keys.Enter))
                {
                    lives = 3;
                    isGameOver = false;
                    gameTimer = 400;
                    musicSpedUp = false;
                    JumpMan.Position = playerSpawnPoint();
                    JumpMan.Velocity = Vector2.Zero;
                    RegenerarObjetos();
                }
                return;
            }
            if (isPaused)
                return;


            if (!useDemoPlayer && !isGameOver)
            {
                gameTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (gameTimer <= 0 && !JumpMan.isRespawning)
                    JumpMan.Die();
            }

            if (gameTimer <= 100 && !musicSpedUp)
            {
                musicSpedUp = true;
                Music.PlayMusicOverworldSpeed();

            }

            if (keyboard.IsKeyDown(Keys.Tab))
            {
                useDemoPlayer = true;
                if (demoPlayer == null)
                {
                    var puntosDeSalto = DemoPlayerCords();
                    demoPlayer = new DemoPlayer(playerSpawnPoint(), RegenerarObjetos, puntosDeSalto);
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

                foreach (var g in goals)
                {
                    if (g.Contains(JumpMan.Position))
                    {
                        isGameOver = true;
                        Music.PlayClear();
                        break;
                    }
                }

                foreach (var coin in coins)
                    coin.Update(JumpMan, gameTime);

                if (WarpManager.CheckWarpTriggers(
                    JumpMan, warpZones, Content, GraphicsDevice,
                    out TiledMap newMap,
                    out TiledMapRenderer newRenderer,
                    out List<Rectangle> newColliders,
                    out List<TiledMapObject> newWarps,
                    out Vector2? spawnPos))
                {
                    // Solo si hubo warp
                    _tiledMap = newMap;
                    _tiledMapRenderer = newRenderer;
                    tileColliders = newColliders;
                    warpZones = newWarps;

                    if (spawnPos.HasValue)
                    {
                        JumpMan.Position = spawnPos.Value;
                        JumpMan.Velocity = Vector2.Zero;
                    }

                    RegenerarObjetos(); 
                }

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

            currentState = JumpMan.Velocity.X switch
            {
                > 0.1f => PlayerState.WalkingRight,
                < -0.1f => PlayerState.WalkingLeft,
                _ => PlayerState.Idle
            };

            if (currentState != PlayerState.Idle)
            {
                animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                if (animationTimer >= frameDuration)
                {
                    animationTimer = 0;
                    currentFrame = (currentFrame + 1) % walkRightFrames.Length;
                }
            }
            else
            {
                currentFrame = 4;
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

            if (useDemoPlayer && demoPlayer != null)
            {
                _spriteBatch.Draw(whiteTexture,
                    new Rectangle(
                        (int)(demoPlayer.Position.X - camera.Position.X),
                        (int)demoPlayer.Position.Y, 32, 32), Color.HotPink);
            }
            else
            {
                Texture2D currentTexture = currentState switch
                {
                    PlayerState.WalkingRight => walkRightFrames[currentFrame],
                    PlayerState.WalkingLeft => walkLeftFrames[currentFrame],
                    PlayerState.Idle => idleFrames[currentFrame],
                    _ => idleFrames[0]
                };
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

            foreach (var block in blocks)
                block.Draw(_spriteBatch, whiteTexture, camera.Position);

            foreach (var coin in coins)
                coin.Draw(_spriteBatch, camera.Position);

            _spriteBatch.DrawString(font, $"Vidas: {lives}", new Vector2(10, 10), Color.Black);
            _spriteBatch.DrawString(font, $"Tiempo: {Math.Round(gameTimer, 2)}", new Vector2(10, 30), Color.Black);

            if (isGameOver)
            {
                _spriteBatch.DrawString(font, "GAME OVER - Presiona Enter para reiniciar",
                    new Vector2(150, 300), Color.Red);
            }
            if (isPaused)
            {
                string texto = "PAUSADO";
                Vector2 tamaño = font.MeasureString(texto);
                Vector2 posicion = new Vector2(
                    (_graphics.PreferredBackBufferWidth - tamaño.X) / 2,
                    (_graphics.PreferredBackBufferHeight - tamaño.Y) / 2);

                _spriteBatch.DrawString(font, texto, posicion, Color.Red);
            }
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}