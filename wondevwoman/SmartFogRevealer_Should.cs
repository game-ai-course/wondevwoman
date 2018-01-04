using System;
using System.Diagnostics;
using FluentAssertions;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class SmartFogRevealer_Should
    {
        [TestCase(
            "0.00.0|0.10.0|020100|021010|.0100.|001000|3 4|3 3|0 1|4 3|",
            "PUSH&BUILD 0 NE NE",
            "0.00.0|0.10.0|020100|121020|.0100.|001000|3 4|3 3|-1 -1|-1 -1|",
            1, TestName = "NobodyVisible_AllRevealable")] //0 2 5 2
        [TestCase(
            "0.00.0|0.10.0|010000|020000|.0100.|001000|3 4|2 2|1 2|4 5|",
            "PUSH&BUILD 1 W NW",
            "0.00.0|0.10.0|020000|020010|.0100.|001000|3 4|2 2|-1 -1|4 4|",
            1, TestName = "OneVisible_Revealable")] //0 1 4 4
        [TestCase(
            "000000|000000|000000|000000|000000|000000|0 0|1 0|-1 -1|-1 -1|",
            "MOVE&BUILD 0 S N",
            "100000|000000|000000|000000|000000|000100|0 1|1 0|-1 -1|-1 -1|",
            240, TestName = "Slow")]
        [TestCase(
            "00000|00000|00000|00200|00100|2 4|1 4|-1 -1|-1 -1|",
            "MOVE&BUILD 0 NE NE",
            "00000|00000|00011|00200|00100|3 3|1 4|2 2|-1 -1|",
            24, TestName = "Crash")]
        [TestCase(
            "...0...|..000..|.00100.|0020000|.00000.|..000..|...0...|1 2|3 1|-1 -1|-1 -1|",
            "MOVE&BUILD 1 SE N",
            "...0...|..001..|.00100.|0021000|.00000.|..000..|...0...|1 2|4 2|5 3|-1 -1|",
            6, TestName = "SomeBug")]
        public void Reveal(string state1, string myMove, string state2, int expectedPositionsCount)
        {
            var revealer = CreateFogRevealer();
            revealer.ConsiderStateBeforeMove(StateReader.Read(state1), 20);
            revealer.RegisterAction(StateReader.ParseAction(myMove));
            var sw = Stopwatch.StartNew();
            revealer.ConsiderStateBeforeMove(StateReader.Read(state2), 20);
            Console.WriteLine(sw.ElapsedMilliseconds);
            revealer.PossibleEnemyPositions.Should().HaveCount(expectedPositionsCount);
        }

        [Test]
        [TestCase(
            "000000|000000|000000|000000|000000|000000|0 0|1 0|-1 -1|-1 -1|",
            870, TestName = "WorstCase")]
        [TestCase(
            "0.00.0|0.10.0|010000|020000|.0100.|001000|3 4|2 2|1 2|4 5|",
            1, TestName = "AllVisible")]
        [TestCase(
            "0.00.0|0.10.0|010000|020000|.0100.|001000|3 4|2 2|-1 -1|-1 -1|",
            210, TestName = "AllInFog")]
        public void Init(string state, int expectedPositionsCount)
        {
            for (var i = 0; i < 10; i++)
            {
                var revealer = new SmartFogRevealer(new Estimator());
                revealer.ConsiderStateBeforeMove(StateReader.Read(state), 50);
                revealer.PossibleEnemyPositions.Should().HaveCount(expectedPositionsCount);
            }
        }

        [Test]
        public void Reveal2()
        {
            var revealer = CreateFogRevealer();
            revealer.ConsiderStateBeforeMove(
                StateReader.Read("...0...|..000..|.00000.|0010000|.00000.|..000..|...0...|3 2|3 1|-1 -1|-1 -1|"),
                new Countdown(20));
            Console.WriteLine(revealer.PossibleEnemyPositions.Count);
            revealer.RegisterAction(StateReader.ParseAction("MOVE&BUILD 0 SW NE"));
            revealer.ConsiderStateBeforeMove(
                StateReader.Read("...0...|..000..|.00100.|0020000|.00000.|..000..|...0...|1 2|3 1|-1 -1|-1 -1"),
                new Countdown(20));
            Console.WriteLine(revealer.PossibleEnemyPositions.StrJoin(";", ps => ps.StrJoin(" ")));
            revealer.RegisterAction(StateReader.ParseAction("MOVE&BUILD 1 SE N"));
            revealer.ConsiderStateBeforeMove(
                StateReader.Read("...0...|..001..|.00100.|0021000|.00000.|..000..|...0...|1 2|4 2|5 3|-1 -1|"),
                new Countdown(20));
            Console.WriteLine(revealer.PossibleEnemyPositions.StrJoin(";", ps => ps.StrJoin(" ")));
            revealer.PossibleEnemyPositions.Should().HaveCount(6);
        }

        private static SmartFogRevealer CreateFogRevealer()
        {
            return new SmartFogRevealer(new Estimator());
        }
    }
}