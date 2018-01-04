using System;

namespace CG.WondevWoman
{
    //https://chessprogramming.wikispaces.com/Zobrist+Hashing
    public class ZobristHasher
    {
#if DEV
        private static readonly ulong[,,] Heights;
        private static readonly ulong[,,] Players;

        static ZobristHasher()
        {
            var random = new Random(1212324);
            Heights = random.CreateRandomTable(20, 20, 5);
            Players = random.CreateRandomTable(20, 20, 2);
        }
#endif

        public ZobristHasher(State state)
        {
#if DEV
            for (var x = 0; x < state.Size; x++)
            for (var y = 0; y < state.Size; y++)
                SwitchHeight(x, y, state.HeightAt(x, y));
            for (var iPlayer = 0; iPlayer < 2; iPlayer++)
            for (var iUnit = 0; iUnit < 2; iUnit++)
            {
                var unit = state.GetUnits(iPlayer)[iUnit];
                SwitchUnit(unit, iPlayer);
            }
#endif
        }

        public ulong HashValue { get; private set; }

        public void SwitchUnit(Vec unit, int iPlayer)
        {
#if DEV
            if (unit.X < 0) return;
            HashValue = HashValue ^ Players[unit.X, unit.Y, iPlayer];
#else
            throw new NotImplementedException();
#endif
        }

        public void SwitchHeight(int x, int y, int height)
        {
#if DEV
            HashValue = HashValue ^ Heights[x, y, height];
#else
            throw new NotImplementedException();
#endif
        }
    }
}