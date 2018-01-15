using System;
using System.Collections.Generic;
using System.Linq;

namespace CG.WondevWoman
{
    public class AlphaBeta<TNode>
    {

        public AlphaBeta(
            Func<TNode, IList<TNode>> getChildren, Func<TNode, ExplainedScore> getScore,
            Func<TNode, IDisposable> openNode,
            Func<bool> timeIsOut = null)
        {
        }

        public int LastSearchTreeSize { get; private set; }

        public ScoredList<TNode> Search(TNode node, int depth, ScoredList<TNode> priorityBranch = null)
        {
            throw new NotImplementedException();
        }

    }
}
