using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger5 : MonoBehaviour
{
    private float timePassed;
    private bool hasEntered;
    private bool hasFadedIn;
    private bool hasFadedOut;
    private bool hasToldStory;
    private bool hasMmphedAbdoul;
    private bool hasFinishedStory;

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

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
        hasFadedIn = false;
        hasFadedOut = false;
        hasToldStory = false;
        hasMmphedAbdoul = false;
        hasFinishedStory = false;
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
            canvas.GetComponent<FadeControl>().FadeOut();
            Destroy(npc1);
            Destroy(npc2);
            Destroy(npc3);
            Destroy(npc4);
            Destroy(npc5);
            Destroy(npc6);
            hasFadedOut = true;
        }
        else if (timePassed > 15.0f && !hasToldStory)
        {
            abdoul.TellStory();
            hasToldStory = true;
        }
        else if (timePassed > 35.0f && !hasMmphedAbdoul)
        {
            abdoul.Idle();
            amina.MmphAbdoul();
            hasMmphedAbdoul = true;
        }
        else if (timePassed > 38.0f && !hasFinishedStory)
        {
            abdoul.FinishStory();
            hasFinishedStory = true;
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
