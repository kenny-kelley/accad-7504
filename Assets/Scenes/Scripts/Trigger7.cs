using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger7 : MonoBehaviour
{
    bool hasEntered;
    public AminaScript amina;

    // Start is called before the first frame update
    void Start()
    {
        hasEntered = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == amina.gameObject && !hasEntered)
        {
            amina.StopSlideBack();
            hasEntered = true;
        }
    }
}
