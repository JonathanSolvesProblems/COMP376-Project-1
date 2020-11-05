﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class FloorInfo
{
    //data to use for floor generation
    public int Room = 0;
    public int Floor = 0;
}

public class GameManager : MonoBehaviour
{
    [SerializeField] private LoadingScreen m_loadingScreen = default;
    [SerializeField] private GameObject m_TitleScreen = default;
    [SerializeField] private FloorManager m_floorManager = default;
    [SerializeField] private int m_currentLevel = 1;
    [SerializeField] private FloorInfo[] m_floorInfos = default;
    [SerializeField] private PlayerController m_player = default;

    private UnityEvent m_onLevelComplete;
    private UnityEvent m_onRestartLevel;
    private FloorManager m_currentFloor = default;
    private Vector2 m_playerInitPosition = Vector2.zero;

    private void Awake()
    {
        DontDestroyOnLoad(this);
        m_onLevelComplete = new UnityEvent();
        m_onLevelComplete.AddListener(LevelComplete);
        m_onRestartLevel = new UnityEvent();
        m_onRestartLevel.AddListener(RestartLevel);
        //Todo: send events to floor manager (or something) so they can be called when dying or finishing level
    }

    void Start()
    {
        // LoadFirstLevel(); //should be moved to be called by the play button in the main menu
    }

    void OnDestroy()
    {
        m_onLevelComplete.RemoveAllListeners();
        m_onRestartLevel.RemoveAllListeners();
    }

    private IEnumerator LoadLevel()
    {
        m_loadingScreen.FadeOut();
        yield return new WaitUntil(() => !m_loadingScreen.IsFading);

        yield return null;
        print("level loading started");

        if (m_currentFloor)
            Destroy(m_currentFloor.gameObject);

        m_currentFloor = Instantiate(m_floorManager);
        m_currentFloor.Floor = m_currentLevel;

        m_TitleScreen.SetActive(false);
        m_player.transform.position = m_playerInitPosition;
        m_player.Initialize(PlayerController.Weapon.Dagger, m_onRestartLevel); //todo add weapon choice
        m_player.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        m_loadingScreen.FadeIn();
        print("level loading done");
    }

    public void LoadFirstLevel()
    {
        if (m_floorInfos.Length > 0)
            StartCoroutine(LoadLevel());
    }

    private void LevelComplete()
    {
        LoadNextLevel();
    }

    private void RestartLevel()
    {
        StartCoroutine(LoadLevel());
    }

    private void LoadNextLevel()
    {
        if (m_currentLevel < m_floorInfos.Length - 1)
        {
            m_currentLevel++;
            StartCoroutine(LoadLevel());
        }
        else
        {
            //game complete
        }
    }
}
