/*
Author: Trevor Richardson
BrickScript.cs
04-07-2015

	Simple script to automatically destroy bricks when hitting environmental objects.
	
 */

using UnityEngine;
using System.Collections;

public class BrickScript : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D col)
	{
		if(col.gameObject.name != "Player" && col.gameObject.tag != "Enemy_Tux" )
		{
			Destroy (gameObject);
		}
	}
}
