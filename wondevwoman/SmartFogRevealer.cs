using System;
using System.Collections.Generic;
using System.Linq;

namespace CG.WondevWoman
{
    public class SmartFogRevealer
    {
        private readonly IEstimator estimator;

        public SmartFogRevealer(IEstimator estimator)
        {
            this.estimator = estimator;
        }

        public List<Vec[]> PossibleEnemyPositions;
        private IGameAction prevAction;
        private State prevState;

        public void ConsiderStateBeforeMove(State state, Countdown countdown)
        {
            PossibleEnemyPositions =
                PossibleEnemyPositions == null
                    ? InitEnemyPositions(state)
                    : UpdateEnemyPositions(state);
            prevState = state.MakeCopy();
            Vec[] bestPos = null;
            if (countdown.IsFinished)
            {
                bestPos = PossibleEnemyPositions.FirstOrDefault();
                Console.Error.WriteLine("HURRY!!! elapsed time " + countdown.ElapsedMs);
            }
            else
            {
                var copy = prevState.MakeCopy();
                bestPos = PossibleEnemyPositions
                    .Take(50)
                    .OrderBy(pos => EstimatePositionFast(pos, copy))
                    .TakeWhile(x => !countdown.IsFinished)
                    .MinBy(p => EstimatePosition(p, copy, countdown));
            }

            if (bestPos != null)
            {
                // reveal
                state.MoveUnit(1, 0, bestPos[0]);
                state.MoveUnit(1, 1, bestPos[1]);
            }
        }

        private ExplainedScore EstimatePositionFast(Vec[] enemyPos, State state)
        {
            using (SetEnemies(state, enemyPos))
            {
                return estimator.Estimate(state, 0);
            }
        }

        private ExplainedScore EstimatePosition(Vec[] enemyPos, State state, Countdown countdown)
        {
            using (SetEnemies(state, enemyPos))
            {
                var action = new GreedyAi(estimator).GetAction(state, countdown);
                return action.Score;
            }
        }

        private List<Vec[]> InitEnemyPositions(State state)
        {
            var res = new List<Vec[]>();
            foreach (var unit1 in GetPossibleEnemyPositions(0, state))
            foreach (var unit2 in GetPossibleEnemyPositions(1, state))
                if (!unit1.Equals(unit2))
                    res.Add(new[] { unit1, unit2 });
            return res;
        }

        private IEnumerable<Vec> GetPossibleEnemyPositions(int unitId, State state)
        {
            if (state.HisUnits[unitId].X >= 0) return new[] { state.HisUnits[unitId] };
            else
                return Vec.Area(state.Size)
                    .Where(state.IsPassableHeightAt)
                    .Where(p => !p.IsNear8To(state.MyUnits[0]) && !p.IsNear8To(state.MyUnits[1]));
        }

        private List<Vec[]> UpdateEnemyPositions(State state)
        {
            var res = new Dictionary<string, Vec[]>();
            foreach (var enemyPos in PossibleEnemyPositions)
                using (SetEnemies(prevState, enemyPos))
                using (prevAction.ApplyTo(prevState))
                {
                    List<IGameAction> possibleActions;
                    using (ApplyFog(prevState))
                    {
                        possibleActions = prevState.GetPossibleActions();
                    }
                    foreach (var enemyAction in possibleActions)
                        using (enemyAction.ApplyTo(prevState))
                        {
                            var ok = false;
                            using (ApplyFog(prevState))
                            {
                                if (prevState.HashValue() == state.HashValue())
                                    ok = true;
                            }
                            if (ok) res[prevState.HisUnits.StrJoin(" ")] = prevState.HisUnits.ToArray();
                        }
                }
            return res.Values.ToList();
        }

        private Cancelable SetEnemies(State state, Vec[] enemyPos)
        {
            var prevPositions = state.HisUnits.ToList();
            state.MoveUnit(1 - state.CurrentPlayer, 0, enemyPos[0]);
            state.MoveUnit(1 - state.CurrentPlayer, 1, enemyPos[1]);
            return new Cancelable(
                () =>
                {
                    state.MoveUnit(1 - state.CurrentPlayer, 0, prevPositions[0]);
                    state.MoveUnit(1 - state.CurrentPlayer, 1, prevPositions[1]);
                });
        }

        private Cancelable ApplyFog(State state)
        {
            var myUnits = state.MyUnits;
            var hisUnits = state.HisUnits;
            var prevPositions = new Vec[2];
            var him = 1 - state.CurrentPlayer;
            var unknown = new Vec(-1, -1);
            if (!IsVisible(hisUnits[0], myUnits))
            {
                prevPositions[0] = hisUnits[0];
                state.MoveUnit(him, 0, unknown);
            }
            if (!IsVisible(hisUnits[1], myUnits))
            {
                prevPositions[1] = hisUnits[1];
                state.MoveUnit(him, 1, unknown);
            }
            return new Cancelable(
                () =>
                {
                    if (prevPositions[0] != null)
                        state.MoveUnit(him, 0, prevPositions[0]);
                    if (prevPositions[1] != null)
                        state.MoveUnit(him, 1, prevPositions[1]);
                });
        }

        private static bool IsVisible(Vec hisUnit, IReadOnlyList<Vec> myUnits)
        {
            return hisUnit.IsNear8To(myUnits[0]) || hisUnit.IsNear8To(myUnits[1]);
        }

        public void RegisterAction(IGameAction action)
        {
            prevAction = action;
        }
    }
}