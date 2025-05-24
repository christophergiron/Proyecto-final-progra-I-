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
    public class BreakableBlock : Block
    {
        private bool broken = false;

        public BreakableBlock(Rectangle bounds) : base(bounds) { }

        public override bool IsBroken => broken;

        public override void OnHit(Player player)
        {
            broken = true;
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 cameraPosition)
        {
            if (IsBroken) return;

            base.Draw(spriteBatch, texture, cameraPosition);
        }
    }
}
