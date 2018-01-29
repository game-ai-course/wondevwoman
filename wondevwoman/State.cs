using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CG.WondevWoman
{
    public class State
    {
        private readonly bool[] dead;
        private readonly int[][] heights;
        private readonly int[] scores;
        private readonly Vec[][] units;

        public State(int[][] heights, Vec[][] units, bool[] dead, int[] scores)
        {
            this.heights = heights;
            this.units = units;
            this.dead = dead;
            this.scores = scores;
        }

        public int CurrentPlayer { get; private set; }
        public IReadOnlyList<Vec> MyUnits => units[CurrentPlayer];
        public IReadOnlyList<Vec> HisUnits => units[1 - CurrentPlayer];
        public int Size => heights.Length;
        public IEnumerable<Vec> AllUnits => units.SelectMany(us => us);

        public IReadOnlyList<Vec> GetUnits(int player)
        {
            return units[player];
        }

        public bool IsDead(int player)
        {
            return dead[player];
        }

        public int GetScore(int player)
        {
            return scores[player];
        }

        public int HeightAt(Vec pos)
        {
            return heights[pos.Y][pos.X];
        }

        public int HeightAt(int x, int y)
        {
            return heights[y][x];
        }

        [Conditional("CORRECTNESS_CHECKS")]
        public void EnsureValid()
        {
            var badCell = Vec.Area(Size, Size).FirstOrDefault(h => !HeightAt(h).InRange(0, 4));
            if (badCell != null)
                throw new Exception($"cell {badCell} has height {HeightAt(badCell)}");
            foreach (var unit in units.SelectMany(us => us))
                if (unit.X >= 0 && !HeightAt(unit).InRange(0, 3))
                    throw new Exception($"unit {unit} stands on cell with height {HeightAt(unit)}");
        }

        public State MakeCopy()
        {
            return new State(
                heights.Select(r => r.ToArray()).ToArray(),
                units.Select(us => us.ToArray()).ToArray(),
                dead.ToArray(),
                scores.ToArray()
            )
            {
                CurrentPlayer = CurrentPlayer
            };
        }

        public List<IGameAction> GetPossibleActions()
        {
            if (dead[0] && dead[1]) return new List<IGameAction>();
            if (dead[CurrentPlayer])
                return new List<IGameAction> { new AcceptDefeatAction() };
            var actions =
                GetPossibleMoveActions().Concat(GetPossiblePushActions())
                    .ToList();
            if (actions.Count == 0)
                actions.Add(new AcceptDefeatAction());
            return actions;
        }

        private IEnumerable<IGameAction> GetPossiblePushActions()
        {
            for (var index = 0; index < MyUnits.Count; index++)
            {
                var unit = MyUnits[index];
                if (unit.X < 0) continue;
                foreach (var targetDir in Directions.All8)
                {
                    var target = unit + targetDir;
                    if (HisUnits.Any(u => u.Equals(target)))
                        foreach (var pushDir in targetDir.PushDirections())
                            if (CanMove(target, target + pushDir))
                            {
                                var action = new PushAndBuildAction(index, targetDir, pushDir);
                                EnsureMoveValid(action);
                                yield return action;
                            }
                }
            }
        }

        private IEnumerable<IGameAction> GetPossibleMoveActions()
        {
            for (var index = 0; index < MyUnits.Count; index++)
            {
                var unit = MyUnits[index];
                if (unit.X < 0) continue;
                foreach (var moveDir in Directions.All8)
                {
                    var dest = unit + moveDir;
                    if (CanMove(unit, dest))
                        foreach (var buildDir in Directions.All8)
                        {
                            var build = dest + buildDir;
                            if (build.InArea(Size)
                                && HeightAt(build) < 4
                                && !AllUnits.Any(u => u != unit && u.Equals(build)))
                            {
                                var action = new MoveAndBuildAction(index, moveDir, buildDir);
                                EnsureMoveValid(action);
                                yield return action;
                            }
                        }
                }
            }
        }

        [Conditional("CORRECTNESS_CHECKS")]
        private void EnsureMoveValid<TAction>(TAction action) where TAction : IGameAction
        {
            action.ApplyTo(this).Dispose();
        }

        public bool IsPassableHeightAt(Vec dest)
        {
            return dest.InArea(Size) && HeightAt(dest).InRange(0, 3);
        }

        public bool CanMoveIgnoringUnits(Vec source, Vec dest)
        {
            return IsPassableHeightAt(dest) && HeightAt(dest) - HeightAt(source) <= 1;
        }

        public bool CanMove(Vec source, Vec dest)
        {
            return CanMoveIgnoringUnits(source, dest) && !AllUnits.Contains(dest);
        }


        public string Serialize()
        {
            var map = heights.StrJoin("|", row => row.StrJoin(""));
            var my = MyUnits.StrJoin("|");
            var his = HisUnits.StrJoin("|");
            return string.Join("|", map, my, his);
        }

        public override string ToString()
        {
            var map = heights.StrJoin("\n", row => row.StrJoin(""));
            var my = MyUnits.StrJoin("; ");
            var his = HisUnits.StrJoin("; ");
            return $"{map}\n{my}\n{his}\n{CurrentPlayer}";
        }

        public void ChangeCurrentPlayer()
        {
            CurrentPlayer = 1 - CurrentPlayer;
        }

        public void MoveUnit(int player, int index, Vec newPos)
        {
            units[player][index] = newPos;
        }

        public void SetHeight(Vec pos, int newHeight)
        {
            heights[pos.Y][pos.X] = newHeight;
        }

        public void SetDead(int player, bool isDead)
        {
            dead[player] = isDead;
        }

        public void SetScore(int player, int newScore)
        {
            scores[player] = newScore;
        }

    }
}
