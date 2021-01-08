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
            if(Time.realtimeSinceStartup - timeAtBeginning > timeBudget)
            {
                timeAtBeginning = Time.realtimeSinceStartup;
                return true;
            }
          //  Debug.Log(" -> " + Time.realtimeSinceStartup + "  " + timeAtBeginning);
            return false;
        }
    }
}

