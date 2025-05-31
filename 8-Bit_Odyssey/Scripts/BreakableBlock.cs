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
    //clasee reescrita
    public class BreakableBlock : Block
    {
        private bool _isBroken = false;

        // Para animación activada tras golpe
        private Texture2D[] animationFrames;
        private int currentFrame = 0;
        private double animationTimer = 0;
        private double frameDuration = 0.15; // tiempo por frame

        // Textura estática que se muestra antes de golpe
        private Texture2D staticTexture;

        // Control para saber si la animación está activa
        private bool isAnimating = false;

        public BreakableBlock(Rectangle bounds, Texture2D staticTex, Texture2D[] animFrames) : base(bounds)
        {
            staticTexture = staticTex;
            animationFrames = animFrames;
        }

        public override bool IsBroken => _isBroken;

        public override void OnHit(Player player)
        {
            if (!_isBroken)
            {
                _isBroken = true;
                isAnimating = true;
                ScoreManager.AddPoints(300);
                Music.PlayBreakFX();
            }
        }

        public void Update(GameTime gameTime)
        {
            if (!isAnimating) return;

            animationTimer += gameTime.ElapsedGameTime.TotalSeconds;

            if (animationTimer >= frameDuration)
            {
                animationTimer = 0;
                currentFrame++;

                // Termina la animación después del último frame
                if (currentFrame >= animationFrames.Length)
                {
                    isAnimating = false;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, Texture2D texture, Vector2 cameraPosition)
        {
            if (!isAnimating)
            {
                // Mostrar textura estática si no se está animando ni roto
                if (!_isBroken)
                {
                    spriteBatch.Draw(staticTexture, new Rectangle(
                        Bounds.X - (int)cameraPosition.X,
                        Bounds.Y,
                        Bounds.Width,
                        Bounds.Height), Color.White);
                }
            }
            else
            {
                // Mostrar animación si está activa
                if (currentFrame < animationFrames.Length)
                {
                    spriteBatch.Draw(animationFrames[currentFrame], new Rectangle(
                        Bounds.X - (int)cameraPosition.X,
                        Bounds.Y,
                        Bounds.Width,
                        Bounds.Height), Color.White);
                }
            }
        }
    }
}
