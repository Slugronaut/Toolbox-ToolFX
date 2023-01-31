using UnityEngine;


namespace ToolFx
{
    /// <summary>
    /// Rotates a tool.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "Rotate Tool", menuName = "Assets/Useable Tools/Rotate Tool")]
    public class RotateToolEffect : AbstractCommonToolEffect
    {
        public enum RotateModes
        {
            Absolute,
            Additive
        }

        [Tooltip("The amount to increment rotation angles by, each tick.")]
        public Vector3 RotationInc;



        public override void Process(ITool tool)
        {
            tool.gameObject.transform.Rotate(RotationInc);
        }

        protected override void OnDestroy()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}
