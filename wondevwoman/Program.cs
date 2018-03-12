using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

/*
 * Cимвол CORRECTNESS_CHECKS включает дополнительные проверки, что мы всё симулируем без ошибок во время игры.
 * полезно на этапе отладки симуляции и при внесении изменений в симуляцию.
 * После этого стоит отключить, чтобы не было накладных расходов.
 */

// #define CORRECTNESS_CHECKS
 

namespace CG.WondevWoman
{
    public static class Program
    {
        private static void Main()
        {
            var evaluator = new StateEvaluator();
            var fogRevealer = new SimpleFogRevealer();
            var ai = new GreedyAi(evaluator);
            var reader = new StateReader(Console.ReadLine);
            var initData = reader.ReadInitialization();
            var isFirst = true;
            while (true)
            {
                var state = reader.ReadState(initData);
                var countdown = new Countdown(isFirst ? 95 : 45);
                isFirst = false;
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
