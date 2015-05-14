/*
Author: Trevor Richardson
PlayerControllerScript.cs
04-07-2015

	Script for controlling the player as well as miscellaneous game controls.
	
 */

using UnityEngine;
using System.Collections;

public class PlayerControllerScript : MonoBehaviour {

	// movement
	float maxSpeed = 4.0f;
	public bool right = true;
	float jumpForce = 300.0f;

	// brick projectile
	public GameObject brickPrefab;
	float brickSpeed = 10.0f;
	float throwDelay = 0.25f;
	float throwReady = 0.25f;

	// applies a merciful delay on player hit collision
	float hitDelay = 1.0f;
	float hitReady = 1.0f;

	// Applies a pushback / knockback effect on the player
	Vector2 pushBack = new Vector2(-25.0f, 0.0f);

	Animator anim;

	// Physics2D overlap circle for detecting ground contact
	bool onGround = false;
	public Transform groundCircle;
	float groundRadius = 0.05f;
	public LayerMask GroundMask;

	// GUI elements
	int health = 3;
	bool gameOver = false;
	bool gameOverStarted = false;
	bool gameWon = false;
	public GameObject heart1;
	public GameObject heart2;
	public GameObject heart3;
	public GameObject winText;
	public GameObject replayText;
	bool winDisplayed = false;

	// Audio
	public AudioSource jumpAudio;
	public AudioSource damagedAudio;
	public AudioSource throwAudio;
	public AudioSource KOAudio;

	// Enemy prefab
	public GameObject tuxPrefab;

	// Retrieve player animator
	void Start () {
		anim = GetComponent<Animator>();
	}

	void FixedUpdate () {

		// player fell off world
		if (transform.position.y < -2)
			gameOver = true;

		// detect contact with ground
		onGround = Physics2D.OverlapCircle(groundCircle.position, groundRadius, GroundMask);
		anim.SetBool("Grounded", onGround);

		// zero-out movement and disable control if game over / won
		if (gameOver || gameWon) {
			anim.SetFloat ("vSpeed", 0f);
			anim.SetFloat ("hSpeed", 0f);
			return;
		}
		// update jump/fall velocity in animator
		anim.SetFloat ("vSpeed", rigidbody2D.velocity.y);

		// horizontal movement
		float hSpeed = Input.GetAxis("Horizontal");
		anim.SetFloat("hSpeed", Mathf.Abs(hSpeed));
		rigidbody2D.velocity = new Vector2(hSpeed * maxSpeed, rigidbody2D.velocity.y);
		// Flip player sprite horizontally if moving left and not facing right 
		if (hSpeed < 0 && right)
			Flip ();
		// and vice versa
		else if ( hSpeed > 0 && !right)
			Flip ();
	
	}

	// Send KO signal to animator and wait 3 seconds to respawn
	IEnumerator GameOver() {
		anim.SetBool("GameOver", true);
		yield return new WaitForSeconds(3.0f);
		Application.LoadLevel(0);
	}

	// Spawn a bunch of penguins
	IEnumerator MarchOfThePenguins() {
		for (int i = 0; i < 10; ++i) {
			GameObject tux = (GameObject)Instantiate(tuxPrefab, new Vector2(13.0f, 1.3f), Quaternion.identity);
			yield return new WaitForSeconds(0.25f);
		}
	}

	void Update() {

		// if game over disable controls and start game over coroutine
		if (gameOver) {
			if (!gameOverStarted) {
				gameOverStarted = true;
				// KO audio
				KOAudio.Play();
				StartCoroutine(GameOver());
			}
			return;
		}
		// if game won display GUI text and enable replay input, controls disabled
		if (gameWon) {
			if (!winDisplayed) {
				winDisplayed = true;
				winText.guiText.text = "Congratulations!";
				replayText.guiText.text = "Press Enter or right-click to replay.";
			}
			if (Input.GetButtonDown("Fire2")) {
				Application.LoadLevel(0);
			}
			return;
		}

		// time buffers for throwing and getting hit
		throwReady += Time.deltaTime;
		hitReady += Time.deltaTime;

		// follow player w/ camera + baxkground
		Camera.main.transform.position = new Vector3(gameObject.transform.position.x + 1.0f, 
		                                             Camera.main.transform.position.y, 
		                                             Camera.main.transform.position.z);

		// if on the ground and jump key pressed, apply upward jump force + play jump fx
		if (Input.GetButtonDown("Jump") && onGround) {
			onGround = false;
			rigidbody2D.AddForce(new Vector2(0, jumpForce));
			jumpAudio.Play();
		}
		// throw brick on inputl, taking into account the throw delay
		if (Input.GetButtonDown("Fire1") && throwReady > throwDelay) {
			throwReady = 0.0f;
			// play throw animation
			anim.SetTrigger("ThrowTrigger");
			// spawn brick and send it in the facing direction
			GameObject brick = (GameObject)Instantiate(brickPrefab, transform.position, Quaternion.identity);
			if (right)
				brick.rigidbody2D.velocity = new Vector2(brickSpeed, 0);
			else
				brick.rigidbody2D.velocity = new Vector2(-brickSpeed, 0);
			// play throw fx and destroy brick after 1s
			throwAudio.Play();
			Destroy (brick, 1.0f);
		}
		// Don't press L
		if (Input.GetKeyDown(KeyCode.L) && onGround) {
			Hit();
		}
	}

	// If hit, deplete a heart, update heart GUI appropriately, play hit fx and hit anim
	void Hit() {
		--health;
		damagedAudio.Play();
		anim.SetTrigger("HitTrigger");
		switch (health) {
		case 2:
			heart3.SetActive(false);
			break;
		case 1:
			heart2.SetActive(false);
			break;
		case 0:
			heart1.SetActive(false);
			// KO
			gameOver = true;
			break;
		}
	}

	// flip sprite horizontally, revert facing right bool
	void Flip() {
		right = !right;
		transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	// Damage player on contract with enemy/environmental hazards and apply knockback
	void OnCollisionStay2D (Collision2D col)
	{
		if(col.gameObject.tag == "Enemy_Tux" || col.gameObject.tag == "Spikes")
		{
			rigidbody2D.AddForce(pushBack);
			// hit time buffer
			if (hitReady > hitDelay) {
				hitReady = 0f;
				Hit ();
			}
		}
	}

	// Spawn penguin march / finish game if respective triggers are entered
	void OnTriggerEnter2D (Collider2D col)
	{
		if (col.gameObject.tag == "March")
		{
			// remove trigger so the player doesn't trigger it multiple times
			Destroy(col.gameObject);
			StartCoroutine(MarchOfThePenguins());
		}
		if (col.gameObject.tag == "Finish")
		{
			gameWon = true;
		}
	}
}
