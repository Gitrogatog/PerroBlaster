using System;
using System.Collections.Generic;
using System.Net;
using MoonTools.ECS;
using MoonWorks.Audio;
using MoonWorks.Math;
using MyGame.Components;
using MyGame.Content;
using MyGame.Data;
using MyGame.Utility;

namespace MyGame.Systems;

public class Audio : MoonTools.ECS.System
{
    public const int MaxVolume = 10;

    Filter SFXFilter;
    Filter ContinuousFilter;
    Filter StartMusicFilter;
    Filter StopMusicFilter;
    Filter StopMusicUnlessFilter;
    Filter StartAndSetVolumeFilter;
    AudioDevice AudioDevice;
    AudioDataQoa MusicData;
    StreamingSoundID CurrentMusicID = new StreamingSoundID(-1);
    PersistentVoice MusicVoice;

    // PersistentVoice MusicVoice;
    // PersistentVoice CrowdVoice;
    // PersistentVoice HumVoice;
    Dictionary<StaticSoundID, PersistentVoice> ContinuousSFX = new Dictionary<StaticSoundID, PersistentVoice>();
    // Dictionary<StreamingSoundID, PersistentVoice> MusicVoices = new Dictionary<StreamingSoundID, PersistentVoice>();
    HashSet<PersistentVoice> UnseenVoices = new HashSet<PersistentVoice>();
    // List<PersistentVoice> PlayingContinuousSFX = new List<PersistentVoice>();
    // Dictionary<Format, Queue<PersistentVoice>> AvailableVoices = new Dictionary<Format, Queue<PersistentVoice>>();
    // AudioDataQoa MusicData;
    // AudioDataQoa HumData;
    // AudioDataQoa CrowdData;

    bool hasPlayedMusic = false;
    public Audio(World world, AudioDevice audioDevice) : base(world)
    {

        // Set(CreateEntity(), new SFXVolume(saveData.SFXVolume));
        // Set(CreateEntity(), new MusicVolume(saveData.MusicVolume));

        AudioDevice = audioDevice;
        SFXFilter = FilterBuilder.Include<PlayStaticSFX>().Build();
        ContinuousFilter = FilterBuilder.Include<PlayContinuousSFX>().Build();
        // StartMusicFilter = FilterBuilder.Include<StartMusic>().Build();
        StopMusicFilter = FilterBuilder.Include<StopMusic>().Build();
        StopMusicUnlessFilter = FilterBuilder.Include<StopMusicUnless>().Build();
        StartAndSetVolumeFilter = FilterBuilder.Include<StartMusicAndSetVolume>().Build();
        // PlaySongFilter = FilterBuilder.Include<Song>().Include<SongChanged>().Build();

        // var streamingAudioData = StreamingAudio.Lookup(StreamingAudio.stage_1);
        // streamingAudioData.Loop = true;
        // HumData = StreamingAudio.Lookup(StreamingAudio.hum);
        // HumData.Loop = true;
        // HumVoice = AudioDevice.Obtain<PersistentVoice>(HumData.Format);
        // HumData.SendTo(HumVoice);
        // HumVoice.Stop();

        // CrowdData = StreamingAudio.Lookup(StreamingAudio.crowd);
        // CrowdData.Loop = true;
        // CrowdVoice = AudioDevice.Obtain<PersistentVoice>(CrowdData.Format);
        // CrowdData.SendTo(CrowdVoice);
        // CrowdVoice.Stop();

        MusicData = StreamingAudio.Lookup(StreamingAudio.castle);
        MusicData.Loop = true;
        MusicVoice = AudioDevice.Obtain<PersistentVoice>(MusicData.Format);
        // MusicData.SendTo(MusicVoice);
        // MusicVoice.Stop();


    }

    public override void Update(TimeSpan delta)
    {
        // MusicVoice.SetVolume(0.1f);
        // CrowdVoice.SetVolume(1f);
        // HumVoice.SetVolume(0.7f);
        foreach (var entity in SFXFilter.Entities)
        {
            var sfxData = Get<PlayStaticSFX>(entity);
            PlayStaticSound(
                sfxData.Sound,
                sfxData.Volume,
                sfxData.Pitch,
                sfxData.Pan
            );
            Destroy(entity);
        }
        UnseenVoices.Clear();
        foreach (var voice in ContinuousSFX.Values)
        {
            if (voice.State == SoundState.Playing)
            {
                UnseenVoices.Add(voice);
            }
        }
        foreach (var entity in ContinuousFilter.Entities)
        {
            var sfxData = Get<PlayContinuousSFX>(entity);
            var sound = sfxData.Sound;
            if (!ContinuousSFX.ContainsKey(sfxData.StaticSoundID))
            {
                // StreamingAudio.Lookup(StreamingAudio.attentiontwerkers).See
                var voice = AudioDevice.Obtain<PersistentVoice>(sound.Format);
                ContinuousSFX[sfxData.StaticSoundID] = voice;
                voice.SetVolume(sfxData.Volume);
                voice.SetPitch(sfxData.Pitch);
                voice.SetPan(sfxData.Pan);
                voice.Submit(sound);
            }
            else
            {
                UnseenVoices.Remove(ContinuousSFX[sfxData.StaticSoundID]);
            }
        }
        foreach (var voice in UnseenVoices)
        {
            voice.Stop();
        }
        if (Some<PlayMusic>())
		{
            

            // MusicVoice?.Stop();
            // MusicData?.Disconnect();
            // MusicVoice?.Dispose();
            

            //  var streamingAudioData = StreamingAudio.Lookup(StreamingAudio.stage_1);
            // streamingAudioData.Loop = true;
            // MusicVoice = AudioDevice.Obtain<PersistentVoice>(streamingAudioData.Format);
            // MusicVoice.SetVolume(0.5f);
            // MusicData?.Close();
            // MusicData.Dispose();


            // MusicData = StreamingAudio.Lookup(GetSingleton<PlayMusic>().ID);
            // MusicData.Seek(0);
            // MusicData.Loop = true;
            // MusicVoice = AudioDevice.Obtain<PersistentVoice>(MusicData.Format);
		    // MusicVoice.SetVolume(0.5f);
            // MusicData.SendTo(MusicVoice);
            // MusicVoice.Play();
            // var musicVoice = InitMusicVoice(GetSingleton<PlayMusic>().ID);
            // MusicVoice.Stop();
            var mID = GetSingleton<PlayMusic>().ID;
            if(CurrentMusicID != mID) {
                CurrentMusicID = mID;
                MusicData.Disconnect();
                MusicData = StreamingAudio.Lookup(mID);
                MusicData.Seek(0);
                MusicData.Loop = true;
                MusicData.SendTo(MusicVoice);
                
                MusicVoice.Play();

                Console.WriteLine($"playing music id: {GetSingleton<PlayMusic>().ID}");
            }
            
            DestroyAll<PlayMusic>();
            // MusicData = StreamingAudio.Lookup(GetSingleton<PlayMusic>().ID);
            
            // // Music.Seek(0);
			// MusicData.SendTo(MusicVoice);

			// MusicVoice.Play();
		}
        // foreach(var entity in StartMusicFilter.Entities){
        //     // Console.WriteLine("playing mysiuc!");
        //     StreamingSoundID id = Get<StartMusic>(entity).ID;
        //     StartMusic(InitMusicVoice(id));
        //     // CrowdData = StreamingAudio.Lookup(StreamingAudio.crowd);
        //     // CrowdData.Loop = true;
        //     // CrowdData.Seek(0);
        //     // CrowdData.SendTo(CrowdVoice);
        //     // CrowdVoice.Play();
        // }
        foreach(var entity in StopMusicFilter.Entities){

            // Console.WriteLine("stopping music");
            MusicData.Disconnect();
            MusicVoice.Stop();
            Destroy(entity);
            CurrentMusicID = new StreamingSoundID(-1);
            // StopMusic(InitMusicVoice(id));
            // Console.WriteLine($"stopping {id}");
        }
        foreach(var entity in StopMusicUnlessFilter.Entities){

            // Console.WriteLine("stopping music");
            var mID = Get<StopMusicUnless>(entity).ID;
            if(mID != CurrentMusicID) {
                MusicData.Disconnect();
                MusicVoice.Stop();
                CurrentMusicID = new StreamingSoundID(-1);
            }
            Destroy(entity);
            
            // StopMusic(InitMusicVoice(id));
            // Console.WriteLine($"stopping {id}");
        }
        foreach(var entity in StartAndSetVolumeFilter.Entities){
            (float volume, StreamingSoundID mID) = Get<StartMusicAndSetVolume>(entity);

            CurrentMusicID = mID;
            MusicData.Disconnect();
            MusicData = StreamingAudio.Lookup(mID);
            MusicData.Seek(0);
            MusicData.Loop = true;
            MusicData.SendTo(MusicVoice);
            MusicVoice.SetVolume(volume);
            
            MusicVoice.Play();
            // Console.WriteLine(
            //     $"playing music {id} and settin volume {volume}"
            // );
            // var voice = InitMusicVoice(id);
            // if(voice.State != SoundState.Playing){
            //     // Console.WriteLine("i am playing!");
            //     StartMusic(voice);
            //     // Console.WriteLine(
            //     //     "playing music"
            //     // );
            // }
            // else {
            //     // Console.WriteLine("i am not playing!");
            //     // Console.WriteLine(
            //     //     "already playing!"
            //     // );
            // }
            // voice.SetVolume(volume);
            Destroy(entity);
        }
        // while(Some<SetMusicVolume>()){
        //     (float volume, StreamingSoundID id) = GetSingleton<SetMusicVolume>();
        //     Destroy(GetSingletonEntity<SetMusicVolume>());
        //     MusicVoices[id]?.SetVolume(volume);
        //     // IDToVoice(id)?.SetVolume(volume);
        // }
        while(Some<StopAllMusic>()) {
            Destroy(GetSingletonEntity<StopAllMusic>());
            MusicData.Disconnect();
            MusicVoice.Stop();
            // StopAllMusic();
        }
        if(GlobalInput.Current.Cancel.IsPressed){
            // PrintVoiceState(MusicVoice, "music");
            // PrintVoiceState(HumVoice, "hum");
            // PrintVoiceState(CrowdVoice, "crowd");
        }
    }
    private void StopAllMusic() {
        // foreach(var voice in MusicVoices.Values) {
        //     StopMusic(voice);
        // }
    }
    // private PersistentVoice InitMusicVoice(StreamingSoundID id) {
    //     if(MusicVoices.ContainsKey(id)) return MusicVoices[id];
    //     var musicData = StreamingAudio.Lookup(id);
    //     musicData.Loop = true;
    //     var musicVoice = AudioDevice.Obtain<PersistentVoice>(musicData.Format);
    //     musicData.SendTo(musicVoice);
    //     musicVoice.Stop();
    //     MusicVoices[id] = musicVoice;
    //     return musicVoice;
    // }
    private void PrintVoiceState(PersistentVoice voice, string name){
        Console.WriteLine($"{name} state {voice.State} volume {voice.Volume}");
    }
    // private AudioDataQoa IDToData(int id) => id switch
    // {
    //     1 => HumData,
    //     2 => CrowdData,
    //     _ => MusicData,
    // };
    // private PersistentVoice IDToVoice(int id) => id switch
    // {
    //     1 => HumVoice,
    //     2 => CrowdVoice,
    //     _ => MusicVoice,
    // };
    private void StartMusicAtBeginning(AudioDataQoa data, PersistentVoice voice){
        data?.Seek(0);
        voice?.Play();
    }
    private void StartMusic(PersistentVoice voice){
        voice?.Play();
    }
    private void StopMusic(PersistentVoice voice){
        if(voice != null && voice.State == SoundState.Playing){
            voice.Stop();
        }
    }
    private void PlayStaticSound(
    AudioBuffer sound,
    float volume,
    float pitch,
    float pan
    )
    {
        SourceVoice voice = AudioDevice.Obtain<TransientVoice>(sound.Format);
        voice.SetVolume(volume);
        voice.SetPitch(pitch);
        voice.SetPan(pan);
        voice.Submit(sound);
        voice.Play();
    }
    public void Dispose() {
        Console.WriteLine("stopping all audio");
        MusicData.Disconnect();
        MusicVoice.Dispose();
        // foreach(var voice in MusicVoices.Values) {
        //     Console.WriteLine("stopping 1 voice");
        //     voice.Stop();
        //     voice.Return();
        //     voice.Dispose();
        //     // voice.Dispose();
        // }
        // MusicVoices.Clear();
    }
    // private PersistentVoice GetNewStaticVoice(
    // AudioBuffer sound,
    // float volume,
    // float pitch,
    // float pan
    // )
    // {
    //     PersistentVoice voice = GetVoice(sound.Format);

    //     voice.SetVolume(volume);
    //     voice.SetPitch(pitch);
    //     voice.SetPan(pan);
    //     voice.Submit(sound);
    //     voice.Play();
    //     return voice;
    // }
    // PersistentVoice GetVoice(Format format)
    // {
    //     if (!AvailableVoices.ContainsKey(format))
    //     {
    //         AvailableVoices[format] = new Queue<PersistentVoice>();
    //         return AudioDevice.Obtain<PersistentVoice>(format);
    //     }
    //     else if (AvailableVoices[format].Count == 0)
    //     {
    //         return AudioDevice.Obtain<PersistentVoice>(format);
    //     }
    //     else
    //     {
    //         return AvailableVoices[format].Dequeue();
    //     }
    // }
    // void ReturnVoice(PersistentVoice voice)
    // {
    //     AvailableVoices[voice.Format].Enqueue(voice);
    // }
}