namespace CG.WondevWoman
{
    public interface IEstimator
    {
        ExplainedScore Estimate(State state, int playerIndex);
    }
}