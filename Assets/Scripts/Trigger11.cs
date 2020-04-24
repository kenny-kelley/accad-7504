using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger11 : MonoBehaviour
{
    public NPCScript npc1;
    public NPCScript npc2;
    public NPCScript npc3;
    public NPCScript npc4;
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
        if (other.gameObject == npc1.gameObject && !npc1.HasReachedAisle)
        {
            npc1.WalkToDoor();
        }
        if (other.gameObject == npc2.gameObject && !npc2.HasReachedAisle)
        {
            npc2.WalkToDoor();
        }
        if (other.gameObject == npc3.gameObject && !npc3.HasReachedAisle)
        {
            npc3.WalkToDoor();
        }
        if (other.gameObject == npc4.gameObject && !npc4.HasReachedAisle)
        {
            npc4.WalkToDoor();
        }
        if (other.gameObject == abdoul.gameObject && !abdoul.HasReachedAisle)
        {
            abdoul.WalkToDoor();
        }
    }
}
