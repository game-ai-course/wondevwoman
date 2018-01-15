namespace CG.WondevWoman
{
    public interface IStateEvaluator
    {
        ExplainedScore Evaluate(State state, int playerIndex);
    }
}
