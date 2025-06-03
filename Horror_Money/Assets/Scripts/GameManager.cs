using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalItemValue = 0;
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
        totalItemValue += value;
        Debug.Log("Valeur totale ramassée : " + totalItemValue);
    }
}
