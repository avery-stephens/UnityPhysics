using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class ControllerCharacter2D : MonoBehaviour, IDamagable
{
	[SerializeField] Animator animator;
	[SerializeField] SpriteRenderer spriteRenderer;
	[SerializeField] float speed;
	[SerializeField] float jumpHeight;
	[SerializeField] float doubleJumpHeight;
	[SerializeField, Range(1, 5)] float fallRateMultiplier;
	[SerializeField, Range(1, 5)] float lowJumpRateMultiplier;
	[Header("Ground")]
	[SerializeField] Transform groundTransform;
	[SerializeField] LayerMask groundLayerMask;
	[SerializeField] float groundRadius;
	[Header("Attack")]
	[SerializeField] Transform attackTransform;
	[SerializeField] float attackRadius;
	//[Header("Wall Grab")]
	//[SerializeField] Transform frontCheck;
	//[SerializeField] float wallSlidingSpeed;
	//bool isWallSliding;
	//bool isTouchingFront;

	//private bool wallJumping;
	//private float wallJumpDirection;
	//private float wallJumpTime = 0.2f;
	//private float wallJumpingCounter;
	//private float wallJumpingDuration = 0.4f;
	//private Vector2 wallJumpingPower = new Vector2(8f, 16f);

	public int health = 100;
	private int score = 0;
	bool isDead = false;

	Rigidbody2D rb;
	Vector2 velocity = Vector2.zero;
	bool faceRight = true;
	float groundAngle = 0;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();

		GetComponent<Health>().onDamage += OnDamage;
		GetComponent<Health>().onDeath += OnDeath;
		GetComponent<Health>().onHeal += OnHeal;
	}

	void Update()
	{
		//check if on ground
		bool onGround = UpdateGroundCheck() && (velocity.y <= 0);
		//bool isGrounded = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask);

		// get direction input
		
		Vector2 direction = Vector2.zero;
		direction.x = Input.GetAxis("Horizontal");

		velocity.x = direction.x * speed;
		// set velocity
		if (onGround)
		{
			if(velocity.y < 0) velocity.y = 0;
			if (Input.GetButtonDown("Jump"))
			{
				velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
				StartCoroutine(DoubleJump());
				animator.SetTrigger("Jump");
			}

			if(Input.GetMouseButtonDown(0))
			{
				animator.SetTrigger("Attack");
			}
		}

		velocity.y += Physics.gravity.y * Time.deltaTime;

		//adjust gravity for jump
		float gravityMultiplier = 1;
		if (!onGround && velocity.y < 0) gravityMultiplier = fallRateMultiplier;
		if (!onGround && velocity.y > 0 && !Input.GetButton("Jump")) gravityMultiplier = lowJumpRateMultiplier;

		velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;

		//move character
		rb.velocity = velocity;

		// flip character to face direction of movement
		if (velocity.x > 0 && !faceRight) Flip();
		if (velocity.x < 0 && faceRight) Flip();

		//update animator
		animator.SetFloat("Speed", Mathf.Abs(velocity.x));
		animator.SetBool("Fall", !onGround && velocity.y < -0.1f);

		if (health <= 0)
		{
			OnDeath();
		}

		//if (isWalled() && direction != Vector2.zero)
		//{
		//	isWallSliding = true;
		//	rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
		//}
		//else 
		//{
		//	isWallSliding = false;
		//}

		//WallJump();
	}

	//private bool isWalled()
	//{
	//	return Physics2D.OverlapCircle(frontCheck.position, 0.2f, groundLayerMask);
	//}

	//private void WallJump()
	//{
	//	if (isWallSliding)
	//	{
	//		wallJumping = false;
	//		wallJumpDirection = -transform.localScale.x;
	//		wallJumpingCounter = wallJumpTime;

	//		CancelInvoke(nameof(StopWallJump));
	//	}
	//	else 
	//	{
	//		wallJumpingCounter -= Time.deltaTime;
	//	}

	//	if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
	//	{
	//		wallJumping = true;
	//		rb.velocity = new Vector2(wallJumpDirection * wallJumpingPower.x, wallJumpingPower.y);
	//		wallJumpingCounter = 0f;

	//		if (transform.localScale.x != wallJumpDirection)
	//		{
	//			faceRight = !faceRight;
	//			Vector3 localScale = transform.localScale;
	//			localScale.x *= -1f;
	//			transform.localScale = localScale;
	//		}
	//		Invoke(nameof(StopWallJump), wallJumpingDuration);
	//	}
	//}

	//private void StopWallJump()
	//{
	//	wallJumping = false;
	//}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(groundTransform.position, groundRadius);

		//Gizmos.color = Color.blue;
		//Gizmos.DrawSphere(frontCheck.position, 0.1f);
	}

	IEnumerator DoubleJump()
	{
		// wait a little after the jump to allow a double jump
		yield return new WaitForSeconds(0.01f);
		// allow a double jump while moving up
		while (velocity.y > 0)
		{
			// if "jump" pressed add jump velocity
			if (Input.GetButtonDown("Jump"))
			{
				velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
				break;
			}
			yield return null;
		}
	}
	private void Flip()
	{
		if (faceRight && velocity.x < 0f || !faceRight && velocity.x > 0f)
		{
			faceRight = !faceRight;
			Vector3 localScale = transform.localScale;
			localScale.x *= -1f;
			transform.localScale = localScale;
		}
		//transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	}

	private void CheckAttack()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(attackTransform.position, attackRadius);
		foreach (Collider2D collider in colliders)
		{
			if (collider.gameObject == gameObject) continue;

			if (collider.gameObject.TryGetComponent<IDamagable>(out var damagable))
			{
				damagable.Damage(10);
			}
		}
	}
	private bool UpdateGroundCheck()
	{
		// check if the character is on the ground
		Collider2D collider = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask);
		if (collider != null)
		{
			RaycastHit2D raycastHit = Physics2D.Raycast(groundTransform.position, Vector2.down, groundRadius, groundLayerMask);
			if (raycastHit.collider != null)
			{
				// get the angle of the ground (angle between up vector and ground normal)
				groundAngle = Vector2.SignedAngle(Vector2.up, raycastHit.normal);
				Debug.DrawRay(raycastHit.point, raycastHit.normal, Color.red);
			}
		}
		return (collider != null);
	}

	public void Damage(int damage)
	{
		health -= damage;
		UIManager.Instance.SetHealth(health);
		animator.SetTrigger("Hurt");
		print("player health " + health);
	}

	public void OnDeath()
	{
		if (!isDead)
		{
			GameManager.Instance.SetGameOver();
			animator.SetBool("Death", true);
			Debug.Log("dead");
		}
		isDead = true;
	}

	public void OnHeal()
	{
		UIManager.Instance.SetHealth((int)GetComponent<Health>().health);
		Debug.Log("yay");
	}

	public void OnDamage()
	{
		UIManager.Instance.SetHealth((int)GetComponent<Health>().health);
		animator.SetTrigger("Hurt");
		Debug.Log("ow");
	}

	public void AddPoints(int points)
	{
		score += points;
		UIManager.Instance.SetScore(score);
	}
}
