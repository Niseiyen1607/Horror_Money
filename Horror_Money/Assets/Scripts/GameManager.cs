using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int totalItemValue = 0;

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

    public void AddItemValue(int value)
    {
        totalItemValue += value;
        Debug.Log("Valeur totale ramassée : " + totalItemValue);
    }
}
