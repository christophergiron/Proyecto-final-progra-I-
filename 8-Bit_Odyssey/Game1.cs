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

        private int lives = 3;
        private bool isGameOver = false;
        private double gameTimer = 400;
        private bool musicSpedUp = false;

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
        private int currentFrame = 3; // idle4
        private double animationTimer;
        private double frameDuration = 0.1;

        private enum PlayerState { Idle, WalkingRight, WalkingLeft }
        private PlayerState currentState = PlayerState.Idle;
      
        // Animaciones monedas
        private List<Texture2D> coinFrames;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            blocks = new List<Block>();
            camera = new Camera(_graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            musicManager = new Music();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });

            _tiledMap = Content.Load<TiledMap>("Stages/Levels/World_1/Test32x");
            _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

            // Carga colisiones tiles
            tileColliders = new List<Rectangle>();
            var collisionLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");
            for (int y = 0; y < collisionLayer.Height; y++)
                for (int x = 0; x < collisionLayer.Width; x++)
                {
                    var tile = collisionLayer.GetTile((ushort)x, (ushort)y);
                    if (!tile.IsBlank)
                        tileColliders.Add(new Rectangle(x * _tiledMap.TileWidth, y * _tiledMap.TileHeight, _tiledMap.TileWidth, _tiledMap.TileHeight));
                }
            var warpLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("Warps");
            warpZones = warpLayer?.Objects?.ToList() ?? new List<TiledMapObject>();

            // Carga animaciones jugador
            walkRightFrames = new Texture2D[6];
            walkLeftFrames = new Texture2D[6];
            idleFrames = new Texture2D[6];
            for (int i = 0; i < 6; i++)
            {
                walkRightFrames[i] = Content.Load<Texture2D>($"Personaje/walk{i + 1}");
                walkLeftFrames[i] = Content.Load<Texture2D>($"Personaje/walk{i + 1}l");
            }
            for (int i = 0; i < 6; i++)
                idleFrames[i] = Content.Load<Texture2D>($"Personaje/idle{i + 1}");



            // Música
            Music.Load(Content);
            Music.PlayMusicOverWorld();

            // Que levante la mano to el que está haciendo dinero
            //Que venga el Año Nuevo y yo voy a josearlo entero
            //Un día de estos llega mi amor verdadero
            //Pero, quiero hacerme rico primero
           // Porque mañana yo no sé qué va a pasar
            //Me siento triste, pero se me va a pasar, je
            //Mi vida está cabrona, hey
            //Yo no me quiero casar


                        // Monedas
                        coinFrames = new List<Texture2D>();
            for (int i = 1; i <= 9; i++)
                coinFrames.Add(Content.Load<Texture2D>($"coin/goldCoin{i}"));

            RegenerarObjetos();

            JumpMan = new Player(new Vector2(100, 300), () =>
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
                    JumpMan.Position = new Vector2(100, 300);
                    JumpMan.Velocity = Vector2.Zero;
                }
                RegenerarObjetos();
            });

            font = Content.Load<SpriteFont>("DefaultFont");
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

            // Agregar monedas a la lista de monedas con sus posiciones, si las quieres generar dinámicamente
            coins.Add(new Coin(new Vector2(250, 300), coinFrames));
            coins.Add(new Coin(new Vector2(280, 300), coinFrames));
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

            if (isGameOver)
            {
                if (keyboard.IsKeyDown(Keys.Enter))
                {
                    lives = 3;
                    isGameOver = false;
                    gameTimer = 400;
                    musicSpedUp = false;
                    JumpMan.Position = new Vector2(100, 300);
                    JumpMan.Velocity = Vector2.Zero;
                    RegenerarObjetos();
                }
                return;
            }

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
                    demoPlayer = new DemoPlayer(new Vector2(100, 369), RegenerarObjetos, puntosDeSalto);
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
                    coin.Update(JumpMan, gameTime);

                if (Keyboard.GetState().IsKeyDown(Keys.S))
                {
                    foreach (var warp in warpZones)
                    {
                        Rectangle warpRect = new Rectangle((int)warp.Position.X, (int)warp.Position.Y, (int)warp.Size.Width, (int)warp.Size.Height);
                        if (JumpMan.Hitbox.Intersects(warpRect))
                        {
                            if (warp.Properties.TryGetValue("warpTarget", out var targetMapObj))
                            {
                                string targetMap = targetMapObj.ToString();
                                float spawnX = 100, spawnY = 300; 

                                if (warp.Properties.TryGetValue("spawnX", out var spawnXProp))
                                    float.TryParse(spawnXProp.ToString(), out spawnX);
                                if (warp.Properties.TryGetValue("spawnY", out var spawnYProp))
                                    float.TryParse(spawnYProp.ToString(), out spawnY);

                                // Cargar nuevo mapa
                                _tiledMap = Content.Load<TiledMap>(targetMap);
                                _tiledMapRenderer = new TiledMapRenderer(GraphicsDevice, _tiledMap);

                                // Recargar todo lo demás (colisiones, objetos, warp zones)
                                tileColliders.Clear();
                                var collisionLayer = _tiledMap.GetLayer<TiledMapTileLayer>("Tile Layer 1");
                                for (int y = 0; y < collisionLayer.Height; y++)
                                    for (int x = 0; x < collisionLayer.Width; x++)
                                    {
                                        var tile = collisionLayer.GetTile((ushort)x, (ushort)y);
                                        if (!tile.IsBlank)
                                            tileColliders.Add(new Rectangle(
                                                x * _tiledMap.TileWidth,
                                                y * _tiledMap.TileHeight,
                                                _tiledMap.TileWidth,
                                                _tiledMap.TileHeight));
                                    }

                                var warpLayer = _tiledMap.GetLayer<TiledMapObjectLayer>("Warps");
                                warpZones = warpLayer?.Objects?.ToList() ?? new List<TiledMapObject>();

                                JumpMan.Position = new Vector2(spawnX, spawnY);
                                JumpMan.Velocity = Vector2.Zero;

                                RegenerarObjetos();
                                break;
                            }
                        }
                    }
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

            // Control animación jugador
            if (JumpMan.Velocity.X > 0.1f)
                currentState = PlayerState.WalkingRight;
            else if (JumpMan.Velocity.X < -0.1f)
                currentState = PlayerState.WalkingLeft;
            else
                currentState = PlayerState.Idle;

            switch (currentState)
            {
                case PlayerState.WalkingRight:
                    animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (animationTimer >= frameDuration)
                    {
                        animationTimer = 0;
                        currentFrame = (currentFrame + 1) % walkRightFrames.Length;
                    }
                    break;

                case PlayerState.WalkingLeft:
                    animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
                    if (animationTimer >= frameDuration)
                    {
                        animationTimer = 0;
                        currentFrame = (currentFrame + 1) % walkLeftFrames.Length;
                    }
                    break;

                case PlayerState.Idle:
                    currentFrame = 4; // idle4 (solo una vez, no cada frame)
                    animationTimer = 0; // Reinicia el timer para evitar animación fantasma
                    break;
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
                        (int)demoPlayer.Position.Y,
                        32,
                        32),
                    Color.HotPink);
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

            // Dibuja enemigos
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

            // Dibuja bloques
            foreach (var block in blocks)
                block.Draw(_spriteBatch, whiteTexture, camera.Position);

            // Dibuja monedas
            foreach (var coin in coins)
                coin.Draw(_spriteBatch, camera.Position);

            // Dibuja HUD
            _spriteBatch.DrawString(font, $"Vidas: {lives}", new Vector2(10, 10), Color.Black);
            _spriteBatch.DrawString(font, $"Tiempo: {Math.Round(gameTimer, 2)}", new Vector2(10, 30), Color.Black);

            if (isGameOver)
            {
                _spriteBatch.DrawString(font, "GAME OVER - Presiona Enter para reiniciar",
                    new Vector2(150, 300), Color.Red);
            }

            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
