using System.Threading.Tasks;
using UnityEngine;

namespace GLTFast
{

    public class MaxTimePerFrameDeferAgent : IDeferAgent
    {
        float timeBudget;

        /// <summary>
        /// Defers work to the next frame if a fix time budget is
        /// used up.
        /// </summary>
        /// <param name="frameBudget">Time budget as part of the target frame rate.</param>
        public MaxTimePerFrameDeferAgent(float maxTime = 0.5f)
        {
            timeBudget = maxTime;
            timeAtBeginning = Time.realtimeSinceStartup;
        }


        private float timeAtBeginning = 0;
        public bool ShouldDefer()
        {
            if (Time.realtimeSinceStartup - timeAtBeginning > timeBudget)
            {
                timeAtBeginning = Time.realtimeSinceStartup;
                return true;
            }
          //  Debug.Log(" -> " + Time.realtimeSinceStartup + "  " + timeAtBeginning);
            return false;
        }

        public bool ShouldDefer(float duration)
        {
            if (Time.realtimeSinceStartup - timeAtBeginning > duration)
            {
                timeAtBeginning = Time.realtimeSinceStartup;
                return true;
            }
            //  Debug.Log(" -> " + Time.realtimeSinceStartup + "  " + timeAtBeginning);
            return false;
        }

#pragma warning disable 1998
        /// <inheritdoc />
        public async Task BreakPoint() { }
        /// <inheritdoc />
        public async Task BreakPoint(float duration) { }
#pragma warning restore 1998
    }
}

