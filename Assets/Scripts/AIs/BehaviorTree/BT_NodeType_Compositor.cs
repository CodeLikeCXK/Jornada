using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompositorNode : BT_Node
{ 
   public List<BT_Node> children = new List<BT_Node>();
}
