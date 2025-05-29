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
        private bool _isBroken = false;

        public BreakableBlock(Rectangle bounds) : base(bounds) { }

        public override bool IsBroken => _isBroken;

        public override void OnHit(Player player)
        {
            _isBroken = true;
           // Music.PlayBreakFX();// ahi quitan el comentario, que no se les olvide porque no lo puedo comprobar si no hay para romper
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 cameraPosition)
        {
            if (_isBroken) return;

            spriteBatch.Draw(texture, new Rectangle(
                Bounds.X - (int)cameraPosition.X,
                Bounds.Y,
                Bounds.Width,
                Bounds.Height
            ), Color.Yellow);
        }
    }
}
