using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.D3DCompiler;

//三角形が表示されないんですわぁ(⌒,_ゝ⌒)
//90割こぴぺ
//http://memeplex.blog.shinobi.jp/directx11/c-でdirectx11%20slimdxチュートリアルその04%20三角形の表示（色なし）


class Game : System.Windows.Forms.Form
{
    public SlimDX.Direct3D11.Device GraphicsDevice;
    public SwapChain SwapChain;
    public RenderTargetView RenderTarget;


    public void Run()
    {
        initDevice();
        SlimDX.Windows.MessagePump.Run(this, Draw);
        disposeDevice();
    }

    private void initDevice()
    {
        MyDirectXHelper.CreateDeviceAndSwapChain(
            this, out GraphicsDevice, out SwapChain
            );

        initRenderTarget();
        initViewport();

        LoadContent();
    }

    private void initRenderTarget()
    {
        using (Texture2D backBuffer
            = SlimDX.Direct3D11.Resource.FromSwapChain<Texture2D>(SwapChain, 0)
            )
        {
            RenderTarget = new RenderTargetView(GraphicsDevice, backBuffer);
            GraphicsDevice.ImmediateContext.OutputMerger.SetTargets(RenderTarget);
        }
    }

    private void initViewport()
    {
        GraphicsDevice.ImmediateContext.Rasterizer.SetViewports(
            new Viewport
            {
                Width = ClientSize.Width,
                Height = ClientSize.Height,
            }
            );
    }

    private void disposeDevice()
    {
        UnloadContent();
        RenderTarget.Dispose();
        GraphicsDevice.Dispose();
        SwapChain.Dispose();
    }

    protected virtual void Draw() { }
    protected virtual void LoadContent() { }
    protected virtual void UnloadContent() { }
}

class Program
{
    static void Main()
    {
        using (Game game = new MyGame())
        {
            game.Run();
        }
    }
}

class MyGame : Game
{
    Effect effect;
    InputLayout vertexLayout;
    Buffer vertexBuffer;

    protected override void Draw()
    {
        GraphicsDevice.ImmediateContext.ClearRenderTargetView(
            RenderTarget,
            new SlimDX.Color4(1, 0.39f, 0.58f, 0.93f)
            );

        initTriangleInputAssembler();
        drawTriangle();

        SwapChain.Present(0, PresentFlags.None);
    }

    private void initTriangleInputAssembler()
    {
        GraphicsDevice.ImmediateContext.InputAssembler.InputLayout = vertexLayout;
        GraphicsDevice.ImmediateContext.InputAssembler.SetVertexBuffers(
            0,
            new VertexBufferBinding(vertexBuffer, VertexPositionColor.SizeInBytes, 0)
            );
        GraphicsDevice.ImmediateContext.InputAssembler.PrimitiveTopology
            = PrimitiveTopology.TriangleList;
    }

    private void drawTriangle()
    {
        effect.GetTechniqueByIndex(0).GetPassByIndex(0).Apply(GraphicsDevice.ImmediateContext);
        GraphicsDevice.ImmediateContext.Draw(3, 0);
    }


    protected override void LoadContent()
    {
        initEffect();
        initVertexLayout();
        initVertexBuffer();
    }

    private void initEffect()
    {
        //しぇーだーナリ
        using (ShaderBytecode shaderBytecode = ShaderBytecode.CompileFromFile(
            "\\myEffect.fx", "fx_5_0",
            ShaderFlags.None,
            EffectFlags.None
            ))
        {
            effect = new Effect(GraphicsDevice, shaderBytecode);
        }
    }

    private void initVertexLayout()
    {
        vertexLayout = new InputLayout(
            GraphicsDevice,
            effect.GetTechniqueByIndex(0).GetPassByIndex(0).Description.Signature,
            VertexPositionColor.VertexElements
            );
    }

    private void initVertexBuffer()
    {
        vertexBuffer = MyDirectXHelper.CreateVertexBuffer(
            GraphicsDevice,
            new[] {
                new VertexPositionColor
                {
                    Position = new Vector3(0, 0.5f, 0),
                    Color = new Vector3(1, 1, 1) 
                },
                new VertexPositionColor
                {
                    Position = new Vector3(0.5f, 0, 0),
                    Color = new Vector3(0, 0, 1)
                },
                new VertexPositionColor
                {
                    Position = new Vector3(-0.5f, 0, 0),
                    Color = new Vector3(1, 0, 0)
                },
            });
    }

    protected override void UnloadContent()
    {
        effect.Dispose();
        vertexLayout.Dispose();
        vertexBuffer.Dispose();
    }
}

struct VertexPositionColor
{
    public Vector3 Position;
    public Vector3 Color;

    public static readonly InputElement[] VertexElements = new[]
    {
        new InputElement
        {
            SemanticName = "SV_Position",
            Format = Format.R32G32B32_Float
        },
        new InputElement
        {
            SemanticName = "COLOR",
            Format = Format.R32G32B32_Float,
            AlignedByteOffset = InputElement.AppendAligned//自動的にオフセット決定
        }
    };

    public static int SizeInBytes
    {
        get
        {
            return System.Runtime.InteropServices.
                Marshal.SizeOf(typeof(VertexPositionColor));
        }
    }
}

class MyDirectXHelper
{
    public static void CreateDeviceAndSwapChain(
        System.Windows.Forms.Form form,
        out SlimDX.Direct3D11.Device device,
        out SlimDX.DXGI.SwapChain swapChain
        )
    {
        SlimDX.Direct3D11.Device.CreateWithSwapChain(
            DriverType.Hardware,
            DeviceCreationFlags.None,
            new SwapChainDescription
            {
                BufferCount = 1,
                OutputHandle = form.Handle,
                IsWindowed = true,
                SampleDescription = new SampleDescription
                {
                    Count = 1,
                    Quality = 0
                },
                ModeDescription = new ModeDescription
                {
                    Width = form.ClientSize.Width,
                    Height = form.ClientSize.Height,
                    RefreshRate = new SlimDX.Rational(60, 1),
                    Format = Format.R8G8B8A8_UNorm
                },
                Usage = Usage.RenderTargetOutput
            },
            out device,
            out swapChain
            );
    }

    public static Buffer CreateVertexBuffer(
        SlimDX.Direct3D11.Device graphicsDevice,
        System.Array vertices
        )
    {
        using (SlimDX.DataStream vertexStream
            = new SlimDX.DataStream(vertices, true, true))
        {
            return new Buffer(
                graphicsDevice,
                vertexStream,
                new BufferDescription
                {
                    SizeInBytes = (int)vertexStream.Length,
                    BindFlags = BindFlags.VertexBuffer,
                }
                );
        }
    }
}