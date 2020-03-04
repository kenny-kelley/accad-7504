using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger6 : MonoBehaviour
{
    bool hasEntered;
    AudioSource motorcycles;

    // Start is called before the first frame update
    void Start()
    {
        hasEntered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!hasEntered)
        {
            motorcycles.Play();
            hasEntered = true;
        }
    }
}
