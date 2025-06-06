using UnityEngine;
using UnityEngine.AI;

public class StealthEnemy : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private Camera playerCamera;
    private NavMeshAgent agent;
    [SerializeField] private Animator enemyAnimator;
    private PlayerHealth playerHealth;

    [Header("Movement Settings")]
    public float idleSpeed = 0f;
    public float sneakSpeed = 5f;
    public float chaseSpeed = 10f;
    public float phase2ChaseSpeed = 50f;
    public float postGoalChaseSpeed = 70f;
    private float currentSpeed = 0f;
    private bool isPhase2Transitioning = false;

    [Header("Detection Settings")]
    public float visionCheckInterval = 0.1f;
    public LayerMask obstacleMask;
    private float noClearViewTimer = 0f;
    private float timeBeforeJumpscare = 5f;
    private bool hadNoClearViewLongEnough = false;
    private bool hadClearViewLastFrame = false;

    [Header("Teleport Settings")]
    public float teleportMinDistance = 20f;
    public float teleportMaxDistance = 40f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float damageInterval = 0.3f;
    public int damageAmount = 90;
    private float damageTimer = 0f;

    [Header("Stun Settings")]
    public float stunDuration = 5f;
    private bool isStunned = false;
    private float stunTimer = 0f;

    [Header("Audio")]
    [SerializeField] private float footstepInterval = 0.2f;
    [SerializeField] private float distanceMaxVolume = 50f;
    [SerializeField] private float distancePhase2Scream = 50f;
    private float enemyFootstepTimer = 0f;

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
        if (isPhase2Transitioning)
        {
            agent.isStopped = true;
            enemyAnimator.SetBool("isWalking", false);
            return;
        }

        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            agent.isStopped = true;

            enemyAnimator.SetBool("isWalking", false);

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

        enemyAnimator.SetBool("isWalking", currentSpeed > 0f);

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

                enemyAnimator.SetTrigger("Attack");

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
        bool hasClearView = isInCameraView && hasLineOfSight;

        if (!hasClearView)
        {
            noClearViewTimer += visionCheckInterval;

            if (noClearViewTimer >= timeBeforeJumpscare)
            {
                hadNoClearViewLongEnough = true;
            }
        }
        else
        {
            if (hadNoClearViewLongEnough && !hadClearViewLastFrame)
            {
                SoundManager.Instance.PlayGlobalOneShot(SoundManager.Instance.stealthJumpscareSounds);
            }

            noClearViewTimer = 0f;
            hadNoClearViewLongEnough = false;
        }

        hadClearViewLastFrame = hasClearView;

        if (hasClearView)
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
            currentSpeed = sneakSpeed;
            agent.speed = currentSpeed;
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

        SoundManager.Instance.PlayPhase2Enemy(transform.position, distancePhase2Scream);

        isPhase2Transitioning = true;
        agent.isStopped = true;
        currentSpeed = 0f;
        enemyAnimator.SetBool("isWalking", false);

        enemyAnimator.SetTrigger("EnterPhase2");

        Invoke(nameof(OnPhase2TransitionComplete), 2f);
    }


    private void SwitchToPhase1()
    {
        currentPhase = EnemyPhase.Phase1;
        currentSpeed = sneakSpeed;
        agent.speed = currentSpeed;
    }

    public void OnPhase2TransitionComplete()
    {
        isPhase2Transitioning = false;
        currentSpeed = phase2ChaseSpeed;
        agent.speed = currentSpeed;
        agent.isStopped = false;
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
