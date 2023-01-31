using Sirenix.OdinInspector;
using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// 
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Charged Effect Collection", menuName = "Assets/Collection Tools/Effect Collection - Charged")]
    public class ChargedToolEffectCollection : AbstractChargedToolEffect
    {
        [Tooltip("An array of effect collection assets. The charge level will be used as an index lookup into this array to see which collection should be executed.")]
        public ToolEffectCollection[] SubEffects;


        public override void Use(ITool tool)
        {
            base.Use(tool);
            if (Trigger == Tool.TriggerPoint.OnUse)
                SubEffects[CurrentLevel(tool)].Use(tool);
        }

        public override void EndUse(ITool tool)
        {
            if ( CanEndUse(tool) )
                SubEffects[CurrentLevel(tool)].Use(tool);
            base.EndUse(tool);
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }


    
}
