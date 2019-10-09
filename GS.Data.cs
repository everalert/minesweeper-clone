using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using SlimDX.Windows;
using SlimDX.DirectInput;
using Device = SlimDX.Direct3D11.Device;
using Resource = SlimDX.Direct3D11.Resource;
using SpriteTextRenderer;
using SpriteRenderer = SpriteTextRenderer.SlimDX.SpriteRenderer;
using TextBlockRenderer = SpriteTextRenderer.SlimDX.TextBlockRenderer;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;

namespace Minesweeper
{
    static partial class GameSystem
    {
        public static class Data
        {

            // GAME ENGINE

            public static Random RNG = new Random();
            public static int Seed;
            public static Game.EngineState GameState = Game.EngineState.Ended;
            public static bool GameStartedThisTick = false;
            public static bool DEBUG = false;
            public static float[] DEBUG_MARGIN = new float[4] { 0.0f, 0.75f, 0.0f, 0.0f }; //URDL
            public static float[] DEBUG_PADDING = new float[4] { 16f, 16f, 16f, 16f }; //URDL
            public static Color DEBUG_BACKGROUND_COLOR = Color.FromArgb(0x80, 0x00, 0x00, 0x00);
            public static Color4 DEBUG_TEXT_COLOR = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            public static string DEBUG_MESSAGE_KEYBOARD;
            public static string DEBUG_MESSAGE_MOUSE;
            public static string DEBUG_MESSAGE_GRIDHOVERCELL;

            // GAMEPLAY

            public static Game.PlayMode PlayMode = Game.PlayMode.Intermediate;
            public static Game.MineInfo[] MineField;
            public static int[] MineFieldNeighbours;
            public static int MineFieldWidth = 30;
            public static int MineFieldHeight = 16;
            public static int MineFieldMineCount = 99;
            public static int MineFieldSafeCellCount;
            public static int MineFieldCellsCleared;
            public static int MineFieldCellsFlagged;
            public static List<int> MineFieldOpenQueue = new List<int>();
            public static List<int> MineFieldChangedQueue = new List<int>();
            public static Stopwatch GameTimer;

            // SETTINGS

            public static int SafeMoves = 1;
            public static int SafeMovesUsed;
            public static int ModeBegWidth = 9;
            public static int ModeBegHeight = 9;
            public static int ModeBegMineCount = 10;
            public static int ModeIntWidth = 16;
            public static int ModeIntHeight = 16;
            public static int ModeIntMineCount = 40;
            public static int ModeAdvWidth = 30;
            public static int ModeAdvHeight = 16;
            public static int ModeAdvMineCount = 99;
            public static int ModeCustomWidth = 30;
            public static int ModeCustomHeight = 16;
            public static int ModeCustomMineCount = 149;
            //public static int ModeCustomMaxWidth, ModeCustomMaxHeight, ModeCustomMaxMines; //no maxes?


            // GAME INTERFACE

            public static bool InterfaceInitialized = false;

            public static RenderForm Window;
            public static string WindowTitle = "Galeforce Minesweeper";
            public static int WindowMinWidth = 128;
            public static int WindowMinHeight = 72;
            public static int WindowStartWidth = 1280;
            public static int WindowStartHeight = 720;
            public static FormStartPosition WindowStartPosition = FormStartPosition.CenterScreen;
            public static FormBorderStyle WindowBorderStyle = FormBorderStyle.Sizable;
            public static Input.WindowResizeState WindowResize = 0;
            public static int WindowResizeBorderSize = 8;
            public static Point WindowResizeStartLoc = new Point();
            public static Rectangle WindowResizeBounds;
            public static Cursor WindowCursorDefault = Cursors.Default;
            public static Cursor WindowCursorHoverCell = Cursors.Hand;
            public static Cursor WindowCursorHoverResize = Cursors.SizeAll;

            public static Stopwatch Timer;
            public static double FrameRate = 60;
            public static double FrameTime;
            public static double FrameProcessingTime;
            public static double ElapsedTimeAtFrameStart;
            public static uint FrameCount = 0;

            public static Device DX11Device;
            public static SwapChain DX11SwapChain;
            public static Viewport DX11Viewport;
            public static RenderTargetView DX11RenderTarget;
            public static Texture2D DX11BackBuffer;
            public static DeviceContext DX11Context;
            public static SpriteRenderer DX11SpriteRenderer;
            public static Texture2D ClosedCellRenderLayerTex;
            public static RenderTargetView ClosedCellRenderLayerRTV;
            public static ShaderResourceView ClosedCellRenderLayerSRV;
            public static SpriteRenderer ClosedCellRenderLayerSR;
            public static bool ClosedCellRenderLayerRendered = false;
            public static Texture2D CellRenderLayerTopTex;
            public static RenderTargetView CellRenderLayerTopRTV;
            public static ShaderResourceView CellRenderLayerTopSRV;
            public static SpriteRenderer CellRenderLayerTopSR;
            public static bool CellRenderLayerTopRendered = false;

            public static Dictionary<Interface.Font, TextBlockRenderer> Fonts = new Dictionary<Interface.Font, TextBlockRenderer>();
            public static Dictionary<Interface.Image, ShaderResourceView> Images = new Dictionary<Interface.Image, ShaderResourceView>();
            public static ShaderResourceView[] NumberedCellImages;
            public static Dictionary<Interface.Colour, Color> Colours = new Dictionary<Interface.Colour, Color>();
            public static Rectangle GridBounds;
            public static int GridWidth;
            public static int GridHeight;
            public static Rectangle GridOuterBounds;
            public static int GridOuterWidth;
            public static int GridOuterHeight;
            public static int GridCellSize;
            public static Vector2 GridCellSizeObj;
            public static int GridCellBaseSize = 16;
            public static double GridZoom = 1;
            public static int GridHoverCell = -1;

            public static Vector2 HUDTimePos = new Vector2(0.5f, 0.95f);
            public static Vector2 HUDClearedPos = new Vector2(0.25f, 0.95f);
            public static Vector2 HUDFlaggedPos = new Vector2(0.75f, 0.95f);

            public static string HUDEndMessage;
            public static Vector2 HUDEndMessagePos = new Vector2(0.5f, 0.5f);

            public static DirectInput DInput;
            public static Keyboard Keyboard;
            public static CooperativeLevel KeyboardCoopLevel = CooperativeLevel.Exclusive | CooperativeLevel.Foreground;
            public static KeyboardState KeyboardNow;
            public static List<KeyboardState> KeyboardBuffer;
            public static int KeyboardBufferSize = 4;
            public static Mouse Mouse;
            public static CooperativeLevel MouseCoopLevel = CooperativeLevel.Nonexclusive | CooperativeLevel.Foreground;
            public static MouseState MouseNow;
            public static List<MouseState> MouseBuffer;
            public static int MouseBufferSize = 4;
            public static int MouseX = 0;
            public static int MouseY = 0;
            public static Point MouseLocation = new Point();
            public static Input.ButtonState MouseClickL = Input.ButtonState.Up;
            public static Input.ButtonState MouseClickM = Input.ButtonState.Up;
            public static Input.ButtonState MouseClickR = Input.ButtonState.Up;
            public static Key INPUT_DEBUG_TOGGLE = Key.Grave;
            public static Input.ButtonState KeyDebug = Input.ButtonState.Up;
        }
    }
}
