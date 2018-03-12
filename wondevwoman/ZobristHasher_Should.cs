using System;
using FluentAssertions;
using System.Linq;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class ZobristHasher_Should
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
            var oldHash = GetHashValue(state);
            foreach (var pos in Vec.Area(state.Size))
            {
                var oldHeight = state.HeightAt(pos);
                for (int h = 0; h < 5; h++)
                {
                    if (h != oldHeight)
                    {
                        state.SetHeight(pos, h);
                        GetHashValue(state).Should().NotBe(oldHash);
                        state.SetHeight(pos, oldHeight);
                        GetHashValue(state).Should().Be(oldHash);
                    }
                }
            }
        }

        private static ulong GetHashValue(State state)
        {
            // return state.HashValue();
            throw new NotImplementedException("Раскомментируйте строчку выше и добейтесь работоспособности");
        }

        [Test]
        public void HashUnits()
        {
            var state = GetState();
            var oldHash = GetHashValue(state);
            foreach (var player in new[] { 0, 1 })
            {
                var units = state.GetUnits(player);
                int unitsCount = units.Count;
                for (int iUnit = 0; iUnit < unitsCount; iUnit++)
                {
                    var unit = units[iUnit];
                    foreach (var pos in Vec.Area(state.Size).Where(p => unit.X < 0 || state.CanMove(unit, p)))
                    {
                        state.MoveUnit(player, iUnit, pos);
                        GetHashValue(state).Should().NotBe(oldHash);
                        state.MoveUnit(player, iUnit, unit);
                        GetHashValue(state).Should().Be(oldHash);
                    }
                }
            }
        }

        [Test]
        public void HashCurrentPlayer()
        {
            var state = GetState();
            var oldHash = GetHashValue(state);
            state.ChangeCurrentPlayer();
            GetHashValue(state).Should().NotBe(oldHash);
            state.ChangeCurrentPlayer();
            GetHashValue(state).Should().Be(oldHash);
        }

        [Test, Timeout(20000)]
        public void BeFast()
        {
            int[,] heights = new int[10, 10];
            Vec[][] units = {
                new[]{ new Vec(0, 0), new Vec(2, 2) },
                new[]{ new Vec(4, 4), new Vec(6,6) }
            };
            var deads = new[] { false, false };
            var scores = new[] { 0, 0 };
            var state = new State(heights, units, deads, scores);
            for(int i=0; i<3000; i++)
                foreach (var pos in Vec.Area(state.Size))
                {
                    for (int h = 0; h < 5; h++)
                    {
                        // Пересчёт значений должен быть на порядок быстрее, чем рассчёт хэша с нуля.
                        state.SetHeight(pos, h);
                        GetHashValue(state);
                        // var v = new ZobristHasher(state).HashValue; // Это рассчёт снуля. Можно раскомментировать и проверить, что это медленнее.
                    }
                }
        }
    }
}
