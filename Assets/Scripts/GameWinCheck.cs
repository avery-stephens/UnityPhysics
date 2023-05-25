using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameWinCheck : MonoBehaviour
{
	public void OnTriggerEnter2D(Collider2D collision)
	{
		GameManager.Instance.SetGameWin();
	}
}
