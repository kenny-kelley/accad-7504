using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger1 : MonoBehaviour
{
    public AminaScript amina;
	bool hasEntered = false;
	
	void Start()
	{
	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!hasEntered)
		{
            amina.AskIssoufWhereAreYou();
			hasEntered = true;	
		}
	}
}
