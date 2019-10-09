using SlimDX.DXGI;
using SlimDX.Windows;
using System;
using System.Windows.Forms;

namespace Minesweeper
{
    static partial class GameSystem
    {
        //public static void MainLoop()
        //{
        //}

        public static void MainLoop()
        {
            while (MessagePump.IsApplicationIdle)
            {
                Interface.Tick();

                Data.DX11Context.ClearRenderTargetView(Data.DX11RenderTarget, Data.Colours[Interface.Colour.Default]);


                #region reference code from swe1r overlay <-
                //if (game_state.State(racer) == GameState.Id.InRace)
                //{
                //    //move this to be contained in InRaceData?
                //    if (game_state.DeepState(racer) == GameState.Id.RaceStarting)
                //    {
                //        race_deaths = 0;
                //        race_pod_dist_total_3d = 0;
                //        race_pod_dist_total_2d = 0;
                //    }

                //    data_in_race.Update(racer);

                //    if (data_in_race.JustDied(racer))
                //        race_deaths++;

                //    if (!data_in_race.IsFinished(racer) && data_in_race.TimeTotal(racer) > 0)
                //    {
                //        race_pod_dist_total_3d += data_in_race.FrameDistance3D(racer);
                //        race_pod_dist_total_2d += data_in_race.FrameDistance3D(racer);
                //    }

                //    OverlayRenderer.InRace.RenderTimes(this, data_in_race.AllTimes(racer));
                //    OverlayRenderer.InRace.RenderMovementData3D(this, data_in_race.FrameDistance3D(racer), data_in_race.Speed3D(racer), race_pod_dist_total_3d, data_in_race.TimeTotal(racer));
                //    //OverlayRenderer.InRace.RenderMovementData2D(this, data_in_race.FrameDistance2D(racer), data_in_race.Speed2D(racer), race_pod_dist_total_2d, data_in_race.TimeTotal(racer));

                //    if (game_state.DeepState(racer) != GameState.Id.RaceEnded)
                //    {
                //        OverlayRenderer.InRace.RenderHeatTimers(this, data_in_race.Heat(racer), data_in_race.HeatRate(racer), data_in_race.CoolRate(racer), data_in_race.IsBoosting(racer));
                //        OverlayRenderer.InRace.RenderEngineBar(this, data_in_race.Heat(racer), race_deaths);
                //    }
                //}

                //if (game_state.DeepState(racer) == GameState.Id.VehicleSelect)
                //{
                //    data_vehicle_select.Update(racer);
                //    OverlayRenderer.VehicleSelect.RenderMainStats(this, data_vehicle_select.AllStats(racer));
                //    OverlayRenderer.VehicleSelect.RenderHiddenStats(this, data_vehicle_select.AllStats(racer));
                //}

                ////debug output
                //if (opt_debug)
                //    Render.DrawText(WINDOW_SCALE, ol_coords["txt_debug"], txt_debug, ol_font["default"], ol_color["txt_debug"], TextAlignment.Left | TextAlignment.Top);
                #endregion

                Input.UpdateMouse();
                Input.UpdateKeyboard();

                if (Input.CheckDebugInput())
                    Data.DEBUG = !Data.DEBUG;

                if (Input.CheckResizeInput())
                    Interface.ResizeWindow2();

                switch (Data.GameState)
                {
                    case Game.EngineState.Ended:
                        {
                            if (Input.CheckStartGameInput())
                                Game.CreateMineField();

                            Interface.DrawMinefield();
                            Interface.DrawHUD();
                            Interface.DrawEndScreen();
                        }
                        break;
                    case Game.EngineState.Playing:
                    default:
                        {
                            if (!Data.GameStartedThisTick)
                            {
                                if (Input.CheckOpenInput())
                                    Game.OpenCell();

                                if (Input.CheckFlagInput())
                                    Game.FlagCell();

                                Game.ProcessOpenQueue();
                            }
                            else
                                Data.GameStartedThisTick = false;

                            Interface.DrawMinefield();
                            Interface.DrawHUD();
                        }
                        break;
                }

                if (Data.DEBUG)
                    Interface.DrawDebugPanel();

                //finalize
                //finalize_rendered_frame:
                Data.DX11SpriteRenderer.Flush();
                Data.DX11SwapChain.Present(0, PresentFlags.None);

                //Data.Window.Text = Data.DEBUG ? string.Format("{0} | {1}ms | {4} | {5} | {3}", Data.WindowTitle, Data.FrameProcessingTime.ToString("00.000"), Data.FrameTime.ToString("00.000"), Data.DEBUG_MESSAGE_MOUSE, Data.DEBUG_MESSAGE_GRIDHOVERCELL, Data.DEBUG_MESSAGE_KEYBOARD) : Data.WindowTitle;
            }
        }

        public static void Initialize()
        {
            Game.Initialize();
            Interface.Initialize();
            Input.Initialize();

            Application.Idle += (o, e) => MainLoop();
            Data.Window.FormClosed += (o, e) => Exit();
        }

        public static void Exit()
        {
            Console.WriteLine("Minesweeper closed.");
            Input.Dispose();
            Interface.Dispose();
            Game.Dispose();
        }
    }
}
