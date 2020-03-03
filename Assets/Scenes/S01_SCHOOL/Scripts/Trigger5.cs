using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger5 : MonoBehaviour
{
    private bool hasEntered;
    public AminaScript amina;

    // Start is called before the first frame update
    void Start()
    {
        hasEntered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "AdultWoman" && !hasEntered)
        {
            amina.StopWalkToFrontCenter();
            hasEntered = true;
        }
    }
}
