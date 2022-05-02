using System;
using BehaviorDesigner.Runtime;

namespace MrPink.Units.Behaviours
{
    [Serializable]
    public class SharedCoverSpot : SharedVariable<CoverSpot>
    {
        public static implicit operator SharedCoverSpot(CoverSpot value) 
            => new SharedCoverSpot { Value = value };
    }
}