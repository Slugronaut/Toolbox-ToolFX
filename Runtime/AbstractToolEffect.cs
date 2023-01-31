using System.Collections;
using UnityEngine;

namespace ToolFx
{

    /// <summary>
    /// 
    /// </summary>
    public abstract class AbstractToolEffect : ScriptableObject
    {
        /// <summary>
        /// Registers an instanced variable with the tool using this tool effect plugin.
        /// The string returned can be used to access that instanced variable on the same
        /// tool instance at a later time. This allows tool effect - which are inherintly shared - 
        /// to use per-instance data.
        /// </summary>
        protected string RegisterVar(string variableName)
        {
            return GetType().Name + Id + "-" + variableName;
        }

        static ushort IdInc = 0;
        protected ushort Id { get; private set; }
        protected virtual void OnEnable()
        {
            Id = IdInc;
            IdInc++;
        }

        protected abstract void OnDisable();
        protected abstract void OnDestroy();
        public virtual IEnumerator Routine(ITool tool) { yield break; }
        public virtual void ToolEnabled(ITool tool) { }
        public virtual void ToolDisabled(ITool tool) { }
        public virtual void ToolDestroyed(ITool tool) { }
        public abstract void Use(ITool tool);
        public abstract void EndUse(ITool tool);
        public virtual void CancelUse(ITool tool) { }
        public virtual void UseFailed(ITool tool) { }
        public virtual void ResetUse(ITool tool) { EndUse(tool); }
    }

}
