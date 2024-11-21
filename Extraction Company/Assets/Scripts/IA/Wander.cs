using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WanderState : State
{
    //Data for changing
    public float playerDetectionArea; //If inside this area change to chase mode
    public float wanderPrecisionArea; //If we arribe to this distance to the selected search node change search node 
    public float searchNodeAreaSelection; //If we arribe to this distance to the selected search node change search node
    [SerializeField] float dt;
    public float dtForChange;
    GameObject targetSearchNode;
    public GameObject listNodes;

    //Last checked 
    public Queue<GameObject> checkedPositons;
    public int maxSizeQueue; //Once we cheked enogh positions we start eliminating them from the queue

    //AI navigation
    [SerializeField] NavMeshAgent agent;

    //State to change when 
    public ChaseState chaseState;
    public override State RunCurrentState()
    {
        dt += Time.deltaTime;

        //If conditions are met choose a new searchNodeObjective
        if (
            targetSearchNode == null
            || wanderPrecisionArea <= Vector3.Distance(this.gameObject.transform.position, targetSearchNode.transform.position)
            || dtForChange <= dt
            ) 
        { 
            targetSearchNode = ChooseNextNode();
            dtForChange = 0.0F;
        }

        //Move towards the objective.

        if (true) 
        {
            return chaseState;
        }
        else 
        {
            return this;
        }

        
    }

    GameObject ChooseNextNode() 
    {
        //In case we didn't assign it or it wasn't saved although it was an instance
        if(listNodes == null) { listNodes = GameObject.FindWithTag("IA wander positions"); }

        if (targetSearchNode != null) 
        {
            //If we reached the maximun amout of stored search nodes dequeue the first
            if(checkedPositons.Count > maxSizeQueue) 
            {
                checkedPositons.Dequeue();
            }

            //Add the last searched node to not take it into account
            checkedPositons.Enqueue(targetSearchNode);
        }

        //We save the list of index we can choose from instead of game objects for optimization
        List<int> indexToRandomChooseFrom = new List<int>();
        for (int i = 0; listNodes.transform.childCount > i; i++) 
        {
            //We check if its on the list of already checked
            bool onQueue = false;
            for (int q = 0; q < checkedPositons.Count; q++) //This is kind of bad as we need to see the full quene each time to reorder it and we cannot break the for
            {
                GameObject temp = checkedPositons.Dequeue();
                if(temp == listNodes.transform.GetChild(i).gameObject) { onQueue = true; }
                checkedPositons.Enqueue(temp);
            }

            if(onQueue) { continue; }

            //Check if is near enough. If we want more weigthed system we can add an index more than once to the list.
            if (Vector3.Distance(this.gameObject.transform.position, listNodes.transform.GetChild(i).position) <= searchNodeAreaSelection) 
            {
                indexToRandomChooseFrom.Add(i);
            }
        }

        int choosenIndex = Random.Range(0, indexToRandomChooseFrom.Count);


        return listNodes.transform.GetChild(choosenIndex).gameObject;
    }

    
}
