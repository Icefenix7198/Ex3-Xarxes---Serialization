using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class IAGeneral : MonoBehaviour
{
    float distanceRendering = 50f; //The area from where the point will be choosen.
    public PlayerManager playerManager;
    public MonsterManager monsterManager;

    List<Vector3> sentTargets = new List<Vector3>();
    [SerializeField] NavMeshAgent agent;

    public enum MONSTER_TYPE 
    {
        COILHEAD,
    }

    public MONSTER_TYPE myMonsterType;

    //State Machine BS
    public State currentState;

    // Update is called once per frame
    void Update()
    {
        if (agent == null)
        {
            agent = this.gameObject.GetComponent<NavMeshAgent>();
        }

        if (playerManager == null)
        {
            GameObject tmp = GameObject.Find("PlayerSpawner");

            if (tmp != null)
            {
                playerManager = tmp.GetComponent<PlayerManager>();
            }
        }

        if (monsterManager == null)
        {
            GameObject tmp = GameObject.Find("MonsterManager");

            if (tmp != null)
            {
                monsterManager = tmp.GetComponent<MonsterManager>();
            }
        }

        RunStateMachine();

        SendMonsterInfo();
    }

    private void SendMonsterInfo()
    {
        for (int i = 0; i < playerManager.playerList.Count; i++) 
        {
            if (sentTargets.Count <= i) //If list of players is bigger than list of added positions add new position
            {
                sentTargets.Add(new Vector3(-9999999, -99999999, -9999999));
            }

            if(Vector3.Distance(this.gameObject.transform.position, playerManager.playerList[i].position) < distanceRendering) 
            {
                if (sentTargets[i] != agent.destination) //Not save 
                {
                    //Send data to client
                    Vector2 posToSend = new Vector2(this.transform.position.x, this.transform.position.z);
                    monsterManager.ManageSendUpdate(playerManager.playerList[i].ID, this.gameObject, posToSend, agent.destination);

                    sentTargets[i] = agent.destination;
                }
            }
        }
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
