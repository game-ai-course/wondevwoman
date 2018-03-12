using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CG.WondevWoman
{

    public class AlphaBetaAi
    {

        private readonly IStateEvaluator evaluator;
        private readonly int maxDepth;

        public AlphaBetaAi(IStateEvaluator evaluator, int maxDepth = int.MaxValue)
        {
            this.evaluator = evaluator;
            this.maxDepth = maxDepth;
        }

        public int LastSearchTreeSize { get; private set; }
        public int LastDepthFullySearched { get; private set; }
        public bool LoggingEnabled { get; set; } = true;

        public IGameAction GetAction(State state, Countdown countdown)
        {
            LastSearchTreeSize = 0; // на каждый новый узел в дереве поиска должен быть инкремент
            throw new NotImplementedException();
        }

    }
}
