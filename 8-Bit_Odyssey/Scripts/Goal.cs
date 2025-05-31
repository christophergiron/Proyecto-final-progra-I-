using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
namespace Bit_Odyssey.Scripts
{
    public class Goal
    {
        public Rectangle Area { get; private set; }

        
        public Goal(Vector2 position)
        {
            int width = 32;   // ancho de la meta
            int height = 64;  // alto de la meta
            Area = new Rectangle((int)position.X, (int)position.Y, width, height);
        }

        
        public bool Contains(Vector2 position)
        {
            return Area.Contains(position);
        }
    }
}
