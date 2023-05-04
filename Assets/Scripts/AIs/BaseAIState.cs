using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIStateID
{
   //more states to be added
   //states detail would be written in C# files
}



public interface AIState
{
   AIStateID GetID();
   void Enter(AIAgent agent);
   void Update(AIAgent agent);
   void Exit(AIAgent agent);
}
