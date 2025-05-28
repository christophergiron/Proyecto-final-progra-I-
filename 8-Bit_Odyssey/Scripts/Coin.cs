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
    public class Coin
    {
        public Vector2 Position;
        public Rectangle Bounds => new Rectangle((int)Position.X, (int)Position.Y, 16, 16);
        public bool Collected { get; private set; } = false;

        public Coin(Vector2 position)
        {
            Position = position;
        }

        public void Update(Player player)
        {
            if (!Collected && player.Hitbox.Intersects(Bounds))
            {
                Collected = true;
                ScoreManager.AddCoin();
                Music.PlayCoinFX(); // Si tienes un efecto
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 camera)
        {
            if (!Collected)
            {
                spriteBatch.Draw(texture,
                    new Rectangle((int)(Position.X - camera.X), (int)Position.Y, 16, 16),
                    Color.Gold);
            }
        }
    }
}
