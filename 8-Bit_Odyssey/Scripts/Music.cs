using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;


namespace Bit_Odyssey.Scripts
{
    internal class Music
    {
        private static Song runningAbout;
        private static SoundEffect fxJump;
        private static SoundEffect fxSquish;
        private static Song underGround;
        public static SoundEffect fxDie;
        public static SoundEffect fxCoin;
        private static bool esperaReset = false;
        private static float resetMusic = 0f;
        
        public static void Load(ContentManager content)
        {
            runningAbout = content.Load<Song>("Music/Overworld");
            fxJump = content.Load<SoundEffect>("SoundFX/Jump");
            fxSquish = content.Load<SoundEffect>("SoundFX/Squish");
            fxDie = content.Load<SoundEffect>("SoundFX/Die");
            underGround = content.Load<Song>("Music/Underground");
            fxCoin = content.Load<SoundEffect>("SoundFX/Coin");
        }

        public static void PlayMusicOverWorld()
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(runningAbout);
        }
        public static void PlayMusicUnderGroud()
        {
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(underGround);
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
        public static void StopMusic()
        {
            MediaPlayer.Stop();
        }
        public static void PlayCoinFX()
        {
            fxCoin.Play();
        }
        public static void ResetMusic(float deathFXDuration)
        {
            esperaReset = true;
            resetMusic = deathFXDuration;
        }

        public static void Update(GameTime gameTime)
        {
            if (esperaReset)
            {
                resetMusic -= (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (resetMusic <= 0)
                {
                    PlayMusicOverWorld();
                    esperaReset = false;
                }
            }
        }
    }
}
