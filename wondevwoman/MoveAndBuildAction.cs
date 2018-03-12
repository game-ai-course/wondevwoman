using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class MoveAndBuildAction : IGameAction
    {
        public readonly int UnitIndex;
        public readonly Direction MoveDirection;
        public readonly Direction BuildDirection;

        public string Message { get; set; }

        public MoveAndBuildAction(int unitIndex, Direction moveDirection, Direction buildDirection)
        {
            UnitIndex = unitIndex;
            MoveDirection = moveDirection;
            BuildDirection = buildDirection;
        }

        public Cancelable ApplyTo(State state)
        {
            try
            {
                var oldPos = state.MyUnits[UnitIndex];
                var newPos = oldPos + MoveDirection;
                var buildPosition = newPos + BuildDirection;
                int oldHeight = state.HeightAt(buildPosition);
                int oldScore = state.GetScore(state.CurrentPlayer);
                state.MoveUnit(state.CurrentPlayer, UnitIndex, newPos);
                if (state.HeightAt(newPos) == 3)
                    state.SetScore(state.CurrentPlayer, oldScore + 1);
                if (!state.HisUnits.Any(u => u.Equals(buildPosition)))
                    state.SetHeight(buildPosition, state.HeightAt(buildPosition) + 1);
                state.ChangeCurrentPlayer();
                state.EnsureValid();
                return new Cancelable(() => Undo(state, buildPosition, oldHeight, oldScore, oldPos));
            }
            catch (Exception e)
            {
                throw new Exception($"Bad move [{ToString()}]. {e.Message}\n{state}", e);
            }
        }

        private void Undo(State state, Vec buildPosition, int oldHeight, int oldScore, Vec oldPos)
        {
            state.ChangeCurrentPlayer();
            state.SetHeight(buildPosition, oldHeight);
            state.SetScore(state.CurrentPlayer, oldScore);
            state.MoveUnit(state.CurrentPlayer, UnitIndex, oldPos);
        }

        public bool Equals(IGameAction other)
        {
            return Equals((object)other);
        }

        public override string ToString()
        {
            return $"MOVE&BUILD {UnitIndex} {MoveDirection} {BuildDirection}";
        }
        protected bool Equals(MoveAndBuildAction other)
        {
            return BuildDirection == other.BuildDirection && MoveDirection == other.MoveDirection && UnitIndex == other.UnitIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MoveAndBuildAction) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) BuildDirection;
                hashCode = (hashCode * 397) ^ (int) MoveDirection;
                hashCode = (hashCode * 397) ^ UnitIndex;
                return hashCode;
            }
        }

        public static bool operator ==(MoveAndBuildAction left, MoveAndBuildAction right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MoveAndBuildAction left, MoveAndBuildAction right)
        {
            return !Equals(left, right);
        }
    }
}
