using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExtractionManager : MonoBehaviour
{
    Dictionary<string, int> extractions_ofPlayers;

    // Start is called before the first frame update
    void Start()
    {
        extractions_ofPlayers = new Dictionary<string, int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveExtraction(string ID, int extraction)
    {
        if (extractions_ofPlayers.ContainsKey(ID))
        {
            extractions_ofPlayers[ID] = extraction;
        }
        else
        {
            extractions_ofPlayers.Add(ID, extraction);
        }
    }
}
