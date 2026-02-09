using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using ImGuiNET;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Input;
using MyGame;
using MyGame.Content;
public class ImguiRenderer
{
    // private string ShaderContentPath = "ContentBuilder/ContentBuilderUI/Content/Shaders";
    // private string S3 = "ContentBuilder\\ContentBuilderUI\\Content\\Shaders\\ImGui.frag.spv";
    // private string ShaderPath2 = Path.Combine("ContentBuilder", "ContentBuilderUI", "Content", "Shaders");

    GraphicsDevice GraphicsDevice;

    private uint VertexCount = 0;
    private uint IndexCount = 0;

    // private Texture RenderTexture;

    private MoonWorks.Graphics.Buffer ImGuiVertexBuffer = null;
    private MoonWorks.Graphics.Buffer ImGuiIndexBuffer = null;
    private GraphicsPipeline ImGuiPipeline;
    private Shader ImGuiVertexShader;
    private Shader ImGuiFragmentShader;
    private Sampler ImGuiSampler;

    Texture FontTexture;

    private ResourceUploader BufferUploader;
    Window Window;


    public ImguiRenderer(GraphicsDevice graphicsDevice, TextureFormat swapchainFormat, Window Window, MoonWorks.Storage.TitleStorage titleStorage)
    {
        this.GraphicsDevice = graphicsDevice;
        this.Window = Window;
        // RenderTexture = Texture.Create2D(GraphicsDevice, "Render Texture", Dimensions.GAME_W, Dimensions.GAME_H, swapchainFormat, TextureUsageFlags.ColorTarget | TextureUsageFlags.Sampler);
        BufferUploader = new ResourceUploader(GraphicsDevice);
        LoadImgui(titleStorage);
    }

    void LoadImgui(MoonWorks.Storage.TitleStorage RootTitleStorage)
    {
        ImGui.CreateContext();
        var io = ImGui.GetIO();
        // io.Fonts.AddFontFromFileTTF(Path.Combine(Fonts.FontContentPath, "FiraCode-Regular.ttf"), 16);
        io.Fonts.AddFontFromFileTTF("Content/Fonts/FiraCode-Regular.ttf", 16);
        io.Fonts.Build();
        io.DisplaySize = new Vector2(Window.Width, Window.Height);
        io.DisplayFramebufferScale = Vector2.One;
        ImGuiVertexShader = Shader.Create(
                GraphicsDevice,
                RootTitleStorage,
                // $"{ShaderContentPath}/ImGui.vert.spv",
                // Path.Combine(ShaderPath2, "ImGui.vert.spv"),
                "Content/Shaders/ImGui.vert.spv",
                "main",
                new ShaderCreateInfo
                {
                    Format = ShaderFormat.SPIRV,
                    Stage = ShaderStage.Vertex,
                    NumUniformBuffers = 1
                }
            );

        ImGuiFragmentShader = Shader.Create(
            GraphicsDevice,
            RootTitleStorage,
            // $"{ShaderContentPath}/ImGui.frag.spv",
            // "C:/Users/rober/Documents/Moonworks/projects/BunnyTower/ContentBuilder/ContentBuilderUI/Content/Shaders/ImGui.frag.spv",
            "Content/Shaders/ImGui.frag.spv",
            // Path.Combine(ShaderContentPath, "ImGui.frag.spv"),
            "main",
            new ShaderCreateInfo
            {
                Format = ShaderFormat.SPIRV,
                Stage = ShaderStage.Fragment,
                NumSamplers = 1
            }
        );

        ImGuiSampler = Sampler.Create(GraphicsDevice, SamplerCreateInfo.LinearClamp);

        ImGuiPipeline = GraphicsPipeline.Create(
            GraphicsDevice,
            new GraphicsPipelineCreateInfo
            {
                TargetInfo = new GraphicsPipelineTargetInfo
                {
                    ColorTargetDescriptions = [
                        new ColorTargetDescription
                            {
                                Format = Window.SwapchainFormat,
                                BlendState = ColorTargetBlendState.NonPremultipliedAlphaBlend
                            }
                    ]
                },
                DepthStencilState = DepthStencilState.Disable,
                VertexShader = ImGuiVertexShader,
                FragmentShader = ImGuiFragmentShader,
                VertexInputState = VertexInputState.CreateSingleBinding<Position2DTextureColorVertex>(),
                PrimitiveType = PrimitiveType.TriangleList,
                RasterizerState = RasterizerState.CW_CullNone,
                MultisampleState = MultisampleState.None
            }
        );
        BuildFontAtlas();
    }

    private unsafe void BuildFontAtlas()
    {
        var textureUploader = new ResourceUploader(GraphicsDevice);

        var io = ImGui.GetIO();

        io.Fonts.GetTexDataAsRGBA32(
            out System.IntPtr pixelData,
            out int width,
            out int height,
            out int bytesPerPixel
        );

        var pixelSpan = new ReadOnlySpan<Color>((void*)pixelData, width * height);

        FontTexture = textureUploader.CreateTexture2D(
            pixelSpan,
            TextureFormat.R8G8B8A8Unorm,
            TextureUsageFlags.Sampler,
            (uint)width,
            (uint)height);

        textureUploader.Upload();
        textureUploader.Dispose();

        io.Fonts.SetTexID(FontTexture.Handle);
        io.Fonts.ClearTexData();

        // TextureStorage.Add(FontTexture);
    }





    public void Draw(CommandBuffer commandBuffer, Texture swapchainTexture)
    {
        // Update();
        // Console.WriteLine("running imgui draw");
        ImGui.Render();

        var io = ImGui.GetIO();
        var drawDataPtr = ImGui.GetDrawData();

        UpdateImGuiBuffers(drawDataPtr);

        // var commandBuffer = GraphicsDevice.AcquireCommandBuffer();
        // var swapchainTexture = commandBuffer.AcquireSwapchainTexture(Window);

        RenderCommandLists(commandBuffer, swapchainTexture, drawDataPtr, io);

        // GraphicsDevice.Submit(commandBuffer);
    }
    bool init = false;
    private unsafe void UpdateImGuiBuffers(ImDrawDataPtr drawDataPtr)
    {
        // if (drawDataPtr.TotalVtxCount == 0) { return; }

        var commandBuffer = GraphicsDevice.AcquireCommandBuffer();

        if (drawDataPtr.TotalVtxCount > VertexCount || !init)
        {
            ImGuiVertexBuffer?.Dispose();

            VertexCount = (uint)(drawDataPtr.TotalVtxCount * 1.5f);
            ImGuiVertexBuffer = MoonWorks.Graphics.Buffer.Create<Position2DTextureColorVertex>(
                GraphicsDevice,
                BufferUsageFlags.Vertex,
                VertexCount
            );
        }

        if (drawDataPtr.TotalIdxCount > IndexCount || !init)
        {
            ImGuiIndexBuffer?.Dispose();

            IndexCount = (uint)(drawDataPtr.TotalIdxCount * 1.5f);
            ImGuiIndexBuffer = MoonWorks.Graphics.Buffer.Create<ushort>(
                GraphicsDevice,
                BufferUsageFlags.Index,
                IndexCount
            );
        }
        init = true;

        uint vertexOffset = 0;
        uint indexOffset = 0;

        // Console.Write($"\rcmd list count: {drawDataPtr.CmdListsCount}");

        for (var n = 0; n < drawDataPtr.CmdListsCount; n += 1)
        {
            var cmdList = drawDataPtr.CmdLists[n];

            BufferUploader.SetBufferData(
                ImGuiVertexBuffer,
                vertexOffset,
                new ReadOnlySpan<Position2DTextureColorVertex>((void*)cmdList.VtxBuffer.Data, cmdList.VtxBuffer.Size),
                n == 0
            );

            BufferUploader.SetBufferData(
                ImGuiIndexBuffer,
                indexOffset,
                new ReadOnlySpan<ushort>((void*)cmdList.IdxBuffer.Data, cmdList.IdxBuffer.Size),
                n == 0
            );

            vertexOffset += (uint)cmdList.VtxBuffer.Size;
            indexOffset += (uint)cmdList.IdxBuffer.Size;
        }

        BufferUploader.Upload();
        GraphicsDevice.Submit(commandBuffer);
    }

    // private unsafe void BuildFontAtlas()
    // {
    //     var textureUploader = new ResourceUploader(GraphicsDevice);

    //     var io = ImGui.GetIO();

    //     io.Fonts.GetTexDataAsRGBA32(
    //         out System.IntPtr pixelData,
    //         out int width,
    //         out int height,
    //         out int bytesPerPixel
    //     );

    //     var pixelSpan = new ReadOnlySpan<Color>((void*)pixelData, width * height);

    //     FontTexture = textureUploader.CreateTexture2D(
    //         pixelSpan,
    //         TextureFormat.R8G8B8A8Unorm,
    //         TextureUsageFlags.Sampler,
    //         (uint)width,
    //         (uint)height);

    //     textureUploader.Upload();
    //     textureUploader.Dispose();

    //     io.Fonts.SetTexID(FontTexture.Handle);
    //     io.Fonts.ClearTexData();

    //     TextureStorage.Add(FontTexture);
    // }

    private void RenderCommandLists(CommandBuffer commandBuffer, Texture renderTexture, ImDrawDataPtr drawDataPtr, ImGuiIOPtr ioPtr)
    {
        var view = Matrix4x4.CreateLookAt(
            new Vector3(0, 0, 1),
            Vector3.Zero,
            Vector3.UnitY
        );

        var projection = Matrix4x4.CreateOrthographicOffCenter(
            0,
            480,
            270,
            0,
            0.01f,
            4000f
        );

        var viewProjectionMatrix = view * projection;

        var renderPass = commandBuffer.BeginRenderPass(
            new ColorTargetInfo(renderTexture, LoadOp.DontCare)
        // new ColorTargetInfo(renderTexture, Color.White)
        );

        renderPass.BindGraphicsPipeline(ImGuiPipeline);

        commandBuffer.PushVertexUniformData(
            Matrix4x4.CreateOrthographicOffCenter(0, ioPtr.DisplaySize.X, ioPtr.DisplaySize.Y, 0, -1, 1)
        );

        renderPass.BindVertexBuffers(ImGuiVertexBuffer);
        renderPass.BindIndexBuffer(ImGuiIndexBuffer, IndexElementSize.Sixteen);

        uint vertexOffset = 0;
        uint indexOffset = 0;

        for (int n = 0; n < drawDataPtr.CmdListsCount; n += 1)
        {
            var cmdList = drawDataPtr.CmdLists[n];

            for (int cmdIndex = 0; cmdIndex < cmdList.CmdBuffer.Size; cmdIndex += 1)
            {
                var drawCmd = cmdList.CmdBuffer[cmdIndex];

                renderPass.BindFragmentSamplers(
                    new TextureSamplerBinding(FontTexture, ImGuiSampler)
                );

                var topLeft = Vector2.Transform(new Vector2(drawCmd.ClipRect.X, drawCmd.ClipRect.Y), viewProjectionMatrix);
                var bottomRight = Vector2.Transform(new Vector2(drawCmd.ClipRect.Z, drawCmd.ClipRect.W), viewProjectionMatrix);

                var width = drawCmd.ClipRect.Z - (int)drawCmd.ClipRect.X;
                var height = drawCmd.ClipRect.W - (int)drawCmd.ClipRect.Y;

                if (width <= 0 || height <= 0)
                {
                    continue;
                }

                renderPass.SetScissor(
                    new Rect(
                        (int)drawCmd.ClipRect.X,
                        (int)drawCmd.ClipRect.Y,
                        (int)width,
                        (int)height
                    )
                );

                renderPass.DrawIndexedPrimitives(
                    drawCmd.ElemCount,
                    1,
                    indexOffset,
                    (int)vertexOffset,
                    0
                );

                indexOffset += drawCmd.ElemCount;
            }

            vertexOffset += (uint)cmdList.VtxBuffer.Size;
        }

        commandBuffer.EndRenderPass(renderPass);
    }


}

public struct Position2DTextureColorVertex : IVertexType
{
    public Vector2 Position;
    public Vector2 TexCoord;
    public Color Color;

    public Position2DTextureColorVertex(
        Vector2 position,
        Vector2 texcoord,
        Color color
    )
    {
        Position = position;
        TexCoord = texcoord;
        Color = color;
    }

    public static VertexElementFormat[] Formats =>
    [
        VertexElementFormat.Float2,
                VertexElementFormat.Float2,
                VertexElementFormat.Ubyte4Norm
    ];

    public static uint[] Offsets =>
    [
        0,
                8,
                16
    ];
}

