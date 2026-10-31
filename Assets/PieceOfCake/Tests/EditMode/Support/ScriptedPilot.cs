using System;
using PieceOfCake.Core;

namespace PieceOfCake.Tests
{
    /// <summary>
    /// Drives a <see cref="StageSimulation"/> with closed-loop helpers. Each helper mirrors the HTML
    /// prototype's debug API exactly, so solutions recorded against the prototype replay identically.
    /// </summary>
    internal sealed class ScriptedPilot
    {
        private const int T = SimConstants.CellSize;
        private const double PW = SimConstants.PlayerWidth;
        private const double PH = SimConstants.PlayerHeight;

        public ScriptedPilot(StageSimulation simulation)
        {
            Sim = simulation;
        }

        public StageSimulation Sim { get; }
        public int Frame { get; private set; }
        /// <summary>Frame count at which the order was delivered, or -1.</summary>
        public int DeliveredFrame { get; private set; } = -1;

        public Player P => Sim.Player;
        public int Column => MathUtil.FloorToInt((P.X + PW / 2) / T);
        public int Row => MathUtil.FloorToInt((P.Y + PH - 1) / T);
        private bool CanAct => !P.IsDead && Sim.Phase == GamePhase.Playing;

        /// <summary>Runs <paramref name="n"/> steps; button presses happen on the first step only.</summary>
        public void Frames(int n, bool left = false, bool right = false, bool jump = false, bool cut = false, bool eat = false)
        {
            for (int k = 0; k < n; k++)
            {
                Sim.Step(new InputFrame(left, right, k == 0 && jump, k == 0 && cut, k == 0 && eat));
                Frame++;
                if (DeliveredFrame < 0 && Sim.Phase != GamePhase.Playing) DeliveredFrame = Frame;
            }
        }

        public void Hold(int direction, int n) => Frames(n, direction < 0, direction > 0);

        public void Face(int direction)
        {
            Hold(direction, 1);
            Frames(10);
        }

        public void Jump(int direction, int airFrames = 40)
        {
            Frames(1, direction < 0, direction > 0, jump: true);
            Hold(direction, airFrames);
            Frames(20);
        }

        public void Cut()
        {
            Frames(1, cut: true);
            Frames(30);
        }

        public void Eat()
        {
            Frames(1, eat: true);
            Frames(30);
        }

        /// <summary>Walks to the centre of a column, hopping when blocked.</summary>
        public void Go(int column, int maxFrames = 1200)
        {
            double target = column * T + T / 2.0;
            int stuck = 0;
            double lastX = P.X;
            for (int k = 0; k < maxFrames; k++)
            {
                double cx = P.X + PW / 2;
                if (Math.Abs(cx - target) < 3 || !CanAct) break;
                int d = cx < target ? 1 : -1;
                if (P.IsGrounded && Math.Abs(P.X - lastX) < 0.3) stuck++;
                else stuck = 0;
                lastX = P.X;
                if (stuck > 4)
                {
                    Frames(1, d < 0, d > 0, jump: true);
                    stuck = 0;
                    continue;
                }

                Hold(d, 1);
            }

            Frames(12);
        }

        /// <summary>Runs to the edge of <paramref name="edgeColumn"/> and jumps, holding the direction.</summary>
        public void RunJump(int edgeColumn, int direction = 1, int airFrames = 50)
        {
            double edge = edgeColumn * T + (direction > 0 ? T : 0);
            for (int k = 0; k < 300; k++)
            {
                double front = direction > 0 ? P.X + PW : P.X;
                if ((direction > 0 && front >= edge - 2) || (direction < 0 && front <= edge + 2)) break;
                Hold(direction, 1);
            }

            Frames(1, direction < 0, direction > 0, jump: true);
            Hold(direction, airFrames);
            Frames(15);
        }

        /// <summary>Runs right to the end of a platform ending at <paramref name="edgeColumn"/> and leaps until landing.</summary>
        public void Leap(int edgeColumn)
        {
            for (int k = 0; k < 60; k++)
            {
                if (P.X + PW >= (edgeColumn + 1) * T - 4) break;
                Hold(1, 1);
            }

            Frames(1, right: true, jump: true);
            for (int k = 0; k < 90; k++)
            {
                if (k > 3 && P.IsGrounded) break;
                Hold(1, 1);
            }
        }

        /// <summary>Idles until <paramref name="condition"/> holds or <paramref name="maxFrames"/> pass.</summary>
        public void WaitUntil(Func<bool> condition, int maxFrames)
        {
            for (int k = 0; k < maxFrames; k++)
            {
                if (condition()) break;
                Frames(1);
            }
        }

        /// <summary>Holds right until <paramref name="condition"/> holds or <paramref name="maxFrames"/> pass.</summary>
        public void PushUntil(Func<bool> condition, int maxFrames)
        {
            for (int k = 0; k < maxFrames; k++)
            {
                if (condition()) break;
                Hold(1, 1);
            }
        }
    }
}
