using System;
using FluentAssertions;
using System.Collections.Generic;
using NUnit.Framework;

namespace CG.WondevWoman
{
    [TestFixture]
    public class AlphabetaAi_Should
    {
        private readonly IEstimator estimator = new Estimator();

        [TestCase("0.11.3|43..33|132013|3.41.3|0.30.3|000331|0 2|3 0|-1 -1|-1 -1|")]
        [TestCase("...0...|..000..|.00000.|0000000|.00010.|..000..|...1...|3 5|5 4|3 4|-1 -1|")]
        public void AbWithDepth1_IsSameAsGreedy(string input)
        {
            var state = StateReader.Read(input);
            var initialStateDump = state.ToString();
            
            var greedyAction = new GreedyAi(estimator).GetAction(state, 100);
            var abAction = new AlphabetaAi(estimator, 1).GetAction(state, 100);

            Console.WriteLine(state.ToString());
            Console.WriteLine(abAction);

            state.ToString().Should().Be(initialStateDump, "ai.GetAction should not change state");
            abAction.ToString().Should().Be(greedyAction.ToString(), "ab-ai with depth=1 should be exactly gready ai");
        }

        [TestCase("00000|00000|00000|00000|00000|1 1|2 3|-1 -1|-1 -1|")]
        [TestCase("03330|24143|24341|14422|12121|4 1|4 0|0 4|2 1")]
        [TestCase("4443444|4423444|4044434|0014143|4104424|4402344|4442444|5 4|3 1|1 3|5 2")]
        public void RunCase(string input)
        {
            var state = StateReader.Read(input);
            var ai = new AlphabetaAi(estimator);
            var action = ai.GetAction(state, 45);
            Console.WriteLine(action);
        }

        private IEnumerable<string> GetStatesCollection()
        {
            return new[]
            {
                "00000|00000|00000|00000|00000|1 1|0 2|2 1|3 3|",
                "...0...|..001..|.11201.|1230200|.13200.|..000..|...0...|2 1|1 3|3 4|4 2|",
                "...0...|..201..|.13424.|1242200|.14210.|..000..|...0...|3 0|1 3|3 4|4 1|",
                "...0...|..000..|.00000.|0000000|.00000.|..000..|...0...|2 4|3 1|3 3|3 4|",
                "...1...|..302..|.02022.|0000001|.01402.|..121..|...0...|2 4|4 2|2 1|5 4|",
                "...2...|..303..|.13033.|0010031|.01412.|..121..|...1...|3 5|5 2|2 2|5 4|",
                "...0...|..001..|.00002.|0000220|.00000.|..010..|...0...|3 3|5 4|4 4|3 2|",
                "00..00|000000|000020|000001|000100|.0101.|3 2|3 4|0 1|4 4|",
                "00..10|010110|021240|040133|013320|.0311.|3 1|1 4|3 2|3 4|",
                "01..22|121211|334440|440244|234333|.3333.|4 4|2 3|5 2|2 1|",
                "00000|00000|00000|00000|00000|3 0|4 2|1 1|1 2|",
                "012130|.3003.|02..22|002322|001021|.0000.|4 4|2 0|5 2|2 3|"
            };
        }

        [Test]
        public void PerformanceTest()
        {
            var state = new StateReader("...0...|..000..|.00000.|0000000|.00010.|..000..|...1...|3 5|5 4|3 4|1 1|")
                .ReadState(new InitializationData(7, 2));
            for (var j = 0; j < 10; j++)
            {
                var alphabetaAi = new AlphabetaAi(estimator);
                for (var i = 0; i < 10; i++)
                {
                    var action = alphabetaAi.GetAction(state, 100);
                    action.ApplyTo(state);
                    state.ChangeCurrentPlayer();
                    //Console.WriteLine(state);
                    Console.WriteLine(action);
                }
            }
        }


        [Test]
        public void TreeSizeMeasurement()
        {
            var totalSize = new StatValue();
            foreach (var stateInput in GetStatesCollection())
            {
                var localSize = new StatValue();
                var state = StateReader.Read(stateInput);
                var ai = new AlphabetaAi(estimator, 3) { Logging = false };
                for (var i = 0; i < 1; i++)
                {
                    var action = ai.GetAction(state, 250);
                    localSize.Add(ai.LastSearchTreeSize);
                    action.ApplyTo(state);
                }
                Console.WriteLine(localSize.ToDetailedString());
                totalSize.AddAll(localSize);
            }
            Console.WriteLine("Total:");
            Console.WriteLine(totalSize.ToDetailedString());
        }
    }
}