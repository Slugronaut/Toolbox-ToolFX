using Peg;


namespace ToolFx
{
    /// <summary>
    /// Uses a tool upon handling a standard UnityEvent.
    /// </summary>
    public class ToolUseOnEvent : AbstractOperationOnEvent
    {
        public Tool.TriggerPoint Event;
        public Tool[] Tools;

        public override void PerformOp()
        {
            if (Event == Tool.TriggerPoint.OnUse)
            {
                for (int i = 0; i < Tools.Length; i++)
                    Tools[i].Use();
            }
            else
            {
                for (int i = 0; i < Tools.Length; i++)
                    Tools[i].EndUse();
            }
        }
    }
}
