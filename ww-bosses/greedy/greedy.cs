using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace CG.WondevWoman
{
    public class AcceptDefeatAction : IGameAction
    {
        private bool prevDead;

        public Cancelable ApplyTo(State state)
        {
            prevDead = state.IsDead(state.CurrentPlayer);
            state.SetDead(state.CurrentPlayer, true);
            state.ChangeCurrentPlayer();
            return new Cancelable(() => Undo(state));
        }

        public string Message { get; set; }
        public ExplainedScore Score { get; set; }

        private void Undo(State state)
        {
            state.ChangeCurrentPlayer();
            state.SetDead(state.CurrentPlayer, prevDead);
        }

        public override string ToString()
        {
            return "ACCEPT-DEFEAT";
        }
    }
}

namespace CG.WondevWoman
{
    public class AlphaBeta<TNode>
    {

        public AlphaBeta(
            Func<TNode, IList<TNode>> getChildren, Func<TNode, ExplainedScore> getScore,
            Func<TNode, IDisposable> openNode,
            Func<bool> timeIsOut = null)
        {
        }

        public int LastSearchTreeSize { get; private set; }

        public ScoredList<TNode> Search(TNode node, int depth, ScoredList<TNode> priorityBranch = null)
        {
            throw new NotImplementedException();
        }

    }
}

namespace CG.WondevWoman
{
    public class AlphabetaAi
    {
        private readonly IStateEvaluator evaluator;
        private readonly int maxDepth;

        public AlphabetaAi(IStateEvaluator evaluator, int maxDepth = int.MaxValue)
        {
            this.evaluator = evaluator;
            this.maxDepth = maxDepth;
        }

        public int LastSearchTreeSize { get; private set; }
        public bool Logging { get; set; } = true;

        public IGameAction GetAction(State state, Countdown countdown)
        {
            throw new NotImplementedException();
        }

    }
}

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
            return actions.MaxBy(a => a.Score ?? double.NegativeInfinity);
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

namespace CG.WondevWoman
{
    public interface IGameAction
    {
        string Message { get; set; }
        ExplainedScore Score { get; set; }
        Cancelable ApplyTo(State state);
    }
}

namespace CG.WondevWoman
{
    public interface IStateEvaluator
    {
        ExplainedScore Evaluate(State state, int playerIndex);
    }
}

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

        public MoveAndBuildAction(int index, Direction moveDirection, Direction buildDirection)
        {
            Index = index;
            MoveDirection = moveDirection;
            BuildDirection = buildDirection;
        }

        public int Index { get; }

        public Vec BuildPos { get; private set; }

        public Cancelable ApplyTo(State state)
        {
            try
            {
                oldPos = state.MyUnits[Index];
                var newPos = oldPos + MoveDirection;
                buildPosition = newPos + BuildDirection;
                BuildPos = buildPosition;
                oldHeight = state.HeightAt(buildPosition);
                oldScore = state.GetScore(state.CurrentPlayer);
                state.MoveUnit(state.CurrentPlayer, Index, newPos);
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
            state.MoveUnit(state.CurrentPlayer, Index, oldPos);
        }

        public override string ToString()
        {
            return $"MOVE&BUILD {Index} {MoveDirection} {BuildDirection}";
        }
    }
}

/*
 * C����� CORRECTNESS_CHECKS �������� �������������� ��������, ��� �� �� ���������� ��� ������ �� ����� ����.
 * ������� �� ����� ������� ��������� � ��� �������� ��������� � ���������.
 * ����� ����� ����� ���������, ����� �� ���� ��������� ��������.
 */

// #define CORRECTNESS_CHECKS
 
    

namespace CG.WondevWoman
{
    public static class Program
    {
        private static void Main()
        {
            var reader = new StateReader(Console.ReadLine);
            var initData = reader.ReadInitialization();
            var evaluator = new StateEvaluator();
            var fogRevealer = new SimpleFogRevealer();
            var ai = new GreedyAi(evaluator);
            while (true)
            {
                var state = reader.ReadState(initData);
                var countdown = new Countdown(45);
                fogRevealer.ConsiderStateBeforeMove(state, 20);
                // ReSharper disable once RedundantAssignment
                var actions = reader.ReadActions();
                EnsureActionsAreSame(state.GetPossibleActions(), actions);
                var action = ai.GetAction(state, countdown);
                WriteOutput(action);
                Console.Error.WriteLine(countdown);
                fogRevealer.RegisterAction(action);
                action.ApplyTo(state);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private static void WriteOutput(IGameAction action)
        {
            var s = action.ToString();
            if (action.Message != null)
                s += " " + action.Message;
            Console.WriteLine(s);
        }

        [Conditional("CORRECTNESS_CHECKS")]
        private static void EnsureActionsAreSame(IEnumerable<IGameAction> actual, List<string> expected)
        {
            var expectedSet = new HashSet<string>(expected);
            if (expectedSet.Count == 0)
                expectedSet.Add("ACCEPT-DEFEAT");
            foreach (var action in actual)
                if (!expectedSet.Remove(action.ToString()))
                    throw new Exception($"Extra action {action}");
            if (expectedSet.Any())
                throw new Exception($"missing action {expectedSet.First()}");
        }
    }
}

namespace CG.WondevWoman
{
    public class PushAndBuildAction : IGameAction
    {
        public readonly Direction PushDirection;
        public readonly Direction TargetDirection;
        private int enemyIndex;
        private Vec enemyOldPos;
        private int oldHeight;

        public PushAndBuildAction(int index, Direction targetDirection, Direction pushDirection)
        {
            Index = index;
            TargetDirection = targetDirection;
            PushDirection = pushDirection;
        }

        public int Index { get; }

        public Vec BuildPos { get; private set; }

        public Cancelable ApplyTo(State state)
        {
            try
            {
                var me = state.MyUnits[Index];
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
            return $"PUSH&BUILD {Index} {TargetDirection} {PushDirection}";
        }
    }
}

namespace CG.WondevWoman
{
    public class SearchNode
    {
        public IGameAction Action;

        public State State;

        public SearchNode(State state, IGameAction action)
        {
            State = state;
            Action = action;
        }
    }
}

namespace CG.WondevWoman
{
    public class SimpleFogRevealer
    {
        private State prevState;
        private IGameAction prevAction;

        public void ConsiderStateBeforeMove(State state, Countdown countdown)
        {
            // countdown ����� �� �������, ����� ��������� ������ ��� ���-�� ������.
            if (prevState != null)
            {
                prevAction.ApplyTo(prevState);
                RevealFromFog(state, prevState);
            }
            prevState = state.MakeCopy();
        }
        
        private static void RevealFromFog(State state, State prevState)
        {
            // �������, ��� ��������� ����� �������� ���, ��� �� ������ ��������� ���,
            // ���� ��� ������ ����� ������.
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
        public int StepsCompleted { get; private set; }

        public bool Finished => StepsCompleted > 400;
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
                StepsCompleted = StepsCompleted,
                CurrentPlayer = CurrentPlayer
            };
        }

        public List<IGameAction> GetPossibleActions()
        {
            if (dead[0] && dead[1]) return new List<IGameAction>();
            if (dead[CurrentPlayer])
                return new List<IGameAction> { new AcceptDefeatAction() };
            var actions =
                GetPossiblePushActions().Concat(GetPossibleMoveActions())
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
                                && !AllUnits.Any(u => u.Equals(build)))
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
            StepsCompleted++;
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

namespace CG.WondevWoman
{
    public class StateEvaluator : IStateEvaluator
    {
        public ExplainedScore Evaluate(State state, int playerIndex)
        {
            return 0.1 * state.GetPossibleActions().Count + state.MyUnits.Sum(state.HeightAt);
        }
    }
}

namespace CG.WondevWoman
{
    public class InitializationData
    {
        public int Size;
        public int UnitsPerPlayer;

        public InitializationData(int size, int unitsPerPlayer)
        {
            Size = size;
            UnitsPerPlayer = unitsPerPlayer;
        }
    }

    public class StateReader : BaseStateReader
    {
        public StateReader(string input) : base(input)
        {
        }

        public StateReader(Func<string> consoleReadLine) : base(consoleReadLine)
        {
        }

        public static State Read(string input, int unitsCount = 2)
        {
            var size = input.Split('|')[0].Length;
            return new StateReader(input)
                .ReadState(new InitializationData(size, unitsCount));
        }

        public InitializationData ReadInitialization()
        {
            var size = int.Parse(readLine());
            var unitsPerPlayer = int.Parse(readLine());
            Console.Error.WriteLine();
            return new InitializationData(size, unitsPerPlayer);
        }

        public List<string> ReadActions()
        {
            var legalActionCount = int.Parse(readLine());
            var actions = Enumerable.Range(0, legalActionCount)
                .Select(i => readLine())
                .ToList();
            if (logToError) Console.Error.WriteLine();
            return actions;
        }

        public State ReadState(InitializationData data)
        {
            try
            {
                var cells = Enumerable.Range(0, data.Size)
                    .Select(i => readLine().Select(c => c == '.' ? 4 : c - '0').ToArray())
                    .ToArray();
                var myUnits = Enumerable.Range(0, data.UnitsPerPlayer).Select(i => Vec.Parse(readLine())).ToArray();
                var hisUnits = Enumerable.Range(0, data.UnitsPerPlayer).Select(i => Vec.Parse(readLine())).ToArray();
                // ReSharper disable once UnusedVariable
                if (logToError) Console.Error.WriteLine();
                return new State(cells, new[] { myUnits, hisUnits }, new bool[2], new int[2]);
            }
            catch (Exception e)
            {
                throw new FormatException($"Line [{lastLine}]", e);
            }
        }

        public static IGameAction ParseAction(string line)
        {
            var ps = line.Split();
            if (ps[0] == "MOVE&BUILD")
                return new MoveAndBuildAction(
                    int.Parse(ps[1]),
                    (Direction) Enum.Parse(typeof(Direction), ps[2]),
                    (Direction) Enum.Parse(typeof(Direction), ps[3])
                );
            else if (ps[0] == "PUSH&BUILD")
                return new PushAndBuildAction(
                    int.Parse(ps[1]),
                    (Direction) Enum.Parse(typeof(Direction), ps[2]),
                    (Direction) Enum.Parse(typeof(Direction), ps[3])
                );
            else throw new Exception("unknown action " + ps[0]);
        }
    }
}

namespace CG.WondevWoman
{
    //https://chessprogramming.wikispaces.com/Zobrist+Hashing
    public class ZobristHasher
    {

        public ZobristHasher(State state)
        {
        }

        public ulong HashValue { get; private set; }

        public void SwitchUnit(Vec unit, int iPlayer)
        {
            throw new NotImplementedException();
        }

        public void SwitchHeight(int x, int y, int height)
        {
            throw new NotImplementedException();
        }
    }
}

namespace CG
{
    public class BaseStateReader
    {
        protected string lastLine;
        protected bool logToError = true;
        protected Func<string> readLine;

        public BaseStateReader(string input)
        {
            var lines = input.Split('|');
            var index = 0;
            logToError = false;
            readLine = () => index < lines.Length ? lines[index++] : null;
        }

        public BaseStateReader(Func<string> consoleReadLine)
        {
            readLine = () =>
            {
                lastLine = consoleReadLine();
                if (logToError)
                    Console.Error.Write(lastLine + "|");
                return lastLine;
            };
        }

        public int ReadInt()
        {
            return int.Parse(readLine());
        }

        public int[] ReadInts()
        {
            return readLine().Split().Select(int.Parse).ToArray();
        }

        public Vec ReadVec()
        {
            return Vec.Parse(readLine());
        }
    }
}

namespace CG
{
    public class Cancelable : IDisposable
    {
        private readonly Action cancel;

        public Cancelable(Action cancel)
        {
            this.cancel = cancel;
        }

        public void Dispose()
        {
            cancel();
        }
    }
}

namespace CG
{
    public class Collision
    {
        public Disk Object1, Object2;
        public double Time;

        public Collision(Disk object1, Disk object2, double time)
        {
            Object1 = object1;
            Object2 = object2;
            Time = time;
        }

        public override string ToString()
        {
            return $"{nameof(Time)}: {Time:0.00}, {nameof(Object1)}: {Object1}, {nameof(Object2)}: {Object2}";
        }
    }
}

namespace CG
{
    public class Countdown
    {
        private readonly Stopwatch stopwatch;
        private readonly int timeAvailableMs;

        public Countdown(int ms)
        {
            stopwatch = Stopwatch.StartNew();
            timeAvailableMs = ms;
        }

        public bool IsFinished => TimeLeftMs <= 0;

        public long TimeLeftMs => timeAvailableMs - stopwatch.ElapsedMilliseconds;
        public long ElapsedMs => stopwatch.ElapsedMilliseconds;

        public override string ToString()
        {
            return $"Elapsed {ElapsedMs} ms. Available {timeAvailableMs} ms";
        }

        public static implicit operator Countdown(int milliseconds)
        {
            return new Countdown(milliseconds);
        }
    }
}

namespace CG
{
    public enum Direction
    {
        N = 0,
        NE = 1,
        E = 2,
        SE = 3,
        S = 4,
        SW = 5,
        W = 6,
        NW = 7
    }

    public static class Directions
    {
        public static readonly Direction[] All8 =
        {
            Direction.N, Direction.NE, Direction.E, Direction.SE, Direction.S, Direction.SW, Direction.W, Direction.NW
        };

        public static bool IsNear8To(this Vec a, Vec b)
        {
            return Math.Abs(a.X - b.X) <= 1 && Math.Abs(a.Y - b.Y) <= 1;
        }

        public static IEnumerable<Direction> PushDirections(this Direction targetDir)
        {
            for (var d = (int) targetDir - 1; d <= (int) targetDir + 1; d++)
            {
                var pushDir = (Direction) ((d + 8) % 8);
                yield return pushDir;
            }
        }
    }
}

namespace CG
{
    public class Disk
    {
        public Disk(Vec pos, Vec v, int mass, int radius)
        {
            Pos = pos;
            V = v;
            Mass = mass;
            Radius = radius;
        }

        public Vec Pos { get; private set; }
        public Vec V { get; protected set; }
        public int Mass { get; protected set; }
        public int Radius { get; }

        public void Move(double time)
        {
            Pos += time * V;
        }

        public double GetCollisionTime(Disk other)
        {
            // TODO: ������ ���������������� ����������. 
            // ���������� �� ������� � double-��, � �������� � ����� ����.
            // ������ �� ���� �������� ������������ ���������� ��������� � �����������.
            var dr = other.Pos - Pos;
            var dv = other.V - V;
            long dvdr = dv.ScalarProd(dr);
            if (dvdr > 0) return double.PositiveInfinity;
            long dvdv = dv.ScalarProd(dv);
            if (dvdv == 0) return double.PositiveInfinity;
            long drdr = dr.ScalarProd(dr);
            var sigma = Radius + other.Radius;
            var d = dvdr * dvdr - dvdv * (drdr - sigma * sigma);
            if (d < 0) return double.PositiveInfinity;
            var collisionTime = -(dvdr + Math.Sqrt(d)) / dvdv;
            if (collisionTime < 0) return double.PositiveInfinity;
            return collisionTime;
        }

        public void BounceOff(Disk other)
        {
            var impulse = ComputeImpulse(Pos, V, Mass, other.Pos, other.V, other.Mass);
            var vs = BouncedSpeed(V, Mass, other.V, other.Mass, impulse);
            V = vs.Item1;
            other.V = vs.Item2;
        }

        public void BounceOffWithMinimumImpulse(Disk other, double minImpulse)
        {
            var impulse = ComputeImpulse(Pos, V, Mass, other.Pos, other.V, other.Mass);
            var impulseSize = Math.Max(impulse.Length(), impulse.Length() * 0.5 + minImpulse);
            var adjusted = (Pos - other.Pos).Resize(impulseSize);
            var vs = BouncedSpeed(V, Mass, other.V, other.Mass, adjusted);
            V = vs.Item1;
            other.V = vs.Item2;
        }

        private static Vec ComputeImpulse(Vec p1, Vec v1, int m1, Vec p2, Vec v2, int m2)
        {
            var dp = p2 - p1;
            var dv = v2 - v1;
            var drdr = dp.ScalarProd(dp);
            var dvdr = dp.ScalarProd(dv);
            var massCoefficient = (m1 + m2) / (double) (m1 * m2);
            return 2 * dvdr / (massCoefficient * drdr) * dp;
        }

        private static Tuple<Vec, Vec> BouncedSpeed(Vec v1, int m1, Vec v2, int m2, Vec impulse)
        {
            return Tuple.Create(v1 + impulse / m1, v2 - impulse / m2);
        }

        public override string ToString()
        {
            return $"{nameof(Pos)}: {Pos}, {nameof(V)}: {V}, {nameof(Mass)}: {Mass}, {nameof(Radius)}: {Radius}";
        }
    }
}

namespace CG
{
    public class ExplainedScore : IComparable<ExplainedScore>, IComparable
    {
        public readonly string Explanation;

        public readonly double Value;

        public ExplainedScore(double value, string explanation = null)
        {
            Explanation = explanation;
            Value = value;
        }

        public int CompareTo(object obj)
        {
            if (obj is ExplainedScore s) return CompareTo(s);
            return -1;
        }

        public int CompareTo(ExplainedScore other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return Value.CompareTo(other.Value);
        }

        public static explicit operator double(ExplainedScore score)
        {
            return score.Value;
        }

        public static implicit operator ExplainedScore(double score)
        {
            return new ExplainedScore(score);
        }

        public override string ToString()
        {
            return $"{Value:0.00} because {Explanation}";
        }
    }
}

namespace CG
{
    public static class Extensions
    {
        public static int IndexOf<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            var i = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return i;
                i++;
            }
            return -1;
        }

        public static T MinBy<T>(this IEnumerable<T> items, Func<T, IComparable> getKey)
        {
            var best = default(T);
            IComparable bestKey = null;
            var found = false;
            foreach (var item in items)
                if (!found || getKey(item).CompareTo(bestKey) < 0)
                {
                    best = item;
                    bestKey = getKey(best);
                    found = true;
                }
            return best;
        }

        public static double MaxOrDefault<T>(this IEnumerable<T> items, Func<T, double> getCost, double defaultValue)
        {
            var bestCost = double.NegativeInfinity;
            foreach (var item in items)
            {
                var cost = getCost(item);
                if (cost > bestCost)
                    bestCost = cost;
            }
            return double.IsNegativeInfinity(bestCost) ? defaultValue : bestCost;
        }

        public static T MaxBy<T>(this IEnumerable<T> items, Func<T, IComparable> getKey)
        {
            var best = default(T);
            IComparable bestKey = null;
            var found = false;
            foreach (var item in items)
                if (!found || getKey(item).CompareTo(bestKey) > 0)
                {
                    best = item;
                    bestKey = getKey(best);
                    found = true;
                }
            return best;
        }

        public static int BoundTo(this int v, int left, int right)
        {
            if (v < left) return left;
            if (v > right) return right;
            return v;
        }

        public static double BoundTo(this double v, double left, double right)
        {
            if (v < left) return left;
            if (v > right) return right;
            return v;
        }

        public static double TruncateAbs(this double v, double maxAbs)
        {
            if (v < -maxAbs) return -maxAbs;
            if (v > maxAbs) return maxAbs;
            return v;
        }

        public static int TruncateAbs(this int v, int maxAbs)
        {
            if (v < -maxAbs) return -maxAbs;
            if (v > maxAbs) return maxAbs;
            return v;
        }

        public static IEnumerable<T> Times<T>(this int count, Func<int, T> create)
        {
            return Enumerable.Range(0, count).Select(create);
        }

        public static IEnumerable<T> Times<T>(this int count, T item)
        {
            return Enumerable.Repeat(item, count);
        }

        public static bool InRange(this int v, int min, int max)
        {
            return v >= min && v <= max;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> readOnlyList, T value)
        {
            var count = readOnlyList.Count;
            var equalityComparer = EqualityComparer<T>.Default;
            for (var i = 0; i < count; i++)
            {
                var current = readOnlyList[i];
                if (equalityComparer.Equals(current, value)) return i;
            }
            return -1;
        }

        public static TV GetOrCreate<TK, TV>(this IDictionary<TK, TV> d, TK key, Func<TK, TV> create)
        {
            TV v;
            if (d.TryGetValue(key, out v)) return v;
            return d[key] = create(key);
        }

        public static TV GetOrDefault<TK, TV>(this IDictionary<TK, TV> d, TK key, TV def = default(TV))
        {
            TV v;
            if (d.TryGetValue(key, out v)) return v;
            return def;
        }

        public static int ElementwiseHashcode<T>(this IEnumerable<T> items)
        {
            unchecked
            {
                return items.Select(t => t.GetHashCode()).Aggregate((res, next) => (res * 379) ^ next);
            }
        }

        public static List<T> Shuffle<T>(this IEnumerable<T> items, Random random)
        {
            var copy = items.ToList();
            for (var i = 0; i < copy.Count; i++)
            {
                var nextIndex = random.Next(i, copy.Count);
                var t = copy[nextIndex];
                copy[nextIndex] = copy[i];
                copy[i] = t;
            }
            return copy;
        }

        public static double NormAngleInRadians(this double angle)
        {
            while (angle > Math.PI) angle -= 2 * Math.PI;
            while (angle <= -Math.PI) angle += 2 * Math.PI;
            return angle;
        }

        public static double NormDistance(this double value, double worldDiameter)
        {
            return value / worldDiameter;
        }

        public static int ToInt(this string s)
        {
            return int.Parse(s);
        }

        public static string StrJoin<T>(this IEnumerable<T> items, string delimiter)
        {
            return string.Join(delimiter, items);
        }

        public static string StrJoin<T>(this IEnumerable<T> items, string delimiter, Func<T, string> toString)
        {
            return items.Select(toString).StrJoin(delimiter);
        }
    }

    public static class RandomExtensions
    {
        public static ulong NextUlong(this Random r)
        {
            var a = unchecked ((ulong) r.Next());
            var b = unchecked ((ulong) r.Next());
            return (a << 32) | b;
        }
        
        public static double NextDouble(this Random r, double min, double max)
        {
            return r.NextDouble() * (max - min) + min;
        }

        public static ulong[,,] CreateRandomTable(this Random r, int size1, int size2, int size3)
        {
            var res = new ulong[size1, size2, size3];
            for (var x = 0; x < size1; x++)
            for (var y = 0; y < size2; y++)
            for (var h = 0; h < size3; h++)
            {
                var value = r.NextUlong();
                res[x, y, h] = value;
            }
            return res;
        }

        public static ulong[,] CreateRandomTable(this Random r, int size1, int size2)
        {
            var res = new ulong[size1, size2];
            for (var x = 0; x < size1; x++)
            for (var y = 0; y < size2; y++)
            {
                var value = r.NextUlong();
                res[x, y] = value;
            }
            return res;
        }
    }
}

namespace CG
{
    public class ScoredList<TItem> : IEnumerable<TItem>
    {
        public readonly TItem Node;
        public readonly ScoredList<TItem> PrevItems;
        public readonly ExplainedScore Score;

        public ScoredList(TItem node, ScoredList<TItem> prevItems, ExplainedScore score)
        {
            Node = node;
            PrevItems = prevItems;
            Score = score;
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            var current = this;
            while (current != null)
            {
                yield return current.Node;
                current = current.PrevItems;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

namespace CG
{
    public class StatValue
    {
        public long Count { get; set; }
        public double Sum { get; set; }
        public double Sum2 { get; set; }
        public double Min { get; set; } = double.PositiveInfinity;
        public double Max { get; set; } = double.NegativeInfinity;

        public double StdDeviation => Math.Sqrt(Count * Sum2 - Sum * Sum) / Count;
        public double ConfIntervalSize => 2 * Math.Sqrt(Count * Sum2 - Sum * Sum) / Count / Math.Sqrt(Count);
        public double Mean => Sum / Count;


        public void Add(double value)
        {
            Count++;
            Sum += value;
            Sum2 += value * value;
            Min = Math.Min(Min, value);
            Max = Math.Max(Max, value);
        }

        public void AddAll(StatValue value)
        {
            Count += value.Count;
            Sum += value.Sum;
            Sum2 += value.Sum2;
            Min = Math.Min(Min, value.Min);
            Max = Math.Max(Max, value.Max);
        }

        public override string ToString()
        {
            return $"{Mean} +- {StdDeviation}";
        }


        public string ToDetailedString()
        {
            return $"{Mean} disp={StdDeviation} range={Min}..{Max} confInt={ConfIntervalSize} count={Count}";
        }


        public StatValue Clone()
        {
            return new StatValue
            {
                Sum = Sum,
                Sum2 = Sum2,
                Max = Max,
                Min = Min,
                Count = Count
            };
        }
    }
}

namespace CG
{
    public class Vec : IEquatable<Vec>, IFormattable
    {
        public readonly int X, Y;

        public Vec(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Vec(double x, double y)
            : this(checked((int) Math.Round(x)), checked((int) Math.Round(y)))
        {
        }

        public int this[int dimension] => dimension == 0 ? X : Y;

        [Pure]
        public bool Equals(Vec other)
        {
            return X == other.X && Y == other.Y;
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return $"{X.ToString(format, formatProvider)} {Y.ToString(format, formatProvider)}";
        }

        public static Vec FromPolar(double len, double angle)
        {
            return new Vec(len * Math.Cos(angle), len * Math.Sin(angle));
        }

        public static Vec Parse(string s)
        {
            var parts = s.Split();
            if (parts.Length != 2) throw new FormatException(s);
            return new Vec(int.Parse(parts[0]), int.Parse(parts[1]));
        }

        public static IEnumerable<Vec> Area(int size)
        {
            return Area(size, size);
        }

        public static IEnumerable<Vec> Area(int width, int height)
        {
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                yield return new Vec(x, y);
        }

        public bool InArea(int size)
        {
            return X >= 0 && X < size && Y >= 0 && Y < size;
        }

        public bool InArea(int w, int h)
        {
            return X.InRange(0, w - 1) && Y.InRange(0, h - 1);
        }

        public Vec BoundTo(int minX, int minY, int maxX, int maxY)
        {
            return new Vec(X.BoundTo(minX, maxX), Y.BoundTo(minY, maxY));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Vec && Equals((Vec) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode() * 397) ^ Y.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool InRadiusTo(Vec b, double radius)
        {
            return SquaredDistTo(b) <= radius * radius;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double DistTo(Vec b)
        {
            return (b - this).Length();
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int SquaredDistTo(Vec b)
        {
            var dx = X - b.X;
            var dy = Y - b.Y;
            return dx * dx + dy * dy;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double Length()
        {
            return Math.Sqrt(X * X + Y * Y);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int LengthSquared()
        {
            return X * X + Y * Y;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator -(Vec a, Vec b)
        {
            return new Vec(a.X - b.X, a.Y - b.Y);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator -(Vec a)
        {
            return new Vec(-a.X, -a.Y);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator +(Vec v, Vec b)
        {
            return new Vec(v.X + b.X, v.Y + b.Y);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(Vec a, int k)
        {
            return new Vec(a.X * k, a.Y * k);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator /(Vec a, int k)
        {
            return new Vec(a.X / k, a.Y / k);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(int k, Vec a)
        {
            return new Vec(a.X * k, a.Y * k);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vec operator *(double k, Vec a)
        {
            return new Vec(a.X * k, a.Y * k);
        }

        [Pure]
        public static Vec operator +(Vec v, Direction d)
        {
            switch (d)
            {
                case Direction.E: return new Vec(v.X + 1, v.Y);
                case Direction.W: return new Vec(v.X - 1, v.Y);
                case Direction.S: return new Vec(v.X, v.Y + 1);
                case Direction.N: return new Vec(v.X, v.Y - 1);
                case Direction.NE: return new Vec(v.X + 1, v.Y - 1);
                case Direction.NW: return new Vec(v.X - 1, v.Y - 1);
                case Direction.SE: return new Vec(v.X + 1, v.Y + 1);
                case Direction.SW: return new Vec(v.X - 1, v.Y + 1);
                default:
                    throw new Exception("Unknown direction " + d);
            }
        }

        [Pure]
        public static Vec operator -(Vec v, Direction d)
        {
            switch (d)
            {
                case Direction.E: return new Vec(v.X - 1, v.Y);
                case Direction.W: return new Vec(v.X + 1, v.Y);
                case Direction.S: return new Vec(v.X, v.Y - 1);
                case Direction.N: return new Vec(v.X, v.Y + 1);
                case Direction.NE: return new Vec(v.X - 1, v.Y + 1);
                case Direction.NW: return new Vec(v.X + 1, v.Y + 1);
                case Direction.SE: return new Vec(v.X - 1, v.Y - 1);
                case Direction.SW: return new Vec(v.X + 1, v.Y - 1);
                default:
                    throw new Exception("Unknown direction " + d);
            }
        }


        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ScalarProd(Vec p2)
        {
            return X * p2.X + Y * p2.Y;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int VectorProdLength(Vec p2)
        {
            return X * p2.Y - p2.X * Y;
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec Translate(int shiftX, int shiftY)
        {
            return new Vec(X + shiftX, Y + shiftY);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec MoveTowards(Vec target, int distance)
        {
            var d = target - this;
            var difLen = d.Length();
            if (difLen < distance) return target;
            var k = distance / difLen;
            return new Vec(X + k * d.X, Y + k * d.Y);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec Rotate90CW()
        {
            return new Vec(Y, -X);
        }

        [Pure]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vec Rotate90CCW()
        {
            return new Vec(-Y, X);
        }

        public Vec Resize(double newLen)
        {
            return newLen / Length() * this;
        }

        /// <returns>angle in (-Pi..Pi]</returns>
        public double GetAngle()
        {
            return Math.Atan2(Y, X);
        }
    }
}

