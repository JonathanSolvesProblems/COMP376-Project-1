﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
    [SerializeField] private GameObject m_readyScreen = default;
    [SerializeField] private FloorManager m_floorManager = default;
    [SerializeField] private GameObject m_rulesScreen = default;
    [SerializeField] private FloorInfo[] m_floorInfos = default;
    [SerializeField] private List<GameObject> m_items = default;
    [SerializeField] private PlayerController m_player = default;

    private int m_currentLevel = 1; // to avoid accidents, keep it private homie :)
    private UnityEvent m_onLevelComplete;
    private UnityEvent m_onRestartLevel;
    private FloorManager m_currentFloor = default;
    private Vector2 m_playerInitPosition = Vector2.zero;
    private PlayerController.Weapon chosenWeapon = PlayerController.Weapon.Dagger;
    private int chosenProjectile = 0;
    private AudioManager audioManager = null;

    public GameObject normalBoss;
    public GameObject finalBoss;

    private void Awake()
    {
        m_onLevelComplete = new UnityEvent();
        m_onLevelComplete.AddListener(LevelComplete);
        m_onRestartLevel = new UnityEvent();
        m_onRestartLevel.AddListener(Restart);
        //Todo: send events to floor manager (or something) so they can be called when dying or finishing level
    }

    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.Play("GameMenu");
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
        m_readyScreen.SetActive(false);
        m_rulesScreen.SetActive(false);
        m_player.transform.position = m_playerInitPosition;

        m_player.gameObject.SetActive(true);

        switch (m_currentLevel)
        {
            case 1:
                m_player.Initialize(chosenWeapon, chosenProjectile, m_onRestartLevel);
                audioManager.Stop("GameMenu");
                audioManager.Play("Lvl1");
                break;
            case 2:
                audioManager.Stop("Lvl1");
                audioManager.Play("Lvl2");
                break;
            case 3:
                audioManager.Stop("Lvl2");
                audioManager.Play("Lvl3");
                break;
            case 4:
                audioManager.Stop("Lvl3");
                audioManager.Play("Lvl4");
                break;
            case 5:
                audioManager.Stop("Lvl4");
                audioManager.Play("FinalBoss");
                break;
        }

        print("m_currentLevel: " + m_currentLevel);

        m_loadingScreen.FadeIn();
        print("level loading done");
    }
    private IEnumerator RestartGame()
    {
        m_loadingScreen.FadeOut();
        yield return new WaitUntil(() => !m_loadingScreen.IsFading);

        yield return null;
        print("Returning to main menu");
        
        SceneManager.LoadScene("MainScene");
        Destroy(gameObject);
        m_loadingScreen.FadeIn();
        print("Main Menu");
    }

    private IEnumerator LoadRules() {
        m_loadingScreen.FadeOut();
        yield return new WaitUntil(() => !m_loadingScreen.IsFading);

        yield return null;
        print("rules loading started");

        m_TitleScreen.SetActive(false);
        m_rulesScreen.SetActive(true);

        yield return new WaitForSeconds(0.25f);

        m_loadingScreen.FadeIn();
        print("rule loading done");
    }

    private IEnumerator LoadTitleScreen() {
        m_loadingScreen.FadeOut();
        yield return new WaitUntil(() => !m_loadingScreen.IsFading);

        yield return null;
        print("title screen loading started");

        m_rulesScreen.SetActive(false);
        //m_readyScreen.SetActive(false); // uncommenting makes it disappear when hitting "back" but breaks UI behavior for choice highlighting..
        m_TitleScreen.SetActive(true);
        

        yield return new WaitForSeconds(0.25f);

        m_loadingScreen.FadeIn();
        print("rule loading done");
    }
    
    public void CompleteLevel()
    {
        m_currentLevel++;
        StartCoroutine(LoadLevel());
    }
    
    public int getLevel()
    {
        return m_currentLevel;
    }

    public void LoadFirstLevel()
    {
        if (m_floorInfos.Length > 0)
            StartCoroutine(LoadLevel());
    }

    public void SetChosenWeapon(int weapon)
    {
        chosenWeapon = (PlayerController.Weapon)weapon;
    }

    public void SetChosenProjectile(int choice)
    {
        chosenProjectile = choice;
    }

    public void GoToReadyScreen()
    {
        m_TitleScreen.SetActive(false);
        m_readyScreen.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
        // print("GOING TO MAIN MENU");

        // FindObjectOfType<AudioManager>().Play("GameMenu");
    }

    public void LoadRulesLevel() 
    {
        StartCoroutine(LoadRules());
    }

    public void LoadTitleScreenMain() 
    {
        StartCoroutine(LoadTitleScreen());
    }

    private void LevelComplete()
    {
        LoadNextLevel();
    }

    private void Restart()
    {
        StartCoroutine(RestartGame());
    }

    private void LoadNextLevel()
    {
        print("NEXT LEVEL");
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
    
    public GameObject RandomItem()
    {
        int randomIndex = Random.Range(0, m_items.Count);
        GameObject randomPosition = m_items[randomIndex];
        m_items.RemoveAt(randomIndex);
        return randomPosition;
    }

    public GameObject GetBoss()
    {
        if (m_currentLevel <= 3)
        {
            return normalBoss;
        }

        return finalBoss;
    }
}
