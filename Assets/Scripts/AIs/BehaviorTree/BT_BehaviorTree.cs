using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BehaviorTree : ScriptableObject
{
    public BT_Node rootNode;
    public BT_Node.State treeState = BT_Node.State.Running;
    
    public BT_Node.State Update()
    {
        return rootNode.Update();
    }
}
