using UnityEngine;

namespace ToolFx
{

    /// <summary>
    /// Implements a simple check for when the tool is triggered and sends the
    /// signla to the apprpriate handler in the implemnting class.
    /// </summary>
    public abstract class AbstractCommonToolEffect : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void Use(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnUse)
                Process(tool);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void EndUse(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnEndUse)
                Process(tool);
        }

        public override void UseFailed(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnFailed)
                Process(tool);
        }

        public abstract void Process(ITool tool);
    }

}