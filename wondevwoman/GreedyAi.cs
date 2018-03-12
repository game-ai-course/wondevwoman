using System.Collections.Generic;
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
            var scores = new Dictionary<IGameAction, ExplainedScore>();
            foreach (var action in actions)
            {
                if (countdown.IsFinished) break;
                scores[action] = Evaluate(state, action);
                
            }
            return actions.MaxBy(a => scores.TryGetValue(a, out var score) ? score : double.NegativeInfinity);
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
