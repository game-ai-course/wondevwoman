using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class PushAndBuildAction : IGameAction
    {
        public readonly int UnitIndex;
        public readonly Direction TargetDirection;
        public readonly Direction PushDirection;

        public string Message { get; set; }

        public PushAndBuildAction(int unitIndex, Direction targetDirection, Direction pushDirection)
        {
            UnitIndex = unitIndex;
            TargetDirection = targetDirection;
            PushDirection = pushDirection;
        }

        public Cancelable ApplyTo(State state)
        {
            try
            {
                var me = state.MyUnits[UnitIndex];
                var enemyOldPos = me + TargetDirection;
                var enemyDest = enemyOldPos + PushDirection;
                int enemyIndex = state.HisUnits.IndexOf(enemyOldPos);
                int oldHeight = state.HeightAt(enemyOldPos);

                if (enemyIndex >= 0 && !state.MyUnits.Concat(state.HisUnits).Any(p => p.Equals(enemyDest)))
                {
                    state.SetHeight(enemyOldPos, state.HeightAt(enemyOldPos) + 1);
                    state.MoveUnit(1 - state.CurrentPlayer, enemyIndex, enemyDest);
                }
                state.ChangeCurrentPlayer();
                state.EnsureValid();
                return new Cancelable(() => Undo(state, enemyIndex, enemyOldPos, oldHeight));
            }
            catch (Exception e)
            {
                throw new Exception($"Bad move [{ToString()}]. {e.Message}\n{state}", e);
            }
        }

        private void Undo(State state, int enemyIndex, Vec enemyOldPos, int oldHeight)
        {
            state.ChangeCurrentPlayer();
            if (enemyIndex >= 0)
                state.MoveUnit(1 - state.CurrentPlayer, enemyIndex, enemyOldPos);
            state.SetHeight(enemyOldPos, oldHeight);
        }

        public bool Equals(IGameAction other)
        {
            return Equals((object) other);
        }

        public override string ToString()
        {
            return $"PUSH&BUILD {UnitIndex} {TargetDirection} {PushDirection}";
        }

        protected bool Equals(PushAndBuildAction other)
        {
            return PushDirection == other.PushDirection && TargetDirection == other.TargetDirection && UnitIndex == other.UnitIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PushAndBuildAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) PushDirection;
                hashCode = (hashCode * 397) ^ (int) TargetDirection;
                hashCode = (hashCode * 397) ^ UnitIndex;
                return hashCode;
            }
        }

        public static bool operator ==(PushAndBuildAction left, PushAndBuildAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PushAndBuildAction left, PushAndBuildAction right)
        {
            return !Equals(left, right);
        }
    }
}
