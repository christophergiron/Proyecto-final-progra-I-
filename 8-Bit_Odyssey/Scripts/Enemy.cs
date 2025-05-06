using JumpMan;
using Bit_Odyssey;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Bit_Odyssey.Scripts
{
    public abstract class Enemy
    {
        public Vector2 Position;
        public Vector2 Velocity;
        protected bool IsOnGround;
        protected float gravity = 0.4f;
        public virtual Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
        protected bool movingLeft = true; // Dirección inicial

        public virtual void Update(GameTime gameTime, List<Rectangle> tileColliders)
        {
            if (!IsOnGround)
                Velocity.Y += gravity;

            // Movimiento lateral automático
            Velocity.X = movingLeft ? -1.0f : 1.0f;

            Position += Velocity;

            IsOnGround = false;

            foreach (var tile in tileColliders)
            {
                if (Hitbox.Intersects(tile))
                {
                    Rectangle intersection = Rectangle.Intersect(Hitbox, tile);

                    if (intersection.Height < intersection.Width)
                    {
                        // Colisión vertical
                        if (Velocity.Y > 0)
                        {
                            Position.Y = tile.Top - Hitbox.Height;
                            Velocity.Y = 0;
                            IsOnGround = true;
                        }
                        else if (Velocity.Y < 0)
                        {
                            Position.Y = tile.Bottom;
                            Velocity.Y = 0;
                        }
                    }
                    else
                    {
                        // Colisión horizontal → invertir dirección
                        if (Velocity.X > 0)
                        {
                            Position.X = tile.Left - Hitbox.Width;
                        }
                        else
                        {
                            Position.X = tile.Right;
                        }

                        movingLeft = !movingLeft;
                        Velocity.X = 0;
                    }
                }
            }
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }

}