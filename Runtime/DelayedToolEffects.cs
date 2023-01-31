using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// After a delay period, activates all the the effects in the list.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Delayed Tool Fx", menuName = "Assets/Collection Tools/Delayed Tool Fx")]
    public class DelayedToolEffects : AbstractTimerToolEffect
    {
        [Tooltip("Are we triggering the OnUse effects or the OnEndUse effects?")]
        public Tool.TriggerPoint FxTrigger;

        public AbstractToolEffect[] Fx;


        protected override void OnStartTimer(ITool tool)
        {
        }

        protected override void OnEndTimer(ITool tool)
        {
            if(FxTrigger == Tool.TriggerPoint.OnUse)
            {
                for (int i = 0; i < Fx.Length; i++)
                    Fx[i].Use(tool);
            }
            else
            {
                for (int i = 0; i < Fx.Length; i++)
                    Fx[i].EndUse(tool);
            }
        }

        public override void CancelUse(ITool tool)
        {
            for (int i = 0; i < Fx.Length; i++)
                Fx[i].UseFailed(tool);
        }

    }

}
