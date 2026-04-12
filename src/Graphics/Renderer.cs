using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using MoonTools.ECS;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using MoonWorks.Storage;
using MyGame.Components;
using MyGame.Content;
using MyGame.Relations;
using MyGame.Systems;
using MyGame.Utility;

namespace MyGame;

public class Renderer : MoonTools.ECS.Renderer
{
    GraphicsDevice GraphicsDevice;
    GraphicsPipeline TextPipeline;
    TextBatch TextBatch;

    SpriteBatch ArtSpriteBatch;

    Texture RenderTexture;
    Texture DepthTexture;

    Texture SpriteAtlasTexture;

    Sampler PointSampler;

    MoonTools.ECS.Filter RectangleFilter;
    MoonTools.ECS.Filter TextFilter;
    MoonTools.ECS.Filter SpriteAnimationFilter;
    MoonTools.ECS.Filter SpriteFilter;
    MoonTools.ECS.Filter SliceFilter;
    PriorityQueue<Entity, float> DrawPriority = new();
    Window MainWindow;
    // ImguiRenderer imguiRenderer;

    public Renderer(World world, GraphicsDevice graphicsDevice, TitleStorage titleStorage, Window window, TextureFormat swapchainFormat) : base(world)
    {
        GraphicsDevice = graphicsDevice;
        MainWindow = window;
        RectangleFilter = FilterBuilder.Include<Rectangle>().Include<Position>().Include<DrawAsRectangle>().Build();
        TextFilter = FilterBuilder.Include<Text>().Include<Position>().Build();
        SpriteAnimationFilter = FilterBuilder.Include<SpriteAnimation>().Include<Position>().Build();
        SpriteFilter = FilterBuilder.Include<Sprite>().Include<Position>().Build();
        SliceFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<NineSlice>().Build();

        RenderTexture = Texture.Create2D(GraphicsDevice, "Render Texture", Dimensions.GAME_W, Dimensions.GAME_H, swapchainFormat, TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler);
        DepthTexture = Texture.Create2D(GraphicsDevice, "Depth Texture", Dimensions.GAME_W, Dimensions.GAME_H, TextureFormat.D16Unorm, TextureUsageFlags.DepthStencilTarget | TextureUsageFlags.Sampler);

        // SpriteAtlasTexture = TextureAtlases.TP_Sprites.Texture;
        SpriteAtlasTexture = TextureAtlases.BJ_Sprites.Texture;

        TextPipeline = GraphicsPipeline.Create(
            GraphicsDevice,
            new GraphicsPipelineCreateInfo
            {
                TargetInfo = new GraphicsPipelineTargetInfo
                {
                    DepthStencilFormat = TextureFormat.D16Unorm,
                    HasDepthStencilTarget = true,
                    ColorTargetDescriptions =
                    [
                        new ColorTargetDescription
                        {
                            Format = swapchainFormat,
                            BlendState = ColorTargetBlendState.PremultipliedAlphaBlend
                        }
                    ]
                },
                DepthStencilState = new DepthStencilState
                {
                    EnableDepthTest = true,
                    EnableDepthWrite = true,
                    CompareOp = CompareOp.LessOrEqual
                },
                VertexShader = GraphicsDevice.TextVertexShader,
                FragmentShader = GraphicsDevice.TextFragmentShader,
                VertexInputState = GraphicsDevice.TextVertexInputState,
                RasterizerState = RasterizerState.CCW_CullNone,
                PrimitiveType = PrimitiveType.TriangleList,
                MultisampleState = MultisampleState.None,
                Name = "Text Pipeline"
            }
        );
        TextBatch = new TextBatch(GraphicsDevice);

        PointSampler = Sampler.Create(GraphicsDevice, SamplerCreateInfo.PointClamp);

        ArtSpriteBatch = new SpriteBatch(GraphicsDevice, titleStorage, swapchainFormat, TextureFormat.D16Unorm);
        // imguiRenderer = new ImguiRenderer(graphicsDevice, swapchainFormat, window, titleStorage);
    }

    public void Render(Window window)
    {
        var commandBuffer = GraphicsDevice.AcquireCommandBuffer();

        var swapchainTexture = commandBuffer.AcquireSwapchainTexture(window);

        if (swapchainTexture != null)
        {

            ArtSpriteBatch.Start();
            

            // foreach (var entity in SpriteAnimationFilter.Entities)
            // {
            //     if (HasInRelation<DontDraw>(entity) || Has<DontDraw>(entity))
            //         continue;
            //     DrawPriority.Enqueue(entity, 1f - (Has<Depth>(entity) ? (Get<Depth>(entity).Value) : 0.5f));
            //     // var position = Get<Position>(entity);
            //     // var animation = Get<SpriteAnimation>(entity);
            //     // var sprite = animation.CurrentSprite;
            //     // DrawSprite(entity, new Vector2(position.X, position.Y), animation.CurrentSprite, animation.Origin);
            // }
            DrawPriority.Clear();
            foreach (var entity in SpriteFilter.Entities)
            {
                if (HasInRelation<DontDraw>(entity))
                    continue;
                DrawPriority.Enqueue(entity, 1f - (Has<Depth>(entity) ? (Get<Depth>(entity).Value) : 0.5f));
            }
            foreach (var entity in SpriteAnimationFilter.Entities)
            {
                if (HasInRelation<DontDraw>(entity) || Has<DontDraw>(entity))
                    continue;
                DrawPriority.Enqueue(entity, Has<Depth>(entity) ? 1f - Get<Depth>(entity).Value : 0.5f);
            }
            while(DrawPriority.Count > 0) {
                var entity = DrawPriority.Dequeue();
                var position = Get<Position>(entity);
                Sprite sprite;
                Vector2 origin;
                if(Has<SpriteAnimation>(entity)) {
                    var spriteAnim = Get<SpriteAnimation>(entity);
                    sprite = spriteAnim.CurrentSprite;
                    origin = spriteAnim.Origin;
                }
                else {
                    sprite = Get<Sprite>(entity);
                    origin = new Vector2(MathF.Truncate(sprite.FrameRect.W * 0.5f), MathF.Truncate(sprite.FrameRect.H * 0.5f));
                }
                // var sprite = Has<Sprite>(entity) ?
                //     Get<Sprite>(entity) : Get<SpriteAnimation>(entity).CurrentSprite;
                DrawSprite(entity, new Vector2(position.X, position.Y), sprite, origin);
            
            }

            // foreach (var entity in SliceFilter.Entities)
            // {
            //     if (HasInRelation<DontDraw>(entity))
            //     {
            //         continue;
            //     }
            //     var position = Get<Position>(entity);
            //     // var animation = Get<SpriteAnimation>(entity);
            //     // var sprite = animation.CurrentSprite;
            //     // var origin = animation.Origin;
            //     var depth = 0.999f;
            //     var colorBlend = Color.White;
            //     var colorOverlay = new Color();
            //     var rect = Get<Rectangle>(entity);
            //     position += new Vector2(rect.X, rect.Y);
            //     var nineSlice = Get<NineSlice>(entity);
            //     var sprite = nineSlice.TopLeft;
            //     var top = nineSlice.TopLeft;
            //     var center = nineSlice.CenterLeft;
            //     var bottom = nineSlice.BottomLeft;
            //     bool hasChanged = false;
            //     for (int x = 0; x < rect.Width - nineSlice.Width; x += nineSlice.Width)
            //     {
            //         ArtSpriteBatch.Add(new Vector3(position.X + x, position.Y, depth), 0, new Vector2(top.SliceRect.W, top.SliceRect.H), colorBlend, colorOverlay, top.UV.LeftTop, top.UV.Dimensions);
            //         for (int y = nineSlice.Height; y < rect.Height - nineSlice.Height; y += nineSlice.Height)
            //         {
            //             ArtSpriteBatch.Add(new Vector3(position.X + x, position.Y + y, depth), 0, new Vector2(center.SliceRect.W, center.SliceRect.H), colorBlend, colorOverlay, center.UV.LeftTop, center.UV.Dimensions);
            //         }
            //         ArtSpriteBatch.Add(new Vector3(position.X + x, position.Y + rect.Height - nineSlice.Height, depth), 0, new Vector2(bottom.SliceRect.W, bottom.SliceRect.H), colorBlend, colorOverlay, bottom.UV.LeftTop, bottom.UV.Dimensions);
            //         if (!hasChanged)
            //         {
            //             hasChanged = true;
            //             top = nineSlice.TopMid;
            //             center = nineSlice.CenterMid;
            //             bottom = nineSlice.BottomMid;
            //         }
            //     }
            //     int finalX = rect.Width - nineSlice.Width;
            //     top = nineSlice.TopRight;
            //     center = nineSlice.CenterRight;
            //     bottom = nineSlice.BottomRight;
            //     ArtSpriteBatch.Add(new Vector3(position.X + finalX, position.Y, depth), 0, new Vector2(top.SliceRect.W, top.SliceRect.H), colorBlend, colorOverlay, top.UV.LeftTop, top.UV.Dimensions);
            //     for (int y = nineSlice.Height; y < rect.Height - nineSlice.Height; y += nineSlice.Height)
            //     {
            //         ArtSpriteBatch.Add(new Vector3(position.X + finalX, position.Y + y, depth), 0, new Vector2(center.SliceRect.W, center.SliceRect.H), colorBlend, colorOverlay, center.UV.LeftTop, center.UV.Dimensions);
            //     }
            //     ArtSpriteBatch.Add(new Vector3(position.X + finalX, position.Y + rect.Height - nineSlice.Height, depth), 0, new Vector2(bottom.SliceRect.W, bottom.SliceRect.H), colorBlend, colorOverlay, bottom.UV.LeftTop, bottom.UV.Dimensions);

            //     //     ArtSpriteBatch.Add(new Vector3(position.X, position.Y, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);
            //     // sprite = nineSlice.TopRight;
            //     // ArtSpriteBatch.Add(new Vector3(position.X + nineSlice.Width, position.Y, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);
            //     // sprite = nineSlice.BottomLeft;
            //     // ArtSpriteBatch.Add(new Vector3(position.X, position.Y + nineSlice.Height, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);
            //     // sprite = nineSlice.BottomRight;
            //     // ArtSpriteBatch.Add(new Vector3(position.X + nineSlice.Width, position.Y + nineSlice.Height, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);

            // }

            foreach (var entity in RectangleFilter.Entities)
            {
                var position = Get<Position>(entity);
                var rectangle = Get<Rectangle>(entity);
                // var orientation = Has<Rotation>(entity) ? Get<Rotation>(entity).Value : 0.0f;
                float orientation = 0;
                var color = Has<ColorBlend>(entity) ? Get<ColorBlend>(entity).Color : Color.White;
                // Console.WriteLine($"rect color: {color} has color blend: {Has<ColorBlend>(entity)}");
                // if(Has<ColorBlend>(entity)) {
                //     Console.WriteLine(Get<ColorBlend>(entity));
                // }
                var depth = 0.001f;
                if (Has<Depth>(entity))
                {
                    depth = Get<Depth>(entity).Value;
                }

                var sprite = SpriteAnimations.Pixel.Frames[0];
                ArtSpriteBatch.Add(new Vector3(position.X + rectangle.X, position.Y + rectangle.Y, depth), orientation, new Vector2(rectangle.Width, rectangle.Height), color, color, sprite.UV.LeftTop, sprite.UV.Dimensions);
            }

            TextBatch.Start();
            foreach (var entity in TextFilter.Entities)
            {
                if (HasInRelation<DontDraw>(entity))
                    continue;

                var text = Get<Text>(entity);
                var position = Get<Position>(entity);
                if (Has<TextOffset>(entity))
                {
                    (int x, int y) = Get<TextOffset>(entity);
                    position = new Position(position.X + x, position.Y + y);
                }

                var str = Data.TextStorage.GetString(text.TextID);
                int spanLength = Has<DisplayCharCount>(entity) ? Math.Min(str.Length, Get<DisplayCharCount>(entity).Value) : str.Length;
                if (spanLength <= 0)
                {
                    continue;
                }

                // var span = Has<DisplayCharCount>(entity) ? str.AsSpan(0, Math.Clamp(0, str.Length, Get<DisplayCharCount>(entity).Value)) : str.AsSpan();

                // Console.Write($"\r span length: {spanLength}  2nd: {span.Length} ");
                var font = Fonts.FromID(text.FontID);
                var color = Has<Color>(entity) ? Get<Color>(entity) : new Color(189, 223, 255);
                var depth = 1f;
                Vector2 scale = Vector2.One;
                if (Has<SpriteScale>(entity))
                {
                    scale *= Get<SpriteScale>(entity).Scale;
                    // origin *= new Vector2(scale.X, scale.Y);
                }

                // if (Has<ColorBlend>(entity))
                // {
                //     color = Get<ColorBlend>(entity).Color;
                // }

                if (Has<Depth>(entity))
                {
                    depth = Get<Depth>(entity).Value;
                }
                if (!Has<WordWrap>(entity))
                {
                    var span = str.AsSpan(0, spanLength);
                    TextBatch.AddSpan(
                        font,
                        span,
                        text.Size,
                        Matrix4x4.Multiply(
                            Matrix4x4.CreateTranslation(position.X, position.Y, depth),
                            Matrix4x4.CreateScale(new Vector3(scale.X, scale.Y, 1))),
                        color,
                        text.HorizontalAlignment,
                        text.VerticalAlignment
                    );
                }
                else
                {
                    
                    var max = Get<WordWrap>(entity).Max;
                    // Console.WriteLine("has wordwrap: " + max);
                    // var words = str.Split(' ');
                    // StringBuilder.Clear();
                    float y = position.Y;
                    // var currentSpan = str.AsSpan(0, 0);

                    WellspringCS.Wellspring.Rectangle rect;
                    int startIndex = 0;
                    int endIndex = 0;
                    int prevEndIndex = 0;
                    int executions = 10;

                    while (endIndex < spanLength && executions > 0)
                    {
                        endIndex = TextUtils.GetPositionOfNext(str, ' ', endIndex + 1);
                        font.TextBoundsSpan(str.AsSpan(startIndex, endIndex - startIndex), text.Size, text.HorizontalAlignment, text.VerticalAlignment, out rect);
                        if (rect.W > max)
                        {
                            if (str[startIndex] == ' ')
                            {
                                startIndex++;
                            }
                            if (startIndex < prevEndIndex)
                            {
                                TextBatch.AddSpan(font, str.AsSpan(startIndex, prevEndIndex - startIndex), text.Size, Matrix4x4.CreateTranslation(new Vector3(position.X, y, depth)), color, text.HorizontalAlignment, text.VerticalAlignment);
                                y += rect.H + 4;
                            }
                            startIndex = prevEndIndex;
                        }
                        prevEndIndex = endIndex;
                        executions--;

                    }
                    if (str[startIndex] == ' ')
                    {
                        startIndex++;
                    }
                    if (startIndex < spanLength)
                    {
                        TextBatch.AddSpan(font, str.AsSpan(startIndex, spanLength - startIndex), text.Size, Matrix4x4.CreateTranslation(new Vector3(position.X, y, depth)), color, text.HorizontalAlignment, text.VerticalAlignment);
                    }

                }
            }

            ArtSpriteBatch.Upload(commandBuffer);
            TextBatch.UploadBufferData(commandBuffer);

            var renderPass = commandBuffer.BeginRenderPass(
                new DepthStencilTargetInfo(DepthTexture, 1, 0),
                new ColorTargetInfo(RenderTexture, Color.Black) // Some<ClearColor>() ? GetSingleton<ClearColor>().Color : 
            );

            var viewProjectionMatrices = new ViewProjectionMatrices(GetCameraMatrix(), GetProjectionMatrix());

            if (ArtSpriteBatch.InstanceCount > 0)
            {
                ArtSpriteBatch.Render(renderPass, SpriteAtlasTexture, PointSampler, viewProjectionMatrices);
            }

            renderPass.BindGraphicsPipeline(TextPipeline);
            TextBatch.Render(renderPass, GetCameraMatrix() * GetProjectionMatrix());

            commandBuffer.EndRenderPass(renderPass);
            // if(MainWindow.ScreenMode)
            commandBuffer.BlitToCenter(RenderTexture, swapchainTexture, MoonWorks.Graphics.Filter.Nearest);
            if(Some<PlayVideo>() && VideoSystem.GlobalVideo != null) {
                commandBuffer.Blit(VideoSystem.GlobalVideo.RenderTexture, swapchainTexture, MoonWorks.Graphics.Filter.Linear);
            }
            // imguiRenderer.Draw(commandBuffer, swapchainTexture);
        }

        GraphicsDevice.Submit(commandBuffer);
    }

    void DrawSprite(Entity entity, Vector2 position, Sprite sprite, Vector2 origin)
    {
        // if (HasOutRelation<DontDraw>(entity))
        //     return;

        // var position = Get<Position>(entity);
        // var animation = Get<SpriteAnimation>(entity);
        // var sprite = animation.CurrentSprite;
        // var origin = animation.Origin;
        var depth = 1f;
        var colorBlend = Color.White;
        var colorOverlay = new Color(0, 0, 0, 0);
        var orientation = Has<Rotation>(entity) ? Get<Rotation>(entity).Value : 0.0f;
        bool flip = ShouldFlip(entity);
        origin += new Vector2(sprite.FrameRect.X, sprite.FrameRect.Y);

        Vector2 scale = Vector2.One;
        if (Has<SpriteScale>(entity))
        {
            scale *= Get<SpriteScale>(entity).Scale;
            origin *= new Vector2(scale.X, scale.Y);
        }

        var offset = -origin;
        // var offset = Vector2.Zero;

        if (Has<ColorBlend>(entity))
        {
            colorBlend = Get<ColorBlend>(entity).Color;
        }
        if(Has<ColorOverlayTimer>(entity)) {
            colorOverlay = Get<ColorOverlayTimer>(entity).Color;
        }
        else if (Has<ColorOverlay>(entity)){
            colorOverlay = Get<ColorOverlay>(entity).Color;
        }

        if (Has<ColorFlicker>(entity))
        {
            var colorFlicker = Get<ColorFlicker>(entity);
            if (colorFlicker.ElapsedFrames % 2 == 0)
            {
                colorBlend = colorFlicker.Color;
            }
        }

        if (Has<Depth>(entity))
        {
            depth = Get<Depth>(entity).Value;
        }

        // ArtSpriteBatch.Add(new Vector3(position.X + offset.X, position.Y + offset.Y, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H) * scale, color, sprite.UV.LeftTop, sprite.UV.Dimensions);
        if (flip)
        {
            ArtSpriteBatch.Add(new Vector3(position.X - offset.X, position.Y + offset.Y, depth), orientation, new Vector2(-sprite.SliceRect.W, sprite.SliceRect.H) * scale, colorBlend, colorOverlay, sprite.UV.LeftTop, sprite.UV.Dimensions);//sprite.UV.RightTop, new Vector2(-sprite.UV.Dimensions.X, sprite.UV.Dimensions.Y));
        }
        else
        {
            ArtSpriteBatch.Add(new Vector3(position.X + offset.X, position.Y + offset.Y, depth), orientation, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H) * scale, colorBlend, colorOverlay, sprite.UV.LeftTop, sprite.UV.Dimensions);
        }
    }

    public Matrix4x4 GetCameraMatrix()
    {
        return Matrix4x4.CreateTranslation(new Vector3(-Globals.CameraX, -Globals.CameraY, 0));
    }

    public Matrix4x4 GetProjectionMatrix()
    {
        return Matrix4x4.CreateOrthographicOffCenter(
            0,
            Dimensions.GAME_W,
            Dimensions.GAME_H,
            0,
            0.01f,
            1000
        );
    }

    bool ShouldFlip(Entity entity)
    {
        if (!Has<Facing>(entity))
        {
            return false;
        }
        return !Get<Facing>(entity).Right;
    }
    public void Dipose() {
        //RenderTexture
//DepthTexture
//TextPipeline
//TextBatch
//PointSampler
//ArtSpriteBatch
        RenderTexture.Dispose();
        DepthTexture.Dispose();
        TextPipeline.Dispose();
        TextBatch.Dispose();
        PointSampler.Dispose();
        ArtSpriteBatch.Dispose();
    }

}
