using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CoilHeadChase : State
{
    //Targeting and detection
    [Header("Target Player")]
    [SerializeField] GameObject playersList;
    [SerializeField]GameObject target;
    public float detectionArea; //Should be higher than area to change from wander to this state

    //Time change state
    [Header ("Time for Change to Wander")] 
    [SerializeField] float currentSuspicionTime = 0.0f;
    public float timeForChange;
    public State wander;
    bool sigthLocked;

    //AI navigation
    [Header("Monster control")]
    [SerializeField] NavMeshAgent agent;

    public void Start() 
    {
        playersList = GameObject.Find("Players");
        sigthLocked = false;
    }

    public override State RunCurrentState()
    {
        //Choose target
        if (target == null || !target.GetComponent<PlayerMovement>().alive)
        {
            float distanceToBeat = detectionArea;
            for (int i = 0; playersList.transform.childCount > i; i++)
            {
                if (playersList.transform.GetChild(i).gameObject.GetComponent<PlayerMovement>().alive) 
                {
                    float playerDistance = Vector3.Distance(this.gameObject.transform.position, playersList.transform.GetChild(i).position);
                    if (playerDistance < distanceToBeat && playersList.transform.GetChild(i).gameObject.GetComponent<PlayerMovement>().alive) 
                    {
                        distanceToBeat = playerDistance;
                        target = playersList.transform.GetChild(i).gameObject;
                    }
                }
                else { currentSuspicionTime += Time.deltaTime * 20 / playersList.transform.childCount; }
            }

            currentSuspicionTime += Time.deltaTime;
        }
        else if(target.GetComponent<PlayerMovement>().alive)
        {
            //Lose target
            if (Vector3.Distance(this.gameObject.transform.position, target.transform.position) > detectionArea * 1.5f)
            {
                target = null;
            }

            currentSuspicionTime = 0.0f;
        }
        else if (!target.GetComponent<PlayerMovement>().alive) 
        {
            target = null;
        }

        sigthLocked = false;
        for (int i = 0; i < playersList.transform.childCount; i++) 
        {
            bool seen = false;
            //seen = IsInView(playersList.transform.GetChild(i).gameObject, this.gameObject);
            if (seen) 
            {
                sigthLocked = true;
                target = playersList.transform.GetChild(i).gameObject; //Last player that looked at 
            }
        }

        if (target != null && !sigthLocked) 
        {
            agent.SetDestination(target.transform.position);
        }
        else 
        {
            agent.SetDestination(this.gameObject.transform.position);
        }
        

        if (currentSuspicionTime > timeForChange) 
        {
            currentSuspicionTime = 0.0f;
            return wander;
        }
        else 
        {
            return this;
        }

        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool IsInView(GameObject origin, GameObject toCheck) //Origin is player, toCheck is the monster
    {
        Vector3 pointOnScreen = origin.GetComponentInChildren<Camera>().WorldToScreenPoint(toCheck.GetComponentInChildren<Renderer>().bounds.center);

        //Is in front
        if (pointOnScreen.z < 0)
        {
            Debug.Log("Behind: " + toCheck.name);
            return false;
        }

        //Is in FOV
        if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
                (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
        {
            Debug.Log("OutOfBounds: " + toCheck.name);
            return false;
        }

        RaycastHit hit;
        Vector3 heading = toCheck.transform.position - origin.transform.position;
        Vector3 direction = heading.normalized;// / heading.magnitude;

        if (Physics.Linecast(origin.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, out hit))
        {
            if (hit.transform.name != toCheck.name)
            {
                /* -->
                Debug.DrawLine(cam.transform.position, toCheck.GetComponentInChildren<Renderer>().bounds.center, Color.red);
                Debug.LogError(toCheck.name + " occluded by " + hit.transform.name);
                */
                Debug.Log(toCheck.name + " occluded by " + hit.transform.name);
                return false;
            }
        }
        return true;
    }

    
}
