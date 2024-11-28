using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    [Header("Server things")]
    public ClientUDP c_udp;
    public ServerUDP s_udp;
    public Serialization serialization;
    public GameObject clientParent;

    [Header("Server things")]
    public List<GameObject> monsterList;
    float dt = 0.0f;
    [SerializeField] float timeToSpawn;

    // Start is called before the first frame update
    void Start()
    {
        
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
                
            }
        }
    }

    void SpawnEnemy() 
    {
    
    }
}
