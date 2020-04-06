using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger12 : MonoBehaviour
{
    public NPCScript npc1;
    public NPCScript npc2;
    public NPCScript npc3;
    public NPCScript npc4;
    public NPCScript npc5;
    public NPCScript npc6;
    public NPCScript abdoul;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == npc1.gameObject && !npc1.HasReachedDoor)
        {
            npc1.ExitSchool();
        }
        if (other.gameObject == npc2.gameObject && !npc2.HasReachedDoor)
        {
            npc2.ExitSchool();
        }
        if (other.gameObject == npc3.gameObject && !npc3.HasReachedDoor)
        {
            npc3.ExitSchool();
        }
        if (other.gameObject == npc4.gameObject && !npc4.HasReachedDoor)
        {
            npc4.ExitSchool();
        }
        if (other.gameObject == npc5.gameObject && !npc5.HasReachedDoor)
        {
            npc5.ExitSchool();
        }
        if (other.gameObject == npc6.gameObject && !npc6.HasReachedDoor)
        {
            npc6.ExitSchool();
        }
        if (other.gameObject == abdoul.gameObject && !abdoul.HasReachedDoor)
        {
            abdoul.ExitSchool();
        }
    }
}
