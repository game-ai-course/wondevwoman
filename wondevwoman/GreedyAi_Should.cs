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
            var state = new StateReader("...0...|..000..|.00000.|0000000|.00000.|..000..|...0...|3 2|5 2|-1 -1|4 1|")
                .ReadState(new InitializationData(7, 2));
            var action = new GreedyAi(new StateEvaluator()).GetAction(state, 100);
            action.ToString().Should().Be("MOVE&BUILD 0 W N");
        }
    }
}
