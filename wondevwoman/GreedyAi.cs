using System.Linq;

namespace CG.WondevWoman
{
    public class GreedyAi
    {
        private readonly IEstimator estimator;

        public GreedyAi(IEstimator estimator)
        {
            this.estimator = estimator;
        }

        public IGameAction GetAction(State state, Countdown countdown)
        {
            var actions = state.GetPossibleActions();
            foreach (var action in actions)
            {
                if (countdown.IsFinished) break;
                action.Score = Estimate(state, action);
                
            }
            return actions.MaxBy(a => a.Score);
        }

        private ExplainedScore Estimate(State state, IGameAction action)
        {
            using (action.ApplyTo(state))
            {
                return estimator.Estimate(state, 0);
            }
        }
    }
}