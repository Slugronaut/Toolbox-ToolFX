using NodeCanvas.Framework;
using System;
using Peg;
using UnityEngine;
using static ToolFx.Tool;
using UnityEngine.Events;

namespace ToolFx
{

    /// <summary>
    /// 
    /// </summary>
    public interface ITool
    {
        float EffectDelay { get; set; }
        LockModes LockMode { get; set; }
        AimOffsetModes AimMode { get; set; }
        Vector3 AimOffset { get; set; }
        bool MirrorLocalOffset { get; set; }
        float ReuseDelay { get; set; }
        float SemiReuseDelay { get; set; }
        float EffectiveReuseDelay { get; }
        AbstractToolEffect[] UseEffects { get; set; }
        GameObject gameObject { get; }
        GameObject CurrentTarget { get; set; }
        EntityRoot Owner { get; }
        Blackboard Blackboard { get; }

        bool InUse { get; }
        bool IsLocked { get; }
        bool DisableSelfLocking { get; set; }
        bool DisableSelfTiming { get; set; }
        float LastUseTime { get; set; }
        bool Locked { get; set; }
        void CancelToolEffects();
        bool HasFailed { get; }

        void Use();
        void EndUse();
        void ResetUse();
        void LockReset();
        void InitializeEffects();

        Variable<T> GetBlackboardVar<T>(string name);
        Variable<T> GetBlackboardVar<T>(string name, T defaultValue);
        T GetInstVar<T>(string name);
        void SetInstVar<T>(string name, T val);
        void InvokeEffectCallback(AbstractToolEffect effect);
        Coroutine DelayedInvoke(Action<ITool> func, float time);
        void CancelDelayedInvoke(Coroutine routine);
        Coroutine StartToolEffectCoroutine(AbstractToolEffect effect);
        void StopToolEffectCoroutine(AbstractToolEffect effect, Coroutine coroutine);
        IToolActionDependency[] ActionDependencies { get; }

        void InvokeOnCanUse(UnityAction<ITool> action);
        void InvokeOnUseFailed();
        void InvokeOnUse();
        void InvokeOnLocked();

    }

}
