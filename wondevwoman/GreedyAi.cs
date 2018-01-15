using System.Linq;

namespace CG.WondevWoman
{
    public class GreedyAi
    {
        private readonly IStateEvaluator evaluator;

        public GreedyAi(IStateEvaluator evaluator)
        {
            this.evaluator = evaluator;
        }

        public IGameAction GetAction(State state, Countdown countdown)
        {
            var actions = state.GetPossibleActions();
            foreach (var action in actions)
            {
                if (countdown.IsFinished) break;
                action.Score = Evaluate(state, action);
                
            }
            return actions.MaxBy(a => a.Score);
        }

        private ExplainedScore Evaluate(State state, IGameAction action)
        {
            using (action.ApplyTo(state))
            {
                return evaluator.Evaluate(state, 0);
            }
        }
    }
}
