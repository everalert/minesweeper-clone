using System;
using System.Diagnostics;
using System.Linq;

namespace Minesweeper
{
    static partial class GameSystem
    {
        /*
         *  NOTES
         *  - Glitch: weird loop if new minefield is generated while clear queue is still processing
         */

        public static class Game
        {
            public static void Initialize()
            {
                CreateMineField();
            }

            public static void Dispose()
            {

            }

            public static int SanitizeCellIndex(int i)
            {
                // should probably be an assertion instead of a function but too late lol
                return i % (Data.MineFieldWidth * Data.MineFieldHeight);
            }

            public static void CreateMineField()
            {
                int cell_count = Data.MineFieldWidth * Data.MineFieldHeight;
                Data.MineField = new MineInfo[cell_count];
                Data.MineFieldNeighbours = new int[cell_count];
                int assigned_mines = 0;
                Data.Seed = Environment.TickCount;
                Data.RNG = new Random(Data.Seed);
                while (assigned_mines < Data.MineFieldMineCount)
                {
                    int i = SanitizeCellIndex(Data.RNG.Next());
                    if (!Data.MineField[i].HasFlag(MineInfo.IsMine))
                    {
                        Data.MineField[i] |= MineInfo.IsMine;
                        assigned_mines++;
                    }
                }
                for (int i = 0; i < Data.MineField.Length; i++)
                {
                    SetMineNeighbourData(i);
                }
                StartGame();
            }

            public static void StartGame()
            {
                Data.MineFieldChangedQueue.Clear();
                Data.MineFieldSafeCellCount = Data.MineFieldWidth * Data.MineFieldHeight - Data.MineFieldMineCount;
                Data.SafeMovesUsed = 0;
                Data.MineFieldCellsCleared = 0;
                Data.MineFieldCellsFlagged = 0;
                Data.ClosedCellRenderLayerRendered = false;
                Data.CellRenderLayerTopRendered = false;
                Data.GameStartedThisTick = true;
                Data.GameState = EngineState.Playing;
                Data.GameTimer = new Stopwatch();
                Data.GameTimer.Start();
            }

            public static void EndGame(string message)
            {
                Data.GameTimer.Stop();
                Data.GameState = EngineState.Ended;
                Data.HUDEndMessage = message;
                Data.CellRenderLayerTopRendered = false;
            }

            public static void SetMineNeighbourData(int i)
            {
                int[] n = GetMineNeighbourIds(i);
                int count = 0;

                foreach (int idx in n)
                    if (idx >= 0 && Data.MineField[idx].HasFlag(MineInfo.IsMine))
                        count++;

                Data.MineField[i] |= (n[0] >= 0 && Data.MineField[n[0]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtMidRight : 0;
                Data.MineField[i] |= (n[1] >= 0 && Data.MineField[n[1]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtBotRight : 0;
                Data.MineField[i] |= (n[2] >= 0 && Data.MineField[n[2]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtBotCenter : 0;
                Data.MineField[i] |= (n[3] >= 0 && Data.MineField[n[3]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtBotLeft : 0;
                Data.MineField[i] |= (n[4] >= 0 && Data.MineField[n[4]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtMidLeft : 0;
                Data.MineField[i] |= (n[5] >= 0 && Data.MineField[n[5]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtTopLeft : 0;
                Data.MineField[i] |= (n[6] >= 0 && Data.MineField[n[6]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtTopCenter : 0;
                Data.MineField[i] |= (n[7] >= 0 && Data.MineField[n[7]].HasFlag(MineInfo.IsMine)) ? MineInfo.MineAtTopRight : 0;

                Data.MineFieldNeighbours[i] = count;
            }

            public static int[] GetMineNeighbourIds(int i)
            {
                i = SanitizeCellIndex(i);
                bool cond_v, cond_h;
                int[] output = new int[8];
                AdjacentCells edges = GetAdjacentCells(i);
                MineInfo mine = Data.MineField[i];

                cond_h = edges.HasFlag(AdjacentCells.Right);
                output[0] = cond_h ? i + 1 : -1;
                cond_v = edges.HasFlag(AdjacentCells.Bottom);
                output[1] = cond_v && cond_h ? i + Data.MineFieldWidth + 1 : -1;
                output[2] = cond_v ? i + Data.MineFieldWidth : -1;
                cond_h = edges.HasFlag(AdjacentCells.Left);
                output[3] = cond_v && cond_h ? i + Data.MineFieldWidth - 1 : -1;
                output[4] = cond_h ? i - 1 : -1;
                cond_v = edges.HasFlag(AdjacentCells.Top);
                output[5] = cond_v && cond_h ? i - Data.MineFieldWidth - 1 : -1;
                output[6] = cond_v ? i - Data.MineFieldWidth : -1;
                cond_h = edges.HasFlag(AdjacentCells.Right);
                output[7] = cond_v && cond_h ? i - Data.MineFieldWidth + 1 : -1;

                return output;
            }

            public static void SetMineLabel(int i)
            {
                i = SanitizeCellIndex(i);
                int o = 0;
                MineInfo compare = MineInfo.MineAtMidRight | MineInfo.MineAtBotRight | MineInfo.MineAtBotCenter | MineInfo.MineAtBotLeft
                    | MineInfo.MineAtMidLeft | MineInfo.MineAtTopLeft | MineInfo.MineAtTopCenter | MineInfo.MineAtTopRight;
                compare &= Data.MineField[i];
                foreach (MineInfo m in Enum.GetValues(typeof(MineInfo)))
                    o += compare.HasFlag(m) ? 1 : 0;
                Data.MineFieldNeighbours[i] = o;
            }

            public static AdjacentCells GetAdjacentCells(int i)
            {
                i = SanitizeCellIndex(i);
                AdjacentCells output = 0;
                output |= i % Data.MineFieldWidth < Data.MineFieldWidth-1 ? AdjacentCells.Right : 0;
                output |= i < (Data.MineFieldHeight - 1) * Data.MineFieldWidth ? AdjacentCells.Bottom : 0;
                output |= i % Data.MineFieldWidth > 0 ? AdjacentCells.Left : 0;
                output |= i >= Data.MineFieldWidth ? AdjacentCells.Top : 0;
                return output;
            }



            public static void OpenCell(int i)
            {
                if (i >= 0)
                {
                    i = SanitizeCellIndex(i);
                    if (!(Data.MineField[i].HasFlag(MineInfo.IsFlagged) && !Data.MineField[i].HasFlag(MineInfo.FlagIsQuestionMark)))
                    {
                        if (Data.MineField[i].HasFlag(MineInfo.IsMine))
                            EndGame("Game Lost");
                            //CreateMineField();
                            //TODO: implement safe moves
                        else if (!Data.MineField[i].HasFlag(MineInfo.IsCleared))
                        {
                            Data.MineField[i] |= MineInfo.IsCleared;
                            if (Data.MineFieldNeighbours[i] == 0)
                            {
                                foreach (int idx in GetMineNeighbourIds(i))
                                    Data.MineFieldOpenQueue.Add(idx);
                            }
                            Data.MineField[i] &= ~(MineInfo.IsFlagged | MineInfo.FlagIsQuestionMark);
                            Data.MineFieldCellsCleared++;
                            if (Data.MineFieldCellsCleared >= Data.MineFieldSafeCellCount)
                                EndGame("Game Won");
                            AddToCellChangedQueue(i);
                        }
                    }
                }
            }

            public static void OpenCell()
            {
                // opens the currently highlighted cell
                OpenCell(Data.GridHoverCell);
            }

            public static void ProcessOpenQueue()
            {
                byte i = Byte.MaxValue;
                while (Data.MineFieldOpenQueue.Count > 0 && i > 0)
                {
                    OpenCell(Data.MineFieldOpenQueue.First());
                    Data.MineFieldOpenQueue.RemoveAt(0);
                    i--;
                }
            }

            public static void AddToCellChangedQueue(int i)
            {
                if (!Data.MineFieldChangedQueue.Contains(i))
                    Data.MineFieldChangedQueue.Add(i);
            }

            public static void FlagCell()
            {
                if (Data.GridHoverCell >= 0)
                {
                    if (!Data.MineField[Data.GridHoverCell].HasFlag(MineInfo.IsCleared))
                    {
                        if (!Data.MineField[Data.GridHoverCell].HasFlag(MineInfo.IsFlagged))
                        {
                            Data.MineField[Data.GridHoverCell] = (Data.MineField[Data.GridHoverCell] | MineInfo.IsFlagged) & ~MineInfo.FlagIsQuestionMark;
                            Data.MineFieldCellsFlagged++;
                        }
                        else
                        {
                            if (!Data.MineField[Data.GridHoverCell].HasFlag(MineInfo.FlagIsQuestionMark))
                            {
                                Data.MineField[Data.GridHoverCell] |= MineInfo.FlagIsQuestionMark;
                            }
                            else
                            {
                                Data.MineField[Data.GridHoverCell] &= ~(MineInfo.IsFlagged | MineInfo.FlagIsQuestionMark);
                                Data.MineFieldCellsFlagged--;
                            }
                        }
                        Data.MineFieldChangedQueue.Add(Data.GridHoverCell);
                    }
                }
            }



            [Flags]
            public enum MineInfo
            {
                IsMine = 0x01,
                IsCleared = 0x02,
                IsFlagged = 0x04,
                FlagIsQuestionMark = 0x08,
                MineAtMidRight = 0x10,
                MineAtBotRight = 0x20,
                MineAtBotCenter = 0x40,
                MineAtBotLeft = 0x80,
                MineAtMidLeft = 0x100,
                MineAtTopLeft = 0x200,
                MineAtTopCenter = 0x400,
                MineAtTopRight = 0x800
            }

            [Flags]
            public enum AdjacentCells
            {
                Right = 0x01,
                Bottom = 0x02,
                Left = 0x04,
                Top = 0x08
            }

            public enum EngineState
            {
                Playing,
                Ended
            }

            public enum PlayMode
            {
                Beginner,
                Intermediate,
                Advanced,
                Custom
            }
        }
    }
}
