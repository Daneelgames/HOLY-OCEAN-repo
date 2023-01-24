using UnityEngine;

namespace Kamgam.TriplanarShader
{
    public class DemoCamera : MonoBehaviour
    {
        public int TargetIndex;
        public Transform[] Targets;

        public void Update()
        {
            var pos = transform.position;
            var targetX = Targets[TargetIndex].position.x;
            if (Mathf.Abs(pos.x - targetX) > 0.001f)
            {
                pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * 5f);
            }
            else
            {
                pos.x = targetX;
            }

            transform.position = pos;
        }

        public void Previous()
        {
            TargetIndex--;
            TargetIndex = TargetIndex % Targets.Length;
        }

        public void Next()
        {
            TargetIndex++;
            TargetIndex = TargetIndex % Targets.Length;
        }
    }
}