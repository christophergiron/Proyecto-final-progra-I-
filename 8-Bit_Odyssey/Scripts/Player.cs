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
using Bit_Odyssey.Scripts;

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
        public bool isRespawning = false;
        private double respawnTimer = 0;
        private const double respawnDelay = 2.0;
        public Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
        public bool IsRespawning()
        {
            return isRespawning;
        }

        private Action onDeathCallback;

        public Player(Vector2 position, Action onDeath = null)
        {
            Position = position;
            onDeathCallback = onDeath;
        }
        //fisicas y logica de player 
        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            float acceleration = 0.15f;
            float deceleration = 0.1f;
            float maxSpeed = 6f;
            float walkSpeed = 3f;
            float targetSpeed =  keyboard.IsKeyDown(Keys.A) ? maxSpeed : walkSpeed;
            //controla el tiempo de respawn
            if (isRespawning)
            {
                respawnTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (respawnTimer <= 0)
                {
                    isRespawning = false;
                }
                return;
            }
            //maneja el movimiento lateral del player 
            if (keyboard.IsKeyDown(Keys.Left))
            {
                Velocity.X -= acceleration;
                if (Velocity.X < -targetSpeed)
                {
                    Velocity.X = -targetSpeed;
                }
            }
            else if (keyboard.IsKeyDown(Keys.Right))
            {
                Velocity.X += acceleration;
                if (Velocity.X > targetSpeed)
                {
                    Velocity.X = targetSpeed;
                }
            }
            else
            {
                if (Velocity.X > 0)
                {
                    Velocity.X -= deceleration;
                    if (Velocity.X < 0)
                    {
                        Velocity.X = 0;
                    }
                }
                else if (Velocity.X < 0)
                {
                    Velocity.X += deceleration;
                    if (Velocity.X > 0)
                    {
                        Velocity.X = 0;
                    }
                }
            }
            //maneja el salto de player
            if (keyboard.IsKeyDown(Keys.S) && IsOnGround)
            {
                Velocity.Y = jumpForce;
                IsOnGround = false;
                Music.PlayJumpFX();
            }

            if (!IsOnGround)
            {
                Velocity.Y += gravity;
            }


            if (Position.Y > fallLimit)
            {
                Die();
            }
        }
        //Se encarga de las colisiones e interaccion con bloques
        public void CheckCollisions(List<Rectangle> tileColliders, List<Block> blocks)
        {
            // Movimiento horizontal
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

            // Movimiento vertical
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
                        // Colisión desde arriba
                        Position.Y = block.Bounds.Top - hitboxY.Height;
                        IsOnGround = true;
                        Velocity.Y = 0;
                    }
                    else if (Velocity.Y < 0)
                    {
                        // Golpe desde abajo: romper el bloque
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

        //maneja la interaccion con los enemigos 
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
                        ScoreManager.AddPoints(200);
                    }
                    else
                    {
                        Die();
                    }
                }
                else if (enemy is Koopa k)
                {
                    k.HandlePlayerCollision(this);
                    Music.PlaySquishFX();
                }
            }
        }
        //maneja la muerte del player
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
            Console.WriteLine($" Game Over - Puntos: {ScoreManager.Points} | Monedas: {ScoreManager.Coins}");
        }
    }
}