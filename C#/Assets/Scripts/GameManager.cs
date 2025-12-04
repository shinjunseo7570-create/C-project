using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public float gameTime;
    public float maxGameTime = 2 * 10f;
    public static GameManager instance;
    public PlayerInteract player;
    public PoolManager pool;

    public GameObject gameOverUI;

    public void GameOver()
    {
        gameOverUI.SetActive(true); // UI 보이게         
        Time.timeScale = 0f; // 게임 시간 멈추기
        Debug.Log("게임 종료");
    }



    void Awake()
    {
        instance = this;

        
    }
    
    void Update()
    {
        gameTime += Time.deltaTime;

        if(gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
        }
    }

    public void Retry()
    {        
        Time.timeScale = 1f; // 시간 다시 흐르게 이거 안하면 Retry 눌러도 겜 멈춰있음
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 현재 켜져 있는 Scene 다시 로드
    }

    public void Quit()
    {
        Debug.Log("게임이 종료되었습니다.");    
    }
}

