using BehaviorDesigner.Runtime.Tasks;

namespace MrPink.Units.Behaviours
{
    public abstract class BaseUnitConditional : Conditional
    {
        protected Unit selfUnit;
        
        public override void OnAwake()
        {
            selfUnit = GetComponent<Unit>();
        }
    }
}