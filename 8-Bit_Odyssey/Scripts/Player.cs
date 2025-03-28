using JumpMan;
using Bit_Odyssey;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bit_Odyssey.Scripts{
    public class Player
    {
        public Vector2 Position;
        private Vector2 Velocity;
        private float gravity = 0.4f;
        private float jumpForce = -10f;
        private bool isOnGround;
        private float fallLimit = 600;
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Player(Vector2 position)
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
}
