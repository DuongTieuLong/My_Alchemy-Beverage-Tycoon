using UnityEngine;
using System;


public enum Rank
{
    Apprentice,
    Adept,
    Expert,
    Master,
    Grandmaster
}

[CreateAssetMenu(fileName = "New Expedition", menuName = "Expedition/Expedition Military")] 
public class ExpeditionMilitary : ScriptableObject
{
    public string expeditionMilitaryName;
    public int troopCount;
    public int maxTroopCapacity;
    public float totalPower;
    public Rank expeditionRequired;
    public int valueToUnlock;
    public bool isInExpedition;
    public bool isUnlocked;
    public Sprite icon;
    // Cấu hình cơ bản
    public  float basePowerPerTroop = 10f; // mỗi quân cơ bản có 10 sức mạn
    public int trooptPerLevel = 50; // chi phí quân mỗi cấp độ


    // Tính sức mạnh dựa trên quân số và cấp độ
    public float CalculatePower()
    {
      return  totalPower = troopCount * basePowerPerTroop;
    }

    // Thêm quân
    public void AddTroops(int amount)
    {
        if (amount <= 0) return;
        troopCount += amount;
        CalculatePower();
    }

    // Trừ quân
    public void RemoveTroops(int amount)
    {
        if (amount <= 0) return;
        troopCount = Mathf.Max(0, troopCount - amount);
        CalculatePower();
    }

    // Thông tin đoàn
    public override string ToString()
    {
        return $"[Đoàn: {expeditionMilitaryName}] | | Quân: {troopCount} | Sức mạnh: {totalPower}";
    }
}
