using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger10 : MonoBehaviour
{
    private float timePassed;

    private bool hasEntered;
    
    public Soldier2Script soldier2;
    public NPCScript abdoul;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasEntered)
            timePassed += Time.deltaTime;

        if (timePassed > 2.0f && hasEntered)
            abdoul.WalkToAisle();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == soldier2.gameObject && !hasEntered)
        {
            soldier2.StopTurnAndWalkTowardCloset();
            soldier2.TellIssoufToMoveIt();
            hasEntered = true;
        }
    }
}
