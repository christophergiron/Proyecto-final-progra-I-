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

        public override void Update(GameTime gameTime)
        {
            if (!IsInShell || IsMovingShell)
            {
                base.Update(gameTime);
            }
            else
            {
                Velocity = Vector2.Zero;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(pixel, Hitbox, Color.Green);
        }

        public void EnterShell()
        {
            IsInShell = true;
            IsMovingShell = false;
            Velocity = Vector2.Zero;
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
    }
}