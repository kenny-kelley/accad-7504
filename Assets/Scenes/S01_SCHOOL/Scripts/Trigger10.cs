using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger10 : MonoBehaviour
{
    float timePassed;

    private bool hasEntered;
    private bool burnThePlace;

    public Soldier1Script soldier1;
    public Soldier2Script soldier2;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
        burnThePlace = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasEntered)
            timePassed += Time.deltaTime;

        if (timePassed > 4.0f && !soldier1.HasSaidBurnThePlace)
        {
            soldier1.SayBurnThePlace();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == soldier2.gameObject && !hasEntered)
        {
            soldier2.StopTurnAndWalkTowardCloset();
            soldier2.TellIssoufToMoveIt();
            burnThePlace = true;
            hasEntered = true;
        }
    }
}
