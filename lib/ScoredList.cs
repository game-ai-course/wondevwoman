using System.Collections;
using System.Collections.Generic;

namespace CG
{
    public class ScoredList<TItem> : IEnumerable<TItem>
    {
        public readonly TItem Node;
        public readonly ScoredList<TItem> PrevItems;
        public readonly ExplainedScore Score;

        public ScoredList(TItem node, ScoredList<TItem> prevItems, ExplainedScore score)
        {
            Node = node;
            PrevItems = prevItems;
            Score = score;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            var current = this;
            while (current != null)
            {
                yield return current.Node;
                current = current.PrevItems;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}