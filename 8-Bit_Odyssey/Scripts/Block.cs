using JumpMan;
using Bit_Odyssey;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using System.Reflection.Metadata;

namespace Bit_Odyssey.Scripts
{
    public abstract class Block
    {
        public Rectangle Bounds;
        public bool IsSolid => !IsBroken;

        public virtual bool IsBroken => false;

        public Block(Rectangle bounds)
        {
            Bounds = bounds;
        }

        public abstract void OnHit(Player player);

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 cameraPosition)
        {
            spriteBatch.Draw(texture, new Rectangle(
                Bounds.X - (int)cameraPosition.X,
                Bounds.Y,
                Bounds.Width,
                Bounds.Height
            ), Color.Yellow);
        }
    }
}