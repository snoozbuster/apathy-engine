using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework;
using System.Threading;
using Microsoft.Xna.Framework.Content;
using System.IO;
using ApathyEngine.Input;
using ApathyEngine.Utilities;

namespace ApathyEngine.Media
{
    public static class MediaSystem
    {
        /// <summary>
        /// True if there is playing voice acting. False if there is not.
        /// </summary>
        public static bool PlayingVoiceActing { get { if(playingVoiceActing != null) return !(playingVoiceActing.State == SoundState.Paused || playingVoiceActing.State == SoundState.Stopped) && !InputManager.WindowsOptions.Muted; return false; } }
        /// <summary>
        /// True if the siren for nearly losing is going.
        /// </summary>
        public static bool SirenPlaying { get { if(siren != null) return siren.State == SoundState.Playing; return false; } }

        /// <summary>
        /// True if the BGM is currently playing.
        /// </summary>
        public static bool PlayingBGM { get { if(playingBGM == null) return false; return playingBGM.State == SoundState.Playing || (playingBGMintro != null && playingBGMintro.State == SoundState.Playing); } }

        /// <summary>
        /// True if the music is custom.
        /// </summary>
        public static bool PlayingCustomMusic { get; private set; }

        #region Private Members
        private static MediaLibrary library;
        //private static AudioEngine engine;
        //private static WaveBank bgmWaveBank;
        //private static SoundBank bgmSoundBank;
        private static SoundEffectInstance playingBGM;
        private static SoundEffectInstance playingBGMintro;
        private static SoundEffectInstance playingBGMoutro;
        private static SoundEffectInstance crossfadingFrom;
        private static SoundEffectInstance crossfadingTo;
        private static SoundEffectInstance playingBGMbackup;
        private static bool hasIntro;
        private static bool hasOutro;
        //private static SoundEffectInstance playingAmbience;
        private static SoundEffectInstance siren;
        //private static WaveBank sfxWaveBank;
        //private static SoundBank sfxSoundBank;
        private static SoundEffectInstance playingVoiceActing;
        private static int level;
        private static List<MachineSoundInstance> autonomousList = new List<MachineSoundInstance>();

        private static Dictionary<int, MachineSoundInstance> machineDict;
        private static SoundEffectInstance winSound;
        private static float volToReturnTo = 0;
        //private static float volToReturnToSFX;
        private static bool shouldQuietMusic = false;
        //private static bool initallyQuiet = false;

        private static SongOptions playingSong = SongOptions.Credits;

//#if WINDOWS
//        private static readonly string path = "Content\\Music\\";
//#elif XBOX
//        private static readonly string path = "??";
//#endif

        private static GameState stateLastFrame;
        private static bool playingAlbum;

        private static SoundEffect[] voiceActing;
        //private static Dictionary<SongOptions, SoundEffect> ambience;
        private static Dictionary<SFXOptions, SoundEffect> soundEffects;
        private static List<SoundEffectInstance> playingSFX = new List<SoundEffectInstance>();

        private static MediaState playerState = MediaState.Stopped;

        private static float maxSFXVolume = 0.9f;
        private static float maxVoiceVolume = 1f;
        private static float maxBGMVolume = .92f;

        private static Dictionary<SongOptions, List<SoundEffect>> bgmList;

        private static SoundEffectInstance songToResumeWhenUnpaused;
        #endregion

        static MediaSystem()
        {
//            string demo = "";
//#if DEMO
//            demo = "_Demo";
//#endif
            //engine = new AudioEngine(path + "music" + demo + ".xgs");
            //bgmSoundBank = new SoundBank(engine, path + "Background_Music"  + demo + ".xsb");
            //sfxSoundBank = new SoundBank(engine, path + "Sound_Effects" + demo + ".xsb");
            //bgmWaveBank = new WaveBank(engine, path + "Background_Music" + demo + ".xwb");
            //sfxWaveBank = new WaveBank(engine, path + "Sound_Effects" + demo + ".xwb");

            machineDict = new Dictionary<int, MachineSoundInstance>();
            for(int i = 1; i <= 10; i++)
                machineDict.Add(i, null);

            //volToReturnToSFX = engine.GetGlobalVariable("SFXVolume");
            //volToReturnTo = engine.GetGlobalVariable("BGMVolume");
            //if(!Program.Game.IsActive)
            //{
            //    initallyQuiet = true;
            //engine.SetGlobalVariable("BGMVolume", maxBGMVolume);
            //engine.SetGlobalVariable("SFXVolume", maxMachineVolume);
            //}

            library = new MediaLibrary();
        }

        #region Update
        /// <summary>
        /// Updates the media.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime, bool isActive)
        {
            //if(playingBGM != null && isActive && playingBGM.State != SoundState.Playing && !InputManager.WindowsOptions.Muted)
            //{
            //    if(playingBGM.IsPrepared || playingBGM.IsPreparing)
            //        playingBGM.Play();
            //    else if(playingBGM.IsPaused)
            //        playingBGM.Resume();
            //    else if(playingBGM.IsStopped)
            //    {
            //        if(GameManager.CurrentLevel != null)
            //            PlayTrack(GameManager.CurrentLevel.LevelSong);
            //        else
            //            PlayTrack(SongOptions.Menu);
            //    }
            //}
            //if(initallyQuiet && Program.Game.IsActive)
            //{
            //    engine.SetGlobalVariable("SFXVolume", volToReturnToSFX);
            //    engine.SetGlobalVariable("BGMVolume", volToReturnTo);
            //    initallyQuiet = false;
            //}
            if(PlayingCustomMusic && MediaPlayer.Volume == 1 && shouldQuietMusic)
                MediaPlayer.Volume = 0.15f;
            else if(PlayingCustomMusic && MediaPlayer.Volume == 0.5f && !shouldQuietMusic)
                MediaPlayer.Volume = 1;

            if(winSound != null && !winSound.IsDisposed && winSound.State == SoundState.Stopped)
            {
                shouldQuietMusic = false;
                winSound.Dispose();
                playingSFX.Remove(winSound);
                winSound = null;
            }
            if(!InputManager.WindowsOptions.Muted)
            {
                //volToReturnTo = InputManager.WindowsOptions.BGMVolume;
                //volToReturnToSFX = InputManager.WindowsOptions.SFXVolume;
                //engine.SetGlobalVariable("BGMVolume", maxBGMVolume);
                //engine.SetGlobalVariable("SFXVolume", maxMachineVolume);

                foreach(SoundEffectInstance s in playingSFX)
                    if(s != null && !s.IsDisposed)
                        s.Volume = maxSFXVolume;
                //if(playingAmbience != null)
                //    playingAmbience.Volume = 1;
                if(playingVoiceActing != null && !playingVoiceActing.IsDisposed)
                    playingVoiceActing.Volume = maxVoiceVolume;
                if(playingBGMintro != null && !playingBGMintro.IsDisposed && playingBGMintro.State == SoundState.Stopped && playingBGM.State == SoundState.Stopped)
                {
                    playingBGM.Play();
                    if(shouldQuietMusic)
                        playingBGM.Volume = maxBGMVolume / 2;
                    if(PlayingCustomMusic)
                        playingBGM.Pause();
                }
                if(playingBGM != null && !playingBGM.IsDisposed && playingBGM.State == SoundState.Playing)
                {
                    playingBGM.Volume = maxBGMVolume;
                    if(shouldQuietMusic)
                        playingBGM.Volume /= 2;
                }
            }
            else
            {
                foreach(SoundEffectInstance s in playingSFX)
                    if(s != null && !s.IsDisposed)
                        s.Volume = 0;
                //if(playingAmbience != null)
                //    playingAmbience.Volume = 0;
                if(playingVoiceActing != null)
                    playingVoiceActing.Volume = 0;
                if(crossfadingFrom != null && !crossfadingFrom.IsDisposed)
                    crossfadingFrom.Stop();
                crossfadingFrom = crossfadingTo = null;
                if(hasIntro && !playingBGMintro.IsDisposed && playingBGMintro.State == SoundState.Playing)
                    playingBGMintro.Stop();

                if(playingBGM != null && !playingBGM.IsDisposed && playingBGM.State == SoundState.Paused)
                    playingBGM.Resume();
                else if(playingBGM != null && !playingBGM.IsDisposed && playingBGM.State == SoundState.Stopped)
                    playingBGM.Play();
                if(playingBGM != null)
                    playingBGM.Volume = 0;
            }
            updateCrossfade();

            for(int i = 0; i < playingSFX.Count; i++)
            {
                SoundEffectInstance c = playingSFX[i];//.ElementAt(i).Key;
                if(c.IsDisposed || c.State == SoundState.Stopped)
                {
                    c.Dispose();
                    playingSFX.Remove(c);
                    i--;
                }
            }
            for(int i = 1; i <= 10; i++)
                if(machineDict[i] != null)
                {
                    if(machineDict[i].IsDisposed)
                    {
                        machineDict[i] = null;
                        continue;
                    }

                    //machineDict[i].Update();
                    if(machineDict[i].State == SoundState.Stopped)
                    {
                        machineDict[i].Dispose();
                        machineDict[i] = null;
                    }
                    //if(machineDict[i].IsPreparing)
                    //    continue;
                    //else if(machineDict[i].IsPrepared)
                    //    machineDict[i].Play();
                    //if(machineDict[i].IsStopped)
                    //{

                    //}
                }
            for(int i = 0; i < autonomousList.Count; i++)
                if(autonomousList[i].IsDisposed)
                {
                    autonomousList.RemoveAt(i);
                    i--;
                }
                else if(autonomousList[i].State == SoundState.Stopped)
                {
                    autonomousList[i].Dispose();
                    autonomousList.RemoveAt(i);
                    i--;
                }
            if(level != GameManager.LevelNumber)
                 LevelReset();
            if(playingVoiceActing != null && !playingVoiceActing.IsDisposed)
                if(playingVoiceActing.State == SoundState.Stopped)
                {
                    shouldQuietMusic = false;
                    playingVoiceActing.Dispose();
                    playingVoiceActing = null;
                    if(playingBGMintro != null && playingBGMintro.State == SoundState.Playing)
                        playingBGMintro.Volume = maxBGMVolume;
                    else if(playingBGM != null && playingBGM.State == SoundState.Playing)
                        playingBGM.Volume = maxBGMVolume;
                    //engine.SetGlobalVariable("BGMVolume", maxBGMVolume);
                }
            //foreach(Cue c in playingSFX.Keys)
            //    if(c.IsPlaying)
            //        c.Apply3D(RenderingDevice.Camera.Ears, playingSFX[c]);
#if WINDOWS
            if(InputManager.KeyboardState.WasKeyJustPressed(InputManager.WindowsOptions.MuteKey))
            {
                ToggleMute();
            }
#endif
            if(GameManager.State == GameState.Paused && stateLastFrame == GameState.Running)
            {
                PauseAll();
                
                ResumeBGM();
            }
            else if(GameManager.State == GameState.Running && stateLastFrame == GameState.Paused)
            {
                PlayAll();
            }
            stateLastFrame = GameManager.State;

            //engine.Update();
        }
        #endregion

        #region Play BGM/SFX/Voice
        /// <summary>
        /// Plays a BGM and ambience, if any.
        /// </summary>
        /// <param name="song">The BGM to play.</param>
        public static void PlayTrack(SongOptions song)
        {
            level = GameManager.LevelNumber;
            if(song == playingSong && !playingBGM.IsDisposed) // necessary so we don't crossfade into ourselves
                return;
            playingSong = song;

            //if(playingAmbience != null)
            //    playingAmbience.Stop(true);
            //playingAmbience = null;
            //if(song != SongOptions.Menu && song != SongOptions.Credits)
            //{
            //    playingAmbience = ambience[song].CreateInstance();
            //    playingAmbience.IsLooped = true;
            //}
            if(crossfadingFrom != null && !crossfadingFrom.IsDisposed)
                crossfadingFrom.Stop();
            else if(crossfadingFrom != null && crossfadingFrom.IsDisposed)
                crossfadingFrom = null;

            if(crossfadingTo != null && !crossfadingTo.IsDisposed)
                crossfadingFrom = crossfadingTo;
            else if(playingBGMoutro != null && !playingBGMoutro.IsDisposed)
            {
                crossfadingFrom = playingBGMoutro;
                if(playingBGM != null && !playingBGM.IsDisposed)
                    playingBGM.Stop();
                playingBGM = playingBGMoutro;
                playingBGM.Play();
            }
            else if(playingBGM != null && !playingBGM.IsDisposed && playingBGM.State == SoundState.Playing)
                crossfadingFrom = playingBGM;
            else if(playingBGMintro != null && !playingBGMintro.IsDisposed && playingBGMintro.State == SoundState.Playing)
            {
                crossfadingFrom = playingBGMintro;
                if(!playingBGM.IsDisposed)
                    playingBGMbackup = playingBGM;
            }

            List<SoundEffect> bgm = bgmList[song];
            switch(bgm.Count)
            {
                case 1: playingBGMintro = playingBGMoutro = null;
                    playingBGM = crossfadingTo = bgm[0].CreateInstance();
                    hasOutro = hasIntro = false;
                    break;
                case 2: playingBGMoutro = null;
                    playingBGMintro = crossfadingTo = bgm[0].CreateInstance();
                    playingBGM = bgm[1].CreateInstance();
                    hasIntro = true;
                    hasOutro = false;
                    break;
                case 3: playingBGMoutro = bgm[2].CreateInstance();
                    playingBGMintro = crossfadingTo = bgm[0].CreateInstance();
                    playingBGM = bgm[1].CreateInstance();
                    hasIntro = hasOutro = true;
                    break;
                default: throw new InvalidOperationException("No BGM loaded.");
            }
            crossfadingTo.Volume = volToReturnTo = 0;
            playingBGM.IsLooped = true;
            crossfadingTo.Play();

            if(PlayingCustomMusic)
            {
                if(hasIntro)
                {
                    playingBGMintro.Stop();
                    playingBGM.Play();
                }
                crossfadingTo.Volume = maxBGMVolume;
                crossfadingFrom = crossfadingTo = null;
                playingBGM.Pause();
            }

            if(crossfadingFrom == null)
            {
                crossfadingTo.Volume = maxBGMVolume;
                crossfadingTo = null;
            }

            //switch(song)
            //{
            //    case SongOptions.Credits:
            //        break;
            //    case SongOptions.Menu:
            //        if(playingBGM != null)
            //            playingBGM.Stop(AudioStopOptions.AsAuthored);
            //        playingBGM = bgmSoundBank.GetCue("Menu_BGM");
            //        if(!playingBGM.IsPlaying)
            //        {
            //            if(playingBGM.IsPaused)
            //                playingBGM.Resume();
            //            playingBGM.Play();
            //        }
            //        break;
            //    case SongOptions.Generic:
            //        playingBGM.Stop(AudioStopOptions.AsAuthored);
            //        playingBGM = bgmSoundBank.GetCue("Generic_BGM");
            //        playingBGM.Play();
            //        break;
            //    case SongOptions.Lava:
            //        playingBGM.Stop(AudioStopOptions.AsAuthored);
            //        playingBGM = bgmSoundBank.GetCue("Lava_BGM");
            //        playingBGM.Play();
            //        break;
            //    case SongOptions.Ice:
            //        playingBGM.Stop(AudioStopOptions.AsAuthored);
            //        playingBGM = bgmSoundBank.GetCue("Ice_BGM");
            //        playingBGM.Play();
            //        break;
            //    case SongOptions.Beach:
            //        playingBGM.Stop(AudioStopOptions.AsAuthored);
            //        playingBGM = bgmSoundBank.GetCue("Beach_BGM");
            //        playingBGM.Play();
            //        break;
            //    case SongOptions.Sky:
            //        playingBGM.Stop(AudioStopOptions.AsAuthored);
            //        playingBGM = bgmSoundBank.GetCue("Sky_BGM");
            //        playingBGM.Play();
            //        break;
            //    case SongOptions.Space:
            //        playingBGM.Stop(AudioStopOptions.AsAuthored);
            //        playingBGM = bgmSoundBank.GetCue("Space_BGM");
            //        playingBGM.Play();
            //        break;
            //}
            //if(playingAmbience != null)
            //    playingAmbience.Play();
            //if(PlayingCustomMusic)
            //    playingBGM.Pause();
        }

        /// <summary>
        /// Plays a sound effect at the center of the 3D space.
        /// </summary>
        /// <param name="s">Sound effect to play.</param>
        //public static void PlaySoundEffect(SFXOptions s)
        //{
        //    PlaySoundEffect(s, Vector3.Zero);
        //}

        /// <summary>
        /// Plays a sound effect.
        /// </summary>
        /// <param name="s">The sound effect to play.</param>
        /// <param name="position">The 3D position to play it at.</param>
        public static void PlaySoundEffect(SFXOptions s)//, Vector3 position)
        {
            SoundEffectInstance c = null;
            c = soundEffects[s].CreateInstance();

            if(s == SFXOptions.Siren)
            {
                c.IsLooped = true;
                if(siren != null && !siren.IsDisposed && siren.State != SoundState.Stopped)
                    siren.Stop(true);
                siren = c;
            }
            else if(s == SFXOptions.Win)
            {
                winSound = c;
                shouldQuietMusic = true;
            }

            if(c != null)
            {
                playingSFX.Add(c);//, new AudioEmitter() { Position = position, DopplerScale = 2, Up = Vector3.UnitZ });
                //c.Apply3D(RenderingDevice.Camera.Ears, playingSFX[c]);
                c.Volume = maxSFXVolume;
                c.Play();
                if(InputManager.WindowsOptions.Muted)
                    c.Volume = 0;
            }
        }

        /// <summary>
        /// Pretty much trackNo = level.
        /// 10.5 is 11.
        /// Handle With Care is 12.
        /// </summary>
        public static void PlayVoiceActing(int trackNo)
        {
            if(GameManager.LevelNumber > 10)
                return;

            if(playingVoiceActing != null && !playingVoiceActing.IsDisposed)
                playingVoiceActing.Stop(true);

            if(!InputManager.WindowsOptions.VoiceClips)
                return;

            playingVoiceActing = voiceActing[trackNo].CreateInstance();
            playingVoiceActing.Play();

            playingVoiceActing.Play();
            playingVoiceActing.Volume = maxVoiceVolume;
            if(!GameManager.Game.IsActive || GameManager.State == GameState.Paused || GameManager.State == GameState.Paused_SelectingMedia ||
                GameManager.State == GameState.Paused_PadQuery || GameManager.State == GameState.Paused_DC)
                playingVoiceActing.Pause();

            shouldQuietMusic = trackNo != 12;
            //volToReturnTo = engine.GetGlobalVariable("BGMVolume");
            level = GameManager.LevelNumber;
            trackNumber = trackNo;
            if(shouldQuietMusic)
            {
                if(playingBGMintro != null && playingBGMintro.State == SoundState.Playing)
                    playingBGMintro.Volume /= 2;
                else if(playingBGM.State == SoundState.Playing)
                    playingBGM.Volume /= 2;
            }
        }
        private static int trackNumber;
        #endregion

        #region Loading
        public static void LoadVoiceActing(ContentManager content)
        {
            if(voiceActing != null)
                Log.WriteToLog("MediaSystem", "Warning: Voice acting was reloaded.");
            
            string basePath = "Music/Voice Acting/";
            voiceActing = new SoundEffect[13];
            for(int i = 0; i < 10; i++)
                voiceActing[i] = content.Load<SoundEffect>(basePath + "Level0" + i);

            voiceActing[10] = content.Load<SoundEffect>(basePath + "Level10");
            voiceActing[11] = content.Load<SoundEffect>(basePath + "Level10-5");
            voiceActing[12] = content.Load<SoundEffect>(basePath + "HandleWithCare");
        }

        public static void LoadSoundEffects(ContentManager content)
        {
            if(soundEffects != null)
                Log.WriteToLog("MediaSystem", "Sound effects were reloaded.");

            string basePath = "Music/SFX/";
            soundEffects = new Dictionary<SFXOptions, SoundEffect>();
            loadSoundEffect(SFXOptions.Achievement, basePath + "Achievement", content);
            loadSoundEffect(SFXOptions.Box_Death, basePath + "Box_Death", content);
            loadSoundEffect(SFXOptions.Box_Success, basePath + "Possible_Box_Success", content);
            loadSoundEffect(SFXOptions.Button_Press, basePath + "Button_Depress", content);
            loadSoundEffect(SFXOptions.Button_Release, basePath + "Button_Release", content);
            loadSoundEffect(SFXOptions.Button_Rollover, basePath + "Button_Roll_Over", content);
            loadSoundEffect(SFXOptions.Explosion, basePath + "Explosion", content);
            loadSoundEffect(SFXOptions.Fail, basePath + "Fail", content);
            loadSoundEffect(SFXOptions.Laser, basePath + "Laser", content);
            loadSoundEffect(SFXOptions.Machine_Button_Press, basePath + "button_depress_2", content);
            loadSoundEffect(SFXOptions.Pause, basePath + "Pause_Jingle", content);
            loadSoundEffect(SFXOptions.Press_Start, basePath + "Press_Start", content);
            loadSoundEffect(SFXOptions.Result_Da, basePath + "Possible_Result_Da", content);
            loadSoundEffect(SFXOptions.Siren, basePath + "Siren", content);
            loadSoundEffect(SFXOptions.Win, basePath + "Level_Win", content);
            loadSoundEffect(SFXOptions.Startup, basePath + "startup", content);            
        }

        private static bool loadSoundEffect(SFXOptions sfx, string path, ContentManager content)
        {
            try { soundEffects.Add(sfx, content.Load<SoundEffect>(path)); }
            catch(Exception e) { Log.WriteExceptionToLog(e, false); return false; }
            return true;
        }

        public static void LoadMachineSounds()
        {
            string basePath = "Music/SFX/";
            MachineSound.ClearList();
            basePath += "Machine_Sound_";
            basePath = "Content/" + basePath;
            string ext = ".wav";
            MachineSound.LoadMachineSound(null, loadBytes(basePath + "00" + ext), null, 0);
            MachineSound.LoadMachineSound(null, loadBytes(basePath + "01" + ext), null, 1);
            MachineSound.LoadMachineSound(loadBytes(basePath + "02_intro" + ext),
                loadBytes(basePath + "02" + ext), loadBytes(basePath + "02_outro" + ext), 2);
            MachineSound.LoadMachineSound(loadBytes(basePath + "05_intro" + ext),
                loadBytes(basePath + "05" + ext), loadBytes(basePath + "05_outro" + ext), 5);
            MachineSound.LoadMachineSound(loadBytes(basePath + "07_intro" + ext),
                loadBytes(basePath + "07" + ext), loadBytes(basePath + "07_outro" + ext), 7);
            MachineSound.LoadMachineSound(loadBytes(basePath + "08_intro" + ext),
                loadBytes(basePath + "08" + ext), loadBytes(basePath + "08_outro" + ext), 8);
            MachineSound.LoadMachineSound(loadBytes(basePath + "09_intro" + ext),
                loadBytes(basePath + "09" + ext), loadBytes(basePath + "09_outro" + ext), 9);
            MachineSound.LoadMachineSound(loadBytes(basePath + "10_intro" + ext),
                loadBytes(basePath + "10" + ext), loadBytes(basePath + "10_outro" + ext), 10);
            MachineSound.LoadMachineSound(loadBytes(basePath + "04_intro" + ext),
                loadBytes(basePath + "04" + ext), loadBytes(basePath + "04_outro" + ext), 4);
#if !DEMO
            MachineSound.LoadMachineSound(loadBytes(basePath + "03_intro" + ext),
                loadBytes(basePath + "03" + ext), loadBytes(basePath + "03_outro" + ext), 3);
            MachineSound.LoadMachineSound(loadBytes(basePath + "06_intro" + ext),
                loadBytes(basePath + "06" + ext), loadBytes(basePath + "06_outro" + ext), 6);
#endif
        }

        private static byte[] loadBytes(string pcmFile)
        {
            BinaryReader reader = new BinaryReader(File.Open(pcmFile, FileMode.Open, FileAccess.Read));
            int chunkID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();

            if(fmtSize == 18)
            {
                // Read any extra values
                int fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(fmtExtraSize);
            }

            int dataID = reader.ReadInt32();
            int dataSize = reader.ReadInt32();

            return reader.ReadBytes(dataSize);
        }

        //public static void LoadAmbience(ContentManager content)
        //{
        //    if(ambience != null)
        //        throw new InvalidOperationException("Can't load this twice.");

        //    string basePath = "Music/AD_Music/Ambience_";
        //    ambience = new Dictionary<SongOptions, SoundEffect>();
        //    ambience.Add(SongOptions.Beach, content.Load<SoundEffect>(basePath + "Beach"));
        //    ambience.Add(SongOptions.Generic, content.Load<SoundEffect>(basePath + "Generic"));
        //    ambience.Add(SongOptions.Ice, content.Load<SoundEffect>(basePath + "Ice"));
        //    ambience.Add(SongOptions.Lava, content.Load<SoundEffect>(basePath + "Lava"));
        //    ambience.Add(SongOptions.Sky, content.Load<SoundEffect>(basePath + "Sky"));
        //    ambience.Add(SongOptions.Space, content.Load<SoundEffect>(basePath + "Space"));
        //}

        public static void LoadBGM(ContentManager content)
        {
            if(bgmList != null)
                Log.WriteToLog("MediaSystem", "BGM was reloaded.");

            string basePath = "Music/AD_Music/";
            bgmList = new Dictionary<SongOptions, List<SoundEffect>>();
#if !DEMO
            bgmList.Add(SongOptions.Beach, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Beach Intro"), 
                content.Load<SoundEffect>(basePath + "Beach Loop") });
            bgmList.Add(SongOptions.Credits, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Credits-Project 38") });
            bgmList.Add(SongOptions.Sky, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Sky Intro"), 
                content.Load<SoundEffect>(basePath + "Sky Loop") });
            bgmList.Add(SongOptions.Space, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Stg5-Trance Idea") });
#endif
            bgmList.Add(SongOptions.Ice, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Ice Intro"), 
                content.Load<SoundEffect>(basePath + "Ice Loop") });
            bgmList.Add(SongOptions.Lava, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Lava Loop") });
            bgmList.Add(SongOptions.Menu, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Menu Intro"), 
                content.Load<SoundEffect>(basePath + "Menu Loop") });
            bgmList.Add(SongOptions.Generic, new List<SoundEffect>() { content.Load<SoundEffect>(basePath + "Generic Loop") });
        }
        #endregion

        #region Machine SFX
        /// <summary>
        /// This gets a machine noise. Returns null if the machine number already has a noise playing.
        /// Does not play the noise.
        /// </summary>
        /// <param name="soundIndex">Values from 0 to 10 inclusive return a sound. -1 returns null. All other values
        /// throw an exception.</param>
        /// <param name="machineNo"></param>
        /// <returns></returns>
        public static MachineSoundInstance GetMachineNoise(int soundIndex, int machineNo)
        {
            if(soundIndex == -1)
                return null;

            if(soundIndex < 0 || soundIndex > 10)
                throw new ArgumentException("soundIndex must be between 0 and 10 inclusive.");
            if(machineNo == 0)
            {
                //Cue c = sfxSoundBank.GetCue("Machine_Sound_" + soundIndex);
                MachineSoundInstance c = MachineSound.GetMachineSound(soundIndex);
                autonomousList.Add(c);
                return c;
            }

            if(machineDict[machineNo] == null || machineDict[machineNo].IsDisposed || machineDict[machineNo].State == SoundState.Stopped || machineDict[machineNo].IsStopping)
                machineDict[machineNo] = MachineSound.GetMachineSound(soundIndex);
            else
                return null;
            
            return machineDict[machineNo];
        }

        public static void LevelReset()
        {
            //if(playingVoiceActing != null)
            //{
            //    playingVoiceActing.Stop(true);
            //    playingVoiceActing.Dispose();
            //    playingVoiceActing = null;
            //    if(playingBGMintro != null && playingBGMintro.State == SoundState.Playing)
            //        playingBGMintro.Volume = maxBGMVolume;
            //    else if(playingBGM != null && playingBGM.State == SoundState.Playing)
            //        playingBGM.Volume = maxBGMVolume;
            //    shouldQuietMusic = false;
            //}
            while(autonomousList.Count > 0)
            {
                if(!autonomousList[0].IsDisposed && autonomousList[0].State == SoundState.Playing)
                    autonomousList[0].Stop(true);
                autonomousList[0].Dispose();
                autonomousList.RemoveAt(0);
            }
            for(int i = 1; i <= 10; i++)
            {
                if(machineDict[i] != null && !machineDict[i].IsDisposed && machineDict[i].State == SoundState.Playing)
                {
                    machineDict[i].Stop(true);
                machineDict[i].Dispose();
                    }
                machineDict[i] = null;
            }
        }
        #endregion

        #region Miscellaneous
        /// <summary>
        /// Stops the siren.
        /// </summary>
        public static void StopSiren()
        {
            if(siren != null && siren.State != SoundState.Stopped)
                siren.Stop(true);
            siren = null;
        }

        /// <summary>
        /// Call when a level ends.
        /// </summary>
        public static void EndingLevel()
        {
            LevelReset();
        }

        /// <summary>
        /// Call to play the startup noise.
        /// </summary>
        public static void Ready(ContentManager content)
        {
            LoadSoundEffects(content);
            LoadMachineSounds();
            LoadBGM(content);
            contentManager = content;
            PlaySoundEffect(SFXOptions.Startup);
            MediaSystem.PlayTrack(SongOptions.Menu);
        }

        private static ContentManager contentManager;

        /// <summary>
        /// Mutes or unmutes the game.
        /// </summary>
        public static void ToggleMute()
        {
            InputManager.WindowsOptions.Muted = !InputManager.WindowsOptions.Muted;
            //if(InputManager.WindowsOptions.Muted)
            //{
            //    engine.SetGlobalVariable("BGMVolume", 0);
            //    engine.SetGlobalVariable("SFXVolume", 0);
            //}
            //else
            //{
            //    //if(Program.Game.Manager != null)
            //    //{
            //    engine.SetGlobalVariable("BGMVolume", maxBGMVolume);
            //    engine.SetGlobalVariable("SFXVolume", maxMachineVolume);
            //    //}
            //    //else
            //    //{
            //    //    engine.SetGlobalVariable("BGMVolume", 100);
            //    //    engine.SetGlobalVariable("SFXVolume", 100);
            //    //    engine.SetGlobalVariable("VoiceVolume", 100);
            //    //}
            //}
        }

        private static void updateCrossfade()
        {
            if(crossfadingTo == null || crossfadingFrom == null)
                return;

            if(InputManager.WindowsOptions.Muted)
            {
                crossfadingTo.Volume = volToReturnTo;
                crossfadingFrom.Volume = maxBGMVolume - volToReturnTo;
            }
            else if(shouldQuietMusic)
            {
                crossfadingTo.Volume *= MathHelper.Clamp(crossfadingTo.Volume * 2, 0, 1);
                crossfadingFrom.Volume = MathHelper.Clamp(crossfadingFrom.Volume * 2, 0, 1);
            }

            if(hasIntro && crossfadingTo.State == SoundState.Stopped)
            {
                crossfadingFrom = playingBGMbackup;
                playingBGMbackup.Volume = maxBGMVolume - crossfadingTo.Volume;
                crossfadingFrom.IsLooped = true;
                crossfadingFrom.Play();
                playingBGMbackup = null;
            }

            if(crossfadingTo.Volume + 0.01f > maxBGMVolume || crossfadingFrom.Volume - 0.01f < 0)
            {
                crossfadingFrom.Stop();
                crossfadingTo.Volume = maxBGMVolume;
                if(shouldQuietMusic)
                    crossfadingTo.Volume /= 2;
                crossfadingTo = crossfadingFrom = null;
            }
            else
            {
                crossfadingTo.Volume += 0.01f;
                crossfadingFrom.Volume -= 0.01f;
            }

            if(crossfadingTo != null && crossfadingFrom != null)
            {
                if(InputManager.WindowsOptions.Muted)
                {
                    volToReturnTo = crossfadingTo.Volume;
                    crossfadingTo.Volume = crossfadingFrom.Volume = 0;
                }
                else if(shouldQuietMusic)
                {
                    crossfadingTo.Volume /= 2;
                    crossfadingFrom.Volume /= 2;
                }
            }
        }

        public static void OnGDMReset()
        {
            LoadBGM(contentManager);
            PlayTrack(playingSong);
            if(playingVoiceActing != null)
                PlayVoiceActing(trackNumber);
            if(siren != null)
                PlaySoundEffect(SFXOptions.Siren);
            while(autonomousList.Count > 0)
            {
                if(!autonomousList[0].IsDisposed && autonomousList[0].State == SoundState.Playing)
                    autonomousList[0].Stop(true);
                autonomousList[0].Dispose();
                autonomousList.RemoveAt(0);
            }
            for(int i = 1; i <= 10; i++)
            {
                if(machineDict[i] != null && !machineDict[i].IsDisposed && machineDict[i].State == SoundState.Playing)
                {
                    machineDict[i].Stop(true);
                    machineDict[i].Dispose();
                }
                machineDict[i] = null;
            }
        }
        #endregion

        #region Play/Pause All
        /// <summary>
        /// Pauses everything for when the game gets paused.
        /// </summary>
        public static void PauseAuxilary()
        {
            if(GameManager.State != GameState.MainMenu)
            {
                foreach(SoundEffectInstance c in playingSFX)
                    if(!c.IsDisposed && c.State == SoundState.Playing)
                        c.Pause();
            }
            else
                foreach(SoundEffectInstance c in playingSFX)
                    if(!c.IsDisposed && c.State == SoundState.Playing)
                        c.Stop(true);
            if(playingVoiceActing != null && playingVoiceActing.State == SoundState.Playing)
                playingVoiceActing.Pause();
            if(siren != null && siren.State == SoundState.Playing)
                siren.Pause();
            foreach(MachineSoundInstance c in machineDict.Values)
                if(c != null && !c.IsDisposed && c.State == SoundState.Playing)
                    c.Pause();
            foreach(MachineSoundInstance c in autonomousList)
                if(c != null && !c.IsDisposed && c.State == SoundState.Playing)
                    c.Pause();
        }

        /// <summary>
        /// Pauses everything for when the game goes out of focus.
        /// </summary>
        public static void PauseAll()
        {
            if(playingBGM != null && !playingBGM.IsDisposed && playingBGM.State == SoundState.Playing)
            {
                playingBGM.Pause();
                songToResumeWhenUnpaused = playingBGM;
            }
            else if(playingBGMintro != null && !playingBGMintro.IsDisposed && playingBGMintro.State == SoundState.Playing)
            {
                playingBGMintro.Pause();
                songToResumeWhenUnpaused = playingBGMintro;
            }
            else if(playingBGMoutro != null && !playingBGMoutro.IsDisposed && playingBGMoutro.State == SoundState.Playing)
            {
                playingBGMoutro.Pause();
                songToResumeWhenUnpaused = playingBGMoutro;
            }
            if(crossfadingFrom != null && !crossfadingFrom.IsDisposed && crossfadingFrom.State == SoundState.Playing)
                crossfadingFrom.Pause();

            if(GameManager.State != GameState.MainMenu)
            {
                int i = 0;
                foreach(SoundEffectInstance c in playingSFX)
                {
                    if(!c.IsDisposed && c.State == SoundState.Playing && ((i != playingSFX.Count - 1 && GameManager.State == GameState.Paused) || GameManager.State != GameState.Paused))
                        c.Pause();
                    i++;
                }
            }
            else
                foreach(SoundEffectInstance c in playingSFX)
                    if(!c.IsDisposed && c.State == SoundState.Playing)
                        c.Stop(true);
            if(playingVoiceActing != null && !playingVoiceActing.IsDisposed && playingVoiceActing.State == SoundState.Playing)
                playingVoiceActing.Pause();
            if(siren != null && !siren.IsDisposed && siren.State == SoundState.Playing)
                siren.Pause();
            //if(playingAmbience != null && playingAmbience.State == SoundState.Playing)
            //    playingAmbience.Pause();
            foreach(MachineSoundInstance c in machineDict.Values)
                if(c != null && !c.IsDisposed && c.State == SoundState.Playing)
                    c.Pause();
            foreach(MachineSoundInstance c in autonomousList)
                if(c != null && !c.IsDisposed && c.State == SoundState.Playing)
                    c.Pause();
            //volToReturnToSFX = engine.GetGlobalVariable("SFXVolume");
            //engine.SetGlobalVariable("SFXVolume", 0);
            pauseCustomMusic();
        }

        /// <summary>
        /// Resumes the BGM.
        /// </summary>
        public static void ResumeBGM()
        {
            if(!PlayingCustomMusic && songToResumeWhenUnpaused != null)
            {
                songToResumeWhenUnpaused.Resume();
                songToResumeWhenUnpaused = null;
            }
            else
                resumeCustomMusic();
            if(crossfadingFrom != null && crossfadingFrom.State == SoundState.Paused)
                crossfadingFrom.Resume();
            //if(playingBGM != null && playingBGM.State == SoundState.Playing && !PlayingCustomMusic)
            //    playingBGM.Resume();
            //if(playingAmbience != null && playingAmbience.State == SoundState.Paused)
            //    playingAmbience.Resume();
            //if(PlayingCustomMusic)
            //    resumeCustomMusic();
        }

        /// <summary>
        /// Resumes all music for when the game receives focus.
        /// </summary>
        public static void PlayAll()
        {
            if(!PlayingCustomMusic && songToResumeWhenUnpaused != null)
            {
                songToResumeWhenUnpaused.Resume();
                songToResumeWhenUnpaused = null;
            }
            else
                resumeCustomMusic();
            if(crossfadingFrom != null && crossfadingFrom.State == SoundState.Paused)
                crossfadingFrom.Resume();
            foreach(SoundEffectInstance c in playingSFX)//.Keys)
                if(c != null && !c.IsDisposed && c.State == SoundState.Paused)
                    c.Resume();
            if(playingVoiceActing != null && playingVoiceActing.State == SoundState.Paused)
                playingVoiceActing.Resume();
            if(siren != null && siren.State == SoundState.Paused)
                siren.Resume();
            //if(playingAmbience != null)
            //    playingAmbience.Resume();
            foreach(MachineSoundInstance c in machineDict.Values)
                if(c != null && c.State == SoundState.Paused)
                    c.Resume();
        }
        #endregion

        #region Stopping functions
        /// <summary>
        /// Stops all SFX.
        /// </summary>
        public static void StopSFX()
        {
            foreach(SoundEffectInstance c in playingSFX)
                if(c != null && c.State == SoundState.Playing)
                    c.Stop(true);
            //if(playingAmbience != null)
            //    playingAmbience.Stop(true);
            //playingAmbience = null;
        }

        /// <summary>
        /// Stops the voice acting.
        /// </summary>
        public static void StopVoiceActing()
        {
            if(playingVoiceActing != null && !(playingVoiceActing.State == SoundState.Stopped))
                playingVoiceActing.Stop(true);
            playingVoiceActing = null;
        }
#endregion

        #region Custom Music
        /// <summary>
        /// Begins playing custom music on the Shuffle All setting.
        /// </summary>
        /// <returns>An error string or string.Empty.</returns>
        public static string StartShuffleCustomMusic()
        {
            if(MediaPlayer.IsShuffled && playerState != MediaState.Stopped)
                return string.Empty;

            MediaPlayer.IsShuffled = true;
            MediaPlayer.IsRepeating = true;
            try
            {
                if(playerState == MediaState.Playing)
                    MediaPlayer.Stop();
                MediaPlayer.Play(library.Songs);
                playerState = MediaState.Playing;
                if(playingBGM != null && playingBGM.State == SoundState.Playing)
                    playingBGM.Pause();
                PlayingCustomMusic = true;
            }
            catch
            {
                playerState = MediaState.Stopped;
                return "Could not shuffle all music. There may not be any songs to shuffle; run Windows Media Player to create a library.";
            }
            return string.Empty;
        }

        /// <summary>
        /// Begins playing custom music on the Albums setting.
        /// </summary>
        /// <param name="album">Album to play.</param>
        /// <returns>An error string, or string.Empty if success.</returns>
        public static string StartAlbumCustomMusic(Album album)
        {
            if(playingAlbum && playerState != MediaState.Stopped)
                return string.Empty;
            
            playingAlbum = true;
            MediaPlayer.IsShuffled = false;
            MediaPlayer.IsRepeating = true;
            try
            {
                if(playerState == MediaState.Playing)
                    MediaPlayer.Stop();
                MediaPlayer.Play(album.Songs);
                playerState = MediaState.Playing;
                if(playingBGM != null && playingBGM.State == SoundState.Playing)
                    playingBGM.Pause();
                PlayingCustomMusic = true;
            }
            catch
            {
                playerState = MediaState.Stopped;
                return "Could not play the album.";
            }
            return string.Empty;
        }

        /// <summary>
        /// Begins playing custom music on the Artists setting.
        /// </summary>
        /// <param name="artist">Artist to play.</param>
        /// <returns>An error string, or string.Empty if successful.</returns>
        public static string StartArtistsCustomMusic(Artist artist)
        {
            if(!playingAlbum && playerState != MediaState.Stopped)
                return string.Empty;

            playingAlbum = false;
            MediaPlayer.IsShuffled = false;
            MediaPlayer.IsRepeating = true;
            try
            {
                if(MediaPlayer.State == MediaState.Playing)
                    MediaPlayer.Stop();
                MediaPlayer.Play(artist.Songs);
                playerState = MediaState.Playing;
                if(playingBGM != null && playingBGM.State == SoundState.Playing)
                    playingBGM.Pause();
                PlayingCustomMusic = true;
            }
            catch
            {
                playerState = MediaState.Stopped;
                return "Could not play the artist.";
            }
            return string.Empty;
        }

        /// <summary>
        /// Gets a list of the artists in the library.
        /// </summary>
        /// <returns></returns>
        public static ArtistCollection GetArtistsInLibrary()
        {
            return library.Artists;
        }

        /// <summary>
        /// Gets a list of the albums in the library.
        /// </summary>
        /// <returns></returns>
        public static AlbumCollection GetAlbumsInLibrary()
        {
            return library.Albums;
        }

        /// <summary>
        /// Stops playing custom music and restarts the normal BGM.
        /// </summary>
        public static void StopCustomMusic()
        {
            MediaPlayer.Stop();
            if(playingBGM != null && playingBGM.State == SoundState.Paused)
                playingBGM.Resume();
            else if(playingBGM != null && playingBGM.State == SoundState.Stopped)
                playingBGM.Play();
            PlayingCustomMusic = false;
            playerState = MediaState.Stopped;
        }

        /// <summary>
        /// Goes to the previous track.
        /// </summary>
        public static string MovePrevious()
        {
            int attempts = 5;
            for(int i = 0; i < attempts; i++)
            {
                Thread moveNext = null;
                if(i == 0)
                    moveNext = new Thread(MediaPlayer.MovePrevious); // doesn't this just generate a ton of threads...?
                else
                    moveNext = new Thread(delegate() { MediaPlayer.Queue.ActiveSongIndex -= i + 1; });
                moveNext.Start();
                bool success = moveNext.Join(2000);

                if(success)
                {
                    playerState = MediaState.Playing;
                    if(i == 0)
                        return string.Empty;
                    return "Playing: " + GetPlayingSong() + ". Skipped " + i + " unplayable song" + (i == 1 ? "" : "s") + " (likely due to DRM).";
                }
            }
            playerState = MediaState.Stopped;
            return "Could not play previous track. Please try again.";
        }
        
        /// <summary>
        /// Goes to the next track.
        /// </summary>
        public static string MoveNext()
        {
            int attempts = 5;
            for(int i = 0; i < attempts; i++)
            {
                Thread moveNext = null;
                if(i == 0)
                    moveNext = new Thread(MediaPlayer.MoveNext);
                else
                    moveNext = new Thread(delegate() { MediaPlayer.Queue.ActiveSongIndex += i + 1; });
                moveNext.Start();
                bool success = moveNext.Join(2000);

                if(success)
                {
                    playerState = MediaState.Playing;
                    if(i != 0)
                        return "Playing: " + GetPlayingSong() + ". Skipped " + i + " unplayable song" + (i == 1 ? "" : "s") + " (likely due to DRM).";
                    return string.Empty;
                }
            }
            playerState = MediaState.Stopped;
            return "Could not play next track. Please try again.";
        }

        /// <summary>
        /// Gets a string in the format of (song name) by (artist name) on (album name). Returns (nothing) if no song is playing.
        /// </summary>
        /// <returns></returns>
        public static string GetPlayingSong()
        {
            if(playerState == MediaState.Stopped)
                return "(nothing)";

            Song playingSong = MediaPlayer.Queue.ActiveSong;
            string artist = playingSong.Artist.Name;
            string album = playingSong.Album.Name;
            return playingSong.Name + " by " + (artist == string.Empty ? "Unknown Artist" : artist) + " on " + (album == string.Empty ? "Unknown Album" : album);
        }

        private static void pauseCustomMusic()
        {
            if(PlayingCustomMusic)
                if(playerState == MediaState.Playing)
                {
                    playerState = MediaState.Paused;
                    MediaPlayer.Pause();
                }
        }

        private static void resumeCustomMusic()
        {
            if(PlayingCustomMusic)
                if(playerState == MediaState.Paused)
                {
                    playerState = MediaState.Playing;
                    MediaPlayer.Resume();
                }
        }
        #endregion

        private class MachineSound
        {
            //private SoundEffect intro;
            //private SoundEffect loop;
            //private SoundEffect outro;

            private byte[] intro, loop, outro;

            private static Dictionary<int, MachineSound> assignedNumbers = new Dictionary<int, MachineSound>();

            //private MachineSound(SoundEffect intro, SoundEffect loop, SoundEffect outro)
            private MachineSound(byte[] intro, byte[] loop, byte[] outro)
            {
                this.intro = intro;
                this.loop = loop;
                this.outro = outro;
            }

            public static void ClearList()
            {
                assignedNumbers.Clear();
            }

            //public static void LoadMachineSound(SoundEffect intro, SoundEffect loop, SoundEffect outro, int soundNumber)
            public static void LoadMachineSound(byte[] intro, byte[] loop, byte[] outro, int soundNumber)
            {
                if(assignedNumbers.Keys.Contains(soundNumber))
                    throw new ArgumentException("The requested sound number has already been assigned.");
                if(loop == null)
                    throw new ArgumentException("loop cannot be null.");              

                assignedNumbers.Add(soundNumber, new MachineSound(intro, loop, outro));
            }

            public static MachineSoundInstance GetMachineSound(int soundNumber)
            {
                if(!assignedNumbers.Keys.Contains(soundNumber))
                    throw new ArgumentException("That sound number is not assigned.");

                //SoundEffectInstance i, l, o;
                //i = l = o = null;
                MachineSound m = assignedNumbers[soundNumber];
                
                //if(m.intro != null)
                //{
                //    i = m.intro.CreateInstance();
                //    i.Volume = maxMachineVolume;
                //}

                //l = m.loop.CreateInstance();
                //l.IsLooped = m.intro != null;
                //l.Volume = maxMachineVolume;

                //if(m.outro != null)
                //{
                //    o = m.outro.CreateInstance();
                //    o.Volume = maxMachineVolume;
                //}

                //return new MachineSoundInstance(i, l, o);
                return new MachineSoundInstance(m.intro, m.loop, m.outro);
            }
        }
    }

    public class MachineSoundInstance
    {
        //private SoundEffectInstance intro;
        //private SoundEffectInstance loop;
        //private SoundEffectInstance outro;

        private byte[] intro, loop, outro;

        private DynamicSoundEffectInstance sound;

        private enum InstanceState { Intro, Loop, Outro }
        private InstanceState state;

        private bool isPlaying;
        private bool isStopped;
        private bool isPaused;

        //public bool IsDisposed { get; private set; }
        public bool IsDisposed { get { return sound.IsDisposed; } }

        public float Volume
        {
            get
            {
                //switch(state)
                //{
                //    case InstanceState.Intro: return intro.Volume;
                //    case InstanceState.Loop: return loop.Volume;
                //    case InstanceState.Outro: return outro.Volume;
                //    default: throw new ArgumentException("State is invalid.");
                //}
                return sound.Volume;
            }
            set
            {
                //if(intro != null)
                //    intro.Volume = value;
                //loop.Volume = value;
                //if(outro != null)
                //    outro.Volume = value;
                sound.Volume = value;
            }
        }

        public SoundState State
        {
            get
            {
                return sound.State;
                //switch(state)
                //{
                //    case InstanceState.Intro: return intro.State;
                //    case InstanceState.Loop: return loop.State;
                //    case InstanceState.Outro: return outro.State;
                //    default: throw new ArgumentException("State is invalid.");
                //}
            }
        }

        public bool IsStopping { get { return state == InstanceState.Outro; } }
        private static float maxMachineVolume = .89f;

        //public MachineSoundInstance(SoundEffectInstance i, SoundEffectInstance l, SoundEffectInstance o)
        public MachineSoundInstance(byte[] i, byte[] l, byte[] o)
        {
            intro = i;
            loop = l;
            outro = o;
            isPlaying = isPaused = false;
            isStopped = true;
            
            sound = new DynamicSoundEffectInstance(44100, AudioChannels.Stereo);
            sound.Volume = maxMachineVolume;
            if(intro == null)
                state = InstanceState.Loop;
            else
                sound.SubmitBuffer(intro);
            sound.SubmitBuffer(loop);
            sound.BufferNeeded += bufferNeeded;
        }

        public void Play()
        {
            isPlaying = true;
            isPaused = isStopped = false;
            //if(intro != null)
            //{
            //    intro.Play();
            //    state = InstanceState.Intro;
            //}
            //else
            //{
            //    loop.Play();
            //    state = InstanceState.Loop;
            //}
            sound.Play();
        }

        public void Pause()
        {
            isPlaying = isStopped = false;
            isPaused = true;
            //switch(state)
            //{
            //    case InstanceState.Intro: intro.Pause();
            //        break;
            //    case InstanceState.Loop: loop.Pause();
            //        break;
            //    case InstanceState.Outro: outro.Pause();
            //        break;
            //}
            sound.Pause();
        }

        public void Stop(bool immediate)
        {
            if(IsStopping && !immediate) // needed to make multiple calls to Stop(false) safe
                return;                  // without it there's ugly-ness

            if(immediate)
            {
                //switch(state)
                //{
                //    case InstanceState.Intro: intro.Stop(immediate);
                //        break;
                //    case InstanceState.Loop: loop.Stop(immediate);
                //        break;
                //    case InstanceState.Outro: outro.Stop(immediate);
                //        break;
                //}
                isStopped = true;
                isPlaying = isPaused = false;
                sound.Stop(immediate);
            }
            else
            {
                isPlaying = true;
                isPaused = isStopped = false;
                //if(state == InstanceState.Intro)
                //    intro.Stop();
                //else
                //    loop.Stop();
                sound.Stop(true);
                if(outro != null)
                {
                    //outro.Play();
                    sound.SubmitBuffer(outro);
                    sound.Play();
                    state = InstanceState.Outro;
                }
            }
        }

        public void Resume()
        {
            //switch(state)
            //{
            //    case InstanceState.Intro: intro.Resume();
            //        break;
            //    case InstanceState.Loop: loop.Resume();
            //        break;
            //    case InstanceState.Outro: outro.Resume();
            //        break;
            //}
            isPlaying = true;
            isPaused = isStopped = false;
            sound.Resume();
        }

        //public void Update()
        //{
        //    if(isStopped || isPaused)
        //        return;

        //    switch(state)
        //    {
        //        case InstanceState.Intro:
        //            if(intro.State == SoundState.Stopped)
        //            {
        //                loop.Play();
        //                state = InstanceState.Loop;
        //            }
        //            break;
        //        case InstanceState.Loop: break; // nothing to do here
        //        case InstanceState.Outro:
        //            if(outro.State == SoundState.Stopped)
        //            {
        //                isStopped = true;
        //                isPlaying = isStopped = false;
        //            }
        //            break;
        //        default: throw new InvalidOperationException("State is invalid.");
        //    }
        //}

        public void Dispose()
        {
            if(IsDisposed)
                return;

            //if(intro != null)
            //    intro.Dispose();
            //loop.Dispose();
            //if(outro != null)
            //    outro.Dispose();
            sound.Dispose();
            //IsDisposed = true;
        }

        private void bufferNeeded(object sender, EventArgs e)
        {
            if(sound.PendingBufferCount == 0 && isPlaying)
                if(state == InstanceState.Loop)
                    if(intro != null)
                        sound.SubmitBuffer(loop);
                    else
                        Stop(true);
                else if(state == InstanceState.Outro)
                    Stop(true);
        }
    }
}