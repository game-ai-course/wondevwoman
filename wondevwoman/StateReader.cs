using System;
using System.Collections.Generic;
using System.Linq;

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