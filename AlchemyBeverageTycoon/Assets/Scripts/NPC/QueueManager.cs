using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using System.Linq;
using System;

public class QueueManager : MonoBehaviour
{
    public static QueueManager Instance;

    [Header("Queue Settings")]
    public Transform counterPoint;
    public Transform[] queuePoints;

    [Header("Runtime Settings")]
    public int maxQueueSize = 10;

    private Queue<NPCController> queueList = new Queue<NPCController>();
    public NPCController currentCustomer = null;

    private void Awake()
    {
        Instance = this;
    }


    private void Start()
    {
        TimeCycleManager.Instance.OnPeriodChanged += EndOfDay;
    }

    public void EndOfDay(TimePeriod period)
    {
        if (period == TimePeriod.Night)
        {
            foreach (NPCController controller in queueList)
            {
                if (ExitRouteHolder.Instance != null && ExitRouteHolder.Instance.exitRoute != null)
                    controller.StartExitRoute(ExitRouteHolder.Instance.exitRoute);
                else
                    Destroy(controller.gameObject, 0.1f);
            }
            queueList.Clear();
        }

    }
    /// <summary>
    /// Register NPC to stand in queue.
    /// If queue is full the NPC will be sent to exit route immediately.
    /// </summary>
    public void RegisterNPC(NPCController npc)
    {
        if (npc == null) return;

        int active = GetActiveCount();
        if (active >= maxQueueSize)
        {
            // politely send away
            if (ExitRouteHolder.Instance != null && ExitRouteHolder.Instance.exitRoute != null)
                npc.StartExitRoute(ExitRouteHolder.Instance.exitRoute);
            else
                Destroy(npc.gameObject, 0.1f);
            return;
        }

        NPCManager.Instance.RemoveActive(npc);
        queueList.Enqueue(npc);
        UpdateQueuePositions();
    }

    /// <summary>
    /// Called when BuyingCounter/Shop finished serving a customer.
    /// The NPC will take exit route and be removed from the queue.
    /// </summary>
    public void OnCustomerDone(NPCController npc)
    {
        if (npc == null)
        {
            return;
        }

        // If currentCustomer is not null and matches, dequeue and send to exit.
        if (currentCustomer == npc)
        {
            // Dequeue the served customer if still in queue
            if (queueList.Count > 0 && queueList.Peek() == npc)
                queueList.Dequeue();
            currentCustomer = null;

            // send to exit route
            if (ExitRouteHolder.Instance != null && ExitRouteHolder.Instance.exitRoute != null)
                npc.StartExitRoute(ExitRouteHolder.Instance.exitRoute);
            else
                Destroy(npc.gameObject, 0.1f);

            UpdateQueuePositions();
        }
        else
        {
            // If served npc is not currentCustomer (edge case), just remove if present
            if (queueList.Contains(npc))
            {
                var list = queueList.ToList();
                list.Remove(npc);
                queueList = new Queue<NPCController>(list);
                npc.StartExitRoute(ExitRouteHolder.Instance?.exitRoute);
                UpdateQueuePositions();
            }
        }
    }
    public void OnEndOfDay()
    {
        currentCustomer?.GetComponent<NPCInteract>().ClosePanel();
        currentCustomer = null;
        ShopManager.Instance.EndOfDay();
        foreach (NPCController npc in queueList)
        {
            if (npc.moveMode == MoveMode.Exit) continue;
            npc.StartExitRoute(ExitRouteHolder.Instance.exitRoute);
        }
    }

    /// <summary>
    /// Called by NPCController when it reaches its destination.
    /// We use this to notify BuyingCounter when the current customer arrives at the counter.
    /// </summary>
    public void OnNPCArrived(NPCController npc, Vector3 arrivedPosition)
    {
        if (npc == null) return;

        // Kiểm tra NPC đúng là khách đang đi tới quầy
        if (currentCustomer != null && npc == currentCustomer)
        {
            // Kiểm tra vị trí THỰC của NPC thay vì arrivedPosition
            if (counterPoint != null)
            {
                float dist = Vector3.Distance(npc.transform.position, counterPoint.position);

                if (dist <= 0.5f)
                {
                    // đảm bảo đứng yên
                    npc.agent.isStopped = true;
                    npc.agent.ResetPath();
                    npc.agent.velocity = Vector3.zero;

                    BuyingCounter.Instance?.OnCustomer(npc);
                }
            }
        }
    }


    private void UpdateQueuePositions()
    {
        NPCController[] arr = queueList.ToArray();

        // Move first (if any) to counter if empty
        if (currentCustomer == null && arr.Length > 0)
        {
            currentCustomer = arr[0];
            currentCustomer.GoTo(counterPoint.position);
        }

        // Move others to queue points
        for (int i = 1; i < arr.Length; i++)
        {
            // queuePoints are positions for 2nd,3rd... (index 0 -> first waiting)
            if (i - 1 < queuePoints.Length)
            {
                var target = queuePoints[i - 1].position;
                arr[i].GoTo(target);
            }
            else
            {
                // No queue point available (shouldn't happen if maxQueueSize <= queuePoints.Length + 1)
                // keep them slightly behind last point
                Vector3 fallback = queuePoints.Length > 0 ? queuePoints[queuePoints.Length - 1].position + Vector3.back * (i - queuePoints.Length) : counterPoint.position + Vector3.back * (i + 1);
                arr[i].GoTo(fallback);
            }
        }
    }

    /// <summary>
    /// Utility: how many NPCs are active in queue system (waiting + being served)
    /// </summary>
    public int GetActiveCount()
    {
        int current = currentCustomer != null ? 1 : 0;
        return queueList.Count + (currentCustomer != null && queueList.Count > 0 && queueList.Peek() == currentCustomer ? 0 : 0) + (currentCustomer != null ? 0 : 0);
    }

    // Expose count directly (simpler and reliable)
    public int GetRegisteredCount() => queueList.Count;


}
