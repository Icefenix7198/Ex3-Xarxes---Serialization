using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectClientSever : MonoBehaviour
{
    public void PassServer()
    {
        SceneManager.LoadScene("ServerScene");
    }

    public void PassClient()
    {
        SceneManager.LoadScene("ClientScene");
    }
}
