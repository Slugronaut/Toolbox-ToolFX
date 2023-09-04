using System.Collections.Generic;
using Peg;
using UnityEngine;

namespace ToolFx
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractTimerToolEffect : AbstractToolEffect
    {
        [Tooltip("When is this effect triggered? Charging can only occur on Use, but finalized results can occur on EndUse if one needs to know the final charge level.")]
        public Tool.TriggerPoint Trigger;
        [Tooltip("How long in seconds before the callback is invoked.")]
        public float WaitTime;
        [Tooltip("Can the timer be restarted while still counting down?")]
        public bool AllowRestart;

        [Tooltip("If other timers of this type are running on this object, should the be interrupt in favor of this one? Ids can be used to track which ones to interrupt and which ones to leave.")]
        public HashedString InterruptId;


        protected string Started;
        protected string TimerRoutine;
        protected string TimerInstance;
        protected string InterruptList = "InterruptList"; //we don't use RegisterVar for this one because it needs to be shared for the whole GO


        protected override void OnEnable()
        {
            base.OnEnable();
            InterruptId.Value = InterruptId.Value; //due to dumbness with SOs
            Started = RegisterVar("Started");
            TimerRoutine = RegisterVar("TimerRoutine");
        }

        /// <summary>
        /// Helper for stopping any previously started timer on this gameobject.
        /// </summary>
        /// <param name="tool"></param>
        Dictionary<int, Coroutine> CancelInterruptables(ITool tool)
        {
            Dictionary<int, Coroutine> list = null;

            if (InterruptId.Hash != 0)
            {
                list = tool.GetInstVar<Dictionary<int, Coroutine>>(InterruptList);
                if (list != null)
                {
                    if (list.TryGetValue(InterruptId.Hash, out Coroutine co))
                        CancelCoroutine(tool, co);
                }
                else
                {
                    //intialize the list
                    list = new Dictionary<int, Coroutine>();
                    tool.SetInstVar(InterruptList, list);
                }
            }

            return list;
        }

        /// <summary>
        /// Helper for stopping a coroutine and resetting the timer.
        /// </summary>
        /// <param name="tool"></param>
        protected void CancelCoroutine(ITool tool, Coroutine co)
        {
            tool.CancelDelayedInvoke(co);
            EndTime(tool);
        }

        void Process(ITool tool)
        {
            if (AllowRestart) CancelTimer(tool); //doing a check so that we don't pay the cost if we aren't even using it
            var list = CancelInterruptables(tool);
            tool.SetInstVar(Started, true);
            OnStartTimer(tool);
            var co = tool.DelayedInvoke(EndTime, WaitTime);
            tool.SetInstVar(TimerRoutine, co);
            if (InterruptId.Hash != 0)
                list[InterruptId.Hash] = co;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void Use(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnUse)
            {
                if (AllowRestart || !tool.GetInstVar<bool>(Started))
                    Process(tool);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void EndUse(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnEndUse)
            {
                if (AllowRestart || !tool.GetInstVar<bool>(Started))
                    Process(tool);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tool"></param>
        public override void UseFailed(ITool tool)
        {
            if (Trigger == Tool.TriggerPoint.OnFailed)
            {
                if (AllowRestart || !tool.GetInstVar<bool>(Started))
                    Process(tool);
            }
        }

        /// <summary>
        /// This must be duplicated exactly every time because Unity can be fucking stupid sometimes.
        /// Especially when ScriptableObject are involved.
        /// </summary>
        protected virtual void EndTime(ITool tool)
        {
            tool.SetInstVar(Started, false);
            this.OnEndTimer(tool);
            tool.InvokeEffectCallback(this);
        }

        /// <summary>
        /// Cancels a previously started timed effect. It will trigger the EndTimer invoke.
        /// If no timer event was running then nothing will happen.
        /// </summary>
        /// <param name="tool"></param>
        protected void CancelTimer(ITool tool)
        {
            if (tool.GetInstVar<bool>(Started))
            {
                tool.CancelDelayedInvoke(tool.GetInstVar<Coroutine>(TimerRoutine));
                EndTime(tool);
            }

        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }

        public override void ToolDisabled(ITool tool)
        {
        }

        public override void ToolDestroyed(ITool tool)
        {
        }

        protected abstract void OnEndTimer(ITool tool);
        protected abstract void OnStartTimer(ITool tool);
    }

}
