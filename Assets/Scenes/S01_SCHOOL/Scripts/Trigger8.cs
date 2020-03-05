﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger8 : MonoBehaviour
{
    bool hasEntered;
    public Soldier1Script soldier1;

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
        if (other.gameObject == soldier1.gameObject && !hasEntered)
        {
            soldier1.StopEnterSchool();
            hasEntered = true;
        }
    }
}
