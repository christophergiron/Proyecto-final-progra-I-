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
        private Song runningAbout;
        private SoundEffect fxJump;
        public void Load(ContentManager content)
        {
            runningAbout = content.Load<Song>("Music/Overworld");
            fxJump = content.Load<SoundEffect>("SoundFX/Jump");
        }

        public void PlayMusic()
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(runningAbout);
        }

        public void PlayJumpFX()
        {
            fxJump.Play();
        }
    }
}
