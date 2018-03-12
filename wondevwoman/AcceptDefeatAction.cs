namespace CG.WondevWoman
{
    public class AcceptDefeatAction : IGameAction
    {
        public Cancelable ApplyTo(State state)
        {
            var prevDead = state.IsDead(state.CurrentPlayer);
            state.SetDead(state.CurrentPlayer, true);
            state.ChangeCurrentPlayer();
            return new Cancelable(() => Undo(state, prevDead));
        }

        public string Message { get; set; }

        private void Undo(State state, bool prevDead)
        {
            state.ChangeCurrentPlayer();
            state.SetDead(state.CurrentPlayer, prevDead);
        }

        public bool Equals(IGameAction other)
        {
            return other is AcceptDefeatAction;
        }

        public override string ToString()
        {
            return "ACCEPT-DEFEAT";
        }

        public override bool Equals(object obj)
        {
            return obj is AcceptDefeatAction;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(AcceptDefeatAction left, AcceptDefeatAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AcceptDefeatAction left, AcceptDefeatAction right)
        {
            return !Equals(left, right);
        }
    }
}
