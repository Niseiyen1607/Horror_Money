using UnityEngine;
using UnityEngine.AI;

public class StealthEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Camera playerCamera;
    private NavMeshAgent agent;

    [Header("Detection Settings")]
    public float visionCheckInterval = 0.1f;
    public LayerMask obstacleMask;

    [Header("Audio")]
    private float enemyFootstepTimer = 0f;
    [SerializeField] private float footstepInterval = 0.2f;

    [Header("Movement Settings")]
    public float idleSpeed = 0f;
    public float sneakSpeed = 2f;
    public float chaseSpeed = 10f;

    private float currentSpeed = 0f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float damageInterval = 0.3f;
    public int damageAmount = 90;

    private float damageTimer = 0f;
    private PlayerHealth playerHealth;

    private bool isStunned = false;
    private float stunTimer = 0f;
    public float stunDuration = 1f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating(nameof(CheckVisibilityAndLineOfSight), 0f, visionCheckInterval);
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            agent.isStopped = true;  
            if (stunTimer <= 0f)
            {
                isStunned = false;
                agent.isStopped = false;
            }
            else
            {
                return; 
            }
        }
        else
        {
            agent.isStopped = false;
        }

        agent.speed = currentSpeed;

        if (currentSpeed > 0f)
        {
            agent.SetDestination(player.position);

            if (agent.velocity.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

                enemyFootstepTimer += Time.deltaTime;
                if (enemyFootstepTimer >= footstepInterval)
                {
                    enemyFootstepTimer = 0f;
                    SoundManager.Instance.PlayEnemyFootstep(transform.position);
                }
            }
            else
            {
                enemyFootstepTimer = 0f;
            }
        }
        else
        {
            agent.SetDestination(transform.position);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange && currentSpeed > 0f)
        {
            if (playerHealth.isDead) return;

            damageTimer += Time.deltaTime;
            if (damageTimer >= damageInterval)
            {
                damageTimer = 0f;
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damageAmount);
                    CameraShake.Instance.Shake(0.3f, 0.2f);

                    // Déclenche l'immobilisation après attaque
                    isStunned = true;
                    stunTimer = stunDuration;
                    currentSpeed = 0f;
                }
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    void CheckVisibilityAndLineOfSight()
    {
        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);
        bool isInCameraView = viewportPoint.z > 0 &&
                              viewportPoint.x > 0 && viewportPoint.x < 1 &&
                              viewportPoint.y > 0 && viewportPoint.y < 1;

        bool hasLineOfSight = !Physics.Linecast(player.position, transform.position, obstacleMask);

        if (isInCameraView && hasLineOfSight)
        {
            currentSpeed = idleSpeed;
        }
        else if (hasLineOfSight)
        {
            currentSpeed = chaseSpeed;
        }
        else
        {
            currentSpeed = sneakSpeed;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        bool hasClearView = !Physics.Linecast(player.position, transform.position, obstacleMask);
        Gizmos.color = hasClearView ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
