using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody2D))]
public class AICharacter2D : MonoBehaviour, IDamagable
{
	[SerializeField] Animator animator;
	[SerializeField] SpriteRenderer spriteRenderer;
	[SerializeField] float speed;
	[SerializeField] float jumpHeight;
	[SerializeField, Range(1, 5)] float fallRateMultiplier;
	[SerializeField, Range(1, 5)] float lowJumpRateMultiplier;
	[Header("Ground")]
	[SerializeField] Transform groundTransform;
	[SerializeField] LayerMask groundLayerMask;
	[SerializeField] float groundRadius;
	[Header("AI")]
	[SerializeField] Transform[] waypoints;
	[SerializeField] float rayDistance = 1;
	[SerializeField] string enemyTag;
	[SerializeField] LayerMask raycastLayerMask;
	[Header("Attack")]
	[SerializeField] Transform attackTransform;
	[SerializeField] float attackRadius;

	public float health = 100;

	Rigidbody2D rb;

	Vector2 velocity = Vector2.zero;
	bool faceRight = true;
	Transform targetWaypoint = null;
	GameObject enemy = null;

	enum State
	{
		IDLE,
		PATROL,
		CHASE,
		ATTACK,
		DEATH
	}

	State state = State.IDLE;
	float stateTimer = 1;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		//update AI
		CheckEnemySeen();
		Vector2 direction = Vector2.zero;
		switch (state)
		{
			case State.IDLE:
				if (enemy != null) state = State.CHASE;
				stateTimer -= Time.deltaTime;
				if (stateTimer <= 0)
				{
					SetNewWaypointTarget();
					state = State.PATROL;
				}
				break;
			case State.PATROL:
				{
					if (enemy != null) state = State.CHASE;
					direction.x = Mathf.Sign(targetWaypoint.position.x - transform.position.x);
					float dx = Mathf.Abs(transform.position.x - targetWaypoint.position.x);
					if (dx <= 0.25f)
					{
						state = State.IDLE;
						stateTimer = 1f;
						//direction.x = 0;
					}
					//if (transform.position.x > targetWaypoint.position.x) direction.x = 1;
					//else direction.x = -1;
				}
				break;
			case State.CHASE:
				{
					//if enemy isnt seen, return to idle state
					if (enemy == null)
					{
						state = State.IDLE;
						stateTimer = 1;
						break;
					}
					
					//if position/distance from enemy is less than one, set state to attack and set attack trigger
					float dx = Mathf.Abs(enemy.transform.position.x - transform.position.x);
					if (dx <= 1.75f)
					{
						animator.SetTrigger("Attack");
						state = State.ATTACK;
					}
					else
					{
						direction.x = Mathf.Sign(enemy.transform.position.x - transform.position.x);
					}
				}
				break;
			case State.ATTACK:
				//if animator normalized time is greater than one and animation is not in transition, set state to chase
				if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0))
				{
					state = State.CHASE;
				}
				break;
			case State.DEATH:
				speed = 0;
				break;
			default: 
				break;
		}

		if (health <= 0)
		{
			Death();
			state = State.DEATH;
		}

		//check if on ground
		bool onGround = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask) != null;

		// get direction input

		velocity.x = direction.x * speed;
		// set velocity
		if (onGround)
		{
			if(velocity.y < 0) velocity.y = 0;
			//if (Input.GetButtonDown("Jump"))
			//{
			//	velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
			//	StartCoroutine(DoubleJump());
			//	animator.SetTrigger("Jump");
			//}
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
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(groundTransform.position, groundRadius);
	}

	//IEnumerator DoubleJump()
	//{
	//	// wait a little after the jump to allow a double jump
	//	yield return new WaitForSeconds(0.01f);
	//	// allow a double jump while moving up
	//	while (velocity.y > 0)
	//	{
	//		// if "jump" pressed add jump velocity
	//		if (Input.GetButtonDown("Jump"))
	//		{
	//			velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
	//			break;
	//		}
	//		yield return null;
	//	}
	//}

	private void Flip()
	{
		faceRight = !faceRight;
		spriteRenderer.flipX = !faceRight;
	}

	private void SetNewWaypointTarget()
	{
		Transform waypoint;
		do
		{
			waypoint = waypoints[Random.Range(0, waypoints.Length)];
		} while (waypoint == targetWaypoint);
		targetWaypoint = waypoint;
	}

	private void Death()
	{
		animator.SetBool("Death", true);
		var scoreManage = FindObjectOfType<ControllerCharacter2D>();
		scoreManage.AddPoints(100);
		//yield return new WaitForSeconds(4.0f);
		Destroy(gameObject);
	}

	private void CheckAttack()
	{
		Collider2D[] colliders = Physics2D.OverlapCircleAll(attackTransform.position, attackRadius);
		foreach (Collider2D collider in colliders)
		{
			if (collider.gameObject == gameObject) continue;

			if (collider.gameObject.TryGetComponent<IDamagable>(out var damagable))
			{
				damagable.Damage(5);
			}
		}
	}

	private void CheckEnemySeen()
	{
		enemy = null;
		RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left), rayDistance, raycastLayerMask);
		if (raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag(enemyTag))
		{
			enemy = raycastHit.collider.gameObject;
			Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * rayDistance, Color.red);
		}
	}

	public void Damage(int damage)
	{
		health -= damage;
		animator.SetTrigger("Hurt");
		print(health);
	}
}
