namespace CG.WondevWoman
{
    public interface IGameAction
    {
        string Message { get; set; }
        ExplainedScore Score { get; set; }
        Cancelable ApplyTo(State state);
    }
}
