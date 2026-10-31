using System.Linq;
using NUnit.Framework;
using PieceOfCake.Core;
using PieceOfCake.Game.Levels;

namespace PieceOfCake.Tests
{
    public class LevelDataTests
    {
        [Test]
        public void ShippedLevelFile_HasFiveValidStages()
        {
            Assert.That(TestLevels.Shipped.Count, Is.EqualTo(5));
            for (int i = 0; i < TestLevels.Shipped.Count; i++)
            {
                var layout = TestLevels.Stage(i);
                Assert.That(layout.Pieces.Count(p => p.IsOrder), Is.EqualTo(1), $"stage {i + 1} has one order");
            }
        }

        [Test]
        public void Parser_GroupsTouchingUpperCaseCells_AndKeepsLowerCaseSeparate()
        {
            var layout = TestLevels.Map(
                "SCss.O",
                "@...G.",
                "######");

            Assert.That(layout.Pieces.Count, Is.EqualTo(3));
            Assert.That(layout.Pieces[0].Cells.GetLength(1), Is.EqualTo(2), "S and C join");
            Assert.That(layout.Pieces[1].Cells.GetLength(1), Is.EqualTo(2), "s and s join as a loose piece");
        }

        [Test]
        public void Parser_MakesEveryOrderCellItsOwnPiece()
        {
            var layout = TestLevels.Map("@OOG", "####");
            Assert.That(layout.Pieces.Count(p => p.IsOrder), Is.EqualTo(2));
        }

        [Test]
        public void Parser_OrdersSignsByColumnThenRow()
        {
            var layout = LevelParser.Parse(new LevelDefinition("s", "s", "t", 0, "c",
                new[] { "first", "second", "third" },
                new[] { "..t.", "t.t.", "@OG.", "####" }));

            Assert.That(layout.Signs.Select(s => s.Text), Is.EqualTo(new[] { "first", "second", "third" }));
            Assert.That(layout.Signs[0].Cell, Is.EqualTo(new GridPoint(0, 1)));
            Assert.That(layout.Signs[1].Cell, Is.EqualTo(new GridPoint(2, 0)));
        }

        private static readonly TestCaseData[] MalformedMaps =
        {
            new TestCaseData((object)new[] { "@OGx", "####" }).SetName("Unknown symbol"),
            new TestCaseData((object)new[] { "@OG", "####" }).SetName("Ragged rows"),
            new TestCaseData((object)new[] { ".OG", "###" }).SetName("Missing start"),
            new TestCaseData((object)new[] { "@@OG", "####" }).SetName("Two starts"),
            new TestCaseData((object)new[] { "@O.", "###" }).SetName("Missing goal"),
            new TestCaseData((object)new[] { "@SG", "###" }).SetName("Missing order"),
            new TestCaseData((object)new[] { "@oG", "###" }).SetName("Lower-case order"),
            new TestCaseData((object)new[] { "@OGt", "####" }).SetName("Sign without text"),
        };

        [TestCaseSource(nameof(MalformedMaps))]
        public void Parser_RejectsMalformedMaps(string[] map)
        {
            var definition = new LevelDefinition("bad", "bad", "t", 0, "c", new string[0], map);
            Assert.Throws<LevelFormatException>(() => LevelParser.Parse(definition));
        }

        [Test]
        public void Parser_RejectsOversizedMaps()
        {
            string row = new string('.', LevelParser.MaxWidth + 1);
            var definition = new LevelDefinition("big", "big", "t", 0, "c", new string[0], new[] { row });
            Assert.Throws<LevelFormatException>(() => LevelParser.Parse(definition));
        }

        [Test]
        public void Parser_RejectsNegativePar()
        {
            var definition = new LevelDefinition("p", "p", "t", -1, "c", new string[0], new[] { "@OG", "###" });
            Assert.Throws<LevelFormatException>(() => LevelParser.Parse(definition));
        }

        [TestCase("", TestName = "Empty file")]
        [TestCase("{ not json", TestName = "Broken JSON")]
        [TestCase("{\"version\":2,\"stages\":[]}", TestName = "Unsupported version")]
        [TestCase("{\"version\":1,\"stages\":[]}", TestName = "No stages")]
        [TestCase("{\"version\":1,\"stages\":[{\"id\":\"a\"},{\"id\":\"a\"}]}", TestName = "Duplicate ids")]
        public void LevelJson_RejectsMalformedFiles(string json)
        {
            Assert.Throws<LevelFormatException>(() => LevelJson.Parse(json));
        }
    }
}
