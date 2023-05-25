using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/EventRouter")]
public class EventRouter : ScriptableObject
{
	//allow observers to sub to event
	public UnityAction onEvent;

	public void Notify()
	{
		//notify all observers
		onEvent?.Invoke();
	}
}