using Bit_Odyssey.Scripts;
using JumpMan;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Bit_Odyssey.Scripts
{
    internal class DemoPlayer : Player
    {
        private List<float> jumpPositionsX;
        private int currentJumpIndex = 0;

        public DemoPlayer(Vector2 position, Action onDeathCallback, List<float> jumpXs)
            : base(position, onDeathCallback)
        {
            jumpPositionsX = jumpXs ?? new List<float>();
        }

        public void Update(GameTime gameTime, List<Rectangle> tileColliders, List<Block> blocks, List<Enemy> enemies)
        {
            base.Update(gameTime, new KeyboardState());

            if (IsRespawning())
                return;

            // Movimiento automático
            Velocity.X += 0.15f;
            if (Velocity.X > 6f) Velocity.X = 6f;

            HandleJumpTriggers();
            CheckCollisions(tileColliders, blocks);
            CheckEnemyCollisions(enemies);

            if (Position.Y > 600)
            {
                Die(); // Llama al Die() heredado del Player, que ya hace el respawn correctamente
            }
        }

        private void HandleJumpTriggers()
        {
            if (currentJumpIndex < jumpPositionsX.Count &&
                Math.Abs(Position.X - jumpPositionsX[currentJumpIndex]) < 5 &&
                IsOnGround)
            {
                Velocity.Y = -10f;
                IsOnGround = false;
                Music.PlayJumpFX();
                currentJumpIndex++;
            }
        }

        public void ResetDemo()
        {
            Position = Game1.playerSpawnPoint(); // se alinea con el Player
            Velocity = Vector2.Zero;
            currentJumpIndex = 0;
        }
    }
}