using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SequencerNode : CompositorNode
{
    int current;
    
    protected override void OnStart()
    {
        current = 0;
    }
    
    protected override void OnStop()
    {

    }
    
    protected override State OnUpdate()
    {
        var child = children[current];
        switch (child.Update())
        {
            case State.Running:
                return State.Running;
            case State.Failure:
                return State.Failure;
            case State.Success:
                current++;
                break;
        }
        //judge if the current state reach the end of the list? Yes-Success, No-Keep running
        return current == children.Count ? State.Success : State.Running;
    }
}
