using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// Simple container of a list of ToolEffects that should be treated as a single effect.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Effect Collection", menuName = "Assets/Collection Tools/Effect Collection")]
    public class ToolEffectCollection : AbstractToolEffect
    {
        public AbstractToolEffect[] SubEffects;



        public override void EndUse(ITool tool)
        {
            for (int i = 0; i < SubEffects.Length; i++)
            {
                if (tool.HasFailed)
                {
                    UseFailed(tool);
                    break;
                }
                SubEffects[i].EndUse(tool);
            }
        }

        public override void Use(ITool tool)
        {
            for (int i = 0; i < SubEffects.Length; i++)
            {
                if(tool.HasFailed)
                {
                    UseFailed(tool);
                    break;
                }
                SubEffects[i].Use(tool);
            }
        }

        public override void UseFailed(ITool tool)
        {
            for (int i = 0; i < SubEffects.Length; i++)
                SubEffects[i].UseFailed(tool);
        }

        public override void CancelUse(ITool tool)
        {
            for (int i = 0; i < SubEffects.Length; i++)
                SubEffects[i].CancelUse(tool);
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}
