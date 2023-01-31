using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ToolFx
{

    /// <summary>
    /// Any behaviour that exposes this and is attached to the same object as a tool will provide
    /// additional dependancies that tool must check before being useable.
    /// </summary>
    public interface IToolActionDependency
    {
        bool CanUse(ITool tool);
    }

}
