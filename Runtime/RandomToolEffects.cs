using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// Randomly uses one of the tool effects in the provided list.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Random Tool Fx", menuName = "Assets/Collection Tools/Random Tool Fx")]
    public class RandomToolEffects : AbstractToolEffect
    {
        [Tooltip("Are we triggering the OnUse effects or the OnEndUse effects?")]
        public Tool.TriggerPoint FxTrigger;
        [Tooltip("Which event is being triggered on the sub-effect that is chosen?")]
        public Tool.TriggerPoint SubTrigger;

        public AbstractToolEffect[] Fx;

        void Process(ITool tool)
        {
            var fx = Fx[Random.Range(0, Fx.Length)];
            if(SubTrigger == Tool.TriggerPoint.OnUse)
                fx.Use(tool);
            else fx.EndUse(tool);
        }

        public override void EndUse(ITool tool)
        {
            if (FxTrigger == Tool.TriggerPoint.OnEndUse)
                Process(tool);
        }

        public override void Use(ITool tool)
        {
            if (FxTrigger == Tool.TriggerPoint.OnUse)
                Process(tool);
        }

        public override void CancelUse(ITool tool)
        {
            for (int i = 0; i < Fx.Length; i++)
                Fx[i].UseFailed(tool);
        }

        protected override void OnDestroy()
        {
            Fx = null;
        }

        protected override void OnDisable()
        {
        }
    }

}
