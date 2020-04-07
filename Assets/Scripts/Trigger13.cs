using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger13 : MonoBehaviour
{
    private float timePassed;

    private bool burn;

    public NPCScript npc1;
    public NPCScript npc2;
    public NPCScript npc3;
    public NPCScript npc4;
    public NPCScript npc5;
    public NPCScript npc6;
    public NPCScript abdoul;

    public GameObject fire1;
    public GameObject fire2;
    public GameObject fire3;

    public Soldier1Script soldier1;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        burn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!burn && npc1.HasLeftSchool && npc2.HasLeftSchool && npc3.HasLeftSchool && npc4.HasLeftSchool && npc5.HasLeftSchool && npc6.HasLeftSchool)
        {
            soldier1.SayBurnThePlace();
            burn = true;
        }

        if (burn)
            timePassed += Time.deltaTime;

        if (timePassed > 1.0f)
        {
            fire1.GetComponent<ParticleSystem>().Play();
            fire2.GetComponent<ParticleSystem>().Play();
            fire3.GetComponent<ParticleSystem>().Play();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == npc1.gameObject)
        {
            npc1.HasLeftSchool = true;
        }
        if (other.gameObject == npc2.gameObject)
        {
            npc2.HasLeftSchool = true;
        }
        if (other.gameObject == npc3.gameObject)
        {
            npc3.HasLeftSchool = true;
        }
        if (other.gameObject == npc4.gameObject)
        {
            npc4.HasLeftSchool = true;
        }
        if (other.gameObject == npc5.gameObject)
        {
            npc5.HasLeftSchool = true;
        }
        if (other.gameObject == npc6.gameObject)
        {
            npc6.HasLeftSchool = true;
        }
        if (other.gameObject == abdoul.gameObject)
        {
            abdoul.HasLeftSchool = true;
        }
    }
}
