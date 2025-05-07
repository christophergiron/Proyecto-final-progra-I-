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
        protected bool movingLeft = true;

        public virtual Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public virtual void Update(GameTime gameTime, List<Rectangle> tileColliders)
        {
            if (!IsOnGround)
                Velocity.Y += gravity;

            Velocity.X = movingLeft ? -1.0f : 1.0f;

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
                    movingLeft = !movingLeft;
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
                        Position.Y = tile.Top - Hitbox.Height;
                        Velocity.Y = 0;
                        IsOnGround = true;
                    }
                    else if (Velocity.Y < 0)
                    {
                        Position.Y = tile.Bottom;
                        Velocity.Y = 0;
                    }
                    hitboxY = Hitbox;
                }
            }
        }

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}