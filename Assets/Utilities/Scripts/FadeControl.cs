using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeControl : MonoBehaviour
{
    public Image myPanel;
    public float fadeSpeed;
    private float x;
    private bool fadingIn;
    private bool fadingOut;


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse0))
        {
            FadeOut();
        }
        else if (Input.GetKey(KeyCode.Mouse1))
        {
            FadeIn();
        }
        
        if (fadingIn && x > 0)
        {
            x-= fadeSpeed;
        }
        if (fadingOut && x < 1)
        {
            x+= fadeSpeed;
        }
        if (x >= 1)
        {
            fadingOut = false;
        }
        else if (x <= 0)
        {
            fadingIn = false;
        }

        myPanel.color = new Color(0, 0, 0, x);
    }

    public void FadeOut()
    {
        fadingIn = true;
        fadingOut = false;
    }
    public void FadeIn()
    {
        fadingOut = true;
        fadingIn = false;

    }
}
