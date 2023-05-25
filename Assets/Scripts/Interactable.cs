using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
	[SerializeField] protected GameObject interactFX;
	[SerializeField] protected bool destroyOnInteract = true;
	[SerializeField] protected Condition condition;

	public abstract void OnInteract(GameObject target);
}
