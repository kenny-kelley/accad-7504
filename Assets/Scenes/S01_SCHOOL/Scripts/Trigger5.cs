using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger5 : MonoBehaviour
{
    private float timePassed;
    private bool hasEntered;
    private bool hasFadedIn;
    public GameObject canvas;
    public GameObject humanoid;
    public AminaScript amina;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0.0f;
        hasEntered = false;
        hasFadedIn = false;
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
        else if (timePassed > 13.0f && hasEntered)
        {
            humanoid.transform.position = new Vector3(9.917f, 0.07f, 15.529f);
            canvas.GetComponent<FadeControl>().FadeOut();
            hasEntered = false;
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
