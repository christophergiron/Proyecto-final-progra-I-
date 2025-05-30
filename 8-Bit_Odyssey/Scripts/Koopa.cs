using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics.Eventing.Reader;

namespace Bit_Odyssey.Scripts
{
    public class Koopa : Enemy
    {
        private float shellTimer = 0f;
        private const float shellMaxTime = 5f;
        private float shellEntryCooldown = 0f;
        private float bounceCooldown = 0f;
        private const float bounceCooldownMax = 0.2f;

        private int Direction = 0;

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

            if (shellEntryCooldown > 0)
                shellEntryCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (bounceCooldown > 0)
                bounceCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

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
            else
            {
                Velocity.X = 5f * Direction;
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
                        Direction *= -1;

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
            if (!player.Hitbox.Intersects(Hitbox)) return;

            Rectangle intersection = Rectangle.Intersect(player.Hitbox, Hitbox);
            bool isFromAbove = player.Hitbox.Bottom <= this.Hitbox.Top + 10;

            if (!IsInShell)
            {
                if (isFromAbove)
                {
                    EnterShell();
                    shellEntryCooldown = 0.2f;
                    player.Velocity = new Vector2(player.Velocity.X, -5f);
                }
                else
                {
                    player.Die();
                }
            }
            else
            {
                if (shellEntryCooldown > 0)
                    return;

                if (isFromAbove)
                {
                    if (!IsMovingShell && bounceCooldown <= 0f)
                    {
                        int dir = player.Position.X < Position.X ? 1 : -1;
                        KickShell(dir);
                        player.Velocity = new Vector2(player.Velocity.X, -6f);
                        player.Position.X -= dir * 5;
                        bounceCooldown = bounceCooldownMax;
                    }
                    else if (IsMovingShell && bounceCooldown <= 0f)
                    {
                        StopShell();
                        player.Velocity = new Vector2(player.Velocity.X, -6f);
                        player.Position.X -= (player.Position.X < Position.X ? 1 : -1) * 5;
                        shellTimer = 0f;
                        bounceCooldown = bounceCooldownMax;
                    }
                }
                else
                {
                    if (IsMovingShell)
                    {
                        player.Die();
                    }
                    else
                    {
                        int dir = player.Position.X < Position.X ? 1 : -1;
                        KickShell(dir);
                        player.Position.X -= dir * 10;
                    }
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
            Direction = direction;
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