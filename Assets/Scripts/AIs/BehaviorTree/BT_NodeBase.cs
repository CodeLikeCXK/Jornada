using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BT_Node : ScriptableObject
{
   public enum State
   {
      Running,
      Failure,
      Success
   }

   public State state = State.Running;
   public bool IsStarted = false;

   public State Update()
   {
      if (!IsStarted)
      {
         OnStart();
         IsStarted = true;
      }

      state = OnUpdate();

      if (state == State.Failure || state == State.Success)
      {
         OnStop();
         IsStarted = false;
      }
      //next is to create node type

      return state;
   }

   protected abstract void OnStart();
   protected abstract void OnStop();
   protected abstract State OnUpdate();
}
