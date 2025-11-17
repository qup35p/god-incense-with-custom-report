using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerReportSystem : MonoBehaviour
{
    [Header("Customer Report UI References")]
    [Tooltip("三個數值顯示的TextMeshPro組件")]
    public TextMeshProUGUI[] customerValueTexts = new TextMeshProUGUI[3];

    [Tooltip("三個增加按鈕(+)")]
    public Button[] increaseButtons = new Button[3];

    [Tooltip("三個減少按鈕(-)")]
    public Button[] decreaseButtons = new Button[3];

    [Tooltip("確認儲存按鈕")]
    public Button confirmButton;

    [Header("Customer Report Settings")]
    [Tooltip("三個數值的名稱，可以自訂")]
    public string[] valueNames = { "陰德值", "業力值", "誠意度" };

    [Tooltip("初始數值，預設都是60")]
    public int[] initialValues = { 60, 60, 60 };

    [Tooltip("數值的最小值")]
    public int minValue = 0;

    [Tooltip("數值的最大值")]
    public int maxValue = 100;

    [Tooltip("每次點擊按鈕改變的數值")]
    public int changeAmount = 1;

    [Header("Display Settings")]
    [Tooltip("數字的顏色")]
    public Color numberColor = Color.yellow;

    [Tooltip("數字的字體大小")]
    public int fontSize = 36;

    [Tooltip("是否使用粗體")]
    public bool useBold = true;

    [Header("Audio Settings")]
    [Tooltip("音效播放器")]
    public AudioSource audioSource;

    [Tooltip("按鈕點擊音效")]
    public AudioClip buttonClickSound;

    [Tooltip("確認音效")]
    public AudioClip confirmSound;

    // 私有變數
    private int[] customerValues = new int[3];

    private void Start()
    {
        InitializeCustomerReport();
        SetupEventListeners();
        UpdateCustomerValueDisplays();
    }

    /// <summary>
    /// 初始化客戶報表數值
    /// </summary>
    private void InitializeCustomerReport()
    {
        // 複製初始數值到當前數值陣列
        for (int i = 0; i < customerValues.Length; i++)
        {
            if (i < initialValues.Length)
            {
                customerValues[i] = initialValues[i];
            }
            else
            {
                customerValues[i] = 60; // 預設值
            }
        }
    }

    /// <summary>
    /// 設置按鈕事件監聽器
    /// </summary>
    private void SetupEventListeners()
    {
        // 設置增加和減少按鈕
        for (int i = 0; i < 3; i++)
        {
            int index = i; // 避免閉包問題

            if (increaseButtons[i] != null)
            {
                increaseButtons[i].onClick.AddListener(() => ModifyCustomerValue(index, changeAmount));
            }

            if (decreaseButtons[i] != null)
            {
                decreaseButtons[i].onClick.AddListener(() => ModifyCustomerValue(index, -changeAmount));
            }
        }

        // 設置確認按鈕
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmCustomerValues);
        }
    }

    /// <summary>
    /// 修改指定數值
    /// </summary>
    /// <param name="index">數值索引 (0-2)</param>
    /// <param name="change">改變量</param>
    public void ModifyCustomerValue(int index, int change)
    {
        if (index < 0 || index >= customerValues.Length) return;

        // 計算新數值並限制範圍
        customerValues[index] = Mathf.Clamp(customerValues[index] + change, minValue, maxValue);

        // 更新顯示
        UpdateCustomerValueDisplays();

        // 播放音效
        PlayButtonSound();

        // 可以在這裡添加其他邏輯，如振動效果等
        Debug.Log($"{valueNames[index]} 變更為: {customerValues[index]}");
    }

    /// <summary>
    /// 更新數值顯示
    /// </summary>
    private void UpdateCustomerValueDisplays()
    {
        for (int i = 0; i < customerValues.Length && i < customerValueTexts.Length; i++)
        {
            if (customerValueTexts[i] != null)
            {
                // 使用Rich Text格式化數字顯示
                string colorHex = ColorUtility.ToHtmlStringRGBA(numberColor);
                string boldTag = useBold ? "b" : "";

                if (useBold)
                {
                    customerValueTexts[i].text = $"<color=#{colorHex}><size={fontSize}><{boldTag}>{customerValues[i]}</{boldTag}></size></color>";
                }
                else
                {
                    customerValueTexts[i].text = $"<color=#{colorHex}><size={fontSize}>{customerValues[i]}</size></color>";
                }

                // 直接設置組件屬性（備用方案）
                customerValueTexts[i].fontSize = fontSize;
                customerValueTexts[i].color = numberColor;
                customerValueTexts[i].fontStyle = useBold ? FontStyles.Bold : FontStyles.Normal;
                customerValueTexts[i].alignment = TextAlignmentOptions.Center;
            }
        }
    }

    /// <summary>
    /// 確認並儲存數值
    /// </summary>
    public void ConfirmCustomerValues()
    {
        // 播放確認音效
        PlayConfirmSound();

        // 生成確認訊息
        string confirmMessage = "客戶報表已儲存 - ";
        for (int i = 0; i < customerValues.Length && i < valueNames.Length; i++)
        {
            confirmMessage += $"{valueNames[i]}:{customerValues[i]}";
            if (i < customerValues.Length - 1) confirmMessage += " ";
        }

        Debug.Log(confirmMessage);

        // 可以在這裡添加儲存到PlayerPrefs或其他持久化邏輯
        SaveToPlayerPrefs();

        // 觸發確認事件（可以讓其他系統監聽）
        OnCustomerReportConfirmed?.Invoke(customerValues);
    }

    /// <summary>
    /// 儲存數值到PlayerPrefs
    /// </summary>
    private void SaveToPlayerPrefs()
    {
        for (int i = 0; i < customerValues.Length; i++)
        {
            PlayerPrefs.SetInt($"CustomerValue_{i}", customerValues[i]);
        }
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 從PlayerPrefs載入數值
    /// </summary>
    public void LoadFromPlayerPrefs()
    {
        for (int i = 0; i < customerValues.Length; i++)
        {
            customerValues[i] = PlayerPrefs.GetInt($"CustomerValue_{i}", initialValues[i]);
        }
        UpdateCustomerValueDisplays();
    }

    /// <summary>
    /// 重置所有數值到初始值
    /// </summary>
    public void ResetToDefault()
    {
        InitializeCustomerReport();
        UpdateCustomerValueDisplays();
        Debug.Log("客戶報表已重置到預設值");
    }

    /// <summary>
    /// 播放按鈕音效
    /// </summary>
    private void PlayButtonSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }

    /// <summary>
    /// 播放確認音效
    /// </summary>
    private void PlayConfirmSound()
    {
        if (audioSource != null && confirmSound != null)
        {
            audioSource.PlayOneShot(confirmSound);
        }
    }

    // 公開方法供其他腳本調用

    /// <summary>
    /// 獲取指定數值
    /// </summary>
    /// <param name="index">數值索引</param>
    /// <returns>數值</returns>
    public int GetCustomerValue(int index)
    {
        if (index >= 0 && index < customerValues.Length)
            return customerValues[index];
        return -1;
    }

    /// <summary>
    /// 設置指定數值
    /// </summary>
    /// <param name="index">數值索引</param>
    /// <param name="value">新數值</param>
    public void SetCustomerValue(int index, int value)
    {
        if (index >= 0 && index < customerValues.Length)
        {
            customerValues[index] = Mathf.Clamp(value, minValue, maxValue);
            UpdateCustomerValueDisplays();
        }
    }

    /// <summary>
    /// 獲取所有數值的副本
    /// </summary>
    /// <returns>數值陣列</returns>
    public int[] GetAllCustomerValues()
    {
        int[] valuesCopy = new int[customerValues.Length];
        customerValues.CopyTo(valuesCopy, 0);
        return valuesCopy;
    }

    /// <summary>
    /// 設置所有數值
    /// </summary>
    /// <param name="newValues">新的數值陣列</param>
    public void SetAllCustomerValues(int[] newValues)
    {
        for (int i = 0; i < customerValues.Length && i < newValues.Length; i++)
        {
            customerValues[i] = Mathf.Clamp(newValues[i], minValue, maxValue);
        }
        UpdateCustomerValueDisplays();
    }

    // 事件系統
    public System.Action<int[]> OnCustomerReportConfirmed;
    public System.Action<int, int> OnValueChanged; // index, newValue

    /// <summary>
    /// 觸發數值改變事件
    /// </summary>
    private void TriggerValueChangedEvent(int index, int newValue)
    {
        OnValueChanged?.Invoke(index, newValue);
    }

    // 編輯器用的測試方法
#if UNITY_EDITOR
    [ContextMenu("Test - Add 10 to All Values")]
    private void TestAddToAllValues()
    {
        for (int i = 0; i < 3; i++)
        {
            ModifyCustomerValue(i, 10);
        }
    }

    [ContextMenu("Test - Reset Values")]
    private void TestResetValues()
    {
        ResetToDefault();
    }
#endif
}