using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger5 : MonoBehaviour
{
    private float timePassed;
    private bool hasFadedIn;
    private bool hasFadedOut;
    
    private AudioSource[] audios;
    public GameObject canvas;
    public GameObject humanoid;
    public AminaScript amina;
    public AbdoulScript abdoul;

    public GameObject npc1;
    public GameObject npc2;
    public GameObject npc3;
    public GameObject npc4;
    public GameObject npc5;
    public GameObject npc6;

    public Trigger6 trigger6;
    public bool hasChalk;

    public bool HasEntered { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        HasEntered = false;
        hasFadedIn = false;
        hasFadedOut = false;
        hasChalk = false;
        audios = GetComponents<AudioSource>();
    }

    void Update()
    {
        if (HasEntered)
            timePassed += Time.deltaTime;

        if (timePassed > 10.0f && timePassed <= 13.0f && !hasFadedIn)
        {
            audios[1].Stop();
            canvas.GetComponent<FadeControl>().FadeIn();
            hasFadedIn = true;
        }
        else if (timePassed > 13.0f && !hasFadedOut)
        {
            humanoid.transform.position = new Vector3(9.047f, 0.000792563f, 15.58642f);
            humanoid.transform.rotation = new Quaternion(0.0f, 180.0f, 0.0f, 0.0f);
            audios[0].Play();
            canvas.GetComponent<FadeControl>().FadeOut();
            Destroy(npc1);
            Destroy(npc2);
            Destroy(npc3);
            Destroy(npc4);
            Destroy(npc5);
            Destroy(npc6);
            hasFadedOut = true;
        }
        else if (timePassed > 15.0f && !amina.HasTakenAttendance)
        {
            amina.TakeAttendance();
        }
        else if (timePassed > 31.0f && !abdoul.HasToldStory)
        {
            abdoul.TellStory();
        }
        else if (timePassed > 44.0f && !amina.HasMmphedAbdoul)
        {
            abdoul.Idle();
            amina.MmphAbdoul();
        }
        else if (timePassed > 45.0f && !abdoul.HasFinishedStory) 
        {
            abdoul.FinishStory();
        }
        else if (timePassed > 55.0f && !amina.HasMmphedAbdoulAndIssouf) 
        {
            amina.MmphAbdoulAndIssouf();
        }
        else if (timePassed > 57.0f && !abdoul.HasApologized)
        {
            abdoul.Apologize();
        }
        else if (timePassed > 59.0f && !amina.HasOrderedIssoufToGetChalk)
        {
            amina.OrderIssoufToGetChalk();
        }
        else if (!hasChalk && timePassed%20 < 1 && timePassed > 70.0f)
        {
            amina.NoChalk();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == amina.gameObject && !HasEntered)
        {
            amina.StopWalkToFrontCenter();
            HasEntered = true;
        }
    }

    public void gotChalk()
    {
        hasChalk = true;
    }
}
