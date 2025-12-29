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
    ImguiRenderer imguiRenderer;

    public Renderer(World world, GraphicsDevice graphicsDevice, TitleStorage titleStorage, Window window, TextureFormat swapchainFormat) : base(world)
    {
        GraphicsDevice = graphicsDevice;

        RectangleFilter = FilterBuilder.Include<Rectangle>().Include<Position>().Include<DrawAsRectangle>().Build();
        TextFilter = FilterBuilder.Include<Text>().Include<Position>().Build();
        SpriteAnimationFilter = FilterBuilder.Include<SpriteAnimation>().Include<Position>().Build();
        SpriteFilter = FilterBuilder.Include<Sprite>().Include<Position>().Build();
        SliceFilter = FilterBuilder.Include<Position>().Include<Rectangle>().Include<NineSlice>().Build();

        RenderTexture = Texture.Create2D(GraphicsDevice, "Render Texture", Dimensions.GAME_W, Dimensions.GAME_H, swapchainFormat, TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler);
        DepthTexture = Texture.Create2D(GraphicsDevice, "Depth Texture", Dimensions.GAME_W, Dimensions.GAME_H, TextureFormat.D16Unorm, TextureUsageFlags.DepthStencilTarget);

        // SpriteAtlasTexture = TextureAtlases.TP_Sprites.Texture;
        SpriteAtlasTexture = TextureAtlases.BT_Sprites.Texture;

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
        imguiRenderer = new ImguiRenderer(graphicsDevice, swapchainFormat, window, titleStorage);
    }

    public void Render(Window window)
    {
        var commandBuffer = GraphicsDevice.AcquireCommandBuffer();

        var swapchainTexture = commandBuffer.AcquireSwapchainTexture(window);

        if (swapchainTexture != null)
        {

            ArtSpriteBatch.Start();
            if (false)
            {
                foreach (var entity in RectangleFilter.Entities)
                {
                    var position = Get<Position>(entity);
                    var rectangle = Get<Rectangle>(entity);
                    var orientation = Has<Orientation>(entity) ? Get<Orientation>(entity).Angle : 0.0f;
                    var color = Has<ColorBlend>(entity) ? Get<ColorBlend>(entity).Color : Color.Red;
                    var depth = -2f;
                    if (Has<Depth>(entity))
                    {
                        depth = -Get<Depth>(entity).Value;
                    }

                    var sprite = SpriteAnimations.Pixel.Frames[0];
                    ArtSpriteBatch.Add(new Vector3(position.X + rectangle.X, position.Y + rectangle.Y, depth), orientation, new Vector2(rectangle.Width, rectangle.Height), color, sprite.UV.LeftTop, sprite.UV.Dimensions);
                }
            }

            foreach (var entity in SpriteAnimationFilter.Entities)
            {
                if (HasInRelation<DontDraw>(entity))
                    continue;

                var position = Get<Position>(entity);
                var animation = Get<SpriteAnimation>(entity);
                var sprite = animation.CurrentSprite;
                DrawSprite(entity, new Vector2(position.X, position.Y), animation.CurrentSprite, animation.Origin);
            }

            foreach (var entity in SpriteFilter.Entities)
            {
                if (HasInRelation<DontDraw>(entity))
                    continue;

                var position = Get<Position>(entity);
                var sprite = Get<Sprite>(entity);
                DrawSprite(entity, new Vector2(position.X, position.Y), sprite, new Vector2(MathF.Truncate(sprite.FrameRect.W * 0.5f), MathF.Truncate(sprite.FrameRect.H * 0.5f)));
            }

            foreach (var entity in SliceFilter.Entities)
            {
                if (HasInRelation<DontDraw>(entity))
                {
                    continue;
                }
                var position = Get<Position>(entity);
                // var animation = Get<SpriteAnimation>(entity);
                // var sprite = animation.CurrentSprite;
                // var origin = animation.Origin;
                var depth = -1f;
                var color = Color.White;
                var rect = Get<Rectangle>(entity);
                position += new Vector2(rect.X, rect.Y);
                var nineSlice = Get<NineSlice>(entity);
                var sprite = nineSlice.TopLeft;
                var top = nineSlice.TopLeft;
                var center = nineSlice.CenterLeft;
                var bottom = nineSlice.BottomLeft;
                bool hasChanged = false;
                for (int x = 0; x < rect.Width - nineSlice.Width; x += nineSlice.Width)
                {
                    ArtSpriteBatch.Add(new Vector3(position.X + x, position.Y, depth), 0, new Vector2(top.SliceRect.W, top.SliceRect.H), color, top.UV.LeftTop, top.UV.Dimensions);
                    for (int y = nineSlice.Height; y < rect.Height - nineSlice.Height; y += nineSlice.Height)
                    {
                        ArtSpriteBatch.Add(new Vector3(position.X + x, position.Y + y, depth), 0, new Vector2(center.SliceRect.W, center.SliceRect.H), color, center.UV.LeftTop, center.UV.Dimensions);
                    }
                    ArtSpriteBatch.Add(new Vector3(position.X + x, position.Y + rect.Height - nineSlice.Height, depth), 0, new Vector2(bottom.SliceRect.W, bottom.SliceRect.H), color, bottom.UV.LeftTop, bottom.UV.Dimensions);
                    if (!hasChanged)
                    {
                        hasChanged = true;
                        top = nineSlice.TopMid;
                        center = nineSlice.CenterMid;
                        bottom = nineSlice.BottomMid;
                    }
                }
                int finalX = rect.Width - nineSlice.Width;
                top = nineSlice.TopRight;
                center = nineSlice.CenterRight;
                bottom = nineSlice.BottomRight;
                ArtSpriteBatch.Add(new Vector3(position.X + finalX, position.Y, depth), 0, new Vector2(top.SliceRect.W, top.SliceRect.H), color, top.UV.LeftTop, top.UV.Dimensions);
                for (int y = nineSlice.Height; y < rect.Height - nineSlice.Height; y += nineSlice.Height)
                {
                    ArtSpriteBatch.Add(new Vector3(position.X + finalX, position.Y + y, depth), 0, new Vector2(center.SliceRect.W, center.SliceRect.H), color, center.UV.LeftTop, center.UV.Dimensions);
                }
                ArtSpriteBatch.Add(new Vector3(position.X + finalX, position.Y + rect.Height - nineSlice.Height, depth), 0, new Vector2(bottom.SliceRect.W, bottom.SliceRect.H), color, bottom.UV.LeftTop, bottom.UV.Dimensions);

                //     ArtSpriteBatch.Add(new Vector3(position.X, position.Y, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);
                // sprite = nineSlice.TopRight;
                // ArtSpriteBatch.Add(new Vector3(position.X + nineSlice.Width, position.Y, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);
                // sprite = nineSlice.BottomLeft;
                // ArtSpriteBatch.Add(new Vector3(position.X, position.Y + nineSlice.Height, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);
                // sprite = nineSlice.BottomRight;
                // ArtSpriteBatch.Add(new Vector3(position.X + nineSlice.Width, position.Y + nineSlice.Height, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H), color, sprite.UV.LeftTop, sprite.UV.Dimensions);

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
                var color = Has<Color>(entity) ? Get<Color>(entity) : Color.White;
                var depth = -1f;

                if (Has<ColorBlend>(entity))
                {
                    color = Get<ColorBlend>(entity).Color;
                }

                if (Has<Depth>(entity))
                {
                    depth = -Get<Depth>(entity).Value;
                }

                // if (Has<TextDropShadow>(entity))
                // {
                //     var dropShadow = Get<TextDropShadow>(entity);

                //     var dropShadowPosition = position + new Position(dropShadow.OffsetX, dropShadow.OffsetY);

                //     TextBatch.SpanAdd(
                //         font,
                //         span,
                //         text.Size,
                //         Matrix4x4.CreateTranslation(dropShadowPosition.X, dropShadowPosition.Y, depth - 1),
                //         new Color(0f, 0, 0, color.A),
                //         text.HorizontalAlignment,
                //         text.VerticalAlignment
                //     );
                // }
                if (!Has<WordWrap>(entity))
                {
                    var span = str.AsSpan(0, spanLength);
                    TextBatch.AddSpan(
                        font,
                        span,
                        text.Size,
                        Matrix4x4.CreateTranslation(position.X, position.Y, depth),
                        color,
                        text.HorizontalAlignment,
                        text.VerticalAlignment
                    );
                }
                else
                {
                    var max = Get<WordWrap>(entity).Max;
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
                                y += rect.H + 2;
                            }
                            // StringBuilder.Clear();
                            // StringBuilder.Append(word);
                            // StringBuilder.Append(" ");
                            startIndex = prevEndIndex;
                        }
                        // current = StringBuilder.ToString();
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
                // else
                // {
                //     var max = Get<WordWrap>(entity).Max;
                //     // var font = Stores.FontStorage.Get(text.FontID);
                //     // var str = Stores.TextStorage.Get(text.TextID);
                //     var words = str.Split(' ');
                //     StringBuilder.Clear();
                //     float y = position.Y;
                //     var current = "";
                //     WellspringCS.Wellspring.Rectangle rect;

                //     foreach (var word in words)
                //     {
                //         StringBuilder.Append(word);
                //         StringBuilder.Append(" ");
                //         font.TextBounds(StringBuilder.ToString(), text.Size, text.HorizontalAlignment, text.VerticalAlignment, out rect);
                //         if (rect.W >= max)
                //         {
                //             TextBatch.Add(font, current, text.Size, Matrix4x4.CreateTranslation(new Vector3(position.X, y, depth)), color, text.HorizontalAlignment, text.VerticalAlignment);
                //             y += rect.H + 2;
                //             StringBuilder.Clear();
                //             StringBuilder.Append(word);
                //             StringBuilder.Append(" ");
                //         }
                //         current = StringBuilder.ToString();
                //     }

                //     TextBatch.Add(font, current, text.Size, Matrix4x4.CreateTranslation(new Vector3(position.X, y, depth)), color, text.HorizontalAlignment, text.VerticalAlignment);

                // }

            }

            ArtSpriteBatch.Upload(commandBuffer);
            TextBatch.UploadBufferData(commandBuffer);

            var renderPass = commandBuffer.BeginRenderPass(
                new DepthStencilTargetInfo(DepthTexture, 1, 0),
                new ColorTargetInfo(RenderTexture, Color.Black)
            );

            var viewProjectionMatrices = new ViewProjectionMatrices(GetCameraMatrix(), GetProjectionMatrix());

            if (ArtSpriteBatch.InstanceCount > 0)
            {
                ArtSpriteBatch.Render(renderPass, SpriteAtlasTexture, PointSampler, viewProjectionMatrices);
            }

            renderPass.BindGraphicsPipeline(TextPipeline);
            TextBatch.Render(renderPass, GetCameraMatrix() * GetProjectionMatrix());

            commandBuffer.EndRenderPass(renderPass);



            commandBuffer.Blit(RenderTexture, swapchainTexture, MoonWorks.Graphics.Filter.Nearest);
            imguiRenderer.Draw(commandBuffer, swapchainTexture);
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
        var depth = -1f;
        var color = Color.White;
        bool flip = ShouldFlip(entity);
        origin += new Vector2(sprite.FrameRect.X, sprite.FrameRect.Y);

        Vector2 scale = Vector2.One;
        if (Has<SpriteScale>(entity))
        {
            scale *= Get<SpriteScale>(entity).Scale;
            origin *= new Vector2(scale.X, scale.Y);
        }

        var offset = -origin;

        if (Has<ColorBlend>(entity))
        {
            color = Get<ColorBlend>(entity).Color;
        }

        if (Has<ColorFlicker>(entity))
        {
            var colorFlicker = Get<ColorFlicker>(entity);
            if (colorFlicker.ElapsedFrames % 2 == 0)
            {
                color = colorFlicker.Color;
            }
        }

        if (Has<Depth>(entity))
        {
            depth = -Get<Depth>(entity).Value;
        }

        // ArtSpriteBatch.Add(new Vector3(position.X + offset.X, position.Y + offset.Y, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H) * scale, color, sprite.UV.LeftTop, sprite.UV.Dimensions);
        if (flip)
        {
            ArtSpriteBatch.Add(new Vector3(position.X - offset.X, position.Y + offset.Y, depth), 0, new Vector2(-sprite.SliceRect.W, sprite.SliceRect.H) * scale, color, sprite.UV.LeftTop, sprite.UV.Dimensions);//sprite.UV.RightTop, new Vector2(-sprite.UV.Dimensions.X, sprite.UV.Dimensions.Y));
        }
        else
        {
            ArtSpriteBatch.Add(new Vector3(position.X + offset.X, position.Y + offset.Y, depth), 0, new Vector2(sprite.SliceRect.W, sprite.SliceRect.H) * scale, color, sprite.UV.LeftTop, sprite.UV.Dimensions);
        }
    }

    public Matrix4x4 GetCameraMatrix()
    {
        return Matrix4x4.Identity;
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

}
