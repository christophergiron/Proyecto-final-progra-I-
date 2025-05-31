using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Bit_Odyssey.Scripts
{
    public class Goomba : Enemy
    {
        private Texture2D walkTexture;  // <-- textura del sprite

        public override Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Goomba(Vector2 position, Texture2D walkTexture)
        {
            Position = position;
            Velocity = new Vector2(-1.0f, 0);
            this.walkTexture = walkTexture;  // asignar la textura recibida
        }

        public override void Update(GameTime gameTime, List<Rectangle> tileColliders)
        {
            if (!IsOnGround)
                Velocity.Y += gravity;

            // Movimiento lateral
            Velocity.X = movingLeft ? -1.0f : 1.0f;

            // --- Colisión eje X ---
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
                    movingLeft = !movingLeft;
                    break;
                }
            }

            // --- Colisión eje Y ---
            Position.Y += Velocity.Y;
            Rectangle hitboxY = Hitbox;
            IsOnGround = false;

            foreach (var tile in tileColliders)
            {
                if (hitboxY.Intersects(tile))
                {
                    Rectangle intersection = Rectangle.Intersect(hitboxY, tile);
                    if (intersection.Height < intersection.Width)
                    {
                        if (Velocity.Y > 0)
                        {
                            Position.Y = tile.Top - hitboxY.Height;
                            Velocity.Y = 0;
                            IsOnGround = true;
                        }
                        else if (Velocity.Y < 0)
                        {
                            Position.Y = tile.Bottom;
                            Velocity.Y = 0;
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(pixel, Hitbox, Color.SaddleBrown);
        }
    }
}