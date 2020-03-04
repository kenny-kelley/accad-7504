﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger6 : MonoBehaviour
{
    float timePassed;

    bool hasEntered;
    
    public AminaScript amina;
    public AbdoulScript abdoul;

    private AudioSource motorcycles;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
        motorcycles = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (hasEntered)
            timePassed += Time.deltaTime;

        if (timePassed > 10.5f && !amina.HasToldChildrenToHide)
        {

        }
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
