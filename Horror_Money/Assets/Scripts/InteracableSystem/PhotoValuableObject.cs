using UnityEngine;

public class PhotoValuableObject : MonoBehaviour
{
    [Header("Valeur de base aléatoire")]
    public int minPhotoValue = 30;
    public int maxPhotoValue = 80;

    [Header("Diminution de la valeur")]
    public float valueDecayFactor = 0.7f;

    [HideInInspector] public int photoCount = 0;
    private int initialPhotoValue;

    private void Start()
    {
        initialPhotoValue = Random.Range(minPhotoValue, maxPhotoValue + 1);
    }

    public int GetCurrentPhotoValue()
    {
        float currentValue = initialPhotoValue * Mathf.Pow(valueDecayFactor, photoCount);
        return Mathf.Max(1, Mathf.RoundToInt(currentValue)); 
    }

    public int RegisterPhoto()
    {
        int valueToGive = GetCurrentPhotoValue();
        photoCount++;
        return valueToGive;
    }
}
