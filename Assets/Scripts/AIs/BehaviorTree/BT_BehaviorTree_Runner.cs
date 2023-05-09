using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BT_BehaviorTree_Runner : MonoBehaviour
{
    BehaviorTree tree;
    // Start is called before the first frame update
    void Start()
    {
        tree = ScriptableObject.CreateInstance<BehaviorTree>();
        
        var log1 = ScriptableObject.CreateInstance<DebugLogNode>();
        log1.message = "BT-7274 is here";
        
        var log2 = ScriptableObject.CreateInstance<DebugLogNode>();
        log2.message = "Adjusting";
        
        var log3 = ScriptableObject.CreateInstance<DebugLogNode>();
        log3.message = "Protocol 3";
        
        var sequence = ScriptableObject.CreateInstance<SequencerNode>();
        sequence.children.Add(log1);
        sequence.children.Add(log2);
        sequence.children.Add(log3);

        var loop = ScriptableObject.CreateInstance<RepeatNode>();
        loop.child = sequence;
        
        tree.rootNode = loop;

    }

    // Update is called once per frame
    void Update()
    {
        tree.Update();
    }
}
