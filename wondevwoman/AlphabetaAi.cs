using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CG.WondevWoman
{
    public class AlphabetaAi
    {
        private readonly IStateEvaluator evaluator;
        private readonly int maxDepth;

        public AlphabetaAi(IStateEvaluator evaluator, int maxDepth = int.MaxValue)
        {
            this.evaluator = evaluator;
            this.maxDepth = maxDepth;
        }

        public int LastSearchTreeSize { get; private set; }
        public bool Logging { get; set; } = true;

        public IGameAction GetAction(State state, Countdown countdown)
        {
            throw new NotImplementedException();
        }

    }
}
