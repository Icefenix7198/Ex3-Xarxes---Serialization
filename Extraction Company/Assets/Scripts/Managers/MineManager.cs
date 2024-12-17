using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MineManager : MonoBehaviour
{
    PlayerManager playerManager;
    PlayerMovement playerMovement;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = GameObject.Find("PlayerSpawner").GetComponent<PlayerManager>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Mine")) 
        {
            if (playerMovement.isActiveAndEnabled)
            {
                playerManager.Death();
            }

            Destroy(other.gameObject);
        }
    }
}
