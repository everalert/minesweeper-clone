using System;
using System.Collections.Generic;
using System.Linq;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using SpriteTextRenderer;
using SpriteRenderer = SpriteTextRenderer.SlimDX.SpriteRenderer;
using TextBlockRenderer = SpriteTextRenderer.SlimDX.TextBlockRenderer;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Imaging;

namespace Minesweeper
{
    static partial class GameSystem
    {
        public static class Interface
        {
            public static void Initialize()
            {
                //window
                Data.Window = new RenderForm();
                Data.Window.Resize += (o, e) => ResizeWindow();
                //ResizeWindow(); // switch to this when full manual interface implemented
                Data.Window.Text = Data.WindowTitle;
                Data.Window.StartPosition = Data.WindowStartPosition;
                Data.Window.FormBorderStyle = Data.WindowBorderStyle;
                Data.Window.MinimumSize = new Size(Data.WindowMinWidth, Data.WindowMinHeight);
                Data.Window.ClientSize = new Size(Data.WindowStartWidth, Data.WindowStartHeight);
                //Data.Window.Icon = new Icon("img\\icon.ico");

                Cursor.Current = Data.WindowCursorHoverResize;

                Data.Timer = new Stopwatch();
                Data.Timer.Start();

                //directx
                InitDevice();
                InitContext();
                InitRenderTarget();
                InitViewport();

                //resources
                InitColour();
                InitFont();
                InitImage();
                ResizeWindow();
                InitMinefieldGraphics();

                Data.InterfaceInitialized = true;
            }
            public static void Dispose()
            {
                //resources
                DisposeMinefieldGraphics();
                DisposeFont();
                DisposeImage();


                //directx
                Data.DX11Context.Dispose();
                Data.DX11RenderTarget.Dispose();
                Data.DX11SwapChain.Dispose();
                Data.DX11Device.Dispose();
                Data.DX11SpriteRenderer.Dispose();

                //window
                Data.Window.Dispose();
            }

            public static void Tick()
            {
                //Data.Timer.Stop();
                Data.FrameProcessingTime = Data.Timer.Elapsed.TotalMilliseconds - Data.ElapsedTimeAtFrameStart;
                Thread.Sleep(Math.Max((int)(1f / Data.FrameRate * 1000f - Data.FrameProcessingTime), 0));
                Data.FrameTime = Data.Timer.Elapsed.TotalMilliseconds - Data.ElapsedTimeAtFrameStart;
                //Data.Timer = Stopwatch.StartNew();
                Data.ElapsedTimeAtFrameStart = Data.Timer.Elapsed.TotalMilliseconds;
                Data.FrameCount++;
            }


            //// RESOURCE MANAGEMENT
            //// maybe some of this can be moved to util class too?

            public static void InitDevice()
            {
                var description = new SwapChainDescription()
                {
                    BufferCount = 1,
                    Usage = Usage.RenderTargetOutput,
                    OutputHandle = Data.Window.Handle,
                    IsWindowed = true,
                    ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                    SampleDescription = new SampleDescription(1, 0),
                    Flags = SwapChainFlags.AllowModeSwitch,
                    SwapEffect = SwapEffect.Discard
                };
                Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out Data.DX11Device, out Data.DX11SwapChain);
                Data.DX11Device.Factory.SetWindowAssociation(Data.Window.Handle, WindowAssociationFlags.IgnoreAll);
            }
            public static void InitContext()
            {
                Data.DX11Context = Data.DX11Device.ImmediateContext;
            }
            public static void InitViewport()
            {
                Data.DX11Viewport = new Viewport(0.0f, 0.0f, Data.Window.ClientSize.Width, Data.Window.ClientSize.Height);
                Data.DX11Context.Rasterizer.SetViewports(Data.DX11Viewport);
            }
            public static void InitRenderTarget()
            {
                using (var resource = Resource.FromSwapChain<Texture2D>(Data.DX11SwapChain, 0))
                    Data.DX11RenderTarget = new RenderTargetView(Data.DX11Device, resource);
                Data.DX11Context.OutputMerger.SetTargets(Data.DX11RenderTarget);
            }

            public static void InitFont()
            {
                // eventually style everything properly, setup for now
                // also, separate STR from font setup?
                Data.DX11SpriteRenderer = new SpriteRenderer(Data.DX11Device) { HandleBlendState = true, AllowReorder = false };
                Data.Fonts.Add(Font.Default,
                    new TextBlockRenderer(Data.DX11SpriteRenderer, "Consolas",
                    SlimDX.DirectWrite.FontWeight.Medium,
                    SlimDX.DirectWrite.FontStyle.Normal,
                    SlimDX.DirectWrite.FontStretch.Normal,
                    16)
                );
                Data.Fonts.Add(Font.HUD,
                    new TextBlockRenderer(Data.DX11SpriteRenderer, "Consolas",
                    SlimDX.DirectWrite.FontWeight.Medium,
                    SlimDX.DirectWrite.FontStyle.Normal,
                    SlimDX.DirectWrite.FontStretch.Normal,
                    32)
                );
            }
            public static void DisposeFont()
            {
                foreach (KeyValuePair<Font, TextBlockRenderer> font in Data.Fonts.ToList())
                {
                    font.Value.Dispose();
                    Data.Fonts.Remove(font.Key);
                }
                Data.DX11SpriteRenderer.Dispose();
            }

            public static void InitImage()
            {
                AddImage(Image.Default, @"asset\img\default.gif");
                AddImage(Image.Empty, @"asset\img\grid-empty.png");
                AddImage(Image.Cell, @"asset\img\grid-cell.png");
                AddImage(Image.Cell1, @"asset\img\grid-cell-1.png");
                AddImage(Image.Cell2, @"asset\img\grid-cell-2.png");
                AddImage(Image.Cell3, @"asset\img\grid-cell-3.png");
                AddImage(Image.Cell4, @"asset\img\grid-cell-4.png");
                AddImage(Image.Cell5, @"asset\img\grid-cell-5.png");
                AddImage(Image.Cell6, @"asset\img\grid-cell-6.png");
                AddImage(Image.Cell7, @"asset\img\grid-cell-7.png");
                AddImage(Image.Cell8, @"asset\img\grid-cell-8.png");
                AddImage(Image.CellOutside, @"asset\img\grid-cell-outside.png");
                AddImage(Image.CellClosed, @"asset\img\grid-cell-closed.png");
                AddImage(Image.CellClosedHover, @"asset\img\grid-cell-closed-hover.png");
                AddImage(Image.Mine, @"asset\img\grid-mine.png");
                AddImage(Image.Flag, @"asset\img\grid-flag.png");
                AddImage(Image.QuestionMark, @"asset\img\grid-questionmark.png");
                AddImage(Image.Explosion, @"asset\img\grid-explosion.png");
                CreateDebugBackground();
                Data.NumberedCellImages = new ShaderResourceView[8]
                {
                    Data.Images[Image.Cell1],
                    Data.Images[Image.Cell2],
                    Data.Images[Image.Cell3],
                    Data.Images[Image.Cell4],
                    Data.Images[Image.Cell5],
                    Data.Images[Image.Cell6],
                    Data.Images[Image.Cell7],
                    Data.Images[Image.Cell8]
                };
            }

            public static void AddImage(Image i, string f)
            {
                Data.Images.Add(i, new ShaderResourceView(Data.DX11Device, Texture2D.FromFile(Data.DX11Device, f)));
            }

            public static void CreateDebugBackground()
            {
                while (Data.Images.ContainsKey(Image.DebugBackground))
                {
                    Data.Images[Image.DebugBackground].Resource.Dispose();
                    Data.Images[Image.DebugBackground].Dispose();
                    Data.Images.Remove(Image.DebugBackground);
                }

                using (Bitmap tex = new Bitmap(8, 8, PixelFormat.Format32bppArgb))
                using (Graphics g = Graphics.FromImage(tex))
                {
                    g.Clear(Data.DEBUG_BACKGROUND_COLOR);
                    var data = tex.LockBits(new Rectangle(0, 0, tex.Width, tex.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    var ds = new DataStream(data.Stride * tex.Height, true, true);
                    ds.WriteRange(data.Scan0, data.Stride * tex.Height);
                    ds.Position = 0;
                    Texture2D tex_out = new Texture2D(Data.DX11Device, new Texture2DDescription()
                    {
                        Width = tex.Width,
                        Height = tex.Height,
                        ArraySize = 1,
                        BindFlags = BindFlags.ShaderResource,
                        Usage = ResourceUsage.Immutable,
                        CpuAccessFlags = CpuAccessFlags.None,
                        Format = Format.B8G8R8A8_UNorm,
                        MipLevels = 1,
                        OptionFlags = ResourceOptionFlags.None,
                        SampleDescription = new SampleDescription(1, 0)
                    }, new DataRectangle(data.Stride, ds));
                    Data.Images.Add(Image.DebugBackground, new ShaderResourceView(Data.DX11Device, tex_out));
                    tex.UnlockBits(data);
                }
            }

            public static void DisposeImage()
            {
                foreach (KeyValuePair<Image, ShaderResourceView> img in Data.Images.ToList())
                {
                    img.Value.Resource.Dispose();
                    img.Value.Dispose();
                    Data.Images.Remove(img.Key);
                }
            }

            public static void InitColour()
            {
                Data.Colours.Add(Colour.Default, Color.FromArgb(0, 0, 0, 0));
            }

            public static void CheckInterfaceInitialized()
            {
                if (!Data.InterfaceInitialized)
                    throw new Exception("Interface not yet initialized.");
            }



            public static void ResizeWindow()
            {
                ResizeGrid();
                ResizeResizingBounds();
                try
                {
                    CheckInterfaceInitialized();
                    DisposeFont();
                    DisposeImage();
                    Data.DX11RenderTarget.Dispose();
                    Data.DX11SwapChain.ResizeBuffers(1, Data.Window.ClientSize.Width, Data.Window.ClientSize.Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
                    Data.DX11BackBuffer = Texture2D.FromSwapChain<Texture2D>(Data.DX11SwapChain, 0);
                    InitRenderTarget();
                    InitViewport();
                    InitImage();
                    InitFont();
                    Data.DX11SpriteRenderer.RefreshViewport();

                    SetupMinefieldGraphics();
                    //TODO: resizing causes text and debug background to lose alpha values, disposing and re-initializing works but is laggy af, wat do?
                    //TODO: player can trigger cell with same mouse press as resizing the window, need to prevent
                }
                catch { }
            }

            public static void ResizeWindow2()
            {
                Size old_size = Data.Window.ClientSize;
                int new_x, new_y, new_w, new_h;
                if (Data.WindowResize.HasFlag(Input.WindowResizeState.ResizeR))
                {
                    new_w = (int)Math.Floor((decimal)(Data.MouseLocation.X + Data.WindowResizeBorderSize / 2) / 2) * 2;
                    new_x = Data.Window.Location.X;
                }
                else if (Data.WindowResize.HasFlag(Input.WindowResizeState.ResizeL))
                {
                    new_w = (int)Math.Floor((decimal)(Data.Window.ClientSize.Width + (0 - Data.MouseLocation.X) + Data.WindowResizeBorderSize / 2) / 2) * 2;
                    new_x = Data.Window.Location.X + Data.MouseLocation.X - Data.WindowResizeBorderSize / 2;
                }
                else
                {
                    new_w = Data.Window.ClientSize.Width;
                    new_x = Data.Window.Location.X;
                }
                if (Data.WindowResize.HasFlag(Input.WindowResizeState.ResizeB))
                {
                    new_h = (int)Math.Floor((decimal)(Data.MouseLocation.Y + Data.WindowResizeBorderSize / 2) / 2) * 2;
                    new_y = Data.Window.Location.Y;
                }
                else if (Data.WindowResize.HasFlag(Input.WindowResizeState.ResizeT))
                {
                    new_h = (int)Math.Floor((decimal)(Data.Window.ClientSize.Height + (0 - Data.MouseLocation.Y) + Data.WindowResizeBorderSize / 2) / 2) * 2;
                    new_y = Data.Window.Location.Y + Data.MouseLocation.Y - Data.WindowResizeBorderSize / 2;
                }
                else
                {
                    new_h = Data.Window.ClientSize.Height;
                    new_y = Data.Window.Location.Y;
                }
                Data.Window.Location = new Point(new_x, new_y);
                Data.Window.ClientSize = new Size(Math.Max(new_w,Data.WindowMinWidth), Math.Max(new_h, Data.WindowMinHeight));
                if (!old_size.Equals(Data.Window.ClientSize))
                    ResizeWindow();
            }

            public static void ResizeGrid()
            {
                double w = Data.Window.ClientSize.Width - Data.WindowResizeBorderSize * 2;
                double h = Data.Window.ClientSize.Height - Data.WindowResizeBorderSize * 2;
                double m = Math.Floor(Math.Min(w / Data.MineFieldWidth, h / Data.MineFieldHeight));
                Data.GridCellSize = (int)m;
                Data.GridCellSizeObj = new Vector2(Data.GridCellSize);
                Data.GridWidth = (int)m * Data.MineFieldWidth;
                Data.GridHeight = (int)m * Data.MineFieldHeight;
                Data.GridBounds = new Rectangle((Data.Window.ClientSize.Width - Data.GridWidth) / 2, (Data.Window.ClientSize.Height - Data.GridHeight) / 2, Data.GridWidth, Data.GridHeight);
                Data.GridOuterWidth = (int)Math.Ceiling((double)Data.Window.ClientSize.Width / Data.GridCellSize / 2) * Data.GridCellSize * 2;
                Data.GridOuterHeight = (int)Math.Ceiling((double)Data.Window.ClientSize.Height / Data.GridCellSize / 2) * Data.GridCellSize * 2;
                Data.GridOuterBounds = new Rectangle((Data.Window.ClientSize.Width - Data.GridOuterWidth) / 2, (Data.Window.ClientSize.Height - Data.GridOuterHeight) / 2, Data.GridOuterWidth, Data.GridOuterHeight);
            }

            public static void ResizeResizingBounds()
            {
                Data.WindowResizeBounds = new Rectangle(new Point(Data.WindowResizeBorderSize), Data.Window.ClientSize);
                Data.WindowResizeBounds.Inflate(0 - Data.WindowResizeBorderSize * 2, 0 - Data.WindowResizeBorderSize * 2);
            }



            public static void InitMinefieldGraphics()
            {
                //Data.GridSrcImgCell = new Bitmap(@"asset\img\grid-cell-outside.png");
                //Data.GridSrcImgCellOutside = new Bitmap(@"asset\img\grid-cell-outside.png");
                SetupMinefieldGraphics();
            }

            public static void SetupMinefieldGraphics()
            {
                try { DisposeMinefieldGraphics(); } catch { }

                Texture2DDescription td1 = new Texture2DDescription()
                {
                    Width = Data.GridBounds.Width,
                    Height = Data.GridBounds.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R8G8B8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };
                Data.ClosedCellRenderLayerTex = new Texture2D(Data.DX11Device, td1);
                Data.CellRenderLayerTopTex = new Texture2D(Data.DX11Device, td1);

                RenderTargetViewDescription rtvd1 = new RenderTargetViewDescription()
                {
                    Format = td1.Format,
                    Dimension = RenderTargetViewDimension.Texture2D,
                    MipSlice = 0
                };
                Data.ClosedCellRenderLayerRTV = new RenderTargetView(Data.DX11Device, Data.ClosedCellRenderLayerTex, rtvd1);
                Data.CellRenderLayerTopRTV = new RenderTargetView(Data.DX11Device, Data.CellRenderLayerTopTex, rtvd1);

                ShaderResourceViewDescription srvd1 = new ShaderResourceViewDescription()
                {
                    Format = td1.Format,
                    Dimension = ShaderResourceViewDimension.Texture2D,
                    MostDetailedMip = 0,
                    MipLevels = 1
                };
                Data.ClosedCellRenderLayerSRV = new ShaderResourceView(Data.DX11Device, Data.ClosedCellRenderLayerTex, srvd1);
                Data.CellRenderLayerTopSRV = new ShaderResourceView(Data.DX11Device, Data.CellRenderLayerTopTex, srvd1);

                Data.ClosedCellRenderLayerSR = new SpriteRenderer(Data.ClosedCellRenderLayerSRV.Device) { HandleBlendState = true, AllowReorder = false };
                Data.CellRenderLayerTopSR = new SpriteRenderer(Data.CellRenderLayerTopSRV.Device) { HandleBlendState = true, AllowReorder = false };

                Data.ClosedCellRenderLayerSR.RefreshViewport();
                Data.ClosedCellRenderLayerRendered = false;
                Data.CellRenderLayerTopSR.RefreshViewport();
                Data.CellRenderLayerTopRendered = false;

                //Data.GridLayerBaseOuter = new Bitmap(Data.GridOuterWidth, Data.GridOuterHeight);
                //Data.GridLayerBaseOuterG = Graphics.FromImage(Data.GridLayerBaseOuter);
            }

            public static void DisposeMinefieldGraphics()
            {
                Data.ClosedCellRenderLayerSR.Dispose();
                Data.ClosedCellRenderLayerSRV.Dispose();
                Data.ClosedCellRenderLayerRTV.Dispose();
                Data.ClosedCellRenderLayerTex.Dispose();

                //Data.GridLayerBaseOuterG.Dispose();
            }

            public static void DrawMinefield()
            {
                // functional limit of grid size imposed by either total pixels (int.maxvalue) or max whole double number (when calculating for total pixels)?
                CheckInterfaceInitialized();

                {
                    var loc = new Vector2(Data.GridOuterBounds.X, Data.GridOuterBounds.Y);
                    var size = new Vector2(Data.GridOuterBounds.Width, Data.GridOuterBounds.Height);
                    Data.DX11SpriteRenderer.Draw(Data.Images[Image.CellOutside], loc, size, new Vector2(0), 0d, new Vector2(0), new Vector2(Data.GridOuterWidth / Data.GridCellSize, Data.GridOuterHeight / Data.GridCellSize), new Color4(Color.White), CoordinateType.Absolute);
                }
                {
                    bool cell_change_processed = false;
                    var loc = new Vector2(Data.GridBounds.X, Data.GridBounds.Y);
                    var size = new Vector2(Data.GridBounds.Width, Data.GridBounds.Height);
                    Data.DX11SpriteRenderer.Draw(Data.Images[Image.Cell], loc, size, new Vector2(0), 0d, new Vector2(0), new Vector2(Data.GridWidth / Data.GridCellSize, Data.GridHeight / Data.GridCellSize), new Color4(Color.White), CoordinateType.Absolute);

                    if (Data.CellRenderLayerTopRendered == false)
                    {
                        Data.DX11Context.OutputMerger.SetTargets(Data.CellRenderLayerTopRTV);
                        Data.DX11Context.ClearRenderTargetView(Data.CellRenderLayerTopRTV, Data.Colours[Colour.Default]);
                    }

                    for (int i = 0; i < Data.MineField.Length; i++)
                    {

                        if (Data.MineFieldChangedQueue.Contains(i) || Data.ClosedCellRenderLayerRendered == false || Data.CellRenderLayerTopRendered == false)
                        {
                            var iloc = new Vector2((float)i % Data.MineFieldWidth * Data.GridCellSize, (float)Math.Floor((double)i / Data.MineFieldWidth) * Data.GridCellSize);

                            Data.DX11Context.OutputMerger.SetTargets(Data.ClosedCellRenderLayerRTV);
                            ShaderResourceView img = Data.MineField[i].HasFlag(Game.MineInfo.IsCleared) ? Data.Images[Image.Cell] : Data.Images[Image.CellClosed];
                            Data.ClosedCellRenderLayerSR.Draw(img, iloc, Data.GridCellSizeObj, CoordinateType.Absolute);
                            if (Data.MineField[i].HasFlag(Game.MineInfo.IsCleared) && Data.MineFieldNeighbours[i] > 0)
                                Data.ClosedCellRenderLayerSR.Draw(Data.NumberedCellImages[Data.MineFieldNeighbours[i] - 1], iloc, Data.GridCellSizeObj, CoordinateType.Absolute);

                            Data.DX11Context.OutputMerger.SetTargets(Data.CellRenderLayerTopRTV);
                            if (Data.GameState == Game.EngineState.Ended && Data.MineField[i].HasFlag(Game.MineInfo.IsMine))
                                Data.CellRenderLayerTopSR.Draw(Data.Images[Image.Explosion], iloc, Data.GridCellSizeObj, CoordinateType.Absolute);
                            if (Data.MineField[i].HasFlag(Game.MineInfo.IsFlagged))
                                Data.CellRenderLayerTopSR.Draw(Data.MineField[i].HasFlag(Game.MineInfo.FlagIsQuestionMark) ? Data.Images[Image.QuestionMark] : Data.Images[Image.Flag], iloc, Data.GridCellSizeObj, CoordinateType.Absolute);
                            else
                                Data.CellRenderLayerTopSR.Draw(Data.Images[Image.Empty], iloc, Data.GridCellSizeObj, CoordinateType.Absolute);
                                //TODO: can see both flag and question mark after cycling through

                            if (Data.MineFieldChangedQueue.Contains(i))
                                Data.MineFieldChangedQueue.RemoveAt(Data.MineFieldChangedQueue.IndexOf(i));
                            cell_change_processed = true;
                        }
                    }
                    if (cell_change_processed)
                    {
                        Data.DX11Context.OutputMerger.SetTargets(Data.ClosedCellRenderLayerRTV);
                        Data.ClosedCellRenderLayerRendered = true;
                        Data.ClosedCellRenderLayerSR.Flush();
                        Data.DX11Context.OutputMerger.SetTargets(Data.CellRenderLayerTopRTV);
                        Data.CellRenderLayerTopRendered = true;
                        Data.CellRenderLayerTopSR.Flush();
                    }

                    Data.DX11Context.OutputMerger.SetTargets(Data.DX11RenderTarget);
                    Data.DX11SpriteRenderer.Draw(Data.ClosedCellRenderLayerSRV, loc, size, new Vector2(0), 0d, new Vector2(0), new Vector2(1), new Color4(Color.White), CoordinateType.Absolute);
                    if (Data.GridHoverCell >= 0 && !Data.MineField[Data.GridHoverCell].HasFlag(Game.MineInfo.IsCleared))
                    {
                        Data.DX11SpriteRenderer.Draw(Data.Images[Image.CellClosedHover],
                            new Vector2(Data.GridBounds.X + (float)Data.GridHoverCell % Data.MineFieldWidth * Data.GridCellSize, Data.GridBounds.Y + (float)Math.Floor((double)Data.GridHoverCell / Data.MineFieldWidth) * Data.GridCellSize),
                            Data.GridCellSizeObj, CoordinateType.Absolute);
                    }
                    Data.DX11SpriteRenderer.Draw(Data.CellRenderLayerTopSRV, loc, size, new Vector2(0), 0d, new Vector2(0), new Vector2(1), new Color4(Color.White), CoordinateType.Absolute);
                }
            }

            public static void DrawHUD()
            {
                Data.DX11Context.OutputMerger.SetTargets(Data.DX11RenderTarget);

                string time = string.Format("{0}:{1}",
                    Math.Floor((decimal)Data.GameTimer.ElapsedMilliseconds / 1000 / 60),
                    ((Data.GameTimer.ElapsedMilliseconds / 1000f) % 60f).ToString("00.00"));
                STRVector time_size = Data.Fonts[Font.HUD].MeasureString(time).Size;
                Vector2 time_pos = new Vector2(Data.HUDTimePos.X * Data.Window.ClientSize.Width - time_size.X / 2, Data.HUDTimePos.Y * Data.Window.ClientSize.Height - time_size.Y / 2);
                Data.Fonts[Font.HUD].DrawString(time, time_pos, Data.DEBUG_TEXT_COLOR);

                string cleared = string.Format("{0}/{1} CLEAR", Data.MineFieldCellsCleared, Data.MineFieldSafeCellCount);
                STRVector cleared_size = Data.Fonts[Font.HUD].MeasureString(cleared).Size;
                Vector2 cleared_pos = new Vector2(Data.HUDClearedPos.X * Data.Window.ClientSize.Width - cleared_size.X / 2, Data.HUDClearedPos.Y * Data.Window.ClientSize.Height - cleared_size.Y / 2);
                Data.Fonts[Font.HUD].DrawString(cleared, cleared_pos, Data.DEBUG_TEXT_COLOR);

                string flagged = string.Format("{0}/{1} FOUND", Data.MineFieldCellsFlagged, Data.MineFieldMineCount);
                STRVector flagged_size = Data.Fonts[Font.HUD].MeasureString(flagged).Size;
                Vector2 flagged_pos = new Vector2(Data.HUDFlaggedPos.X * Data.Window.ClientSize.Width - flagged_size.X / 2, Data.HUDFlaggedPos.Y * Data.Window.ClientSize.Height - flagged_size.Y / 2);
                Data.Fonts[Font.HUD].DrawString(flagged, flagged_pos, Data.DEBUG_TEXT_COLOR);
            }

            public static void DrawEndScreen()
            {
                Data.DX11Context.OutputMerger.SetTargets(Data.DX11RenderTarget);

                string message = Data.HUDEndMessage;
                STRVector message_size = Data.Fonts[Font.HUD].MeasureString(message).Size;
                Vector2 message_pos = new Vector2(Data.HUDEndMessagePos.X * Data.Window.ClientSize.Width - message_size.X / 2, Data.HUDEndMessagePos.Y * Data.Window.ClientSize.Height - message_size.Y / 2);
                Data.Fonts[Font.HUD].DrawString(message, message_pos, Data.DEBUG_TEXT_COLOR);
            }

            public static void DrawDebugPanel()
            {
                Data.DX11Context.OutputMerger.SetTargets(Data.DX11RenderTarget);
                Vector2 panel_pos = new Vector2(Data.DEBUG_MARGIN[3] * Data.Window.ClientSize.Width, Data.DEBUG_MARGIN[0] * Data.Window.ClientSize.Height);
                Vector2 panel_size = new Vector2(Data.Window.ClientSize.Width - (Data.DEBUG_MARGIN[1] + Data.DEBUG_MARGIN[3]) * Data.Window.ClientSize.Width, Data.Window.ClientSize.Height - (Data.DEBUG_MARGIN[0] + Data.DEBUG_MARGIN[2]) * Data.Window.ClientSize.Height);
                Data.DX11SpriteRenderer.Draw(Data.Images[Image.DebugBackground], panel_pos, panel_size, CoordinateType.Absolute);
                Vector2 text_pos = new Vector2(panel_pos.X + Data.DEBUG_PADDING[3], panel_pos.Y + Data.DEBUG_PADDING[0]);
                string debug_text = string.Format("DEBUG MENU\n\n\r{0}ms\n\n\rSEED: {5}\n\n\r{2}\n\n\r{3}\n\n\r{4}",
                    Data.FrameProcessingTime.ToString("00.000"),
                    Data.FrameTime.ToString("00.000"),
                    Data.DEBUG_MESSAGE_GRIDHOVERCELL,
                    Data.DEBUG_MESSAGE_MOUSE,
                    Data.DEBUG_MESSAGE_KEYBOARD,
                    Data.Seed);
                Data.Fonts[Font.Default].DrawString(debug_text, text_pos, Data.DEBUG_TEXT_COLOR);
            }

            #region reference drawing code <-
            //public static void DrawIconWithText(Vector2 scale, RectangleF coords, SpriteRenderer sprite, ShaderResourceView image, String text, TextBlockRenderer font, Color color, TextAlignment align, Point offset)
            //{
            //    var fntSz = font.FontSize * scale.X;
            //    var loc = new Vector2(coords.X * scale.X, coords.Y * scale.Y);
            //    var size = new Vector2(coords.Width * scale.X, coords.Height * scale.X); // to avoid img distortion
            //    sprite.Draw(image, loc, size, new Vector2(0, 0), 0, CoordinateType.Absolute);
            //    var region = new RectangleF(
            //        PointF.Add(new PointF(loc.X, loc.Y), new SizeF(size.X + offset.X * scale.X, size.Y / 2 + offset.Y * scale.Y - (float)Math.Ceiling(font.MeasureString(text, fntSz, CoordinateType.Absolute).Size.Y) / 2)),
            //        new SizeF(
            //            (float)Math.Ceiling(font.MeasureString(text, fntSz, CoordinateType.Absolute).Size.X),
            //            (float)Math.Ceiling(font.MeasureString(text, fntSz, CoordinateType.Absolute).Size.Y)
            //        )
            //    );
            //    font.DrawString(text, region, align, fntSz, color, CoordinateType.Absolute);
            //}
            #endregion



            public enum Font
            {
                Default,
                HUD
            }

            public enum Image
            {
                Default,
                Empty,
                Cell,
                Cell1,
                Cell2,
                Cell3,
                Cell4,
                Cell5,
                Cell6,
                Cell7,
                Cell8,
                CellClosed,
                CellClosedHover,
                CellOutside,
                Mine,
                Flag,
                QuestionMark,
                Explosion,
                DebugBackground
            }

            public enum Colour
            {
                Default
            }
        }
    }
}
