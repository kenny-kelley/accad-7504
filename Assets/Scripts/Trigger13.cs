using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger13 : MonoBehaviour
{
    private float timePassed;

    private bool burn;

    private AudioSource[] audios;

    public NPCScript npc1;
    public NPCScript npc2;
    public NPCScript npc3;
    public NPCScript npc4;
    public NPCScript abdoul;

    public ParticleSystem fire1;
    public ParticleSystem fire2;
    public ParticleSystem fire3;

    public Soldier1Script soldier1;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        audios = GetComponents<AudioSource>();
        burn = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!burn && npc1.HasLeftSchool && npc2.HasLeftSchool && npc3.HasLeftSchool && npc4.HasLeftSchool)
        {
            soldier1.SayBurnThePlace();
            audios[0].Play();
            burn = true;
        }

        if (burn)
            timePassed += Time.deltaTime;

        if (timePassed > 3.0f && !fire1.isPlaying)
        {
            audios[1].Play();
            fire1.Play();
        }
        else if (timePassed > 4.0f && !fire2.isPlaying)
        {
            fire2.Play();
        }
        else if (timePassed > 5.0f && !fire3.isPlaying)
        {
            fire3.Play();
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
        if (other.gameObject == abdoul.gameObject)
        {
            abdoul.HasLeftSchool = true;
        }
    }
}
