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
    AudioDevice AudioDevice;

    PersistentVoice MusicVoice;

    AudioDataOgg MusicData;

    Queue<PersistentVoice> Voices = new Queue<PersistentVoice>();
    Queue<PersistentVoice> Playing = new Queue<PersistentVoice>();

    Filter PlaySongFilter;

    PersistentVoice GetVoice()
    {
        if (Voices.Count > 0)
        {
            return Voices.Dequeue();
        }
        var streamingAudio = StreamingAudio.Lookup(StreamingAudio.attention_shoppers_v1);
        return new PersistentVoice(AudioDevice, streamingAudio.Format);
    }

    public Audio(World world, AudioDevice audioDevice) : base(world)
    {

        // Set(CreateEntity(), new SFXVolume(saveData.SFXVolume));
        // Set(CreateEntity(), new MusicVolume(saveData.MusicVolume));

        AudioDevice = audioDevice;
        SFXFilter = FilterBuilder.Include<PlayOnce>().Build();
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

        while (Playing.Count > 0 && Playing.Peek().State != SoundState.Playing)
        {
            Voices.Enqueue(Playing.Dequeue());
        }

        // if (MusicVoice != null)
        //     MusicVoice.SetVolume(Easing.InExpo(GetSingleton<MusicVolume>().Value));

        foreach (var entity in SFXFilter.Entities)
        {
            var voice = GetVoice();
            var buffer = StaticAudio.Lookup(Get<PlayOnce>(entity).AudioID);
            voice.Reset();
            // voice.SetVolume(Easing.InExpo(GetSingleton<SFXVolume>().Value));
            voice.Submit(buffer);
            voice.Play();
            if (Get<PlayOnce>(entity).RandomizePitch)
            {
                voice.SetPitch(Rando.Range(-0.1f, 0.1f));
            }
            Playing.Enqueue(voice);


            Destroy(entity);
        }
    }
}