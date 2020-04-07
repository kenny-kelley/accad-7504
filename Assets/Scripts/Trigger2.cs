using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger2 : MonoBehaviour
{
	bool hasEntered = false;
	public AminaScript amina;
	
	void Start()
	{

	}
	
	void OnTriggerEnter(Collider other)
	{
		if (!hasEntered)
		{
			amina.WalktoCloset();
			hasEntered = true;
		}
	}
}
