using System;
using BehaviorDesigner.Runtime;
using MrPink.Health;

namespace MrPink.Units.Behaviours
{
    [Serializable]
    public class SharedHealthController : SharedVariable<HealthController>
    {
        public static implicit operator SharedHealthController(HealthController value) 
            => new SharedHealthController { Value = value };
    }
}