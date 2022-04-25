using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    public abstract class BaseUnitAction : Action
    {
        protected Unit selfUnit;
        
        public override void OnAwake()
        {
            selfUnit = GetComponent<Unit>();
        }
    }
}