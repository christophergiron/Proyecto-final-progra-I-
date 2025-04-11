using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;


namespace Bit_Odyssey.Scripts
{
    internal class Music
    {
        private static Song runningAbout;
        private static SoundEffect fxJump;
        private static SoundEffect fxSquish;
        private static SoundEffect fxDie;
        public static void Load(ContentManager content)
        {
            runningAbout = content.Load<Song>("Music/Overworld");
            fxJump = content.Load<SoundEffect>("SoundFX/Jump");
            fxSquish = content.Load<SoundEffect>("SoundFX/Squish");
            fxDie = content.Load<SoundEffect>("SoundFX/Die");
        }

        public static void PlayMusic()
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(runningAbout);
        }

        public static void PlayJumpFX()
        {
            fxJump.Play();
        }
        public static void PlaySquishFX()
        {
            fxSquish.Play();
        }
        public static void PlayDieFX()
        {
            fxDie.Play();
        }
    }
}
