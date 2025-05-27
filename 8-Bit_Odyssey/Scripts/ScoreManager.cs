using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bit_Odyssey.Scripts
{
    public static class ScoreManager
    {
        public static int Points { get; private set; } = 0;
        public static int Coins { get; private set; } = 0;

        public static void AddPoints(int amount)
        {
            Points += amount;
            Console.WriteLine($"Puntos: {Points}");
        }

        public static void AddCoin()
        {
            Coins++;
            AddPoints(100); // valor de puntos de las monedas
            Console.WriteLine($"Monedas: {Coins}");
        }

        public static void Reset()
        {
            Points = 0;
            Coins = 0;
        }
    }
}
