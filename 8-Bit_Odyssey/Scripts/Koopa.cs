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
        private float shellTimer = 0f;
        private const float shellMaxTime = 5f;
        public bool IsInShell { get; private set; } = false;
        public bool IsMovingShell { get; private set; } = false;

        public override Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Koopa(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(-1.0f, 0);
            IsInShell = false;
            IsMovingShell = false;
        }

        public override void Update(GameTime gameTime, List<Rectangle> tileColliders)
        {
            if (!IsOnGround)
                Velocity.Y += gravity;

            if (!IsInShell)
            {
                shellTimer = 0f;
                Velocity.X = movingLeft ? -1.0f : 1.0f;
            }
            else if (!IsMovingShell)
            {
                Velocity.X = 0;
                shellTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (shellTimer >= shellMaxTime)
                {
                    ExitShell();
                }
            }

            Position.X += Velocity.X;
            Rectangle hitboxX = Hitbox;

            foreach (var tile in tileColliders)
            {
                if (hitboxX.Intersects(tile))
                {
                    if (Velocity.X > 0)
                        Position.X = tile.Left - Hitbox.Width;
                    else if (Velocity.X < 0)
                        Position.X = tile.Right;

                    if (!IsInShell)
                        movingLeft = !movingLeft;

                    if (IsMovingShell)
                        Velocity.X *= -1;

                    break;
                }
            }

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

        public void HandlePlayerCollision(Player player)
        {
            if (!Hitbox.Intersects(player.Hitbox)) return;

            bool fromAbove = player.Hitbox.Bottom <= this.Hitbox.Top + 6 && player.Velocity.Y > 0;

            if (!IsInShell)
            {
                if (fromAbove)
                {
                    EnterShell();
                    player.Velocity = new Vector2(player.Velocity.X, -6);
                    Music.PlaySquishFX();
                }
                else
                {
                    player.Die();
                }
            }
            else if (!IsMovingShell)
            {
                if (fromAbove)
                {
                    int direction = player.Position.X < this.Position.X ? 1 : -1;
                    KickShell(direction);
                    player.Velocity = new Vector2(player.Velocity.X, -6);
                    Music.PlaySquishFX();
                }
                else
                {
                    int direction = player.Position.X < this.Position.X ? 1 : -1;
                    KickShell(direction);
                }
            }
            else
            {
                if (fromAbove)
                {
                    StopShell();
                    player.Velocity = new Vector2(player.Velocity.X, -6);
                    Music.PlaySquishFX();
                }
                else
                {
                    player.Die();
                }
            }
        }

        public void HandleShellCollisions(List<Enemy> allEnemies)
        {
            if (!IsInShell || !IsMovingShell) return;

            for (int i = allEnemies.Count - 1; i >= 0; i--)
            {
                var other = allEnemies[i];
                if (other == this) continue;

                if (this.Hitbox.Intersects(other.Hitbox))
                {
                    allEnemies.RemoveAt(i);
                    Music.PlaySquishFX();
                }
            }
        }

        public void EnterShell()
        {
            IsInShell = true;
            IsMovingShell = false;
            Velocity = Vector2.Zero;
        }
        public void ExitShell()
        {
            IsInShell = false;
            IsMovingShell = false;
            shellTimer = 0f;
            Velocity.X = -1.0f;
        }
        public void KickShell(int direction)
        {
            IsMovingShell = true;
            Velocity = new Vector2(5 * direction, 0);
        }

        public void StopShell()
        {
            IsMovingShell = false;
            Velocity = Vector2.Zero;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            Color color = IsInShell ? (IsMovingShell ? Color.Orange : Color.Cyan) : Color.Green;
            spriteBatch.Draw(pixel, Hitbox, color);
        }
    }
}