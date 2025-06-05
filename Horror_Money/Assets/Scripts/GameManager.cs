using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Item Collection")]
    public int totalItemValue = 0;

    [SerializeField] private int goalValue;
    [SerializeField] private int minGoalValue = 200;
    [SerializeField] private int maxGoalValue = 1300;

    [Header("Exit Object")]
    [SerializeField] private GameObject exitObject;
    private bool exitSpawned = false;

    [Header("Game State")]
    public bool isPostGoalPhase = false;

    [Header("Phase 2")]
    public float phaseDuration = 300f;
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

    private void Start()
    {
        goalValue = UnityEngine.Random.Range(minGoalValue, maxGoalValue + 1);
        Debug.Log("Valeur cible à atteindre : " + goalValue);

        if (exitObject != null)
            exitObject.SetActive(false);

        UIManager.Instance.UpdateGoalText(totalItemValue, goalValue);
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

    //For enemy phase 2 activation
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

    public void AddItemValue(int value)
    {
        if (isPostGoalPhase)
            value = Mathf.RoundToInt(value * 1.5f);

        totalItemValue += value;
        Debug.Log("Valeur totale ramassée : " + totalItemValue);

        UIManager.Instance.UpdateGoalText(totalItemValue, goalValue);
        UIManager.Instance.ShowItemValuePopup(value);

        if (!exitSpawned && totalItemValue >= goalValue)
        {
            Debug.Log("Objectif atteint ! Apparition de la sortie.");
            exitSpawned = true;
            isPostGoalPhase = true;

            if (exitObject != null)
                exitObject.SetActive(true);

            OnPhaseChanged?.Invoke();
        }
    }

}
