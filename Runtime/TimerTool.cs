using UnityEngine;


namespace ToolFx
{ 
    /// <summary>
    /// Simple Tool Effect that waits for a period of time before invoking the effect callback on the tool.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Weapon Timer", menuName = "Assets/Useable Tools/Timer Tool Fx")]
    public class TimerTool : AbstractTimerToolEffect
    {
        protected override void OnStartTimer(ITool tool)
        {
        }

        protected override void OnEndTimer(ITool tool)
        {
        }

    }
    
}
