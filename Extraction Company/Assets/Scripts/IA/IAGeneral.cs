using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IAGeneral : MonoBehaviour
{
    int distanceWandering; //The area from where the point will be choosen.
    int precisionWandering;

    //State Machine BS
    public State currentState;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RunStateMachine();
    }

    private void RunStateMachine() 
    {
        State nextState = currentState?.RunCurrentState(); //If variable is != null run, else ignore

        if(nextState != null) 
        {
            SwitchToNextState(nextState);
        }
    }

    private void SwitchToNextState(State nextState) 
    {
        currentState = nextState;
    }
}
