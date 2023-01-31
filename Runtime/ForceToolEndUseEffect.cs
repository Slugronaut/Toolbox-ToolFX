
using System;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// Forces this tool to run EndUse().
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Force Tool End Use", menuName = "Assets/Useable Tools/Force Tool End Use")]
    public class ForceToolEndUseEffect : AbstractCommonToolEffect
    {
        public override void Process(ITool tool)
        {
            tool.EndUse();
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}
