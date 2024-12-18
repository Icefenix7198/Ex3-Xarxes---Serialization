using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.AI;

public class MonsterManager : MonoBehaviour
{
    [Header("Server things")]
    public ClientUDP c_udp;
    public ServerUDP s_udp;
    public Serialization serialization;
    public GameObject clientParent;
    public PlayerManager playerManager;

    [Header("Monster things")]
    public List<GameObject> monsterList; //posible monsters to spawn
    float dt = 0.0f;
    [SerializeField] float timeToSpawn;
    [SerializeField] GameObject listPositionsSpawns;
    [SerializeField] GameObject currentMonsters;
    [SerializeField] List<GameObject> currentMonstersList;
    public int[] limitedSpawns;

    public AudioSource monsterAudio;

    // Start is called before the first frame update
    void Start()
    {
        if (limitedSpawns == null)
        {
            limitedSpawns = new int[monsterList.Count];
            for (int i = 0; i < limitedSpawns.Length; i++)
            {
                limitedSpawns[i] = 1;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Server Client Things
        if (s_udp == null && c_udp == null)
        {
            s_udp = GameObject.Find("UDP_Manager").GetComponent<ServerUDP>();
        }

        if (serialization == null)
        {
            serialization = GameObject.Find("UDP_Manager").GetComponent<Serialization>();
        }

        if (c_udp == null && s_udp == null)
        {
            c_udp = GameObject.Find("UDP_Manager").GetComponent<ClientUDP>();
        }

        //If server spawn bichos
        if(s_udp != null) 
        {
            dt += Time.deltaTime;

            if(dt >= timeToSpawn) 
            {
                int rand = UnityEngine.Random.Range(0, monsterList.Count);
                if (limitedSpawns[rand] > 0) 
                {
                    dt = 0.0f;
                    limitedSpawns[rand] = limitedSpawns[rand] - 1;

                    SpawnEnemy(rand,Vector2.zero);
                }
            }

            
        }
    }

    public void SpawnEnemy(int monsterIndex, Vector2 pos, bool spawnSound = false)
    {
        
        if(monsterIndex == -1) 
        {
            monsterIndex = 0;
        }

        GameObject m = Instantiate(monsterList[monsterIndex], currentMonsters.transform.position, Quaternion.identity);
        currentMonstersList.Add(m);

        if(spawnSound ) 
        {
            //Play Sound
            monsterAudio.Play();
        }

        if (s_udp != null) //If you are server 
        {
            int randSpawnPos = UnityEngine.Random.Range(0, listPositionsSpawns.transform.childCount);

            m.transform.position = listPositionsSpawns.transform.GetChild(randSpawnPos).transform.position;

            for (int i = 0; i < playerManager.playerList.Count; i++)
            {
                Vector2 p = new Vector2(m.transform.position.x, m.transform.position.z);
                serialization.serializeCreateMonster(playerManager.playerList[i].ID, p, monsterIndex);
            }

            //gameObject.transform

        }
        else if (c_udp != null)
        {
            m.transform.position = new Vector3(pos.x, 0, pos.y);

            //Deactivate component monster
            m.GetComponent<IAGeneral>().enabled = false;
        }
        m.transform.parent = currentMonsters.transform;
    }

    public List<GameObject> GetExistingMonsterList() 
    {
        List<GameObject> ret = new List<GameObject>();

        foreach(GameObject child in currentMonstersList) //Por cada children en la lista de monstruos spawneados añadirlo a la lista
        {
            ret.Add(child);
        }

        return ret;
    }

    public int GetMonsterID(GameObject monster) 
    {
        int ret = -1;

        for (int i = 0; monsterList.Count > i; i++) 
        {
            if (monsterList[i].gameObject.GetComponent<IAGeneral>().myMonsterType == monster.GetComponent<IAGeneral>().myMonsterType) 
            {
                ret = i; break;
            }
        }

        return ret;
    }

    public void UpdateClientMonster(int monsterIndex,Vector2 pos, Vector3 target)
    {
        if (c_udp != null)
        {
            if (monsterIndex < currentMonstersList.Count && monsterIndex >= 0)
            {
                currentMonstersList[monsterIndex].transform.position = new Vector3(pos.x, 0, pos.y);
                currentMonstersList[monsterIndex].GetComponent<NavMeshAgent>().SetDestination(target);
            }
            else if(monsterIndex >= 0) //In case we recive a request to update a monster that we don't have we call request monsters again, as the packets has become lost
            {
                //We call request monsters again for the player.
                serialization.RequestMonsters(playerManager.player.ID); //ERIC: I don't know if this will break but I will try
            }
        }
    }

    public void ManageSendUpdate(string playerID,GameObject monster,Vector2 position,Vector3 target) 
    {
        int monsterIndex = -1;
        for(int i = 0;i< currentMonstersList.Count; i++) 
        {
            if(monster == currentMonstersList[i].gameObject) 
            {
                monsterIndex = i;
                break;
            }
        }

        serialization.SerializeUpdateMonster(playerID, monsterIndex, position, target);
    }
}
