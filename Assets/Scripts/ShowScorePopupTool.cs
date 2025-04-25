using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowScorePopupTool : Singleton<ShowScorePopupTool>
{
    [Header("�÷�UI����")]
    public GameObject scorePopupPrefab;//�÷�3dUIԤ����
    public Transform playerTransform;//���λ��
    public float uiMoveSpeed = 1f;
    public float uiFadeTime = 1f;

    [Header("3dUI���������")]
    public int poolSize = 5; // ÿ����Ч�Ķ���ش�С
    public Transform poolParent;//������λ��
    private Queue<GameObject> uiPool = new Queue<GameObject>();

    private void Start()
    {
        InitializeUIPool();
    }

    private void InitializeUIPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject ui = Instantiate(scorePopupPrefab, poolParent);
            ui.SetActive(false);
            uiPool.Enqueue(ui);
        }
    }
    public void ShowScorePopup(int addScore)
    {
        if (scorePopupPrefab != null && playerTransform != null)
        {

            if (uiPool.Count == 0) return;

            // �����λ���Ϸ����ɵ÷�UI
            Vector3 spawnPosition = playerTransform.position + Vector3.up * 2f;
            GameObject popup = uiPool.Dequeue();
            popup.transform.position = spawnPosition;
            popup.SetActive(true);

            // �����ı�
            TMP_Text scoreText = popup.GetComponentInChildren<TMP_Text>();

            CanvasGroup canvasGroup = popup.GetComponent<CanvasGroup>();
            if (scoreText != null)
            {
                scoreText.text = $"+{addScore}";
                canvasGroup.alpha = 1f;

                // ������������
                Sequence sequence = DOTween.Sequence();

                // �����ƶ�
                sequence.Join(popup.transform.DOMoveY(popup.transform.position.y + 1f, uiMoveSpeed));
                // ����
                sequence.Join(canvasGroup.DOFade(0, uiFadeTime));

                // ������ɺ���յ������
                sequence.OnComplete(() =>
                {
                    popup.SetActive(false);
                    uiPool.Enqueue(popup);
                });
            }
        }
    }
}
