/*
Author: Trevor Richardson
PlayerControllerScript.cs
04-07-2015

	Script for controlling the enemy. Mirrors the player controls for many functions.
	
 */

using UnityEngine;
using System.Collections;

public class TuxScript : MonoBehaviour {

	// movement
	public bool right = true;
	float hSpeed = -1.0f;

	// detect ground contact
	bool onGround = false;
	public Transform groundCircle;
	float groundRadius = 0.05f;
	public LayerMask GroundMask;

	Animator anim;

	// enemy patrol
	bool atPatrolPoint = false;
	public LayerMask PatrolMask;
	float patrolBuffer = 0.0f;

	bool KOed = false;


	// Ignore collision with other enemies, get animator
	void Start () {
		anim = GetComponent<Animator>();
		// marching penguins move faster (spawned above 1.2)
		if (transform.position.y > 1.2)
			hSpeed = -2.00f;
		Physics2D.IgnoreLayerCollision(10, 10);
	}

	// Movement & patrol controls
	void FixedUpdate () {

		// detect ground contact for anims
		onGround = Physics2D.OverlapCircle(groundCircle.position, groundRadius, GroundMask);
		anim.SetBool("Grounded", onGround);
		// Update vertical speed for fall anim
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);

		// similar to above accept detects contact with patrol points
		atPatrolPoint = Physics2D.OverlapCircle(groundCircle.position, groundRadius, PatrolMask);
		// buffer to make sure enemies have left last patrol point collision box 
		// before reversing direction again
		patrolBuffer += Time.deltaTime;
		if (atPatrolPoint && patrolBuffer > 0.25f) {
			hSpeed = -hSpeed;
			patrolBuffer = 0.0f;
		}

		// enemy fell off world
		if (transform.position.y < -2)
			Destroy (gameObject);

		// update horizontal speed in animator and move enemy
		anim.SetFloat("hSpeed", Mathf.Abs(hSpeed));
		rigidbody2D.velocity = new Vector2(hSpeed, rigidbody2D.velocity.y);

		// Same as player control to determine if a flip is needed
		if (hSpeed < 0 && right)
			Flip ();
		else if ( hSpeed > 0 && !right)
			Flip ();
		
	}

	// Remove enemy collision boxes on death
	void Update() {
		if (KOed) {
			GetComponent<CircleCollider2D>().enabled = false;
			GetComponent<BoxCollider2D>().enabled = false;
		}
	}
	
	// Flip sprite horizontally
	void Flip() {
		right = !right;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	// If the enemy collides with a brick, destroy the brick, play enemy KO anim, and set KOed status to true
	void OnTriggerEnter2D (Collider2D col)
	{
		if (col.gameObject.tag == "Brick")
		{
			Destroy(col.gameObject);
			anim.SetTrigger("KOTrigger");
			KOed = true;
		}
	}
}
