using System;
using System.Collections.Generic;
using System.Linq;

namespace CG.WondevWoman
{
    public class AlphaBeta<TNode> where TNode : class
    {

        public AlphaBeta(
            Func<TNode, IList<TNode>> getChildren, 
            Func<TNode, ExplainedScore> getScore,
            Func<TNode, IDisposable> openNode,
            Func<bool> timeIsOut = null)
        {
        }

        public Scored<TNode> Search(TNode node, int depth)
        {
            throw new NotImplementedException();
        }

    }
}
