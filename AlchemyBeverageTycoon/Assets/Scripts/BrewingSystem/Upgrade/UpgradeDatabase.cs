using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Alchemy/UpgradeDatabase")]
public class UpgradeDatabase : ScriptableObject
{
    public List<UpgradeData> upgrades;
}

[Serializable]
public class UpgradeData
{
    public string id;             // Khóa duy nhất để lưu
    public int addItem;           // + số lượng vật phẩm
    public int addHeat;           // + nhiệt độ
    public int price;             // Giá nâng cấp
    public bool purchased;        // Đã mua hay chưa
}