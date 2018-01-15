using System;
using System.Linq;

namespace CG.WondevWoman
{
    public class SimpleFogRevealer
    {
        private State prevState;
        private IGameAction prevAction;

        public void ConsiderStateBeforeMove(State state, Countdown countdown)
        {
            // countdown нужен на будущее, когда захочется делать тут что-то тяжёлое.
            if (prevState != null)
            {
                prevAction.ApplyTo(prevState);
                RevealFromFog(state, prevState);
            }
            prevState = state.MakeCopy();
        }
        
        private static void RevealFromFog(State state, State prevState)
        {
            // Считает, что невидимые юниты остаются там, где их видели последний раз,
            // если там вообще можно стоять.
            for (int i = 0; i < state.HisUnits.Count; i++)
                if (state.HisUnits[i].X < 0)
                {
                    var prevLoc = prevState.MyUnits[i];
                    if (prevLoc.X >= 0 && !state.MyUnits.Any(u => u.IsNear8To(prevLoc)))
                        state.MoveUnit(1-state.CurrentPlayer, i, prevLoc);
                }
                else if (!state.IsPassableHeightAt(state.HisUnits[i]))
                    state.MoveUnit(1-state.CurrentPlayer, i, new Vec(-1, -1));
        }

        public void RegisterAction(IGameAction action)
        {
            prevAction = action;
        }
    }
}
