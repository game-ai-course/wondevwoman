using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class PushAndBuildAction : IGameAction
    {
        public readonly Direction PushDirection;
        public readonly Direction TargetDirection;
        private int enemyIndex;
        private Vec enemyOldPos;
        private int oldHeight;

        public PushAndBuildAction(int unitIndex, Direction targetDirection, Direction pushDirection)
        {
            UnitIndex = unitIndex;
            TargetDirection = targetDirection;
            PushDirection = pushDirection;
        }

        public int UnitIndex { get; }

        public Vec BuildPos { get; private set; }

        public Cancelable ApplyTo(State state)
        {
            try
            {
                var me = state.MyUnits[UnitIndex];
                enemyOldPos = me + TargetDirection;
                var enemyDest = enemyOldPos + PushDirection;
                BuildPos = enemyOldPos;
                enemyIndex = state.HisUnits.IndexOf(enemyOldPos);
                oldHeight = state.HeightAt(enemyOldPos);

                if (enemyIndex >= 0 && !state.MyUnits.Concat(state.HisUnits).Any(p => p.Equals(enemyDest)))
                {
                    state.SetHeight(enemyOldPos, state.HeightAt(enemyOldPos) + 1);
                    state.MoveUnit(1 - state.CurrentPlayer, enemyIndex, enemyDest);
                }
                state.ChangeCurrentPlayer();
                state.EnsureValid();
                return new Cancelable(() => Undo(state));
            }
            catch (Exception e)
            {
                throw new Exception($"Bad move [{ToString()}]. {e.Message}\n{state}", e);
            }
        }

        public string Message { get; set; }
        public ExplainedScore Score { get; set; }

        private void Undo(State state)
        {
            state.ChangeCurrentPlayer();
            if (enemyIndex >= 0)
                state.MoveUnit(1 - state.CurrentPlayer, enemyIndex, enemyOldPos);
            state.SetHeight(enemyOldPos, oldHeight);
        }

        public override string ToString()
        {
            return $"PUSH&BUILD {UnitIndex} {TargetDirection} {PushDirection}";
        }
    }
}
