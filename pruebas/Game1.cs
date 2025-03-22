using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace MarioPhysics
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Mario mario;
        private List<Rectangle> platforms;
        private List<Enemy> enemies;
        private Texture2D whiteTexture;
        private Camera camera;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            mario = new Mario(new Vector2(100, 300));
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

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            whiteTexture = new Texture2D(GraphicsDevice, 1, 1);
            whiteTexture.SetData(new[] { Color.White });
        }

        protected override void Update(GameTime gameTime)
        {
            KeyboardState keyboard = Keyboard.GetState();
            mario.Update(gameTime, keyboard);
            mario.CheckCollisions(platforms);
            mario.CheckEnemyCollisions(enemies);
            camera.Follow(mario);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(new Color(148, 148, 255));
            _spriteBatch.Begin();

            _spriteBatch.Draw(whiteTexture,
                new Rectangle((int)(mario.Position.X - camera.Position.X), (int)mario.Position.Y, 32, 32),
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

    public class Mario
    {
        public Vector2 Position;
        private Vector2 Velocity;
        private float gravity = 0.4f;
        private float jumpForce = -10f;
        private bool isOnGround;
        private float fallLimit = 600;
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Mario(Vector2 position)
        {
            Position = position;
        }

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float acceleration = 0.15f;  
            float deceleration = 0.1f;   
            float maxSpeed = 6f;         
            float walkSpeed = 3f;        

            float targetSpeed = keyboard.IsKeyDown(Keys.A) ? maxSpeed : walkSpeed;

            if (keyboard.IsKeyDown(Keys.Left))
            {
                Velocity.X -= acceleration;
                if (Velocity.X < -targetSpeed) Velocity.X = -targetSpeed;
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                Velocity.X += acceleration;
                if (Velocity.X > targetSpeed) Velocity.X = targetSpeed;
            }
            else
            {
                if (Velocity.X > 0)
                {
                    Velocity.X -= deceleration;
                    if (Velocity.X < 0) Velocity.X = 0;
                }
                if (Velocity.X < 0)
                {
                    Velocity.X += deceleration;
                    if (Velocity.X > 0) Velocity.X = 0;
                }
            }

            if (!isOnGround)
                Velocity.Y += gravity;

            if (keyboard.IsKeyDown(Keys.S) && isOnGround)
            {
                Velocity.Y = jumpForce;
                isOnGround = false;
            }

            Position += Velocity;

            if (Position.Y > fallLimit)
            {
                Position = new Vector2(100, 300);
            }
        }

        public void CheckCollisions(List<Rectangle> platforms)
        {
            isOnGround = false;
            foreach (var platform in platforms)
            {
                if (Hitbox.Intersects(platform))
                {
                    Rectangle intersection = Rectangle.Intersect(Hitbox, platform);

                    if (intersection.Height < intersection.Width)
                    {
                        if (Velocity.Y > 0)
                        {
                            Position.Y = platform.Top - Hitbox.Height;
                            isOnGround = true;
                            Velocity.Y = 0;
                        }
                        else if (Velocity.Y < 0)
                        {
                            Position.Y = platform.Bottom;
                            Velocity.Y = 0;
                        }
                    }
                    else
                    {
                        if (Velocity.X > 0)
                        {
                            Position.X = platform.Left - Hitbox.Width;
                            Velocity.X = 0;
                        }
                        else if (Velocity.X < 0)
                        {
                            Position.X = platform.Right;
                            Velocity.X = 0;
                        }
                        Velocity.X = 0;
                    }
                }
            }
        }

        public void CheckEnemyCollisions(List<Enemy> enemies)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                if (Hitbox.Intersects(enemy.Hitbox))
                {
                    if (Velocity.Y > 0)
                    {
                        enemies.RemoveAt(i);
                        Velocity.Y = jumpForce / 2;
                    }
                    else
                    {
                        Position = new Vector2(100, 300);
                    }
                }
            }
        }
    }

    public class Enemy
    {
        public Vector2 Position;
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Enemy(Vector2 position)
        {
            Position = position;
        }
    }

    public class Camera
    {
        public Vector2 Position { get; private set; }
        private int screenWidth, screenHeight;

        public Camera(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            Position = Vector2.Zero;
        }

        public void Follow(Mario mario)
        {
            float newX = mario.Position.X - screenWidth / 2;

            if (newX < 0) newX = 0;

            Position = new Vector2(newX, Position.Y);
        }
    }
}
