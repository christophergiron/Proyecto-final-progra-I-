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

        private List<Texture2D> animationFrames;
        private int currentFrame = 0;
        private double animationTimer = 0;
        private double frameDuration = 0.1; // Tiempo por frame en segundos

        public Coin(Vector2 position, List<Texture2D> frames)
        {
            Position = position;
            animationFrames = frames;
        }

        public void Update(Player player, GameTime gameTime)
        {
            if (!Collected && player.Hitbox.Intersects(Bounds))
            {
                Collected = true;
                ScoreManager.AddCoin();
                Music.PlayCoinFX();
            }

            // Animación
            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;
            if (animationTimer >= frameDuration)
            {
                currentFrame = (currentFrame + 1) % animationFrames.Count;
                animationTimer = 0;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 cameraPosition)
        {
            if (!Collected)
            {
                var frame = animationFrames[currentFrame];
                spriteBatch.Draw(frame, new Vector2(Position.X - cameraPosition.X, Position.Y), Color.White);
            }
        }
    }
}
