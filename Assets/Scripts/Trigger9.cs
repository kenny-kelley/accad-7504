using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger9 : MonoBehaviour
{
    float timePassed;

    public NPCScript npc1;
    public NPCScript npc2;
    public NPCScript npc3;
    public NPCScript npc4;

    bool hasEntered;
    public Soldier2Script soldier2;

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


    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == soldier2.gameObject && !hasEntered)
        {
            soldier2.StopEnterSchool();

            npc1.WalkToAisle();
            npc2.WalkToAisle();
            npc3.WalkToAisle();
            npc4.WalkToAisle();

            soldier2.TurnAndWalkTowardCloset();
            hasEntered = true;
        }
    }
}
