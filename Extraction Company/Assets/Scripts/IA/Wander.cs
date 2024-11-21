using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WanderState : State
{
    public Queue<GameObject> checkedPositons;
    public int maxSizeQueue; //Once we cheked enogh positions we start eliminating them from the queue

    public int playerDetectionArea;

    public ChaseState chaseState;
    public override State RunCurrentState()
    {
        if (true) 
        {
            return chaseState;
        }
        else 
        {
            return this;
        }

        
    }
}
