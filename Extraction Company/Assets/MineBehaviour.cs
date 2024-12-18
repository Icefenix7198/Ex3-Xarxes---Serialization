using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineBehaviour : MonoBehaviour
{
    [SerializeField] GameObject explosion;
    AudioSource mineAudioSource;
    public ParticleSystem particleSystem;
    public AudioClip explosionSound;

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            mineAudioSource = this.gameObject.GetComponent<AudioSource>();
            mineAudioSource.maxDistance *= 3;
            mineAudioSource.volume *= 1.5f;
            mineAudioSource.clip = explosionSound;
            mineAudioSource.loop = false;
            mineAudioSource.Play();

            WaitDoDespawn();
            particleSystem.Play();
            explosion.SetActive(true);
            particleSystem.Play();
            mineAudioSource?.Stop();
            Destroy(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            mineAudioSource = this.gameObject.GetComponent<AudioSource>();
            mineAudioSource.maxDistance *= 3;
            mineAudioSource.volume *= 1.5f;
            mineAudioSource.clip = explosionSound;
            mineAudioSource.loop = false;
            mineAudioSource.Play();

            WaitDoDespawn();
            particleSystem.Play();
            explosion.SetActive(true);
            particleSystem.Play();
            mineAudioSource?.Stop();
            Destroy(this);
        }
    }

    IEnumerator WaitDoDespawn()
    {
        yield return new WaitForSeconds(0.5f);
    }

}
