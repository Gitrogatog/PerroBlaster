using System;
using System.IO;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Video;
using MyGame.Components;
using MyGame.Content;
using MyGame.Spawn;
namespace MyGame.Systems;

public class VideoSystem : MoonTools.ECS.System
{
    private Filter EntityFilter;
    private VideoAV1 Video;
    public static VideoAV1 GlobalVideo;
    private Game MyGame;
    // private static string videoPath = AppContext.BaseDirectory.Replace('\\', '/') + "ContentStatic/Video/out.obu";
    // private static string videoPath = Path.Combine(AppContext.BaseDirectory, "ContentStatic","Video", "out.obu");
    private static string videoPath = "ContentStatic/Video/out.obu";
    bool playVideo = false;
    bool videoEnded = false;
    public VideoSystem(World world, Game game) : base(world)
    {
        EntityFilter = FilterBuilder
            .Include<Position>()
            .Build();
        MyGame = game;
    }

    public override void Update(TimeSpan delta)
    {
        
        if(Some<LoadVideo>() && Video == null) {
            DestroyAll<LoadVideo>();
            Video = VideoAV1.Create(MyGame.GraphicsDevice, MyGame.VideoDevice, MyGame.RootTitleStorage, videoPath, 30);

            // Load the video

            Video.Load(false);
        }
        if(Some<PlayVideo>() && !playVideo) {
            Video.Play();
            GlobalVideo = Video;
            playVideo = true;
            EntityPrefabs.PlaySFX(StaticAudio.nostalgiaintro);
        }
        if(playVideo && !videoEnded) {
            Video.Update(delta);
            if(Video.Ended) {
                EntityPrefabs.CreateTimedMessage(new CloseGameWindow(), 0f);
                videoEnded = true;
                EntityPrefabs.PlaySFX(StaticAudio.Damage1);
            }
        }
    }
    public void Dispose() {
        // if(Video != null) {
        //     Video.Unload();
        // }
    }
}
