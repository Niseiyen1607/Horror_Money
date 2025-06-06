using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Item Collection")]
    public int totalItemValue = 0;     
    public int runItemValue = 0;       

    [SerializeField] private int goalValue;
    [SerializeField] private int minGoalValue = 200;
    [SerializeField] private int maxGoalValue = 1300;

    [Header("Exit Object")]
    [SerializeField] private GameObject exitObject;
    private bool exitSpawned = false;

    [Header("Game State")]
    public bool isPostGoalPhase = false;

    [Header("Phase 2")]
    public float phaseDuration = 30f;
    private float phaseTimer = 0f;
    public bool isInPhase2 = false;

    public event Action OnPhaseChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scene loaded: " + scene.name);

        findExit();

        if (scene.name == "Shop")
        {
            UIManager.Instance.UpdateTotalCoinsText(totalItemValue);
        }
        else
        {
            InitGoalValue();
            UIManager.Instance.UpdateGoalText(runItemValue, goalValue);
        }

        StartCoroutine(PositionPlayerAtSpawn());
    }

    private IEnumerator PositionPlayerAtSpawn()
    {
        yield return null; 

        GameObject playerSpawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawnPoint");
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (playerSpawnPoint != null && player != null)
        {
            player.transform.position = playerSpawnPoint.transform.position;
            player.transform.rotation = playerSpawnPoint.transform.rotation;
            Debug.Log("Player téléporté au point de spawn.");
        }
        else
        {
            Debug.LogWarning("Player ou PlayerSpawnPoint introuvable.");
        }
    }


    private void Start()
    {
        findExit();
    }

    private void Update()
    {
        phaseTimer += Time.deltaTime;

        if (phaseTimer >= phaseDuration)
        {
            phaseTimer = 0f;

            if (isInPhase2)
                SwitchToPhase1();
            else
                TriggerPhase2();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("[DEBUG] Activation manuelle de la Phase 2");
            phaseTimer = 0f;
            TriggerPhase2();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            Debug.Log("[DEBUG] Retour à la Phase 1");
            phaseTimer = 0f;
            SwitchToPhase1();
        }
    }

    public void findExit()
    {
        exitObject = GameObject.FindGameObjectWithTag("EndZone");

        if (exitObject != null)
        {
            exitObject.SetActive(false);
            Debug.Log("Exit object found and deactivated.");
        }
        else
        {
            Debug.LogWarning("Exit object not found in the scene.");
        }
    }

    public void InitGoalValue()
    {
        goalValue = UnityEngine.Random.Range(minGoalValue, maxGoalValue + 1);
        runItemValue = 0; 
        exitSpawned = false;
        isPostGoalPhase = false;
        Debug.Log("Nouvel objectif généré : " + goalValue);

        if (exitObject != null)
            exitObject.SetActive(false);
    }

    private void ActivateBonusVisuals()
    {
        CameraShake.Instance.Shake(1.0f, 0.5f);
        SoundManager.Instance.PlayGlobalOneShot(SoundManager.Instance.phase2Enemy);
    }


    #region Phase Management
    private void TriggerPhase2()
    {
        if (isInPhase2) return;

        isInPhase2 = true;
        OnPhaseChanged?.Invoke();
    }

    private void SwitchToPhase1()
    {
        if (!isInPhase2) return;

        isInPhase2 = false;
        OnPhaseChanged?.Invoke();
    }
    #endregion

    #region Money Management
    public void AddItemValue(int value)
    {
        if (isPostGoalPhase)
            value = Mathf.RoundToInt(value * 1.5f);

        runItemValue += value;
        totalItemValue += value;

        Debug.Log("Valeur ramassée (run): " + runItemValue + " | Total: " + totalItemValue);

        UIManager.Instance.UpdateGoalText(runItemValue, goalValue);
        UIManager.Instance.ShowItemValuePopup(value);

        if (!exitSpawned && runItemValue >= goalValue)
        {
            Debug.Log("Objectif atteint ! Apparition de la sortie.");
            exitSpawned = true;
            isPostGoalPhase = true;

            if (exitObject != null)
                exitObject.SetActive(true);

            OnPhaseChanged?.Invoke();
        }
    }

    public bool SpendCoins(int amount)
    {
        if (totalItemValue >= amount)
        {
            totalItemValue -= amount;
            Debug.Log("Achat effectué. Total restant : " + totalItemValue);
            UIManager.Instance.UpdateTotalCoinsText(totalItemValue);
            return true;
        }

        Debug.Log("Pas assez de pièces !");
        return false;
    }
    #endregion
}
