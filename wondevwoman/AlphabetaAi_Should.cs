using System;
using FluentAssertions;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class AlphabetaAi_Should
    {
        private readonly IStateEvaluator evaluator = new StateEvaluator();

        [TestCase("0.11.3|43..33|132013|3.41.3|0.30.3|000331|0 2|3 0|-1 -1|-1 -1|")]
        [TestCase("...0...|..000..|.00000.|0000000|.00010.|..000..|...1...|3 5|5 4|3 4|-1 -1|")]
        public void BeEquivalentToGreedy_WhenSearchDepthIs1(string input)
        {
            var state = StateReader.Read(input);
            var initialStateDump = state.ToString();

            var greedyAction = new GreedyAi(evaluator).GetAction(state, 100);
            var abAction = new AlphaBetaAi(evaluator, 1).GetAction(state, 100);

            Console.WriteLine(state.ToString());
            Console.WriteLine($"[{abAction}]");

            state.ToString().Should().Be(initialStateDump, "ai.GetAction should not change state");
            abAction.ToString().Should().Be(greedyAction.ToString(), "ab-ai with depth=1 should be exactly gready ai");
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        public void AssignValueToLastDepthFullySearchedProperty_DuringSearch(int depth)
        {
            var state = StateReader.Read("00000|00000|00000|00000|00000|1 1|2 3|-1 -1|-1 -1|");
            var ai = new AlphaBetaAi(evaluator, depth);
            ai.GetAction(state, 1000);
            ai.LastDepthFullySearched.Should().Be(depth);
        }

        [TestCase(1, 9)]
        [TestCase(2, 26)]
        [TestCase(3, 131)]
        [TestCase(4, 316)]
        public void AssignValueToLastSearchTreeSizeProperty_DuringSearch(int depth, int expectedSize)
        {
            var state = StateReader.Read("002|022|200|0 0|1 0|-1 -1|-1 -1|");
            var ai = new AlphaBetaAi(evaluator, depth);
            ai.GetAction(state, 1000);
            ai.LastSearchTreeSize.Should().Be(expectedSize);
        }

        [Test]
        public void MeasurePerformance()
        {
            var totalNodes = new StatValue();
            var avDepth = new StatValue();
            for (int repeat = 0; repeat < 5; repeat++)
            {
                var state = new StateReader("...0...|..000..|.00000.|0000000|.00010.|..000..|...1...|3 5|5 4|3 4|1 1|")
                    .ReadState(new InitializationData(7, 2));
                var alphabetaAi = new AlphaBetaAi(evaluator) { LoggingEnabled = false };
                var totalNodesProcessed = 0;
                var totalDepths = 0;
                var count = 40;
                for (var turn = 0; turn < count; turn++)
                {
                    var action = alphabetaAi.GetAction(state, turn == 0 ? 100 : 50);
                    action.ApplyTo(state);
                    state.ChangeCurrentPlayer();
                    totalNodesProcessed += alphabetaAi.LastSearchTreeSize;
                    totalDepths += alphabetaAi.LastDepthFullySearched;
                }
                totalNodes.Add(totalNodesProcessed);
                avDepth.Add(totalDepths / (double)count);
            }
            Console.WriteLine($"Total Nodes Searched: {totalNodes}");
            Console.WriteLine($"Average search depth: {avDepth}");
        }

        [Test]
        public void MeasureSearchTreeSize()
        {
            var stateInput = "...2...|..303..|.13033.|0010031|.01412.|..121..|...1...|3 5|5 2|2 2|5 4|";
            var state = StateReader.Read(stateInput);
            new AlphaBetaAi(evaluator).GetAction(state, 1); // heat up
            var ai = new AlphaBetaAi(evaluator, 3);
            ai.GetAction(state, 3000); // measure
            Console.WriteLine($"SearchTreeSize: {ai.LastSearchTreeSize}");
        }
    }
}
