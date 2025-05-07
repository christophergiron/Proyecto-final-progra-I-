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
        private float jumpForce = -11f;
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

            if (keyboard.IsKeyDown(Keys.S) && IsOnGround)
            {
                Velocity.Y = jumpForce;
                IsOnGround = false;
                Music.PlayJumpFX();
            }

            if (!IsOnGround)
                Velocity.Y += gravity;

            if (Position.Y > fallLimit)
                Die();
        }

        public virtual void CheckTileCollisions(List<Rectangle> tileColliders)
        {
            IsOnGround = false;

            Position.X += Velocity.X;
            Rectangle hitboxX = Hitbox;

            foreach (var tile in tileColliders)
            {
                if (hitboxX.Intersects(tile))
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
                    hitboxX = Hitbox;
                }
            }

            Position.Y += Velocity.Y;
            Rectangle hitboxY = Hitbox;

            foreach (var tile in tileColliders)
            {
                if (hitboxY.Intersects(tile))
                {
                    if (Velocity.Y > 0)
                    {
                        Position.Y = tile.Top - Hitbox.Height;
                        IsOnGround = true;
                    }
                    else if (Velocity.Y < 0)
                    {
                        Position.Y = tile.Bottom;
                    }
                    Velocity.Y = 0;
                    hitboxY = Hitbox;
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
                    k.HandlePlayerCollision(this);
                }
            }
        }
        public void CheckBreakableBlocks(List<Rectangle> breakables)
        {
            for (int i = breakables.Count - 1; i >= 0; i--)
            {
                Rectangle block = breakables[i];
                Rectangle head = new Rectangle(Hitbox.X, Hitbox.Y, Hitbox.Width, 5);

                if (Velocity.Y < 0 && head.Intersects(block))
                {
                    breakables.RemoveAt(i);
                    Velocity.Y = 0;
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