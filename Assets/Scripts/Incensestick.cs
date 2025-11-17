using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class IncenseStick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Incense Properties")]
    public float angle;
    public bool isClicked = false;

    [Header("Visual Effects")]
    public float hoverScale = 1.1f; // 懸停時的縮放比例

    private IncenseGameManager gameManager;
    private Image incenseImage;
    private RectTransform rectTransform;
    private bool isHovering = false;

    public void Initialize(float incenseAngle, Vector2 position, IncenseGameManager manager)
    {
        angle = incenseAngle;
        gameManager = manager;

        // 設置組件
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }

        incenseImage = GetComponent<Image>();
        if (incenseImage == null)
        {
            incenseImage = gameObject.AddComponent<Image>();
        }

        // 創建香的視覺效果
        CreateIncenseVisual(position);
    }

    void CreateIncenseVisual(Vector2 position)
    {
        // 設置位置
        rectTransform.localPosition = position;
        rectTransform.localScale = Vector3.one;

        // 設置大小 - 根據用戶的63x265 image調整
        rectTransform.sizeDelta = new Vector2(63f, 265f);

        // 完全不設置任何Image屬性，保持Prefab原始設置

        // 設置旋轉角度（調整為正確的視覺角度）
        float visualAngle = angle - 90f; // 90度時垂直向上
        rectTransform.localRotation = Quaternion.Euler(0, 0, visualAngle);
    }

    void CreateSmokeEffect()
    {
        // 創建簡單的煙霧視覺效果
        GameObject smoke = new GameObject("Smoke");
        smoke.transform.SetParent(transform);

        Image smokeImage = smoke.AddComponent<Image>();
        smokeImage.color = new Color(0.8f, 0.8f, 0.8f, 0.3f);

        RectTransform smokeRect = smoke.GetComponent<RectTransform>();
        smokeRect.localPosition = Vector3.up * 140f; // 根據265高度調整煙霧位置
        smokeRect.sizeDelta = new Vector2(20f, 30f); // 相應調整煙霧大小
        smokeRect.localScale = Vector3.one;

        // 添加煙霧動畫
        StartCoroutine(AnimateSmoke(smokeRect));
    }

    System.Collections.IEnumerator AnimateSmoke(RectTransform smokeRect)
    {
        Vector3 originalScale = smokeRect.localScale;

        while (true)
        {
            // 煙霧飄動效果
            float time = Time.time;
            smokeRect.localScale = originalScale * (1f + Mathf.Sin(time * 2f) * 0.1f);
            smokeRect.localPosition = Vector3.up * (140f + Mathf.Sin(time * 1.5f) * 8f); // 根據新位置調整動畫

            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isClicked || gameManager == null) return;

        isHovering = true;
        StartCoroutine(HoverEffect(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isClicked || gameManager == null) return;

        isHovering = false;
        StartCoroutine(HoverEffect(false));
    }

    System.Collections.IEnumerator HoverEffect(bool isEntering)
    {
        Vector3 startScale = rectTransform.localScale;
        Vector3 targetScale = isEntering ? Vector3.one * hoverScale : Vector3.one;

        float duration = 0.2f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // 如果狀態改變了（例如滑鼠快速移動），停止動畫
            if (isEntering && !isHovering) break;
            if (!isEntering && isHovering) break;

            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // 只有縮放動畫，完全不修改Image屬性
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, progress);

            yield return null;
        }

        // 確保最終狀態正確
        if (!isClicked)
        {
            rectTransform.localScale = isHovering ? Vector3.one * hoverScale : Vector3.one;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClicked || gameManager == null) return;

        isClicked = true;
        OnIncenseClicked();
    }

    void OnIncenseClicked()
    {
        // 視覺反饋
        StartCoroutine(ClickFeedback());

        // 通知遊戲管理器
        gameManager.OnIncenseClicked(angle);
    }

    System.Collections.IEnumerator ClickFeedback()
    {
        // 停止懸停效果
        isHovering = false;

        // 點擊時的縮放效果
        Vector3 originalScale = Vector3.one;

        // 縮小
        float shrinkTime = 0.1f;
        float elapsedTime = 0f;

        while (elapsedTime < shrinkTime)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0.8f, elapsedTime / shrinkTime);
            rectTransform.localScale = originalScale * scale;
            yield return null;
        }

        // 恢復
        elapsedTime = 0f;
        while (elapsedTime < shrinkTime)
        {
            elapsedTime += Time.deltaTime;
            float scale = Mathf.Lerp(0.8f, 1f, elapsedTime / shrinkTime);
            rectTransform.localScale = originalScale * scale;
            yield return null;
        }

        rectTransform.localScale = originalScale;

        // 點擊後直接刪除這根香
        yield return new WaitForSeconds(0.2f); // 等待一點時間讓玩家看到點擊效果
        Destroy(gameObject);
    }
}