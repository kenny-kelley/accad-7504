using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger9 : MonoBehaviour
{
    bool hasEntered;
    public SoldierScript soldier2;

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
        if (other.gameObject == soldier2.gameObject && !hasEntered)
        {
            soldier2.StopEnterSchool();
            soldier2.FaceStudents();
            hasEntered = true;
        }
    }
}
