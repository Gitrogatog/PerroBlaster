using System;
using System.Collections.Generic;
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
    AudioDevice AudioDevice;

    PersistentVoice MusicVoice;
    AudioDataOgg MusicData;
    Dictionary<StaticSoundID, PersistentVoice> ContinuousSFX = new Dictionary<StaticSoundID, PersistentVoice>();
    HashSet<PersistentVoice> UnseenVoices = new HashSet<PersistentVoice>();
    // List<PersistentVoice> PlayingContinuousSFX = new List<PersistentVoice>();
    // Dictionary<Format, Queue<PersistentVoice>> AvailableVoices = new Dictionary<Format, Queue<PersistentVoice>>();
    Filter PlaySongFilter;


    public Audio(World world, AudioDevice audioDevice) : base(world)
    {

        // Set(CreateEntity(), new SFXVolume(saveData.SFXVolume));
        // Set(CreateEntity(), new MusicVolume(saveData.MusicVolume));

        AudioDevice = audioDevice;
        SFXFilter = FilterBuilder.Include<PlayStaticSFX>().Build();
        ContinuousFilter = FilterBuilder.Include<PlayContinuousSFX>().Build();
        // PlaySongFilter = FilterBuilder.Include<Song>().Include<SongChanged>().Build();

        var songEntity = CreateEntity();
        // Set(songEntity, Music.Songs[0]);
        // Set(songEntity, new SongChanged());
    }

    public override void Update(TimeSpan delta)
    {
        // if (PlaySongFilter.Count > 0)
        // {
        //     var newSongEntity = PlaySongFilter.NthEntity(0);
        //     var song = Get<Song>(newSongEntity);

        //     var path = Stores.TextStorage.Get(song.PathID);

        //     if (MusicVoice != null)
        //     {
        //         MusicVoice.Stop();
        //     }

        //     if (MusicData != null)
        //     {
        //         MusicData.Close();
        //         MusicData.Dispose();
        //     }

        //     MusicData = AudioDataOgg.Create(AudioDevice);

        //     MusicData.Open(File.ReadAllBytes(path));

        //     if (MusicVoice != null)
        //     {
        //         MusicVoice.Dispose();
        //     }

        //     MusicVoice = new StreamingVoice(AudioDevice, MusicData.Format);
        //     MusicVoice.Loop = true;

        //     MusicVoice.Load(MusicData);

        //     MusicVoice.Play();
        //     Remove<SongChanged>(newSongEntity);
        // }

        // while (Playing.Count > 0 && Playing.Peek().State != SoundState.Playing)
        // {
        //     Voices.Enqueue(Playing.Dequeue());
        // }

        // if (MusicVoice != null)
        //     MusicVoice.SetVolume(Easing.InExpo(GetSingleton<MusicVolume>().Value));

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

        // for (int i = PlayingContinuousSFX.Count - 1; i >= 0; i--)
        // {
        //     continuousVoice = PlayingContinuousSFX[i];
        //     if (continuousVoice.State != SoundState.Stopped)
        //     {
        //         PlayingContinuousSFX.RemoveAt(i);
        //         ReturnVoice(continuousVoice);
        //     }
        // }
        // foreach (var entity in ContinuousFilter.Entities)
        // {
        //     var sfxData = Get<PlayContinuousSFX>(entity);
        //     AudioBuffer sound = sfxData.Sound;
        //     if (sfxData.VoiceID < 0)
        //     {
        //         Set(entity, sfxData.SetID(PlayingContinuousSFX.Count));
        //         continuousVoice = GetVoice(sound.Format);
        //         PlayingContinuousSFX.Add(continuousVoice);
        //     }
        //     else
        //     {
        //         continuousVoice = PlayingContinuousSFX[sfxData.VoiceID];
        //     }

        // }
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