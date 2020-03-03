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

    public GameObject canvas;
    public GameObject humanoid;
    public AminaScript amina;
    public AbdoulScript abdoul;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
        hasFadedIn = false;
        hasFadedOut = false;
        hasToldStory = false;
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
            canvas.GetComponent<FadeControl>().FadeOut();
            hasFadedOut = true;
        }
        else if (timePassed > 15.0f && !hasToldStory)
        {
            abdoul.TellStory();
            hasToldStory = true;
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
