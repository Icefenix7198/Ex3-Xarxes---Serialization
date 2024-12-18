using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehaviour : MonoBehaviour
{
    GameObject explosion;
    AudioSource playerAudioSource;
    public AudioClip explosionSound;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            explosion.SetActive(true);

            playerAudioSource = collision.gameObject.GetComponent<AudioSource>();
            playerAudioSource.PlayOneShot(explosionSound);

            Destroy(collision.gameObject);
        }
    }


}
