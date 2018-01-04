using System;
using System.Collections.Generic;
using System.Linq;

namespace CG.WondevWoman
{
    public class AlphaBeta<TNode>
    {
#if DEV
        private readonly Func<TNode, IList<TNode>> getChildren;
        private readonly Func<TNode, ExplainedScore> getScore;
        private readonly Func<TNode, IDisposable> openNode;
        private readonly Func<bool> timeIsOut;
#endif

        public AlphaBeta(
            Func<TNode, IList<TNode>> getChildren, Func<TNode, ExplainedScore> getScore,
            Func<TNode, IDisposable> openNode,
            Func<bool> timeIsOut = null)
        {
#if DEV
            this.getChildren = getChildren;
            this.getScore = getScore;
            this.openNode = openNode;
            this.timeIsOut = timeIsOut ?? (() => false);
#endif
        }

        public int LastSearchTreeSize { get; private set; }

        public ScoredList<TNode> Search(TNode node, int depth, ScoredList<TNode> bestScoreds = null)
        {
#if DEV
            LastSearchTreeSize = 0;
            return Search(
                node, depth, true, new ExplainedScore(double.NegativeInfinity),
                new ExplainedScore(double.PositiveInfinity),
                bestScoreds);
#else
            throw new NotImplementedException();
#endif
        }

#if DEV
        private ScoredList<TNode> Search(
            TNode node, int depth, bool maximizing, ExplainedScore alpha, ExplainedScore beta,
            ScoredList<TNode> priorityScored)
        {
            using (openNode(node))
            {
                LastSearchTreeSize++;
                if (depth == 0) return new ScoredList<TNode>(node, null, getScore(node));
                IEnumerable<TNode> children = getChildren(node);
                if (!children.Any()) return new ScoredList<TNode>(node, null, getScore(node));
                if (priorityScored != null)
                    children = new[] { priorityScored.Node }.Concat(children);
                ScoredList<TNode> best = null;
                if (maximizing)
                {
                    var priority = priorityScored != null;
                    foreach (var child in children)
                    {
                        var moves = Search(
                            child, depth - 1, false, alpha, beta, priority ? priorityScored.PrevItems : null);
                        priority = false;
                        if (moves.Score.Value > alpha.Value)
                        {
                            best = moves;
                            alpha = best.Score;
                        }
                        if (alpha.Value >= beta.Value || timeIsOut()) break;
                    }
                    return new ScoredList<TNode>(node, best, alpha);
                }
                else
                {
                    var priority = priorityScored != null;
                    foreach (var child in children)
                    {
                        var moves = Search(
                            child, depth - 1, true, alpha, beta, priority ? priorityScored.PrevItems : null);
                        priority = false;
                        if (moves.Score.Value < beta.Value)
                        {
                            best = moves;
                            beta = best.Score;
                        }
                        if (alpha.Value >= beta.Value || timeIsOut()) break;
                    }
                    return new ScoredList<TNode>(node, best, beta);
                }
            }
        }
#endif
    }
}