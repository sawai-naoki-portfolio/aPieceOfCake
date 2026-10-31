using System;
using System.Linq;
using PieceOfCake.Core;

namespace PieceOfCake.Tests
{
    /// <summary>
    /// Reference solutions for the shipped stages, recorded against the HTML prototype. Each one
    /// delivers the order with no misses and waste within par (three stars).
    /// </summary>
    internal static class StageSolutions
    {
        /// <summary>Frame on which the prototype delivered the order, per stage (parity check).</summary>
        public static readonly int[] PrototypeDeliveryFrames = { 917, 1170, 1383, 1830, 1928 };

        public static readonly Action<ScriptedPilot>[] All = { Stage1, Stage2, Stage3, Stage4, Stage5 };

        private static void Stage1(ScriptedPilot d)
        {
            d.Go(18); d.Hold(1, 20); d.Frames(30);   // push the tall cake over: bridge
            d.Go(30);                                // pick up the order
            d.Go(42); d.Cut();                       // on the big cake: cut, the front column topples
            d.Go(54);
        }

        private static void Stage2(ScriptedPilot d)
        {
            d.Go(7); d.Hold(1, 20); d.Frames(30);    // topple the little tower
            d.Go(12); d.Face(1); d.Eat();            // strawberry: jump power
            d.Go(21); d.Face(1); d.Eat();            // second strawberry refreshes the power
            d.RunJump(23, 1, 50);                    // over the wide gap
            d.Go(31);                                // order
            d.Go(38); d.Hold(1, 150);                // push the 2x2 cake to the wall as a step
            d.Go(43); d.Go(47);
            d.Go(54); d.Eat(); d.Eat(); d.Eat();     // eat down the shaft
            d.Go(61);
        }

        private static void Stage3(ScriptedPilot d)
        {
            d.Go(6); d.Hold(1, 120);                 // push the jelly next to the tall block
            d.Jump(1, 70);                           // bounce up
            d.Go(17); d.Go(24);
            d.Go(26); d.Eat();                       // eat chocolate from the top: strength
            d.Go(25); d.Hold(1, 220);                // push the cake into the fork pit
            d.Go(39);
            d.Go(50); d.Eat();                       // remove the chocolate on the cut line
            d.Face(1); d.Cut();                      // the right column topples into a bridge
            d.Go(66);
        }

        private static void Stage4(ScriptedPilot d)
        {
            var sim = d.Sim;
            d.Go(7); d.Frames(30);
            d.WaitUntil(() => sim.Ants[1].VelocityX < 0 && sim.Ants[1].X <= 372, 900);
            d.Hold(1, 30);                           // step down onto the first ant
            d.Go(19); d.Go(30);                      // across the melting ice
            d.Go(35);
            d.PushUntil(() => sim.Pieces.Any(p => p.Width == 1 && p.GetCell(0, 0)?.Type == CellType.Sponge
                                                  && p.GridX >= 51 && p.State == PieceState.Resting && p.GridY == 12), 600);
            d.Go(53);
            d.Leap(53); d.Leap(56); d.Leap(59); d.Leap(62);   // up the melting ice steps
            d.WaitUntil(() => sim.Ants[0].VelocityX < 0 && sim.Ants[0].X - (d.P.X + 26) < 34, 600);
            d.Frames(1, jump: true); d.Frames(50);
            d.Go(66); d.Go(71);
        }

        private static void Stage5(ScriptedPilot d)
        {
            var sim = d.Sim;
            d.Go(6); d.Hold(1, 150);                 // jelly to the wall
            d.Jump(1, 70);
            d.WaitUntil(() => sim.Ants[1].VelocityX < 0 && sim.Ants[1].X - (d.P.X + 26) < 22, 600);
            d.Frames(1, jump: true); d.Frames(45);   // stomp the ant
            d.Go(26); d.Face(1); d.Eat();            // strawberry
            d.Go(31); d.Frames(5);
            d.RunJump(31, 1, 40);                    // powered jump onto the pillar
            d.Go(34); d.Go(44); d.Go(45);            // ice bridge, order
            d.Go(48); d.Frames(20); d.Eat();         // chocolate: strength
            d.Go(47); d.Hold(1, 150);                // push the cake to the wall
            d.Go(53); d.Frames(10);
            d.Leap(54); d.Leap(56); d.Leap(58); d.Leap(61); d.Leap(64);
            d.Go(69); d.Frames(10); d.Eat();         // clear the chocolate from the cut line
            d.Face(1); d.Cut();                      // last bridge
            d.Go(76);
            d.WaitUntil(() => sim.Ants[0].VelocityX < 0 && sim.Ants[0].X <= 3110, 900);
            d.Hold(1, 25);                           // drop onto the ant
            d.Go(79); d.Frames(10);
            d.Go(84);
        }
    }
}
