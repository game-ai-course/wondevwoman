using System;
using FluentAssertions;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class GreedyAi_Should
    {
        [Test]
        public void CalculateNextMove()
        {
            var state = new StateReader("...0...|..000..|.00000.|0000000|.00000.|..000..|...0...|3 2|5 2|-1 -1|-1 -1|")
                .ReadState(new InitializationData(7, 2));
            var evaluator = new StateEvaluator();
            var action = new GreedyAi(evaluator).GetAction(state, 100);
            ExplainedScore score = null;
            using (new MoveAndBuildAction(0, Direction.N, Direction.N).ApplyTo(state))
                score = evaluator.Evaluate(state, 0);
            using (action.ApplyTo(state))
                evaluator.Evaluate(state, 0).Should().BeGreaterOrEqualTo(score);
        }
    }
}
