using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bit_Odyssey.Scripts;

namespace Bit_Odyssey.Scripts
{
    internal class DemoController
    {
        private double idleTime;
        private double demoThreshold = 5; // segundos sin input antes de entrar a demo
        public bool InDemoMode { get; private set; }

        public void Update(GameTime gameTime, KeyboardState keyboard)
        {
            if (keyboard.GetPressedKeys().Length > 0)
            {
                idleTime = 0;
                InDemoMode = false;
            }
            else
            {
                idleTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (idleTime >= demoThreshold)
                {
                    InDemoMode = true;
                }
            }
        }

        public void Reset()
        {
            idleTime = 0;
            InDemoMode = false;
        }
    }

}
