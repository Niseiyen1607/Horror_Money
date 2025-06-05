using UnityEngine;
using UnityEngine.AI;

public class StealthEnemy : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private Camera playerCamera;
    private NavMeshAgent agent;

    [Header("Detection Settings")]
    public float visionCheckInterval = 0.1f;
    public LayerMask obstacleMask;

    [Header("Audio")]
    private float enemyFootstepTimer = 0f;
    [SerializeField] private float footstepInterval = 0.2f;
    [SerializeField] private float distanceMaxVolume = 50f;
    [SerializeField] private float distancePhase2Scream = 50f;

    [Header("Movement Settings")]
    public float idleSpeed = 0f;
    public float sneakSpeed = 5f;
    public float chaseSpeed = 10f;
    public float phase2ChaseSpeed = 50f;
    public float postGoalChaseSpeed = 70f; 

    private float currentSpeed = 0f;

    [Header("Teleport Settings")]
    public float teleportMinDistance = 20f;
    public float teleportMaxDistance = 40f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float damageInterval = 0.3f;
    public int damageAmount = 90;

    private float damageTimer = 0f;
    private PlayerHealth playerHealth;

    [Header("Stun Settings")]
    private bool isStunned = false;
    private float stunTimer = 0f;
    public float stunDuration = 5f;

    private float timeSinceLastSeen = 0f;
    [SerializeField] private float scareSoundDelay = 20f;
    private bool scareSoundPlayed = false;

    public enum EnemyPhase { Phase1, Phase2 }
    public EnemyPhase currentPhase = EnemyPhase.Phase1;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = Camera.main;
        agent = GetComponent<NavMeshAgent>();
        InvokeRepeating(nameof(CheckVisibilityAndLineOfSight), 0f, visionCheckInterval);
        playerHealth = player.GetComponent<PlayerHealth>();
        GameManager.Instance.OnPhaseChanged += HandlePhaseChange;
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
                    SoundManager.Instance.PlayEnemyFootstep(transform.position, distanceMaxVolume);
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
                playerHealth.TakeDamage(damageAmount);
                CameraShake.Instance.Shake(0.3f, 0.2f);

                isStunned = true;
                stunTimer = stunDuration;
                currentSpeed = 0f;
            }
        }
        else
        {
            damageTimer = 0f;
        }
    }

    void CheckVisibilityAndLineOfSight()
    {
        if (GameManager.Instance.isPostGoalPhase)
        {
            currentSpeed = postGoalChaseSpeed;
            return;
        }

        if (currentPhase == EnemyPhase.Phase2)
        {
            currentSpeed = phase2ChaseSpeed;
            return;
        }

        Vector3 viewportPoint = playerCamera.WorldToViewportPoint(transform.position);
        bool isInCameraView = viewportPoint.z > 0 &&
                              viewportPoint.x > 0 && viewportPoint.x < 1 &&
                              viewportPoint.y > 0 && viewportPoint.y < 1;

        bool hasLineOfSight = !Physics.Linecast(player.position, transform.position, obstacleMask);

        if (isInCameraView && hasLineOfSight)
            currentSpeed = idleSpeed;
        else if (hasLineOfSight)
            currentSpeed = chaseSpeed;
        else
            currentSpeed = sneakSpeed;
    }

    public void FlashStun(Vector3 awayFromPosition, float stunTime = 3f, int maxAttempts = 10)
    {
        if (currentPhase == EnemyPhase.Phase2)
        {
            bool positionFound = false;
            NavMeshHit hit = new NavMeshHit();

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector3 randomDirection = Random.insideUnitSphere;
                randomDirection.y = 0;

                float randomDistance = Random.Range(teleportMinDistance, teleportMaxDistance);
                Vector3 candidatePos = player.position + randomDirection.normalized * randomDistance;

                if (NavMesh.SamplePosition(candidatePos, out hit, 5f, NavMesh.AllAreas))
                {
                    positionFound = true;
                    break;
                }
            }

            if (positionFound)
            {
                agent.Warp(hit.position);
            }

            isStunned = true;
            stunTimer = stunTime;
            agent.isStopped = true;
            SoundManager.Instance.PlayGlobalOneShot(SoundManager.Instance.enemyFlashStun);
        }
        else if (currentPhase == EnemyPhase.Phase1)
        {
            isStunned = true;
            stunTimer = stunTime * 0.5f;
            agent.isStopped = true;
            SoundManager.Instance.PlayGlobalOneShot(SoundManager.Instance.enemyFlashStun);
        }
    }

    #region Enemy Phase Handling
    private void HandlePhaseChange()
    {
        if (GameManager.Instance.isPostGoalPhase)
        {
            currentPhase = EnemyPhase.Phase2;
            currentSpeed = postGoalChaseSpeed;
            agent.speed = currentSpeed;
            SoundManager.Instance.PlayPhase2Enemy(transform.position, distancePhase2Scream);
        }
        else if (GameManager.Instance.isInPhase2)
        {
            SwitchToPhase2();
        }
        else
        {
            SwitchToPhase1();
        }
    }
    private void SwitchToPhase2()
    {
        currentPhase = EnemyPhase.Phase2;
        currentSpeed = phase2ChaseSpeed;
        agent.speed = currentSpeed;
        SoundManager.Instance.PlayPhase2Enemy(transform.position, distancePhase2Scream);
    }
    private void SwitchToPhase1()
    {
        currentPhase = EnemyPhase.Phase1;
        currentSpeed = sneakSpeed;
        agent.speed = currentSpeed;
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        bool hasClearView = !Physics.Linecast(player.position, transform.position, obstacleMask);
        Gizmos.color = hasClearView ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, player.position);
    }
}
