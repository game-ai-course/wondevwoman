using System;

namespace CG.WondevWoman
{
    //https://chessprogramming.wikispaces.com/Zobrist+Hashing
    public class ZobristHasher
    {

        public ZobristHasher(State state)
        {
        }

        public ulong HashValue { get; private set; }

        public void SwitchUnit(Vec unit, int iPlayer)
        {
            throw new NotImplementedException();
        }

        public void SwitchHeight(int x, int y, int height)
        {
            throw new NotImplementedException();
        }
    }
}
