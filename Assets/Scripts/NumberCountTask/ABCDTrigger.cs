using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ABCDTrigger : MonoBehaviour
{
    [Header("Trigger����")]
    public MeshRenderer triggerObject;//trigger�ĵ�������
    public Material SelectMaterial;//վ��ȥ��ı���ɫ

    [Header("3dUI����")]
    public TMP_Text countText;//��ʾ����
    [SerializeField] private int ballCount;//��ǰtrigger������ĸ���

    //˽�б���
    private Material triggerOriginalMaterial;
    private bool playerInRange = false;



    private void Awake()
    {
        triggerOriginalMaterial = triggerObject.material;
    }

    private void OnEnable()
    {
        triggerObject.material = triggerOriginalMaterial;
    }

    private void OnDisable()
    {
        ballCount = 0;
        playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Mouse.current.leftButton.wasPressedThisFrame)
        {

            Debug.Log($"ballCount��{ballCount}");
            //�����ж�
            NumberCountManager.Instance.OnNumberSelected(ballCount);
        }
    }

    public void SetCurrentTriggerNum(int count)
    {
        ballCount=count;
        countText.text= count.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            triggerObject.material = SelectMaterial;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            triggerObject.material = triggerOriginalMaterial;
        }

    }
}
