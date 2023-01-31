using Sirenix.OdinInspector;
using UnityEngine;


namespace ToolFx
{
	/// <summary>
    /// 
    /// </summary>
    public abstract class AbstractChargedToolEffect : AbstractToolEffect
    {
        public enum RoundingMethods
        {
            Floor,
            Ceil,
            Round,
        }

        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
		
        [ShowIf("Trigger", Tool.TriggerPoint.OnEndUse)]
        [Indent]
        [Tooltip("If the trigger point is set to OnEndUse, do we need to first call OnUse for it to trigger the EndUse?")]
        public bool RequireUseBeforeEnd = true;

        [Tooltip("When converting the curve to an array index, what method should be used for rounding?")]
        public RoundingMethods RoundingMethod;

        [Tooltip("How charge levels scale over time")]
        public AnimationCurve ChargeScale;

        protected string Using;
        protected string StartTime;




        /// <summary>
        /// 
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            Using = RegisterVar("Using");
            StartTime = RegisterVar("StartTime");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        protected bool CanEndUse(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnEndUse && (!RequireUseBeforeEnd || tool.GetInstVar<bool>(Using)))
            {
                tool.SetInstVar(Using, false);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns an rounded int based on the charge time along the porivded curve. This is intended to be used
        /// as an array lookup in most cases.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        protected int CurrentLevel(ITool tool)
        {
            if(RoundingMethod == RoundingMethods.Floor)
                return Mathf.FloorToInt(ChargeScale.Evaluate(Time.time - tool.GetInstVar<float>(StartTime)));
            else if(RoundingMethod == RoundingMethods.Ceil)
                return Mathf.CeilToInt(ChargeScale.Evaluate(Time.time - tool.GetInstVar<float>(StartTime)));
            else return Mathf.RoundToInt(ChargeScale.Evaluate(Time.time - tool.GetInstVar<float>(StartTime)));
        }

        /// <summary>
        /// Must be invoked by base class before any other operation.
        /// </summary>
        /// <param name="tool"></param>
        public override void Use(ITool tool)
        {
            if (tool.GetInstVar<float>(StartTime) == 0.0f)
                tool.SetInstVar(StartTime, Time.time);

            if (Trigger == Tool.TriggerPoint.OnEndUse && RequireUseBeforeEnd)
                tool.SetInstVar(Using, true);
        }

        /// <summary>
        /// Must be invoked by the base class after all other operations.
        /// </summary>
        /// <param name="tool"></param>
        public override void EndUse(ITool tool)
        {
            tool.SetInstVar(StartTime, 0.0f);
        }

        /// <summary>
        /// Resets the internal use timer
        /// </summary>
        /// <param name="tool"></param>
        public override void CancelUse(ITool tool)
        {
            tool.SetInstVar(StartTime, 0.0f);
            tool.SetInstVar(Using, false);
        }
    }
}