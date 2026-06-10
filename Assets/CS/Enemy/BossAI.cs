using UnityEngine;
using System.Collections;

/// <summary>
/// 王座 Boss 专用 AI
/// 阶段1 — 闪现撞击 + 触碰伤害
/// 阶段2 — 闪更快 + 落地冲击波
/// </summary>
public class BossAI : MonoBehaviour
{
    [Header("移动")]
    public float moveSpeed = 0.8f;
    [Tooltip("二阶段移动速度")]
    public float phase2MoveSpeed = 1.2f;
    public float stoppingDistance = 0.3f;
    public float rotationSpeed = 180f;

    [Header("玩家引用")]
    public Transform player;

    [Header("触碰伤害")]
    [Tooltip("碰到玩家时每次扣多少血")]
    public float contactDamage = 20f;
    [Tooltip("触碰冷却（秒）")]
    public float contactCooldown = 1f;

    [Header("闪现 — 阶段1")]
    [Tooltip("闪现冷却（秒）")]
    public float teleportCooldown = 6f;
    [Tooltip("闪现到离玩家多远")]
    public float teleportOffset = 1.5f;
    [Tooltip("闪现前预警时间")]
    public float teleportWarmup = 0.8f;

    [Header("闪现 — 阶段2")]
    [Tooltip("二阶段闪现冷却（秒）")]
    public float phase2TeleportCooldown = 3.5f;
    [Tooltip("二阶段闪现伤害倍率")]
    public float phase2DamageMultiplier = 2f;

    [Header("冲击波 — 阶段2")]
    [Tooltip("落地后冲击波半径")]
    public float shockwaveRadius = 3f;
    [Tooltip("冲击波伤害")]
    public float shockwaveDamage = 25f;

    [Header("阶段")]
    [Range(0.1f, 1f)]
    public float phase2Threshold = 0.5f;

    [Header("特效（可选）")]
    public GameObject teleportEffectPrefab;

    private EnemyHealth health;
    private bool isDead;
    private bool inPhase2;
    private float lastContactTime;
    private float lastTeleportTime;
    private bool isTeleporting;

    private void Awake()
    {
        health = GetComponent<EnemyHealth>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Start()
    {
        lastTeleportTime = Time.time + 2f;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        if (!inPhase2 && health != null && health.currentHealth <= health.maxHealth * phase2Threshold)
        {
            EnterPhase2();
        }

        // 慢速追踪 + 面朝玩家
        float dist = Vector3.Distance(transform.position, player.position);
        if (!isTeleporting && dist > stoppingDistance)
        {
            ChasePlayer();
        }

        // 闪现
        float cd = inPhase2 ? phase2TeleportCooldown : teleportCooldown;
        if (!isTeleporting && Time.time - lastTeleportTime >= cd)
        {
            StartCoroutine(TeleportRoutine());
        }
    }

    private void ChasePlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }

        float spd = inPhase2 ? phase2MoveSpeed : moveSpeed;
        transform.position = Vector3.MoveTowards(transform.position, player.position, spd * Time.deltaTime);
    }

    // ---------------------------------------------------------------
    //  触碰伤害
    // ---------------------------------------------------------------
    private void OnTriggerStay(Collider other)
    {
        if (isDead || isTeleporting) return;
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastContactTime < contactCooldown) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        lastContactTime = Time.time;
        ph.TakeDamage(contactDamage);
    }

    // ---------------------------------------------------------------
    //  闪现
    // ---------------------------------------------------------------
    private IEnumerator TeleportRoutine()
    {
        isTeleporting = true;
        lastTeleportTime = Time.time;

        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(teleportWarmup);

        if (isDead) { isTeleporting = false; yield break; }

        // 闪现到玩家旁边
        Vector3 dir = Random.insideUnitSphere;
        dir.y = 0;
        if (dir.magnitude < 0.01f) dir = Vector3.forward;
        Vector3 targetPos = player.position + dir.normalized * teleportOffset;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(targetPos, out hit, 5f, UnityEngine.AI.NavMesh.AllAreas))
            targetPos = hit.position;

        transform.position = targetPos;

        if (teleportEffectPrefab != null)
            Instantiate(teleportEffectPrefab, targetPos, Quaternion.identity);

        // 闪现撞击伤害
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= teleportOffset + 1.5f)
        {
            float dmg = inPhase2 ? contactDamage * phase2DamageMultiplier : contactDamage * 1.5f;
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(dmg);
        }

        // 二阶段：落地冲击波
        if (inPhase2)
        {
            StartCoroutine(Shockwave());
        }

        isTeleporting = false;
    }

    // ---------------------------------------------------------------
    //  冲击波（二阶段，落地后 0.3s 炸）
    // ---------------------------------------------------------------
    private IEnumerator Shockwave()
    {
        yield return new WaitForSeconds(0.3f);

        Collider[] hits = Physics.OverlapSphere(transform.position, shockwaveRadius);
        foreach (var c in hits)
        {
            if (c.CompareTag("Player"))
            {
                PlayerHealth ph = c.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(shockwaveDamage);
                break;
            }
        }
    }

    // ---------------------------------------------------------------
    //  阶段 + 死亡
    // ---------------------------------------------------------------
    private void EnterPhase2()
    {
        inPhase2 = true;
        Debug.Log("[BossAI] 进入二阶段 — 闪得更快 + 落地冲击波！");
    }

    public void Die()
    {
        isDead = true;
        StopAllCoroutines();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, shockwaveRadius);
    }
}
