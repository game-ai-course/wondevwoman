using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CG.WondevWoman
{
    public class AlphabetaAi
    {
        private readonly IEstimator estimator;
        private readonly int maxDepth;
#if DEV
        private readonly Dictionary<ulong, ExplainedScore> scoreCache = new Dictionary<ulong, ExplainedScore>();
        private ScoredList<SearchNode> bestMoves;
        private int missCount;
        private int simsCount;
        private Stopwatch stopwatch;
        private int totalSimsCount;
#endif

        public AlphabetaAi(IEstimator estimator, int maxDepth = int.MaxValue)
        {
            this.estimator = estimator;
            this.maxDepth = maxDepth;
        }

        public int LastSearchTreeSize { get; private set; }
        public bool Logging { get; set; } = true;

        public IGameAction GetAction(State state, Countdown countdown)
        {
#if DEV
            stopwatch = Stopwatch.StartNew();
            simsCount = 0;
            var ab = new AlphaBeta<SearchNode>(GetChildren, GetScore, OpenNode, () => countdown.IsFinished);
            bestMoves = ab.Search(new SearchNode(state, null), 1);
            var depth = 2;
            while (depth < maxDepth)
            {
                var moves = ab.Search(new SearchNode(state, null), depth, bestMoves.PrevItems);
                if (countdown.IsFinished)
                    break;
                bestMoves = moves;
                depth++;
            }
            LastSearchTreeSize = ab.LastSearchTreeSize;
            if (Logging)
            {
                Console.Error.WriteLine(bestMoves.Score);
                Console.Error.WriteLine("depth=" + (depth - 1));
                Console.Error.WriteLine(stopwatch.ElapsedMilliseconds + " ms");
                Console.Error.WriteLine(simsCount + " sims");
                Console.Error.WriteLine(100 * missCount / totalSimsCount + "% miss rate of " + totalSimsCount);
            }
            bestMoves.PrevItems.Node.Action.Message = bestMoves.Score + " d=" + (depth - 1);
            return bestMoves.PrevItems.Node.Action;
#else
            throw new NotImplementedException();
#endif
        }

#if DEV
        private IDisposable OpenNode(SearchNode node)
        {
            return node.Action?.ApplyTo(node.State);
        }

        private ExplainedScore GetScore(SearchNode node)
        {
            simsCount++;
            totalSimsCount++;
            return scoreCache.GetOrCreate(
                node.State.HashValue(),
                h =>
                {
                    missCount++;
                    return estimator.Estimate(node.State, 0);
                });
        }

        private IList<SearchNode> GetChildren(SearchNode node)
        {
            return node.State
                .GetPossibleActions()
                .Select(a => new SearchNode(node.State, a))
                .ToList();
        }
#endif
    }
}