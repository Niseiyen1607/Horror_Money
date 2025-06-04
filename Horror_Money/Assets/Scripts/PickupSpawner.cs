using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] pickupPrefabs;
    public Transform[] spawnPoints;
    public int minPickupsPerGroup = 3;
    public int maxPickupsPerGroup = 6;
    public float forceMin = 3f;
    public float forceMax = 7f;
    public float upwardForce = 2f;
    public float torqueForce = 5f;

    [ContextMenu("Spawn Pickups")]
    public void SpawnPickups()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            int pickupCount = Random.Range(minPickupsPerGroup, maxPickupsPerGroup + 1);

            for (int i = 0; i < pickupCount; i++)
            {
                GameObject prefab = pickupPrefabs[Random.Range(0, pickupPrefabs.Length)];
                GameObject pickup = Instantiate(prefab, spawnPoint.position, Quaternion.identity);

                Rigidbody rb = pickup.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 direction = new Vector3(
                        Random.Range(-1.5f, 1.5f),
                        Random.Range(0.5f, 1.5f),
                        Random.Range(-1.5f, 1.5f)
                    ).normalized;

                    float force = Random.Range(forceMin, forceMax);
                    rb.AddForce(direction * force, ForceMode.Impulse);

                    Vector3 randomTorque = new Vector3(
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f),
                        Random.Range(-1f, 1f)
                    ) * torqueForce;

                    rb.AddTorque(randomTorque, ForceMode.Impulse);
                }
            }
        }
    }

    private void Start()
    {
        SpawnPickups();
    }
}
