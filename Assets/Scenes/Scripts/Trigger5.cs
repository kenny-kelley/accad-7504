using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger5 : MonoBehaviour
{
    private float timePassed;
    private bool hasEntered;
    private bool hasFadedIn;
    private bool hasFadedOut;

    private AudioSource chairsScrapingFloor;
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

    public GameObject trigger6;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
        hasFadedIn = false;
        hasFadedOut = false;
        chairsScrapingFloor = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (hasEntered)
            timePassed += Time.deltaTime;

        if (timePassed > 10.0f && timePassed <= 13.0f && !hasFadedIn)
        {
            canvas.GetComponent<FadeControl>().FadeIn();
            hasFadedIn = true;
        }
        else if (timePassed > 13.0f && !hasFadedOut)
        {
            humanoid.transform.position = new Vector3(9.902f, 0.07f, 15.529f);
            humanoid.transform.rotation = new Quaternion(0.0f, 180.0f, 0.0f, 0.0f);
            chairsScrapingFloor.Play();
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
            trigger6.SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "AdultWoman" && !hasEntered)
        {
            amina.StopWalkToFrontCenter();
            hasEntered = true;
        }
    }
}
