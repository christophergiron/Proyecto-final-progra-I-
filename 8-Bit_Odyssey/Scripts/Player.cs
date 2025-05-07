using JumpMan;
using Bit_Odyssey;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using System.Reflection.Metadata;

namespace Bit_Odyssey.Scripts
{
    public class Player
    {
        public Vector2 Position;
        public Vector2 Velocity;
        private float gravity = 0.4f;
        private float jumpForce = -10f;
        protected bool IsOnGround;
        private float fallLimit = 600;
        private bool isRespawning = false;
        private double respawnTimer = 0;
        private const double respawnDelay = 2.0;
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        private Action onDeathCallback;

        public Player(Vector2 position, Action onDeath = null)
        {
            Position = position;
            onDeathCallback = onDeath;
        }

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float acceleration = 0.15f;
            float deceleration = 0.1f;
            float maxSpeed = 6f;
            float walkSpeed = 3f;
            float targetSpeed = keyboard.IsKeyDown(Keys.A) ? maxSpeed : walkSpeed;

            if (isRespawning)
            {
                respawnTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (respawnTimer <= 0)
                    isRespawning = false;
                return;
            }

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

            if (!IsOnGround)
                Velocity.Y += gravity;

            if (keyboard.IsKeyDown(Keys.S) && IsOnGround)
            {
                Velocity.Y = jumpForce;
                IsOnGround = false;
                Music.PlayJumpFX();
            }

            Position += Velocity;

            if (Position.Y > fallLimit)
            {
                Die();
            }
        }

        public virtual void CheckTileCollisions(List<Rectangle> tileColliders)
        {
            IsOnGround = false;
            foreach (var tile in tileColliders)
            {
                if (Hitbox.Intersects(tile))
                {
                    Rectangle intersection = Rectangle.Intersect(Hitbox, tile);

                    if (intersection.Height < intersection.Width)
                    {
                        if (Velocity.Y > 0)
                        {
                            Position.Y = tile.Top - Hitbox.Height;
                            IsOnGround = true;
                            Velocity.Y = 0;
                        }
                        else if (Velocity.Y < 0)
                        {
                            Position.Y = tile.Bottom;
                            Velocity.Y = 0;
                        }
                    }
                    else
                    {
                        if (Velocity.X > 0)
                        {
                            Position.X = tile.Left - Hitbox.Width;
                        }
                        else if (Velocity.X < 0)
                        {
                            Position.X = tile.Right;
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
                if (!Hitbox.Intersects(enemy.Hitbox))
                    continue;

                if (enemy is Goomba)
                {
                    if (Velocity.Y > 0)
                    {
                        enemies.RemoveAt(i);
                        Velocity.Y = jumpForce / 2;
                        Music.PlaySquishFX();
                    }
                    else
                    {
                        Die();
                    }
                }
                else if (enemy is Koopa k)
                {
                    if (!k.IsInShell)
                    {
                        if (Velocity.Y > 0)
                        {
                            k.EnterShell();
                            Velocity.Y = jumpForce / 2;
                            Music.PlaySquishFX();
                        }
                        else
                        {
                            Die();
                        }
                    }
                    else
                    {
                        if (!k.IsMovingShell)
                        {
                            // Solo se puede patear desde los lados, no desde arriba
                            if (Math.Abs(Hitbox.Center.Y - k.Hitbox.Center.Y) < 10)
                            {
                                int direction = (Hitbox.Center.X < k.Hitbox.Center.X) ? 1 : -1;
                                k.KickShell(direction);
                                Velocity.Y = jumpForce / 2;
                            }
                            else
                            {
                                Die();
                            }
                        }
                        else
                        {
                            // Caparazón en movimiento: solo mata si viene hacia el jugador
                            if ((k.Velocity.X > 0 && Hitbox.Center.X > k.Hitbox.Center.X) ||
                                (k.Velocity.X < 0 && Hitbox.Center.X < k.Hitbox.Center.X))
                            {
                                Die();
                            }
                            else
                            {
                                // Viene desde atrás, no muere
                                Velocity.Y = jumpForce / 2;
                            }
                        }
                    }
                }
            }
        }

        public void Die()
        {
            Position = new Vector2(100, 369);
            Velocity = Vector2.Zero;
            isRespawning = true;
            respawnTimer = respawnDelay;

            onDeathCallback?.Invoke();
            Music.StopMusic();
            Music.PlayDieFX();
            Music.ResetMusic((float)Music.fxDie.Duration.TotalSeconds);
        }
    }
}

