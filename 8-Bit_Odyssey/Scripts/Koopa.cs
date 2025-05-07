using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Bit_Odyssey.Scripts
{
    public class Koopa : Enemy
    {
        public bool IsInShell { get; private set; } = false;
        public bool IsMovingShell { get; private set; } = false;
        public override Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Koopa(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(-1.0f, 0);
        }

        public override void Update(GameTime gameTime, List<Rectangle> tileColliders)
        {
            if (IsInShell && !IsMovingShell)
            {
                Velocity.X = 0;
                Velocity.Y += gravity;
                Position += Velocity;
                CheckGround(tileColliders);
                return;
            }

            if (IsMovingShell)
            {
                Velocity.Y += gravity;
                Position += Velocity;

                IsOnGround = false;

                foreach (var tile in tileColliders)
                {
                    if (Hitbox.Intersects(tile))
                    {
                        Rectangle intersection = Rectangle.Intersect(Hitbox, tile);

                        if (intersection.Height < intersection.Width)
                        {
                            // Colisión horizontal: rebota
                            if (Velocity.X > 0)
                                Position.X = tile.Left - Hitbox.Width;
                            else
                                Position.X = tile.Right;

                            Velocity.X *= -1;
                        }
                        else
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
                    }
                }

                return;
            }

            // Koopa caminando normalmente (como Goomba)
            base.Update(gameTime, tileColliders);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            Color color = IsInShell ? (IsMovingShell ? Color.Orange : Color.Blue) : Color.Green;
            spriteBatch.Draw(pixel, Hitbox, color);
        }

        public void EnterShell()
        {
            IsInShell = true;
            IsMovingShell = false;
            Velocity = Vector2.Zero;
        }

        public void KickShell(int direction)
        {
            if (!IsInShell) return;

            IsMovingShell = true;
            Velocity = new Vector2(5 * direction, 0);
        }

        public void StopShell()
        {
            IsMovingShell = false;
            Velocity = Vector2.Zero;
        }

        private void CheckGround(List<Rectangle> tileColliders)
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
    }
}