using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAgent : MonoBehaviour
{
    public AIStateMachine StateMachine;

    public AIStateID initialStateID;
    // Start is called before the first frame update
    void Start()
    {
        StateMachine = new AIStateMachine(this);
        StateMachine.ChangeState(initialStateID);
    }

    // Update is called once per frame
    void Update()
    {
        StateMachine.Update();
    }
}
