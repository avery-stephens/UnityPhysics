using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ControllerCharacter : MonoBehaviour
{
	[SerializeField] float speed;
	[SerializeField] float turnRate;
	[SerializeField] float jumpHeight;
	[SerializeField] float hitForce;

	CharacterController characterController;
	Vector3 velocity = Vector3.zero;

	void Start()
	{
		characterController = GetComponent<CharacterController>();
	}

	void Update()
	{
		// get direction input
		Vector3 direction = Vector3.zero;
		direction.x = Input.GetAxis("Horizontal");
		direction.z = Input.GetAxis("Vertical");

		// set velocity
		if (characterController.isGrounded)
		{
			velocity.x = direction.x * speed;
			velocity.z = direction.z * speed;
			if(velocity.x < 0) velocity.y = 0;
			if (Input.GetButtonDown("Jump"))
			{
				velocity.y += Mathf.Sqrt(jumpHeight * -2 * Physics.gravity.y);
			}
		}

		velocity.y += Physics.gravity.y * Time.deltaTime;

		// move character
		characterController.Move(velocity * Time.deltaTime);

		Vector3 face = new Vector3(velocity.x, 0, velocity.z);
		if(face.magnitude > 0)
		{
			transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(face), Time.deltaTime * turnRate);
		}
	}

	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;

		// no rigidbody
		if (body == null || body.isKinematic)
		{
			return;
		}

		// We dont want to push objects below us
		if (hit.moveDirection.y < -0.3)
		{
			return;
		}

		// Calculate push direction from move direction,
		// we only push objects to the sides never up and down
		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

		// If you know how fast your character is trying to move,
		// then you can also multiply the push velocity by that.

		// Apply the push
		body.velocity = pushDir * hitForce;
	}
}
