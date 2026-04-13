using TMPro;
using UnityEngine;

public class LeaderboardUI : MonoBehaviour
{
    public Transform contentParent;   // nơi chứa các dòng top rank
    public GameObject rowPrefab;      // Prefab một dòng leaderboard
    public Transform playerRow;       // Khung Player luôn hiển thị
    public int showTop = 10;          // số dòng top cần hiển thị


    public void RefreshUI()
    {
        // Xóa danh sách top cũ
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // Lấy dữ liệu
        var leaderboard = Leaderboard.Instance;
        var topList = leaderboard.GetTop(showTop);
        int playerRank = leaderboard.GetPlayerRank();
        var playerEntry = leaderboard.GetPlayerEntry();

        int displayRank = 1;

        // Render TOP N
        foreach (var entry in topList)
        {
            var row = Instantiate(rowPrefab, contentParent);
            var texts = row.GetComponentsInChildren<TextMeshProUGUI>();

            texts[0].text = displayRank.ToString();
            texts[1].text = entry.name;
            texts[2].text = entry.score.ToString();

            if (entry.isPlayer)
            {
                texts[1].color = Color.green; // highlight trong top
            }

            displayRank++;
        }

        // Luôn render PLAYER ROW riêng
        var pTxt = playerRow.GetComponentsInChildren<TextMeshProUGUI>();
        pTxt[0].text = playerRank.ToString();
        pTxt[1].text = playerEntry.name;
        pTxt[2].text = playerEntry.score.ToString();

        // màu player
        pTxt[1].color = Color.green;
    }
}
