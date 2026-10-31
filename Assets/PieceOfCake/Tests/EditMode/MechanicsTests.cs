using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PieceOfCake.Core;

namespace PieceOfCake.Tests
{
    /// <summary>Individual gameplay rules on tiny hand-made maps.</summary>
    public class MechanicsTests
    {
        private static Piece PieceAt(StageSimulation sim, int x, int y) =>
            sim.Pieces.FirstOrDefault(p => x >= p.GridX && x < p.GridX + p.Width && y >= p.GridY && y < p.GridY + p.Height
                                           && p.GetCell(x - p.GridX, y - p.GridY) != null);

        private static List<GameEvent> Drain(StageSimulation sim)
        {
            var list = new List<GameEvent>();
            sim.DrainEvents(list);
            return list;
        }

        [Test]
        public void PushingTallThinCake_TopplesItIntoABridge()
        {
            var sim = TestLevels.Simulate(
                "...B..........",
                "...C..........",
                "@..S.......O.G",
                "##############");
            var pilot = new ScriptedPilot(sim);

            pilot.Hold(1, 45);
            pilot.Frames(30);

            var bridge = sim.Pieces.Single(p => !p.IsOrder);
            Assert.That((bridge.GridX, bridge.GridY, bridge.Width, bridge.Height), Is.EqualTo((3, 2, 3, 1)));
            Assert.That(bridge.GetCell(2, 0).Type, Is.EqualTo(CellType.Strawberry), "the top cell lands furthest away");
        }

        [Test]
        public void PushingLightCake_MovesIt()
        {
            var sim = TestLevels.Simulate(
                "@.CCC.......O.G",
                "###############");
            new ScriptedPilot(sim).Hold(1, 90);

            Assert.That(sim.Pieces.Single(p => !p.IsOrder).GridX, Is.GreaterThan(2));
        }

        [Test]
        public void PushingHeavyCake_IsRefused()
        {
            var sim = TestLevels.Simulate(
                "@.CCCCC.....O.G",
                "###############");
            new ScriptedPilot(sim).Hold(1, 90);

            Assert.That(sim.Pieces.Single(p => !p.IsOrder).GridX, Is.EqualTo(2));
            Assert.That(sim.Player.Message, Is.EqualTo(PlayerMessage.TooHeavy));
        }

        [Test]
        public void EatingChocolate_GivesStrengthToPushHeavyCake()
        {
            var sim = TestLevels.Simulate(
                "@K.CCCCC......O.G",
                "#################");
            var pilot = new ScriptedPilot(sim);

            pilot.Eat();
            Assert.That(sim.Player.IsStrong, Is.True);
            Assert.That(sim.Stats.Eaten, Is.EqualTo(1));

            pilot.Hold(1, 90);
            Assert.That(sim.Pieces.Single(p => !p.IsOrder).GridX, Is.GreaterThan(3));
        }

        [Test]
        public void EatingStrawberry_GivesJumpPower()
        {
            var sim = TestLevels.Simulate(
                "@B.......O.G",
                "############");
            new ScriptedPilot(sim).Eat();

            Assert.That(sim.Player.HasPower, Is.True);
            Assert.That(sim.Player.Message, Is.EqualTo(PlayerMessage.StrawberryPower));
            Assert.That(sim.Pieces.Count(p => !p.IsOrder), Is.Zero);
        }

        [Test]
        public void TheOrder_CannotBeEaten()
        {
            var sim = TestLevels.Simulate(
                "@O.......G",
                "##########");
            new ScriptedPilot(sim).Frames(1, eat: true);

            Assert.That(sim.Stats.Eaten, Is.Zero);
            Assert.That(sim.Player.Message, Is.EqualTo(PlayerMessage.OrderIsNotFood));
        }

        [Test]
        public void CuttingCake_SplitsIt()
        {
            var sim = TestLevels.Simulate(
                "@CC......O.G",
                "############");
            new ScriptedPilot(sim).Cut();

            var cakes = sim.Pieces.Where(p => !p.IsOrder).ToList();
            Assert.That(cakes.Count, Is.EqualTo(2));
            Assert.That(sim.Stats.Loss, Is.Zero, "cutting wastes nothing");
            Assert.That(Drain(sim).Any(e => e.Type == GameEventType.CakeCut), Is.True);
        }

        [Test]
        public void Chocolate_CannotBeCut()
        {
            var sim = TestLevels.Simulate(
                "@KK......O.G",
                "############");
            new ScriptedPilot(sim).Cut();

            Assert.That(sim.Pieces.Count(p => !p.IsOrder), Is.EqualTo(1));
            Assert.That(sim.Player.Message, Is.EqualTo(PlayerMessage.ChocolateTooHard));
        }

        [Test]
        public void Ice_MeltsAwayUnderThePlayer_WithoutCountingAsLoss()
        {
            var sim = TestLevels.Simulate(
                "@.........",
                "IIII....OG",
                "##########");
            new ScriptedPilot(sim).Frames(60);

            Assert.That(PieceAt(sim, 0, 1), Is.Null);
            Assert.That(PieceAt(sim, 3, 1), Is.Not.Null, "untouched ice stays");
            Assert.That(sim.Stats.Loss, Is.Zero);
        }

        [Test]
        public void Jelly_BouncesThePlayer()
        {
            var sim = TestLevels.Simulate(
                "@.........",
                "J.......OG",
                "##########");
            double startY = sim.Player.Y;
            new ScriptedPilot(sim).Frames(10);

            Assert.That(Drain(sim).Any(e => e.Type == GameEventType.JellyBounce), Is.True);
            Assert.That(sim.Player.Y, Is.LessThan(startY - 40));
        }

        [Test]
        public void LandingOnAnAnt_SquashesIt()
        {
            var sim = TestLevels.Simulate(
                "...@......",
                "..........",
                "...a....OG",
                "##########");
            new ScriptedPilot(sim).Frames(40);

            Assert.That(sim.Ants[0].IsAlive, Is.False);
            Assert.That(sim.Misses, Is.Zero);
        }

        [Test]
        public void WalkingIntoAnAnt_IsAMiss_AndRespawnsAtTheCheckpoint()
        {
            var sim = TestLevels.Simulate(
                "@....a....OG",
                "############");
            var pilot = new ScriptedPilot(sim);
            pilot.WaitUntil(() => sim.Misses > 0, 300);

            Assert.That(sim.Misses, Is.EqualTo(1));
            Assert.That(sim.Player.DeathCause, Is.EqualTo(DeathCause.Ant));

            pilot.Frames(80);
            Assert.That(sim.Player.IsDead, Is.False);
            Assert.That(sim.Player.InvulnerableTimer, Is.GreaterThan(0));
            Assert.That(sim.Ants[0].IsAlive, Is.True, "ants respawn with the stage");
        }

        [Test]
        public void Spikes_AreLethal_AndCheckpointsChangeTheRespawnPoint()
        {
            var sim = TestLevels.Simulate(
                "@..F.......OG",
                "#####^#######");
            var pilot = new ScriptedPilot(sim);
            pilot.Go(3);
            Assert.That(sim.Checkpoints[0].IsActive, Is.True);

            for (int k = 0; k < 120 && sim.Misses == 0; k++) pilot.Hold(1, 1);
            Assert.That(sim.Player.DeathCause, Is.EqualTo(DeathCause.Spike));

            pilot.Frames(80);
            Assert.That(pilot.Column, Is.EqualTo(3));
        }

        [Test]
        public void LosingTheOrder_RestoresTheCheckpoint()
        {
            var sim = TestLevels.Simulate(
                "@O.......G",
                "#.########");
            var pilot = new ScriptedPilot(sim);

            pilot.WaitUntil(() => sim.IsOrderLost, 120);
            Assert.That(sim.IsOrderLost, Is.True);
            Assert.That(sim.Pieces.Any(p => p.IsOrder), Is.False);

            pilot.WaitUntil(() => !sim.IsOrderLost, 200);
            Assert.That(sim.Pieces.Any(p => p.IsOrder), Is.True);
            Assert.That(sim.Misses, Is.Zero, "losing the order is not counted as a miss");
        }

        [Test]
        public void Retry_RestoresEatenCake()
        {
            var sim = TestLevels.Simulate(
                "@S.......O.G",
                "############");
            new ScriptedPilot(sim).Eat();
            Assert.That(sim.Stats.Eaten, Is.EqualTo(1));

            Assert.That(sim.RetryFromCheckpoint(), Is.True);
            Assert.That(sim.Stats.Eaten, Is.Zero);
            Assert.That(sim.Pieces.Count(p => !p.IsOrder), Is.EqualTo(1));
        }

        [Test]
        public void DeliveringTheOrder_CompletesTheStage()
        {
            var sim = TestLevels.Simulate(
                "@.O...G.",
                "########");
            var pilot = new ScriptedPilot(sim);
            pilot.Go(6);
            Assert.That(sim.Phase, Is.Not.EqualTo(GamePhase.Playing));

            pilot.Frames(120);
            Assert.That(sim.Phase, Is.EqualTo(GamePhase.Completed));
            Assert.That(sim.Result.Stars, Is.EqualTo(3));
            Assert.That(sim.Result.Loss, Is.Zero);
        }

        [Test]
        public void Step_AfterCompletion_DoesNothing()
        {
            var sim = TestLevels.Simulate(
                "@O..G.",
                "######");
            var pilot = new ScriptedPilot(sim);
            pilot.Go(4);
            pilot.Frames(120);
            double time = sim.StageTime;

            pilot.Frames(60, right: true);
            Assert.That(sim.StageTime, Is.EqualTo(time));
        }

        [TestCase(0, 0, 3)]
        [TestCase(3, 0, 2)]
        [TestCase(4, 0, 1)]
        [TestCase(6, 6, 3)]
        [TestCase(9, 6, 2)]
        [TestCase(10, 6, 1)]
        public void Stars_DependOnWasteAgainstPar(int loss, int par, int stars)
        {
            Assert.That(Scoring.StarsFor(loss, par), Is.EqualTo(stars));
        }
    }
}
