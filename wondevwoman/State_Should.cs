using FluentAssertions;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class State_Should
    {
        private static State GetState()
        {
            return new StateReader("0.11.3|43..33|132013|3.41.3|0.30.3|000331|0 2|3 0|1 1|-1 -1|")
                .ReadState(new InitializationData(6, 2));
        }

        [Test]
        public void HashHeights()
        {
            var state = GetState();
            var oldValue = state.HashValue();
            state.SetHeight(new Vec(2, 0), 2);
            state.HashValue().Should().NotBe(oldValue);
            state.SetHeight(new Vec(2, 0), 1);
            state.HashValue().Should().Be(oldValue);
        }

        [Test]
        public void HashUnitMoves()
        {
            var state = GetState();
            var oldValue = state.HashValue();
            state.MoveUnit(0, 1, new Vec(3, 2));
            state.HashValue().Should().NotBe(oldValue);
            state.MoveUnit(0, 1, new Vec(3, 0));
            state.HashValue().Should().Be(oldValue);
            state.MoveUnit(1, 0, new Vec(-1, -1));
            state.HashValue().Should().NotBe(oldValue);
            state.MoveUnit(1, 1, new Vec(1, 1));
            state.HashValue().Should().Be(oldValue);
        }
    }
}