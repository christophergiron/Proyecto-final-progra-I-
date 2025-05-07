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
        public override Rectangle Hitbox => new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

        public Goomba(Vector2 position)
        {
            Position = position;
            Velocity = new Vector2(-1.0f, 0);
        }

        public override void Update(GameTime gameTime, List<Rectangle> tileColliders)
        {
            base.Update(gameTime, tileColliders);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(pixel, Hitbox, Color.SaddleBrown);
        }
    }
}