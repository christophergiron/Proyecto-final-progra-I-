using JumpMan;
using Bit_Odyssey;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Bit_Odyssey.Scripts;

namespace Bit_Odyssey.Scripts
{
    public class Player
    {
        public Vector2 Position;
        public Vector2 Velocity;
        private float gravity = 0.4f;
        private float jumpForce = -8f;
        private bool jumpHeld = false;
        private float jumpTime = 0f;
        private float maxJumpTime = 0.25f;
        public bool IsOnGround;
        private float fallLimit = 600;
        public bool isRespawning = false;
        private double respawnTimer = 0;
        private const double respawnDelay = 2.0;
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
        private Action onDeathCallback;

        // Animación
        private Dictionary<string, List<Texture2D>> animations;
        private string currentAnimation = "idle";
        private int frameIndex = 0;
        private double frameTimer = 0;
        private double frameInterval = 0.1; // segundos por frame
        private SpriteEffects spriteEffect = SpriteEffects.None;

        public Player(Vector2 position, Action onDeath = null)
        {
            Position = position;
            onDeathCallback = onDeath;
        }

        public void LoadAnimations(Dictionary<string, List<Texture2D>> anims)
        {
            animations = anims;
        }

        public bool IsRespawning() => isRespawning;

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
                {
                    isRespawning = false;
                }
                return;
            }

            // Movimiento horizontal y animaciones
            bool moving = false;

            if (keyboard.IsKeyDown(Keys.Left))
            {
                Velocity.X -= acceleration;
                if (Velocity.X < -targetSpeed) Velocity.X = -targetSpeed;
                currentAnimation = "walk_left";
                spriteEffect = SpriteEffects.None;
                moving = true;
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                Velocity.X += acceleration;
                if (Velocity.X > targetSpeed) Velocity.X = targetSpeed;
                currentAnimation = "walk_right";
                spriteEffect = SpriteEffects.None;
                moving = true;
            }
            else
            {
                if (Velocity.X > 0)
                {
                    Velocity.X -= deceleration;
                    if (Velocity.X < 0) Velocity.X = 0;
                }
                else if (Velocity.X < 0)
                {
                    Velocity.X += deceleration;
                    if (Velocity.X > 0) Velocity.X = 0;
                }

                if (!moving)
                {
                    currentAnimation = "idle";
                }
            }

            // Salto
            if (keyboard.IsKeyDown(Keys.S) && IsOnGround)
            {
                Velocity.Y = jumpForce;
                IsOnGround = false;
                jumpHeld = true;
                jumpTime = 0f;
                Music.PlayJumpFX();
            }

            //salto mantenido
            if (keyboard.IsKeyDown(Keys.S) && jumpHeld)
            {
                jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (jumpTime < maxJumpTime)
                {
                    Velocity.Y -= 0.3f;
                }
            }
            else
            {
                jumpHeld = false;
            }

            if (!IsOnGround)
            {
                Velocity.Y += gravity;
            }

            if (Position.Y > fallLimit)
            {
                Die();
            }

            // Animación por tiempo
            frameTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (frameTimer >= frameInterval)
            {
                frameTimer = 0;
                frameIndex++;
                if (animations != null && animations.ContainsKey(currentAnimation))
                {
                    if (frameIndex >= animations[currentAnimation].Count)
                        frameIndex = 0;
                }
            }
        }

        public void CheckCollisions(List<Rectangle> tileColliders, List<Block> blocks)
        {
            Position.X += Velocity.X;
            Rectangle hitboxX = Hitbox;

            foreach (var tile in tileColliders)
            {
                if (hitboxX.Intersects(tile))
                {
                    if (Velocity.X > 0)
                        Position.X = tile.Left - hitboxX.Width;
                    else if (Velocity.X < 0)
                        Position.X = tile.Right;
                    Velocity.X = 0;
                    hitboxX = Hitbox;
                }
            }

            foreach (var block in blocks)
            {
                if (block.IsBroken) continue;
                if (hitboxX.Intersects(block.Bounds))
                {
                    if (Velocity.X > 0)
                        Position.X = block.Bounds.Left - hitboxX.Width;
                    else if (Velocity.X < 0)
                        Position.X = block.Bounds.Right;
                    Velocity.X = 0;
                    hitboxX = Hitbox;
                }
            }

            Position.Y += Velocity.Y;
            Rectangle hitboxY = Hitbox;
            IsOnGround = false;

            foreach (var tile in tileColliders)
            {
                if (hitboxY.Intersects(tile))
                {
                    if (Velocity.Y > 0)
                    {
                        Position.Y = tile.Top - hitboxY.Height;
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

            foreach (var block in blocks)
            {
                if (block.IsBroken) continue;
                if (hitboxY.Intersects(block.Bounds))
                {
                    if (Velocity.Y > 0)
                    {
                        Position.Y = block.Bounds.Top - hitboxY.Height;
                        IsOnGround = true;
                        Velocity.Y = 0;
                    }
                    else if (Velocity.Y < 0)
                    {
                        Rectangle head = new Rectangle(Hitbox.X, Hitbox.Y - 2, Hitbox.Width, 5);
                        if (head.Intersects(block.Bounds) && block.IsSolid)
                        {
                            block.OnHit(this);
                        }
                        Position.Y = block.Bounds.Bottom;
                        Velocity.Y = 0;
                    }
                    hitboxY = Hitbox;
                }
            }
        }

        public void CheckEnemyCollisions(List<Enemy> enemies)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                Enemy enemy = enemies[i];
                if (!Hitbox.Intersects(enemy.Hitbox)) continue;

                Rectangle intersection = Rectangle.Intersect(Hitbox, enemy.Hitbox);

                bool isAbove = Hitbox.Bottom <= enemy.Hitbox.Top + 10 && Velocity.Y > 1f;

                if (enemy is Goomba)
                {
                    if (isAbove)
                    {
                        enemies.RemoveAt(i);
                        Rebote();
                        Music.PlaySquishFX();
                        ScoreManager.AddPoints(200);
                    }
                    else
                    {
                        Die();
                    }
                }
                else if (enemy is Koopa k)
                {
                    if (isAbove)
                    {
                        k.HandlePlayerCollision(this);
                        Rebote();
                        Music.PlaySquishFX();
                    }
                    else
                    {
                        k.HandlePlayerCollision(this);
                    }
                }
            }
        }
        public void Rebote(float force = 6f)
        {
            Velocity.Y = -force;
        }
        public void Die()
        {
            Position = Game1.playerSpawnPoint();
            Velocity = Vector2.Zero;
            isRespawning = true;
            respawnTimer = respawnDelay;

            onDeathCallback?.Invoke();
            Music.StopMusic();
            Music.PlayDeath();
            Music.ResetMusic((float)Music.death.Duration.TotalSeconds);
            Console.WriteLine($" Game Over - Puntos: {ScoreManager.Points} | Monedas: {ScoreManager.Coins}");
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (animations != null && animations.ContainsKey(currentAnimation))
            {
                Texture2D currentFrame = animations[currentAnimation][frameIndex];
                spriteBatch.Draw(currentFrame, Position, null, Color.White, 0f, Vector2.Zero, 1f, spriteEffect, 0f);
            }
        }
    }
}
