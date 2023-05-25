using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Xml.Serialization;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : Singleton<GameManager> 
{
	[Header("Events")]
	[SerializeField] EventRouter startGameEvent;
	[SerializeField] EventRouter stopGameEvent;
	[SerializeField] EventRouter winGameEvent;

	[SerializeField] AudioSource gameMusic;
	[SerializeField] AudioSource winnerMusic;

	[SerializeField] GameObject playerPrefab;
	[SerializeField] Transform playerStart;


	public enum State
	{
		TITLE,
		START_GAME,
		PLAY_GAME,
		GAME_OVER,
		GAME_WON
	}

	State state = State.TITLE;
	float stateTimer;

	private void Start()
	{
		winGameEvent.onEvent += SetGameWin;
	}

	private void Update()
	{
		switch (state) 
		{
			case State.TITLE:
				gameMusic.Stop();
				UIManager.Instance.ShowTitle(true);
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;

				if (FindObjectOfType<ControllerCharacter2D>() != null)
				{
					Destroy(FindObjectOfType<ControllerCharacter2D>().gameObject);
				}

				break;
			case State.START_GAME:
				UIManager.Instance.SetHealth(100);
				startGameEvent.Notify();
				UIManager.Instance.ShowTitle(false);
				Cursor.lockState = CursorLockMode.Locked;
				if (FindObjectOfType<ControllerCharacter2D>() == null)
				{
					Instantiate(playerPrefab, playerStart.position, playerStart.rotation);
				}
				state = State.PLAY_GAME;
				gameMusic.Play();
				break;
			case State.PLAY_GAME:
				break;
			case State.GAME_OVER:
				stateTimer -= Time.deltaTime;
				//SetGameOver();
				if (stateTimer <= 0)
				{
                    UIManager.Instance.ShowGameOver(false);
                    state = State.TITLE;
				}
				break;
			case State.GAME_WON:
				stateTimer -= Time.deltaTime;
				//SetGameWin();
				if (stateTimer <= 0)
				{
                    UIManager.Instance.ShowGameWin(false);
                    state = State.TITLE;
				}
				break;

			default:
				break;
		}
	}

	public void SetGameOver()
	{
		stopGameEvent.Notify();
		UIManager.Instance.ShowGameOver(true);
		state = State.GAME_OVER;
		stateTimer = 3;
		Debug.Log("lost :(");
	}

	public void SetGameWin()
	{
		stopGameEvent.Notify();
		UIManager.Instance.ShowGameWin(true);
		state = State.GAME_WON;
		stateTimer = 3;
		Debug.Log("Win!!!");
	}

	public void OnStartGame()
	{
		state = State.START_GAME;
	}
}
