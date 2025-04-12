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
        private List<Vector2> jumpTriggers;
        private int currentTriggerIndex = 0;
        private bool isInDemoRespawn = false;
        private double demoRespawnTimer = 0;
        private const double demoRespawnDelay = 2.0;

        // Nueva lista para guardar los movimientos y saltos
        private List<Action> actionHistory = new List<Action>();
        private int actionIndex = 0;

        public DemoPlayer(Vector2 position) : base(position)
        {
            jumpTriggers = new List<Vector2>
        {
            new Vector2(200, 0),
            new Vector2(500, 0)
        };
        }

        public void Update(GameTime gameTime, List<Rectangle> platforms, List<Enemy> enemies)
        {
            if (Position.Y > 600 && !isInDemoRespawn)
            {
                // Detecta caída como "muerte"
                isInDemoRespawn = true;
                demoRespawnTimer = demoRespawnDelay;
                return;
            }

            if (isInDemoRespawn)
            {
                demoRespawnTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                if (demoRespawnTimer <= 0)
                {
                    ResetDemo(new Vector2(100, 369));
                    isInDemoRespawn = false;
                }
                return;
            }

            // Si estamos en respawn, ejecutamos las acciones anteriores
            if (actionIndex < actionHistory.Count)
            {
                actionHistory[actionIndex].Invoke();
                actionIndex++;
            }
            else
            {
                // Si se acaba el historial, simulamos el movimiento como antes
                SimulateRightMovement();
                HandleJumpTriggers();
                HandleEnemyJump(enemies);
            }

            base.Update(gameTime, new KeyboardState());
            CheckCollisions(platforms);
            CheckEnemyCollisions(enemies);
        }

        private void SimulateRightMovement()
        {
            Velocity.X += 0.15f;
            if (Velocity.X > 6f) Velocity.X = 6f;

            // Guardamos el movimiento en el historial
            actionHistory.Add(() => Velocity.X += 0.15f);
        }

        private void SimulateJump()
        {
            if (IsOnGround)
            {
                Velocity.Y = -10f;
                IsOnGround = false;
                Music.PlayJumpFX();

                // Guardamos el salto en el historial
                actionHistory.Add(() => SimulateJump());
            }
        }

        private void HandleEnemyJump(List<Enemy> enemies)
        {
            foreach (var enemy in enemies)
            {
                if (Math.Abs(Position.X - enemy.Position.X) < 50 && IsOnGround)
                {
                    SimulateJump();
                }
            }
        }

        private void HandleJumpTriggers()
        {
            if (currentTriggerIndex < jumpTriggers.Count &&
                Position.X >= jumpTriggers[currentTriggerIndex].X &&
                IsOnGround)
            {
                SimulateJump();
                currentTriggerIndex++;
            }
        }

        private void ResetDemo(Vector2 startPosition)
        {
            Position = startPosition;
            Velocity = Vector2.Zero;
            IsOnGround = false;
            currentTriggerIndex = 0;

            // Limpiamos el historial de acciones
            actionHistory.Clear();
            actionIndex = 0;
        }
    }
}