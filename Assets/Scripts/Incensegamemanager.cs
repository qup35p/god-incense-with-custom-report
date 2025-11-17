using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class IncenseGameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI powerValueText;
    public TextMeshProUGUI systemMessageText;
    public GameObject popupWindow1; // 正確時顯示（有勾勾）
    public GameObject popupWindow2; // 錯誤時顯示（有叉叉）
    public TextMeshProUGUI popupText1; // 正確視窗的文字
    public TextMeshProUGUI popupText2; // 錯誤視窗的文字
    public Image backgroundImage;

    [Header("Game Over UI")]
    public GameObject gamePlayUI; // 遊戲進行時的UI
    public GameObject gameOverUI; // 遊戲結束後的UI
    public GameObject resultIncense; // 結果畫面的香
    public TextMeshProUGUI angleDisplayText; // 顯示角度的文字

    [Header("Game Settings")]
    public float gameTime = 30f;
    public int initialPowerValue = 60;
    public int correctIncenseReward = 5;
    public int wrongIncensePenalty = 5;
    public float popupDuration = 1f;

    [Header("Result Settings")]
    public float receivedAngle = 87.5f; // 假的接收角度，之後會從另一台電腦接收

    [Header("Incense Settings")]
    public GameObject incensePrefab;
    public Transform incenseContainer;
    public Vector2 holderPosition = new Vector2(0, -100);

    private float currentTime;
    private int currentPowerValue;
    private List<IncenseStick> incenseSticks = new List<IncenseStick>();
    private int correctIncenseCount = 5;
    private int totalIncenseCount = 20;
    private bool gameActive = true;

    void Start()
    {
        InitializeGame();
    }

    void InitializeGame()
    {
        currentTime = gameTime;
        currentPowerValue = initialPowerValue;
        gameActive = true;

        // 顯示遊戲UI，隱藏結果UI
        if (gamePlayUI) gamePlayUI.SetActive(true);
        if (gameOverUI) gameOverUI.SetActive(false);

        UpdateUI();
        // 移除CreateIncenseHolder()，不生成香爐
        GenerateIncenseSticks();

        systemMessageText.text = "系統提示:以上為客戶插的香,香是否歪斜可判斷客戶的心正不正,請點擊找出正直的香";
    }

    void Update()
    {
        if (gameActive)
        {
            currentTime -= Time.deltaTime;
            UpdateTimer();

            if (currentTime <= 0)
            {
                GameOver();
            }
        }
    }


    void GenerateIncenseSticks()
    {
        // 清除現有的香
        foreach (IncenseStick incense in incenseSticks)
        {
            if (incense != null)
                DestroyImmediate(incense.gameObject);
        }
        incenseSticks.Clear();

        // 創建香的位置（圓形排列在香爐上）
        List<Vector2> positions = GenerateIncensePositions();
        List<float> angles = GenerateIncenseAngles();

        for (int i = 0; i < totalIncenseCount; i++)
        {
            GameObject incenseObj = Instantiate(incensePrefab, incenseContainer);
            IncenseStick incenseStick = incenseObj.GetComponent<IncenseStick>();

            if (incenseStick == null)
            {
                incenseStick = incenseObj.AddComponent<IncenseStick>();
            }

            incenseStick.Initialize(angles[i], positions[i], this);
            incenseSticks.Add(incenseStick);
        }
    }

    List<Vector2> GenerateIncensePositions()
    {
        List<Vector2> positions = new List<Vector2>();
        Vector2 centerPos = holderPosition + Vector2.up * 180f; // 調整基準位置

        // 使用更寬的水平分布，適應較大的香
        float baseWidth = 200f; // 增加分布寬度
        float baseHeight = 50f;  // 增加高度變化

        for (int i = 0; i < totalIncenseCount; i++)
        {
            // 水平位置：均勻分布，再加上隨機偏移
            float baseX = (i - (totalIncenseCount - 1) * 0.5f) * (baseWidth * 2 / totalIncenseCount);
            float xPos = baseX + Random.Range(-40f, 40f);

            // 垂直位置：稍微變化，模擬插入深度不同
            float yPos = Random.Range(-baseHeight / 2, baseHeight / 2);

            Vector2 pos = centerPos + new Vector2(xPos, yPos);
            positions.Add(pos);
        }

        return positions;
    }

    List<float> GenerateIncenseAngles()
    {
        List<float> angles = new List<float>();

        // 添加正確的香（90度）
        for (int i = 0; i < correctIncenseCount; i++)
        {
            angles.Add(90f);
        }

        // 添加錯誤的香（接近90度但不完全正確）
        for (int i = correctIncenseCount; i < totalIncenseCount; i++)
        {
            float angle;
            float randomType = Random.Range(0f, 1f);

            if (randomType < 0.4f)
            {
                // 40% 稍微傾斜 (85-95度之間)
                angle = Random.Range(80f, 105f);
                // 避開90度附近
                while (Mathf.Abs(angle - 90f) < 2f)
                {
                    angle = Random.Range(80f, 105f);
                }
            }
            else if (randomType < 0.7f)
            {
                // 30% 接近90度的錯誤答案 (89-91度之間，但不是90度)
                angle = Random.Range(87.5f, 92.5f);
                // 確保不是剛好90度
                while (Mathf.Abs(angle - 90f) < 2f)
                {
                    angle = Random.Range(87.5f, 92.5f);
                }
            }
            else
            {
                // 30% 明顯傾斜 (75-105度之間)
                angle = Random.Range(65f, 125f);
                // 避開90度附近
                while (Mathf.Abs(angle - 90f) < 5f)
                {
                    angle = Random.Range(65f, 125f);
                }
            }

            angles.Add(angle);
        }

        // 打亂順序
        for (int i = 0; i < angles.Count; i++)
        {
            float temp = angles[i];
            int randomIndex = Random.Range(i, angles.Count);
            angles[i] = angles[randomIndex];
            angles[randomIndex] = temp;
        }

        return angles;
    }

    public void OnIncenseClicked(float angle)
    {
        if (!gameActive) return;

        if (Mathf.Approximately(angle, 90f))
        {
            // 正確的香
            currentPowerValue += correctIncenseReward;
            ShowPopup("90.0度,很正直!", true);
            systemMessageText.text = "系統提示:此香90度,判斷正確,增加神力值,可繼續尋找正直的香";
        }
        else
        {
            // 錯誤的香
            currentPowerValue -= wrongIncensePenalty;
            ShowPopup($"{angle:F1}度,不夠正直", false);
            systemMessageText.text = "系統提示:此香非90度,判斷錯誤,減少神力值,請找出正直的香";
        }

        UpdatePowerValue();
    }

    void ShowPopup(string message, bool isCorrect)
    {
        GameObject targetWindow = isCorrect ? popupWindow1 : popupWindow2;
        TextMeshProUGUI targetText = isCorrect ? popupText1 : popupText2;

        if (targetWindow == null || targetText == null) return;

        // 隱藏另一個視窗
        GameObject otherWindow = isCorrect ? popupWindow2 : popupWindow1;
        if (otherWindow != null)
        {
            otherWindow.SetActive(false);
        }

        targetText.text = message;
        targetWindow.SetActive(true);

        // 重設透明度
        CanvasGroup canvasGroup = targetWindow.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = targetWindow.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 1f;

        StartCoroutine(FadeOutPopup(canvasGroup, targetWindow));
    }

    IEnumerator FadeOutPopup(CanvasGroup canvasGroup, GameObject popupWindow)
    {
        yield return new WaitForSeconds(popupDuration - 0.2f);

        float fadeTime = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsedTime / fadeTime);
            yield return null;
        }

        popupWindow.SetActive(false);
    }

    void UpdateUI()
    {
        UpdateTimer();
        UpdatePowerValue();
    }

    void UpdateTimer()
    {
        int seconds = Mathf.CeilToInt(currentTime);
        timerText.text = $"倒計時:{seconds}";
    }

    void UpdatePowerValue()
    {
        powerValueText.text = currentPowerValue.ToString();
    }

    void GameOver()
    {
        gameActive = false;
        ShowResultScreen();
    }

    void ShowResultScreen()
    {
        // 隱藏遊戲UI，顯示結果UI
        if (gamePlayUI) gamePlayUI.SetActive(false);
        if (gameOverUI) gameOverUI.SetActive(true);

        // 清除所有香
        ClearAllIncense();

        // 設置結果香的角度和顯示
        SetupResultIncense();

        // 更新系統訊息
        systemMessageText.text = "系統提示：這是客戶為您插的香的數值，請根據結果評估調整客戶報表。";
    }

    void ClearAllIncense()
    {
        // 清除所有生成的香
        foreach (IncenseStick incense in incenseSticks)
        {
            if (incense != null)
            {
                DestroyImmediate(incense.gameObject);
            }
        }
        incenseSticks.Clear();
    }

    void SetupResultIncense()
    {
        if (resultIncense != null)
        {
            // 設置結果香的角度
            RectTransform incenseRect = resultIncense.GetComponent<RectTransform>();
            if (incenseRect != null)
            {
                float visualAngle = receivedAngle - 90f; // 90度時垂直向上
                incenseRect.localRotation = Quaternion.Euler(0, 0, visualAngle);
            }
        }

        // 更新角度顯示文字
        if (angleDisplayText != null)
        {
            angleDisplayText.text = $"角度：{receivedAngle:F1}度";
        }
    }

    // 公開方法用於接收另一台電腦的角度數值
    public void SetReceivedAngle(float angle)
    {
        receivedAngle = angle;
        if (!gameActive) // 如果已在結果畫面，立即更新
        {
            SetupResultIncense();
        }
    }

    public void RestartGame()
    {
        // 清除現有的香
        ClearAllIncense();

        // 重新初始化遊戲
        InitializeGame();
    }
}