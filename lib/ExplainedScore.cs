using System;

namespace CG
{
    public class ExplainedScore : IComparable<ExplainedScore>, IComparable
    {
        public readonly string Explanation;

        public readonly double Value;

        public ExplainedScore(double value, string explanation = null)
        {
            Explanation = explanation;
            Value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj is ExplainedScore s) return CompareTo(s);
            return -1;
        }

        public int CompareTo(ExplainedScore other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Value.CompareTo(other.Value);
        }

        public static explicit operator double(ExplainedScore score)
        {
            return score.Value;
        }

        public static implicit operator ExplainedScore(double score)
        {
            return new ExplainedScore(score);
        }

        public override string ToString()
        {
            return $"{Value:0.00} because {Explanation}";
        }
    }
}
