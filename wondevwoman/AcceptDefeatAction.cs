namespace CG.WondevWoman
{
    public class AcceptDefeatAction : IGameAction
    {
        private bool prevDead;

        public Cancelable ApplyTo(State state)
        {
            prevDead = state.IsDead(state.CurrentPlayer);
            state.SetDead(state.CurrentPlayer, true);
            state.ChangeCurrentPlayer();
            return new Cancelable(() => Undo(state));
        }

        public string Message { get; set; }
        public ExplainedScore Score { get; set; }

        private void Undo(State state)
        {
            state.ChangeCurrentPlayer();
            state.SetDead(state.CurrentPlayer, prevDead);
        }

        public override string ToString()
        {
            return "ACCEPT-DEFEAT";
        }
    }
}
