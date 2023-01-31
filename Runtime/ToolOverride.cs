using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace ToolFx
{
    /// <summary>
    /// Overrides the settings of a Tool MonoBehaviour.
    /// Useful if we want to have a single component represent
    /// a skill or weapon and simply configure it using
    /// a ToolEffect plugin rather than having to add/remove
    /// new gameobjects and components.
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "Tool Override", menuName = "Assets/Collection Tools/Tool Override")]
    public class ToolOverride : AbstractToolEffect
    {
        public float ReuseDelay = 0;
        [Tooltip("Semi-auto delay between uses. Used when the system needs different should have different delays between auto-use and manual use but needs to support both.")]
        [ShowIf("IsSemi")]
        [SerializeField]
        public float SemiReuseDelay;
        public Tool.LockModes LockMode;
        public AimOffsetModes AimMode;
        [PropertyTooltip("Should the local offset be mirrored on the y-axis (similar to sprite mirroring)? Only works when AimMode is set to Bilateral.")]
        [ShowIf("CanMirrorAimMode")]
        public bool MirrorLocalOffset;
        public Vector3 AimOffset;
        public AbstractToolEffect[] _UseEffects;




        /// <summary>
        /// Used by the inspector to determine viewability of other properties.
        /// </summary>
        bool CanMirrorAimMode
        {
            get { return AimMode == AimOffsetModes.Bilateral; }
        }

        /// <summary>
        /// Takes the internal locked state and semi vs full auto modes into account when return the delay time.
        /// </summary>
        public float EffectiveReuseDelay(ITool tool)
        {
            return tool.IsLocked ? SemiReuseDelay : ReuseDelay;
        }

        /// <summary>
        /// Used internall by the inspector.
        /// </summary>
        /// <returns></returns>
        bool IsSemi()
        {
            return LockMode == Tool.LockModes.SemiAuto;
        }

        /// <summary>
        /// Helper for checking overriden locked state of this tool.
        /// </summary>
        /// <param name="tool"></param>
        /// <returns></returns>
        protected bool CheckLockState(ITool tool)
        {
            //This is to avoid race conditions where AIs were trying to use tool before they were fully initialized
            if (tool.Owner == null)
                return false;

            if (LockMode == Tool.LockModes.Toggle && tool.Locked)
                return false;

            //only lock out if we are in manual - if in semi-auto we'll adjust the resuse delay
            if (LockMode == Tool.LockModes.Manual && tool.Locked)
            {
                tool.InvokeOnLocked();
                return false;
            }


            float t = Time.time;
            if (t - tool.LastUseTime < EffectiveReuseDelay(tool)) //ReuseDelay might give different times depending on lock mode (auto vs semi)
                return false; //doesn't count as failure since it is normal, don't lock
            tool.LastUseTime = t;
            return true;
        }

        public override void Use(ITool tool)
        {
            if (!CheckLockState(tool))
                return;

            ProcessUseEffects(tool);

            if (LockMode != Tool.LockModes.FullAuto)
                tool.Locked = true;
        }

        public override void EndUse(ITool tool)
        {
            if (LockMode == Tool.LockModes.Toggle)
                tool.Locked = false;

            ProcessEndUseEffects(tool);
        }

        public override void UseFailed(ITool tool)
        {
            base.UseFailed(tool);
            ProcessUseFailedEffects(tool);
        }

        void ProcessEndUseEffects(ITool tool)
        {
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if (tool.HasFailed) break;
                _UseEffects[i].EndUse(tool);
            }
        }

        void ProcessUseEffects(ITool tool)
        {
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if (tool.HasFailed) break;
                _UseEffects[i].Use(tool);
            }
        }

        void ProcessUseFailedEffects(ITool tool)
        {
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
                _UseEffects[i].UseFailed(tool);
        }
        
        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }

        protected override void OnEnable()
        {
        }

        public override void ToolEnabled(ITool tool)
        {
            base.ToolEnabled(tool);
            tool.AimMode = AimMode;
            tool.LockMode = Tool.LockModes.FullAuto;
            tool.ReuseDelay = 0;
            tool.AimOffset = AimOffset;
            tool.DisableSelfLocking = true;
            tool.DisableSelfTiming = true;
        }
    }
}
