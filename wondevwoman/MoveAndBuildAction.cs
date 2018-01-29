using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class MoveAndBuildAction : IGameAction
    {
        public readonly Direction BuildDirection;
        public readonly Direction MoveDirection;
        private Vec buildPosition;
        private int oldHeight;
        private Vec oldPos;
        private int oldScore;

        public MoveAndBuildAction(int unitIndex, Direction moveDirection, Direction buildDirection)
        {
            UnitIndex = unitIndex;
            MoveDirection = moveDirection;
            BuildDirection = buildDirection;
        }

        public int UnitIndex { get; }

        public Vec BuildPos { get; private set; }

        public Cancelable ApplyTo(State state)
        {
            try
            {
                oldPos = state.MyUnits[UnitIndex];
                var newPos = oldPos + MoveDirection;
                buildPosition = newPos + BuildDirection;
                BuildPos = buildPosition;
                oldHeight = state.HeightAt(buildPosition);
                oldScore = state.GetScore(state.CurrentPlayer);
                state.MoveUnit(state.CurrentPlayer, UnitIndex, newPos);
                if (state.HeightAt(newPos) == 3)
                    state.SetScore(state.CurrentPlayer, oldScore + 1);
                if (!state.HisUnits.Any(u => u.Equals(buildPosition)))
                    state.SetHeight(buildPosition, state.HeightAt(buildPosition) + 1);
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
            state.SetHeight(buildPosition, oldHeight);
            state.SetScore(state.CurrentPlayer, oldScore);
            state.MoveUnit(state.CurrentPlayer, UnitIndex, oldPos);
        }

        public override string ToString()
        {
            return $"MOVE&BUILD {UnitIndex} {MoveDirection} {BuildDirection}";
        }
    }
}
