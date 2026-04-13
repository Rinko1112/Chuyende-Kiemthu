using UnityEngine;
using Fusion;

public class GiantGolem : NetworkBehaviour
{
    [Header("PATROL")]
    [SerializeField] private Transform patrolCenter;
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float idleTime = 2f;

    [Header("DETECTION")]
    [SerializeField] private float detectRadius = 12f;
    [SerializeField] private float chaseRadius = 20f;
    [SerializeField] private float attackRange = 2.5f;

    [Header("MOVE")]
    [SerializeField] private float walkSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;

    [Header("COMBAT")]
    [SerializeField] private int damage = 30;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDelay = 0f; // 🔥 đúng yêu cầu

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip howlSound;
    [SerializeField] private AudioClip footstepSound;

    private Animator anim;
    private CharacterController controller;

    private Vector3 patrolTarget;
    private float idleTimer;

    private PlayerStats targetStats;
    private Transform targetPlayer;

    private float lastAttackTime;
    private float attackTimer;
    private bool hasDealtDamage;

    private float howlTimer;
    private float howlDuration = 1f;

    private float footstepTimer;

    private enum State
    {
        Idle,
        Walk,
        Run,
        Attack,
        Howl,
        Return,
        Dead
    }

    private State state = State.Idle;

    public override void Spawned()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        PickNewPatrolPoint();
    }

    [System.Obsolete]
    public override void FixedUpdateNetwork()
    {
        if (state == State.Dead) return;

        if (Object.HasStateAuthority)
        {
            DetectPlayer();

            switch (state)
            {
                case State.Idle: Idle(); break;
                case State.Walk: Walk(); break;
                case State.Run: Run(); break;
                case State.Attack: Attack(); break;
                case State.Howl: Howl(); break;
                case State.Return: Return(); break;
            }
        }
    }

    // ================= RPC =================
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetBool(string name, bool value)
    {
        anim.SetBool(name, value);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_SetTrigger(string name)
    {
        anim.SetTrigger(name);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    void RPC_PlaySound(string type)
    {
        if (audioSource == null) return;

        if (type == "howl" && howlSound != null)
            audioSource.PlayOneShot(howlSound);

        if (type == "step" && footstepSound != null)
            audioSource.PlayOneShot(footstepSound);
    }

    // ================= DETECT =================
    [System.Obsolete]
    void DetectPlayer()
    {
        if (state == State.Howl) return;

        // 🔥 FIX: bỏ target chết
        if (targetStats != null && targetStats.HP <= 0)
        {
            targetPlayer = null;
            targetStats = null;
        }

        if (targetPlayer != null)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);

            if (dist > chaseRadius)
            {
                targetPlayer = null;
                targetStats = null;
                state = State.Return;
            }
            return;
        }

        foreach (var p in FindObjectsByType<PlayerStats>(FindObjectsSortMode.None))
        {
            if (p.HP <= 0) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist <= detectRadius)
            {
                targetPlayer = p.transform;
                targetStats = p;

                StopMove();

                RPC_SetTrigger("howl");
                RPC_PlaySound("howl");

                howlTimer = 0f;
                state = State.Howl;
                return;
            }
        }
    }

    void Idle()
    {
        StopMove();

        idleTimer += Runner.DeltaTime;

        if (idleTimer >= idleTime)
        {
            idleTimer = 0f;
            PickNewPatrolPoint();
            state = State.Walk;
        }
    }

    void Walk()
    {
        Move(patrolTarget, walkSpeed);

        if (Vector3.Distance(transform.position, patrolTarget) < 1f)
            state = State.Idle;
    }

    void Run()
    {
        if (targetPlayer == null)
        {
            state = State.Return;
            return;
        }

        float dist = Vector3.Distance(transform.position, targetPlayer.position);

        if (dist <= attackRange)
        {
            state = State.Attack;
            attackTimer = 0f;
            hasDealtDamage = false;
            return;
        }

        Move(targetPlayer.position, runSpeed);
    }

    // ================= ATTACK =================
    void Attack()
    {
        if (targetPlayer == null || targetStats == null)
        {
            state = State.Return;
            return;
        }

        if (targetStats.HP <= 0)
        {
            targetPlayer = null;
            targetStats = null;
            state = State.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position, targetPlayer.position);

        if (dist > attackRange)
        {
            state = State.Run;
            return;
        }

        StopMove();

        if (Runner.SimulationTime - lastAttackTime > attackCooldown)
        {
            lastAttackTime = Runner.SimulationTime;

            RPC_SetTrigger("attack");

            attackTimer = 0f;
            hasDealtDamage = false;
        }

        attackTimer += Runner.DeltaTime;

        // 🔥 DELAY 1.2s
        if (!hasDealtDamage && attackTimer >= attackDelay)
        {
            targetStats.RPC_TakeDamage(damage);
            hasDealtDamage = true;
        }
    }

    void Howl()
    {
        StopMove();

        howlTimer += Runner.DeltaTime;

        if (howlTimer >= howlDuration)
            state = State.Run;
    }

    void Return()
    {
        Move(patrolCenter.position, runSpeed);

        if (Vector3.Distance(transform.position, patrolCenter.position) < 2f)
            state = State.Idle;
    }

    void Move(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position);
        dir.y = 0;

        if (dir.magnitude < 0.1f)
        {
            StopMove();
            return;
        }

        dir.Normalize();

        controller.Move(dir * speed * Runner.DeltaTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(dir),
            Runner.DeltaTime * 5f
        );

        RPC_SetBool("isWalking", speed == walkSpeed);
        RPC_SetBool("isRunning", speed == runSpeed);

        // 🔥 FOOTSTEP FIX
        footstepTimer += Runner.DeltaTime;

        float interval = speed == runSpeed ? 0.3f : 0.5f;

        if (footstepTimer >= interval)
        {
            RPC_PlaySound("step");
            footstepTimer = 0f;
        }
    }

    void StopMove()
    {
        RPC_SetBool("isWalking", false);
        RPC_SetBool("isRunning", false);
    }

    void PickNewPatrolPoint()
    {
        if (patrolCenter == null) patrolCenter = transform;

        Vector2 rand = Random.insideUnitCircle * patrolRadius;
        patrolTarget = patrolCenter.position + new Vector3(rand.x, 0, rand.y);
    }

    public void SetPatrol(Transform center, float radius)
    {
        patrolCenter = center;
        patrolRadius = radius;
    }

    public void Die()
    {
        state = State.Dead;

        RPC_SetTrigger("die");

        if (patrolCenter != null)
            Destroy(patrolCenter.gameObject);

        if (Object.HasStateAuthority)
            Runner.Despawn(Object);
    }
}