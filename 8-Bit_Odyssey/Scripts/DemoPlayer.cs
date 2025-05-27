using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bit_Odyssey.Scripts;
using Microsoft.Xna.Framework.Graphics;


namespace Bit_Odyssey.Scripts
{
    internal class DemoPlayer : Player
    {
        private List<float> jumpPositionsX;
        private int currentJumpIndex = 0;

        public DemoPlayer(Vector2 position, Action onDeathCallback, List<float> jumpXs)
            : base(position, onDeathCallback)
        {
            this.jumpPositionsX = jumpXs ?? new List<float>();
        }

        public void Update(GameTime gameTime, List<Rectangle> tileColliders, List<Block> blocks, List<Enemy> enemies)
        {
            base.Update(gameTime, new KeyboardState());

            if (IsRespawning())
                return;

            // Movimiento automático hacia la derecha
            Velocity.X += 0.15f;
            if (Velocity.X > 6f) Velocity.X = 6f;

            HandleJumpTriggers();
            CheckCollisions(tileColliders, blocks);
            CheckEnemyCollisions(enemies);
        }

        private void HandleJumpTriggers()
        {
            if (currentJumpIndex < jumpPositionsX.Count &&
                Math.Abs(Position.X - jumpPositionsX[currentJumpIndex]) < 5 &&
                IsOnGround)
            {
                SimulateJump();
                currentJumpIndex++;
            }
        }

        private void SimulateJump()
        {
            Velocity.Y = -10f;
            IsOnGround = false;
            Music.PlayJumpFX();
        }

        public void ResetDemo(Vector2 startPosition)
        {
            Position = startPosition;
            Velocity = Vector2.Zero;
            IsOnGround = false;
            currentJumpIndex = 0;
        }
    }
}