using JumpMan;
using Bit_Odyssey;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bit_Odyssey.Scripts
{
    public class Camera
    {
        public Vector2 Position { get; private set; }
        private int screenWidth, screenHeight;

        public Camera(int screenWidth, int screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            Position = Vector2.Zero;
        }

        public void Follow(Player player)
        {
            float newX = player.Position.X - screenWidth / 2;

            if (newX < 0) newX = 0;

            Position = new Vector2(newX, Position.Y);
        }
    }
}
