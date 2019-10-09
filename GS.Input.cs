using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SlimDX;
using SlimDX.DirectInput;
using SlimDX.Windows;

namespace Minesweeper
{
    static partial class GameSystem
    {
        public static class Input
        {
            public static void Initialize()
            {
                Data.DInput = new DirectInput();
                try
                {
                    Data.Mouse = new Mouse(Data.DInput);
                    Data.Mouse.SetCooperativeLevel(Data.Window.Handle, Data.MouseCoopLevel);
                    Data.Mouse.Properties.BufferSize = Data.MouseBufferSize;
                    //Data.Mouse.Properties.AxisMode = DeviceAxisMode.Absolute;
                    Data.Mouse.Acquire();
                    Data.Keyboard = new Keyboard(Data.DInput);
                    Data.Keyboard.SetCooperativeLevel(Data.Window.Handle, Data.MouseCoopLevel);
                    Data.Keyboard.Properties.BufferSize = Data.MouseBufferSize;
                    Data.Keyboard.Acquire();
                }
                catch (DirectInputException e) { Console.WriteLine(e.Message); }
                Data.MouseBuffer = new List<MouseState>();
                Data.KeyboardBuffer = new List<KeyboardState>();
            }

            public static void Dispose()
            {
                if (Data.Mouse != null)
                {
                    Data.Mouse.Unacquire();
                    Data.Mouse.Dispose();
                    Data.Mouse = null;
                    Data.Keyboard.Unacquire();
                    Data.Keyboard.Dispose();
                    Data.Keyboard = null;
                }
                Data.DInput.Dispose();
            }


            // INPUT UPDATING

            public static void UpdateMouse()
            {
                if (Data.Mouse.Acquire().IsFailure || Data.Mouse.Poll().IsFailure)
                    return;

                Data.MouseNow = Data.Mouse.GetCurrentState();
                if (Result.Last.IsFailure)
                    return;

                //notes - buffer only updates on change, current is always current... lol
                //so, manually buffer with current
                Data.MouseBuffer.Add(Data.MouseNow);
                while (Data.MouseBuffer.Count > Data.MouseBufferSize)
                    Data.MouseBuffer.RemoveAt(0);

                Data.MouseLocation = Data.Window.PointToClient(RenderForm.MousePosition);
                Data.MouseX = Data.MouseLocation.X;
                Data.MouseY = Data.MouseLocation.Y;

                Data.MouseClickL = Data.MouseNow.IsPressed(0) ? ButtonState.Down : ButtonState.Up;
                Data.MouseClickR = Data.MouseNow.IsPressed(1) ? ButtonState.Down : ButtonState.Up;
                Data.MouseClickM = Data.MouseNow.IsPressed(2) ? ButtonState.Down : ButtonState.Up;

                if (Data.MouseBuffer.Count >= 2)
                {
                    for (int i = Data.MouseBuffer.Count - 2; i >= 0; i--)
                    {
                        ProcessMouseButton(Data.MouseBuffer[i].IsPressed(0), i, ref Data.MouseClickL);
                        ProcessMouseButton(Data.MouseBuffer[i].IsPressed(1), i, ref Data.MouseClickR);
                        ProcessMouseButton(Data.MouseBuffer[i].IsPressed(2), i, ref Data.MouseClickM);
                    }
                }

                ProcessMouseLocation();
                
                Data.DEBUG_MESSAGE_MOUSE = string.Format("L:{0}\n\rR:{1}\n\rM:{2}", Data.MouseClickL, Data.MouseClickR, Data.MouseClickM);
            }

            public static void ProcessMouseButton(bool pressed, int i, ref ButtonState state)
            {
                switch (state)
                {
                    case ButtonState.Down:
                        state = i < Data.MouseBuffer.Count - 2 && !pressed ? ButtonState.Pressed : state;
                        break;
                    case ButtonState.Up:
                        state = i < Data.MouseBuffer.Count - 2 && pressed ? ButtonState.Released : state;
                        break;
                    case ButtonState.Pressed:
                        state = !pressed ? ButtonState.Up : state;
                        break;
                    case ButtonState.Released:
                        state = pressed ? ButtonState.Down : state;
                        break;
                    default:
                        state = ButtonState.Up;
                        break;
                }
            }

            public static void ProcessMouseLocation()
            {
                if (Data.Window.ClientRectangle.Contains(Data.MouseLocation))
                {
                    WindowResizeState wrs = 0;
                    if (!Data.WindowResizeBounds.Contains(Data.MouseLocation))
                    {
                        wrs |= (Data.MouseLocation.X >= Data.Window.ClientRectangle.Width - Data.WindowResizeBorderSize) ? WindowResizeState.HoverR : 0;
                        wrs |= (Data.MouseLocation.X < Data.WindowResizeBorderSize) ? WindowResizeState.HoverL : 0;
                        wrs |= (Data.MouseLocation.Y >= Data.Window.ClientRectangle.Height - Data.WindowResizeBorderSize) ? WindowResizeState.HoverB : 0;
                        wrs |= (Data.MouseLocation.Y < Data.WindowResizeBorderSize) ? WindowResizeState.HoverT : 0;
                    }
                    Data.WindowResize &= ~WindowResizeState.Hover;
                    Data.WindowResize |= wrs;
                }

                if (!Data.GridBounds.Contains(Data.MouseLocation))
                    Data.GridHoverCell = -1;
                else
                    Data.GridHoverCell = (int)Math.Floor(((double)Data.MouseX - Data.GridBounds.X) / Data.GridCellSize) + (int)Math.Floor(((double)Data.MouseY - Data.GridBounds.Y) / Data.GridCellSize) * Data.MineFieldWidth;

                Data.DEBUG_MESSAGE_GRIDHOVERCELL = Data.GridHoverCell >= 0 ? string.Format("Cell {0}\n\r{1}\n\r{2} adjacent", Data.GridHoverCell.ToString("000"), Data.MineField[Data.GridHoverCell].HasFlag(Game.MineInfo.IsMine) ? "MINE" : "SAFE", Data.MineFieldNeighbours[Data.GridHoverCell]) : "NO CELL SELECTED";
            }

            public static void UpdateKeyboard()
            {
                if (Data.Keyboard.Acquire().IsFailure || Data.Keyboard.Poll().IsFailure)
                    return;

                Data.KeyboardNow = Data.Keyboard.GetCurrentState();
                if (Result.Last.IsFailure)
                    return;

                //notes - buffer only updates on change, current is always current... lol
                //so, manually buffer with current
                Data.KeyboardBuffer.Add(Data.KeyboardNow);
                while (Data.KeyboardBuffer.Count > Data.KeyboardBufferSize)
                    Data.KeyboardBuffer.RemoveAt(0);

                Data.KeyDebug = Data.KeyboardNow.IsPressed(Data.INPUT_DEBUG_TOGGLE) ? ButtonState.Down : ButtonState.Up;

                if (Data.KeyboardBuffer.Count >= 2)
                {
                    for (int i = Data.KeyboardBuffer.Count - 2; i >= 0; i--)
                    {
                        ProcessKey(Data.KeyboardBuffer[i].IsPressed(Data.INPUT_DEBUG_TOGGLE), i, ref Data.KeyDebug);
                    }
                }

                Data.DEBUG_MESSAGE_KEYBOARD = string.Format("DEBUG:{0}", Data.KeyDebug);
            }

            public static void ProcessKey(bool pressed, int i, ref ButtonState state)
            {
                switch (state)
                {
                    case ButtonState.Down:
                        state = i < Data.KeyboardBuffer.Count - 2 && !pressed ? ButtonState.Pressed : state;
                        break;
                    case ButtonState.Up:
                        state = i < Data.KeyboardBuffer.Count - 2 && pressed ? ButtonState.Released : state;
                        break;
                    case ButtonState.Pressed:
                        state = !pressed ? ButtonState.Up : state;
                        break;
                    case ButtonState.Released:
                        state = pressed ? ButtonState.Down : state;
                        break;
                    default:
                        state = ButtonState.Up;
                        break;
                }
            }


            // PROCESSING INPUTS

            public static bool CheckDebugInput()
            {
                return Data.KeyDebug == ButtonState.Released;
            }

            public static bool CheckOpenInput()
            {
                return Data.MouseClickL == ButtonState.Released;
            }

            public static bool CheckFlagInput()
            {
                return Data.MouseClickR == ButtonState.Released;
            }

            public static bool CheckStartGameInput()
            {
                return Data.MouseClickL == ButtonState.Released;
            }

            public static bool CheckResizeInput()
            {
                Data.WindowResize &= (Data.WindowResize & WindowResizeState.Resize) > 0 && Data.MouseClickL == ButtonState.Released ? ~WindowResizeState.Resize : Data.WindowResize;
                if ((Data.WindowResize & WindowResizeState.Hover) > 0 && Data.MouseClickL == ButtonState.Pressed)
                {
                    Data.WindowResizeStartLoc = Data.MouseLocation;
                    WindowResizeState wrs = 0;
                    wrs |= Data.WindowResize.HasFlag(WindowResizeState.HoverT) ? WindowResizeState.ResizeT : 0;
                    wrs |= Data.WindowResize.HasFlag(WindowResizeState.HoverB) ? WindowResizeState.ResizeB : 0;
                    wrs |= Data.WindowResize.HasFlag(WindowResizeState.HoverL) ? WindowResizeState.ResizeL : 0;
                    wrs |= Data.WindowResize.HasFlag(WindowResizeState.HoverR) ? WindowResizeState.ResizeR : 0;
                    Data.WindowResize &= ~WindowResizeState.Resize;
                    Data.WindowResize |= wrs;
                }
                return (Data.WindowResize & WindowResizeState.Resize) > 0;
            }


            // DEFINITIONS

            public enum ButtonState
            {
                Up,
                Released,
                Down,
                Pressed
            }

            [Flags]
            public enum WindowResizeState : byte
            {
                Hover = 0xF,
                HoverT = 0x1,
                HoverB = 0x2,
                HoverL = 0x4,
                HoverR = 0x8,
                Resize = 0xF0,
                ResizeT = 0x10,
                ResizeB = 0x20,
                ResizeL = 0x40,
                ResizeR = 0x80
            }
        }
    }
}
