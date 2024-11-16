using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager instance;

    public Animator introCameraAnimator;
    public GameObject Player;
    public Camera mainCamera, cutSceneCamera;
    public GameObject UI;
    public PlayerInput input;
    public EnemySpawner enemySpawner;
    // Start is called before the first frame update
    void Start()
    {
        enemySpawner.enabled = false;
        instance = this;
        mainCamera.enabled = false;
        cutSceneCamera.enabled = true;
        Player.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame(){
        input.enabled = false;
        UI.SetActive(false);
        introCameraAnimator.Play("Test");
    }

    public void afterIntro(){
        Player.transform.position = cutSceneCamera.transform.position;
        mainCamera.transform.rotation = cutSceneCamera.transform.rotation;
        Destroy(cutSceneCamera.transform.parent.gameObject);
        enemySpawner.enabled = false;
        mainCamera.enabled = true;
        Player.SetActive(true);
        UI.SetActive(true);
        input.enabled = true;
    }
}
