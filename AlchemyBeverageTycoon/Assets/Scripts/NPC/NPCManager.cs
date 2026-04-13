using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance;

    [Header("NPC Settings")]
    public bool start_SpawnNPC = false;
    public Transform spawnPoint;
    public Transform poolIdlePoint;

    [Tooltip("Danh sách NPC có sẵn trong scene để bóc ra")]
    public Transform npcPoolContent;
    public List<Transform> npcPools = new List<Transform>();
    public List<NPCController> activeNPCs = new List<NPCController>();

  
    public int minDelaySpawn = 10;
    public int maxDelaySpawn = 30;

    private float timer = 0f;
    private float nextDelay = 0f;

    [Header("Entrance Route")]
    public Transform[] entranceRoute;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        TimeCycleManager.Instance.OnPeriodChanged += ResetOnNewPeriod;

        foreach (Transform t in npcPoolContent)
            npcPools.Add(t);

        ShufflePool();

        nextDelay = GetRankAdjustedDelay();
    }

    private void Update()
    {
        if (start_SpawnNPC && TimeCycleManager.Instance.CurrentPeriod == TimePeriod.Day)
            AutoSpawn();
    }

    public void ResetOnNewPeriod(TimePeriod period)
    {
        
        if(period == TimePeriod.Day)
        {
            QueueManager.Instance.currentCustomer = null;
        }
        else if(period == TimePeriod.Night)
        {
            foreach(var npc in activeNPCs)
            {
                npc.StopAllCoroutines();
                npc.StartExitRoute(ExitRouteHolder.Instance.exitRoute);
            }
            activeNPCs.Clear();
            QueueManager.Instance.OnEndOfDay();
        }
    }

    // ---------------------------------------------------------
    // RANDOMLY SPAWN FROM POOL (NO INSTANTIATE)
    // ---------------------------------------------------------
    public void SpawnNPCs(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Transform npcTf = GetRandomAvailableNPCByRank();

            if (npcTf == null)
            {
                return;
            }

            npcTf.gameObject.SetActive(true);
            npcTf.position = spawnPoint.position;
            npcTf.rotation = Quaternion.identity;

            NPCController npc = npcTf.GetComponent<NPCController>();
            activeNPCs.Add(npc);

            npc.StartEntranceRoute(entranceRoute);
            npc.GetComponent<NPCInteract>().npcRequest.potionCountRequest = (int)Random.Range(1, GetPotinBuyMultiplier(ReputationManager.Instance.currentRank));
        }
    }

    public void RemoveActive(NPCController npcController)
    {
        activeNPCs.Remove(npcController);
    }

    // ---------------------------------------------------------
    // AUTO SPAWN LOOP
    // ---------------------------------------------------------
    void AutoSpawn()
    {
       // if (spawnedCount >= maxNpcs_aday) return;

        timer += Time.deltaTime;

        if (timer >= nextDelay)
        {
            timer = 0;
            nextDelay = GetRankAdjustedDelay();

            SpawnNPCs(1);
        }
    }

    // ---------------------------------------------------------
    // GET RANDOM NPC & SHUFFLE
    // ---------------------------------------------------------
    void ShufflePool()
    {
        for (int i = 0; i < npcPools.Count; i++)
        {
            int rand = Random.Range(0, npcPools.Count);
            (npcPools[i], npcPools[rand]) = (npcPools[rand], npcPools[i]);
        }
    }

    public float GetSpawnMultiplier(Rank rank)
    {
        switch (rank)
        {
            case Rank.Apprentice: return 1.0f;   // 10–20s
            case Rank.Adept: return 0.8f;   // 8–16s
            case Rank.Expert: return 0.6f;   // 6–12s
            case Rank.Master: return 0.45f;  // 4.5–9s
            case Rank.Grandmaster: return 0.35f;  // 3.5–7s
            default: return 1.0f;
        }
    }
    public float GetPotinBuyMultiplier(Rank rank)
    {
        switch (rank)
        {
            case Rank.Apprentice: return 2;   // 10–20s
            case Rank.Adept: return 3;   // 8–16s
            case Rank.Expert: return 5;   // 6–12s
            case Rank.Master: return 7;  // 4.5–9s
            case Rank.Grandmaster: return 10;  // 3.5–7s
            default: return 1.0f;
        }
    }

    float GetRankAdjustedDelay()
    {
        float multiplier = GetSpawnMultiplier(ReputationManager.Instance.currentRank);
        float baseDelay = Random.Range(minDelaySpawn, maxDelaySpawn);
        return baseDelay * multiplier;
    }

    Transform GetRandomAvailableNPCByRank()
    {
        Rank currentRank = ReputationManager.Instance.currentRank;

        List<NPCController> available = new();

        foreach (var t in npcPools)
        {
            if (t.gameObject.activeSelf) continue;

            NPCController controller = t.GetComponent<NPCController>();
            if (controller == null) continue;

            var interact = controller.GetComponent<NPCInteract>();
            if (interact == null) continue;

            var request = interact.npcRequest;
            if (request == null) continue;

            if (request.npcRank > currentRank) continue;

            available.Add(controller);
        }

        if (available.Count == 0) return null;

        // Weighted selection
        float totalWeight = 0f;
        List<float> weights = new();

        foreach (var npc in available)
        {
            float w = 1f;

            var req = npc.GetComponent<NPCInteract>().npcRequest;

            // boost weight if matches event
            if (MarketTrendManager.Instance != null &&
                MarketTrendManager.Instance.IsElementBoosted(req.requestedElement))
            {
                w *= 0.8f;
            }

            weights.Add(w);
            totalWeight += w;
        }

        float rand = Random.Range(0, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < available.Count; i++)
        {
            cumulative += weights[i];
            if (rand <= cumulative)
            {
                return available[i].transform;
            }
        }
        return available[available.Count - 1].transform;
    }

}
