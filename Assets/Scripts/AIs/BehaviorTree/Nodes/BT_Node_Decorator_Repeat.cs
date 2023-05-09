using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RepeatNode : DecoratorNode
{
    protected override void OnStart()
    {
    }
    
    protected override void OnStop()
    {

    }
    
    protected override State OnUpdate()
    {
        child.Update();
        return State.Running;
    }
}
