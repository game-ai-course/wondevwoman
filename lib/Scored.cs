namespace CG
{
    public static class Scored
    {
        public static Scored<TObject> WithScore<TObject>(this TObject obj, ExplainedScore score)
            => new Scored<TObject>(obj, score);
    }

    public class Scored<TObject>
    {
        public readonly TObject Node;
        public readonly ExplainedScore Score;

        public Scored(TObject node, ExplainedScore score)
        {
            Node = node;
            Score = score;
        }
    }
}
