namespace CG.WondevWoman
{
    public class SearchNode
    {
        public IGameAction Action;

        public State State;

        public SearchNode(State state, IGameAction action)
        {
            State = state;
            Action = action;
        }
    }
}
