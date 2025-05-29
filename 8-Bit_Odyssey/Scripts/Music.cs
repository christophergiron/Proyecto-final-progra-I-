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
        private static Song runningAbout;               //Todo esto y lo de abajo son las clases para que se ejecuten ya sea la musica o los efectos
        private static SoundEffect fxJump;
        private static SoundEffect fxSquish;
        private static Song underGround;
        public static SoundEffect fxDie;
        public static SoundEffect fxCoin;
        public static SoundEffect fxBreak;
        private static bool esperaReset = false;
        private static float resetMusic = 0f;
        
        public static void Load(ContentManager content) //aqui se cargan los archivos de la misma
        {
            runningAbout = content.Load<Song>("Music/Overworld");
            fxJump = content.Load<SoundEffect>("SoundFX/Jump");
            fxSquish = content.Load<SoundEffect>("SoundFX/Squish");
            fxDie = content.Load<SoundEffect>("SoundFX/Die");
            underGround = content.Load<Song>("Music/Underground");
            fxCoin = content.Load<SoundEffect>("SoundFX/Coin");
            fxBreak = content.Load<SoundEffect>("SoundFX/Break");
        }

        public static void PlayMusicOverWorld() //es muy obvio que se reproduce cada cancion o efecto correspondiente
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
        public static void PlayBreakFX()
        { 
            fxBreak.Play(); 
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
        public static void ResetMusic(float deathFXDuration)  //esto hace que se repita la musica cuando se muere
        {
            esperaReset = true;
            resetMusic = deathFXDuration;
        }

        public static void Update(GameTime gameTime) //hace que se repita la musica de forma normal, solo falta hacer que se reproduscan las versiones cuando queden 100 segundos, ojala se me olvide quitar esto                                                                                                                                                                            El balatreo 🃏
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
