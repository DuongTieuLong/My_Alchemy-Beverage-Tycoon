using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;


public enum MoveMode { None, Entrance, Exit, Direct }
public class NPCController : MonoBehaviour
{
    public NavMeshAgent agent;
    public Animator anim;

    [Header("Behavior Chance")]
    [Range(0, 1f)]
    public float chanceBuy = 0.85f;
    // animation parameter names
    private static readonly int HashIsWalking = Animator.StringToHash("isWalking");

    private Vector3 currentTarget;
    private bool hasReportedArrival = false;

    // distinguish movement context so we don't interrupt Entrance/Exit coroutines
    public MoveMode moveMode = MoveMode.None;

    // thresholds
    private const float ARRIVE_EPSILON = 0.15f;    // tính tới đích
    private const float ANIM_VEL_SQR_THRESHOLD = 0.01f; // threshold cho animation

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateAnimation();

        if (agent == null) return;

        // Nếu agent đang tính đường thì chờ
        if (agent.pathPending) return;

        // Khi còn path, kiểm tra khoảng cách còn lại
        if (!hasReportedArrival && (agent.hasPath || moveMode == MoveMode.Direct))
        {
            float remaining = agent.remainingDistance;

            // remainingDistance có thể là Infinity khi không có path; kiểm tra an toàn
            if (!float.IsInfinity(remaining) && remaining <= Mathf.Max(agent.stoppingDistance + ARRIVE_EPSILON, 0.2f))
            {
                HandleArrival();
            }
            else
            {
                // thêm trường hợp: nếu desiredVelocity rất nhỏ nhưng remainingDistance ~ 0 (an toàn)
                if (!float.IsInfinity(remaining) && remaining <= ARRIVE_EPSILON)
                    HandleArrival();
            }
        }
    }

    // ---------------------------
    // ANIMATION STATE HANDLING
    // ---------------------------
    private void UpdateAnimation()
    {
        if (agent == null || anim == null) return;

        // Dùng desiredVelocity để tránh rung do rotation nhỏ trên chỗ
        Vector3 dv = agent.desiredVelocity;
        if (dv.sqrMagnitude > ANIM_VEL_SQR_THRESHOLD)
            anim.SetBool(HashIsWalking, true);
        else
            anim.SetBool(HashIsWalking, false);
    }

    private void HandleArrival()
    {
        // đảm bảo chỉ gọi 1 lần
        if (hasReportedArrival) return;
        hasReportedArrival = true;

        // Nếu đây là di chuyển trực tiếp (GoTo), dừng hẳn agent để tránh "nhích nhích"
        if (moveMode == MoveMode.Direct && agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;

            // snap chính xác về vị trí mục tiêu (chỉ snap nhẹ, tránh tele một khoảng lớn)
            // chỉ snap khi khoảng cách nhỏ (tránh dịch chuyển nhìn thấy được)
            if (Vector3.Distance(transform.position, currentTarget) <= 0.5f)
                transform.position = currentTarget;
        }

        // reset animation
        if (anim != null)
            anim.SetBool(HashIsWalking, false);

        // báo về QueueManager
        QueueManager.Instance?.OnNPCArrived(this, currentTarget);

        // nếu cần, đặt moveMode về None -- nhưng giữ nguyên nếu đang trong Entrance/Exit
        if (moveMode == MoveMode.Direct)
            moveMode = MoveMode.None;
    }

    // ---------------------------
    // ENTRANCE ROUTE
    // ---------------------------
    public void StartEntranceRoute(Transform[] route)
    {
        moveMode = MoveMode.Entrance;
        StartCoroutine(FollowEntranceRoute(route));
    }

    private IEnumerator FollowEntranceRoute(Transform[] route)
    {
        if (route == null || route.Length == 0)
        {
            QueueManager.Instance?.RegisterNPC(this);
            moveMode = MoveMode.None;
            yield break;
        }

        foreach (Transform point in route)
        {
            if (point == null)
            {
                continue;
            }

            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(point.position);
            }

            currentTarget = point.position;
            hasReportedArrival = false;

            // chờ tới khi gần đủ đến điểm (dùng vị trí thực để bền hơn)
            while (Vector3.Distance(transform.position, point.position) > 0.5f)
                yield return null;
        }

        // kết thúc route
        moveMode = MoveMode.None;

        float r = UnityEngine.Random.value;

        if (r <= chanceBuy)
        {
            // NPC tham gia hàng
            QueueManager.Instance?.RegisterNPC(this);
        }
        else
        {
            // NPC bỏ đi luôn – chạy ExitRoute
           StartExitRoute(ExitRouteHolder.Instance.exitRoute);
        }
    }

    // ---------------------------
    // DIRECT MOVE
    // ---------------------------
    public void GoTo(Vector3 pos)
    {
        if (agent == null) return;

        moveMode = MoveMode.Direct;
        hasReportedArrival = false;
        currentTarget = pos;

        agent.isStopped = false;
        agent.SetDestination(pos);
    }

    // ---------------------------
    // EXIT ROUTE
    // ---------------------------
    public void StartExitRoute(Transform[] route)
    {
        moveMode = MoveMode.Exit;
        StartCoroutine(FollowExitRoute(route));
    }

    private IEnumerator FollowExitRoute(Transform[] route)
    {
        if (route == null || route.Length == 0)
        {
            ReturnToPool();
            moveMode = MoveMode.None;
            yield break;
        }

        foreach (Transform point in route)
        {
            if (point == null) continue;

            if (agent != null)
            {
                agent.isStopped = false;
                agent.SetDestination(point.position);
            }

            currentTarget = point.position;
            hasReportedArrival = false;

            while (Vector3.Distance(transform.position, point.position) > 0.5f)
                yield return null;
        }

        ReturnToPool();
        moveMode = MoveMode.None;
    }

    // ---------------------------
    // RETURN TO POOL
    // ---------------------------
    public void ReturnToPool()
    {
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }

        // Reset animation
        if (anim != null)
            anim.SetBool(HashIsWalking, false);

        if (NPCManager.Instance != null)
        {
            transform.position = NPCManager.Instance.poolIdlePoint.position;
            NPCManager.Instance.RemoveActive(this);
        }

        gameObject.SetActive(false);
    }
}
