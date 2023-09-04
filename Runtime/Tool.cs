using System;
using System.Collections;
using Peg;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

using NodeCanvas.Framework;
using Sirenix.OdinInspector;

namespace ToolFx
{
    /// <summary>
    /// A kind of interface for using gameobject heirachies useable tools (weapons, lockpick, healing spell, etc...)
    /// </summary>
    [RequireComponent(typeof(Blackboard))]
    public sealed class Tool : MonoBehaviour, ITool
    {
        #region Inner Classes
        [Serializable]
        public sealed class ToolEvent : UnityEvent<Tool> { }

        [Serializable]
        public sealed class ToolEffectEvent : UnityEvent<AbstractToolEffect> { }

        [Serializable]
        public class ToolCanEvent : UnityEvent<UnityAction<ITool>, Tool> { }
        
        [Serializable]
        public class ToolCanDeferredEvent : UnityEvent<UnityAction<ITool>, Tool> { }

        public enum TriggerPoint
        {
            OnUse,
            OnEndUse,
            OnFailed,
        }

        public enum TriggeredEvent
        {
            OnUse       = 1 << 0,
            OnEndUse    = 1 << 1,
            Matched     = OnUse | OnEndUse,
        }

        public enum LockModes
        {
            Manual,
            SemiAuto,
            FullAuto,
            Toggle, //can use again until EndUse has been called
        }
        #endregion

        [SerializeField]
        [HideInInspector]
        EntityRoot _Owner;
        [PropertyTooltip("The entity that currently 'owns' this tool.")]
        [ShowInInspector]
        public EntityRoot Owner { get { return _Owner; } set { _Owner = value; } }

        [PropertyTooltip("A link to the current target of this tool if any. Not all tools require a target.")]
        [ReadOnly]
        public GameObject CurrentTarget { get; set; }

        [SerializeField]
        float _EffectDelay = 0;
        [PropertyTooltip("How long after using this tool before its effects are played.")]
        public float EffectDelay { get => _EffectDelay; set => _EffectDelay = value; }

        [HideInInspector]
        [SerializeField]
        AimOffsetModes _AimMode;
        [PropertyTooltip("The manner in which the AimOffset is applied.")]
        [ShowInInspector]
        public AimOffsetModes AimMode { get { return _AimMode; } set { _AimMode = value; } }


        [HideInInspector]
        [SerializeField]
        [FormerlySerializedAs("_LocalSpaceOffset")]
        Vector3 _AimOffset;
        [PropertyTooltip("Aim offset that this tool applies effects when used. This is just a simple local-space offset that is applied just before use. It takes MirrorLocalOffset into account.")]
        [ShowInInspector]
        public Vector3 AimOffset
        {
            get
            {
                if(_MirrorLocalOffset && _AimMode == AimOffsetModes.Bilateral)
                    return new Vector3(-_AimOffset.x, _AimOffset.y, _AimOffset.z);
                else return _AimOffset;
            }
            set { _AimOffset = value; }
        }

        [HideInInspector]
        [SerializeField]
        bool _MirrorLocalOffset;
        [PropertyTooltip("Should the local offset be mirrored on the y-axis (similar to sprite mirroring)? Only works when AimMode is set to Bilateral.")]
        [ShowInInspector]
        [Indent]
        [ShowIf("CanMirrorAimMode")]
        public bool MirrorLocalOffset { get { return _MirrorLocalOffset; } set { _MirrorLocalOffset = value; } }


        [HideInInspector]
        [SerializeField]
        LockModes _LockMode;
        [PropertyOrder(1)]
        [PropertyTooltip("Is this tool 'auto fire' or does it require the user to manually call LockReset() before it can be used again?")]
        [ShowInInspector]
        //public bool AutoLockReset;
        public LockModes LockMode { get { return _LockMode; } set { _LockMode = value; } }

        [HideInInspector]
        [SerializeField]
        public float _SemiReuseDelay;
        [PropertyTooltip("Semi-auto delay between uses. Used when the system should have different delays between auto-use and manual use but needs to support both.")]
        [ShowInInspector]
        [ShowIf("IsSemi")]
        [Indent]
        [PropertyOrder(2)]
        public float SemiReuseDelay
        {
            get { return _SemiReuseDelay; }
            set { _SemiReuseDelay = value; }
        }

        [HideInInspector]
        [SerializeField]
        float _ReuseDelay;
        [PropertyTooltip("Delay between uses. When in Semi-automatic mode, this will control the automatic reuse delay.")]
        [ShowInInspector]
        [Indent]
        [PropertyOrder(2)]
        public float ReuseDelay
        {
            get { return _ReuseDelay; }
            set { _ReuseDelay = value; }
        }

        [HideInInspector]
        [SerializeField]
        AbstractToolEffect[] _UseEffects;
        [PropertyOrder(10)]
        [PropertyTooltip("The ScriptableObject backend that provides tool behaviour implementation.")]
        [ShowInInspector]
        public AbstractToolEffect[] UseEffects { get { return _UseEffects; } set { _UseEffects = value; } }

        [HideInInspector]
        [SerializeField]
        AbstractToolEffect[] _InitEffects;
        [PropertyOrder(10)]
        [PropertyTooltip("The ScriptableObject backend that provides tool behaviour implementation.")]
        [ShowInInspector]
        public AbstractToolEffect[] InitEffects { get { return _InitEffects; } set { _InitEffects = value; } }


        [FoldoutGroup("Events", 11)]
        [Tooltip("Invoked just before use.")]
        public ToolCanEvent OnCanUse;

        [FoldoutGroup("Events", 11)]
        [Tooltip("Invoked on use.")]
        public ToolEvent OnUse;

        [FoldoutGroup("Events", 11)]
        [Tooltip("Invoked on end of use.")]
        public ToolEvent OnEndUse;

        [FoldoutGroup("Events", 11)]
        [Tooltip("Invoked when the tool cannot be used for some reason. Not triggered when simply locked.")]
        public ToolEvent OnUseFailed;

        [FoldoutGroup("Events", 11)]
        [Tooltip("Invoked when the tool cannot be used because it is locked.")]
        public ToolEvent OnLocked;

        [FoldoutGroup("Events", 11)]
        [Tooltip("Invoked when the tool's use was canceled.")]
        public ToolEvent OnUseCancelled;

        [FoldoutGroup("Events", 11)]
        [Tooltip("Can be invoked by a tool effect to let the system know something happened with that effect.")]
        public ToolEffectEvent OnEffectCallback;


        /// <summary>
        /// Used by the inspector to determine viewability of other properties.
        /// </summary>
        public bool CanMirrorAimMode => AimMode == AimOffsetModes.Bilateral;

        /// <summary>
        /// Used by ToolOverrideEffects to allow them to manually control this tool's lock state.
        /// </summary>
        public bool DisableSelfLocking { get; set; }

        /// <summary>
        /// Used by ToolOverrideEffects to allow them to manually control this tool's timing state.
        /// </summary>
        public bool DisableSelfTiming { get; set; }

        bool Failed;
        public bool HasFailed => Failed;

        /// <summary>
        /// Takes the internal locked state and semi vs full auto modes into account when return the delay time.
        /// </summary>
        public float EffectiveReuseDelay => Locked ? _SemiReuseDelay : _ReuseDelay;

        Blackboard _Blackboard;
        public Blackboard Blackboard
        {
            get
            {
                if (_Blackboard == null)
                {
                    _Blackboard = GetComponent<Blackboard>();
                    if (_Blackboard == null)
                        _Blackboard = gameObject.AddComponent<Blackboard>();
                }
                return _Blackboard;
            }
        }

        public bool InUse => LockMode == LockModes.Toggle ? Locked : Time.time - LastUseTime < EffectiveReuseDelay; 


        /// <summary>
        /// Used internally by the inspector.
        /// </summary>
        /// <returns></returns>
        bool IsSemi => LockMode == LockModes.SemiAuto;

        public bool IsLocked => Locked;

        public bool Locked { get; set; }

        public float LastUseTime { get; set; }

        IToolActionDependency[] ActionDeps;
        public IToolActionDependency[] ActionDependencies => ActionDeps;



        /// <summary>
        /// Mostly here just to allow easier connection to UnityEvents.
        /// </summary>
        /// <param name="flip"></param>
        public void SetMirroredLocalOffset(bool flip)
        {
            _MirrorLocalOffset = flip;
        }

        /// <summary>
        /// Can be manually invoked by Tool Effects to let the tool know something happened.
        /// </summary>
        /// <param name="effect"></param>
        public void InvokeEffectCallback(AbstractToolEffect effect)
        {
            OnEffectCallback.Invoke(effect);
        }
        
        /// <summary>
        /// Invokes the method methodName in time seconds.
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="time"></param>
        public Coroutine DelayedInvoke(Action<ITool> func, float time)
        {
            if (func != null)
                return StartCoroutine(DelayedInvokeHandler(func, time));
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CancelDelayedInvoke(Coroutine routine)
        {
            StopCoroutine(routine);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        IEnumerator DelayedInvokeHandler(Action<ITool> action, float time)
        {
            if (time > 0)
                yield return CoroutineWaitFactory.RequestWait(time);

            action(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routine"></param>
        /// <returns></returns>
        public Coroutine StartToolEffectCoroutine(AbstractToolEffect effect)
        {
            return StartCoroutine(effect.Routine(this));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="effect"></param>
        /// <param name="coroutine"></param>
        public void StopToolEffectCoroutine(AbstractToolEffect effect, Coroutine coroutine)
        {
            if(coroutine != null)
                StopCoroutine(coroutine);
        }

        public void InvokeOnCanUse(UnityAction<ITool> action)
        {
            OnCanUse.Invoke(action, this);
        }

        public void InvokeOnUseFailed()
        {
            OnUseFailed.Invoke(this);
        }

        public void InvokeOnUse()
        {
            OnUse.Invoke(this);
        }

        public void InvokeOnLocked()
        {
            OnLocked.Invoke(this);
        }

        void OnTransformParentChanged()
        {
            var parent = transform.parent;
            if (parent != null)
            {

            }
            else Owner = null;
        }
        
        void Awake()
        {
            ActionDeps = GetComponents<IToolActionDependency>();
        }

        void OnDestroy()
        {
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if (_UseEffects != null)
                {
                    _UseEffects[i].ToolDisabled(this);
                    _UseEffects[i].ToolDestroyed(this);
                }
            }
            Owner = null; //just in case :p

        }

        /// <summary>
        /// Can be used to invoke the Start() method on all tool effects.
        /// It is up to the caller to ensure this is called at the right time
        /// and the correct number of times.
        /// </summary>
        public void InitializeEffects()
        {
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if(_InitEffects[i] != null)
                    _InitEffects[i].Use(this);
            }
        }


        /// <summary>
        /// Can be used to invoke the Start() method on all tool effects.
        /// It is up to the caller to ensure this is called at the right time
        /// and the correct number of times.
        /// </summary>
        public void CleanupEffects()
        {
            Failed = false;
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if(_InitEffects[i] != null)
                    _InitEffects[i].EndUse(this);
            }
        }


        bool FxEnable;
        public void ManuallyEnableToolEffects()
        {
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if(_UseEffects[i] != null)
                    _UseEffects[i].ToolEnabled(this);
            }
            FxEnable = true;
        }
        
        void OnEnable()
        {
            if(!FxEnable)
             ManuallyEnableToolEffects();
        }
        
        void OnDisable()
        {
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if(_UseEffects[i] != null)
                    _UseEffects[i].ToolDisabled(this);
            }
            FxEnable = false;
        }

        /// <summary>
        /// Gets a variable from a blackboard on the tool.
        /// Ensures that a blackboard and the named variable exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tool"></param>
        /// <returns></returns>
        public Variable<T> GetBlackboardVar<T>(string name)
        {
            var bb = Blackboard;
            var v = bb.GetVariable<T>(name);
            if (v == null)
            {
                bb.AddVariable(name, typeof(T));
                v = bb.GetVariable<T>(name);
            }

            return v;
        }

        /// <summary>
        /// Gets a variable from a blackboard on the tool.
        /// Ensures that a blackboard and the named variable exists.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="tool"></param>
        /// <returns></returns>
        public Variable<T> GetBlackboardVar<T>(string name, T defaultValue)
        {
            var bb = Blackboard;
            var v = bb.GetVariable<T>(name);
            if (v == null)
            {
                bb.AddVariable(name, typeof(T));
                v = bb.GetVariable<T>(name);
                v.value = defaultValue;
            }

            return v;
        }

        public void SetInstVar<T>(string name, T val)
        {
            var v = GetBlackboardVar<T>(name);
            v.value = val;
        }

        public T GetInstVar<T>(string name)
        {
            var v = GetBlackboardVar<T>(name);
            return v.value;
        }

        /// <summary>
        /// Similar to EndUse in that it unlocks and resets the tool, however, this version doesn't trigger any effects.
        /// </summary>
        public void CancelUse()
        {
            if (!DisableSelfLocking && LockMode == LockModes.Toggle)
                Locked = false;
            OnUseCancelled.Invoke(this);
            ProcessCancelEffects();
        }

        /// <summary>
        /// 
        /// </summary>
        public void Use()
        {
            //This is to avoid race conditions where AIs were trying to use tool before they were fully initialized
            if (Owner == null)
                return;

            if (LockMode == LockModes.Toggle && Locked)
                return;

            //only lock out if we are in manual - if in semi-auto we'll adjust the resuse delay
            if(LockMode == LockModes.Manual && Locked)
            {
                OnLocked.Invoke(this);
                return;
            }

            if (!DisableSelfLocking)
            {
                float t = Time.time;
                if (t - LastUseTime < EffectiveReuseDelay) //ReuseDelay might give different times depending on lock mode (auto vs semi)
                    return; //doesn't count as failure since it is normal
                LastUseTime = t;
            }

            Failed = false;
            if (ActionDeps != null)
            {
                for (int i = 0; i < ActionDeps.Length; i++)
                {
                    if (!ActionDeps[i].CanUse(this))
                    {
                        Failed = true;
                        break;
                    }
                }
            }
            OnCanUse.Invoke(OnFailure, this);

            if (Failed)
            {
                OnUseFailed.Invoke(this);
                ProcessUseFailedEffects();
            }
            else
            {
                OnUse.Invoke(this);
                if (EffectDelay > 0)
                    Invoke("ProcessUseEffects", EffectDelay);
                else ProcessUseEffects();
            }

            if (!DisableSelfLocking && LockMode != LockModes.FullAuto)
                Locked = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public void EndUse()
        {
            if (!DisableSelfLocking && LockMode == LockModes.Toggle)
                Locked = false;

            Failed = false;
            OnEndUse.Invoke(this);
            ProcessEndUseEffects();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ResetUse()
        {
            Failed = false;
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
                _UseEffects[i].ResetUse(this);
        }

        /// <summary>
        /// Can be used by ToolEffects to stop subsequent effects from being processed.
        /// Only works for OnUse and OnEndUse.
        /// </summary>
        public void CancelToolEffects()
        {
            Failed = true;
        }
        
        /// <summary>
        /// 
        /// </summary>
        void ProcessUseEffects()
        {
            Failed = false;
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if (Failed)
                {
                    ProcessUseFailedEffects();
                    break;
                }
                _UseEffects[i].Use(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ProcessEndUseEffects()
        {
            Failed = false;
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if (Failed)
                {
                    ProcessUseFailedEffects();
                    break;
                }
                _UseEffects[i].EndUse(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ProcessCancelEffects()
        {
            Failed = false;
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
            {
                if (Failed)
                {
                    ProcessUseFailedEffects();
                    break;
                }
                _UseEffects[i].CancelUse(this);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void ProcessUseFailedEffects()
        {
            Failed = false;
            int len = _UseEffects.Length;
            for (int i = 0; i < len; i++)
                _UseEffects[i].UseFailed(this);
        }

        /// <summary>
        /// Resets the internal locked use state.
        /// </summary>
        public void LockReset()
        {
            Locked = false;
        }

        void OnFailure(ITool tool)
        {
            Failed = true;
        }
    }

}
