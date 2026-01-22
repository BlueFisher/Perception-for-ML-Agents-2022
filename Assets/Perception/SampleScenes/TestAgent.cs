using UnityEngine;

using Unity.MLAgents;
using Unity.MLAgents.Actuators;

namespace Perception
{
    public class TestAgent : Agent
    {
        public override void OnEpisodeBegin()
        {
            transform.position = new Vector3(0, 0, -10);
        }

        float m_Move = 0.2f;

        public override void OnActionReceived(ActionBuffers actions)
        {
            if (transform.position.x > 1f)
            {
                m_Move = -Mathf.Abs(m_Move);
            }
            else if (transform.position.x < -1f)
            {
                m_Move = Mathf.Abs(m_Move);
            }
            transform.position += new Vector3(m_Move, 0, 0);
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
        }
    }
}
