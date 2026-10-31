using NUnit.Framework;
using PieceOfCake.Core;

namespace PieceOfCake.Tests
{
    /// <summary>
    /// Plays every shipped stage with its reference solution. Proves each stage is beatable with three
    /// stars and that the Unity port behaves frame-for-frame like the HTML prototype.
    /// </summary>
    public class StageSolutionTests
    {
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ReferenceSolution_DeliversWithThreeStars(int stage)
        {
            var layout = TestLevels.Stage(stage);
            var pilot = new ScriptedPilot(new StageSimulation(layout));

            StageSolutions.All[stage](pilot);

            Assert.That(pilot.Sim.Misses, Is.Zero, "the solution should not miss");
            Assert.That(pilot.DeliveredFrame, Is.GreaterThan(0), "the order should be delivered");
            Assert.That(pilot.Sim.Stats.Loss, Is.LessThanOrEqualTo(layout.Definition.Par));

            pilot.Frames(120);
            Assert.That(pilot.Sim.Phase, Is.EqualTo(GamePhase.Completed));
            Assert.That(pilot.Sim.Result.Stars, Is.EqualTo(3));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void ReferenceSolution_MatchesPrototypeFrameExactly(int stage)
        {
            var pilot = new ScriptedPilot(new StageSimulation(TestLevels.Stage(stage)));

            StageSolutions.All[stage](pilot);

            Assert.That(pilot.DeliveredFrame, Is.EqualTo(StageSolutions.PrototypeDeliveryFrames[stage]));
        }

        [Test]
        public void Simulation_IsDeterministic()
        {
            var a = new ScriptedPilot(new StageSimulation(TestLevels.Stage(4)));
            var b = new ScriptedPilot(new StageSimulation(TestLevels.Stage(4)));

            StageSolutions.All[4](a);
            StageSolutions.All[4](b);

            Assert.That(b.P.X, Is.EqualTo(a.P.X));
            Assert.That(b.P.Y, Is.EqualTo(a.P.Y));
            Assert.That(b.Frame, Is.EqualTo(a.Frame));
            Assert.That(b.Sim.Pieces.Count, Is.EqualTo(a.Sim.Pieces.Count));
        }
    }
}
