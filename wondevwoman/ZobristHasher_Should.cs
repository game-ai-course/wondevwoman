using System;
using FluentAssertions;
using System.Linq;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class ZobristHasher_Should
    {
        private readonly Random random = new Random(093853);

        public void MaintainHash()
        {
            var state = CreateRandomState();

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

        private State CreateRandomState()
        {
            var hs = 3.Times(
                y => 3.Times(random.Next(0, 5)).ToArray()).ToArray();
            var us = 2.Times(
                p => 2.Times(new Vec(random.Next(0, 3), random.Next(0, 3))).ToArray()).ToArray();
            return new State(hs, us, new[] { false, false }, new int[2]);
        }

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
    }
}