using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CG.WondevWoman
{
    public class Estimator : IEstimator
    {
        public ExplainedScore Estimate(State state, int playerIndex)
        {
#if DEV
            var voronoi = new VoronoiDiagram(state);
            var myScore = EstimateForPlayer(state, voronoi, playerIndex);
            var hisScore = EstimateForPlayer(state, voronoi, 1 - playerIndex);
            return new ExplainedScore(
                myScore.Value - hisScore.Value, myScore.Explanation + " vs " + hisScore.Explanation);
#else
            throw new NotImplementedException();
#endif
        }

#if DEV
        private ExplainedScore EstimateForPlayer(State state, VoronoiDiagram voronoi, int playerIndex)
        {
            
            var value = voronoi.AreaOfPlayer(playerIndex);
            return new ExplainedScore(value, value.ToString(CultureInfo.InvariantCulture));
        }
#endif
    }
}