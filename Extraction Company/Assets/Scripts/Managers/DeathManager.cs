using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeathManager : MonoBehaviour
{
    PlayerManager playerManager;
    PlayerMovement playerMovement;

    AudioSource playerAudioSource;
    public AudioClip explosion;

    float dt = 0;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = GameObject.Find("PlayerSpawner").GetComponent<PlayerManager>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Death") || other.CompareTag("Enemy"))
        {
            if (playerMovement.isActiveAndEnabled)
            {
                playerManager.Death();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Mine")) 
        //{
        //    if (playerMovement.isActiveAndEnabled)
        //    {
        //        playerManager.Death();
        //    }

        //    playerAudioSource = other.GetComponent<AudioSource>();
        //    playerAudioSource.PlayOneShot(explosion);

        //    Destroy(other.gameObject);
        //}
    }
}
