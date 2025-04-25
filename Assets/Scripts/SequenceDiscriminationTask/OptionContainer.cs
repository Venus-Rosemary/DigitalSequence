using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class OptionContainer : MonoBehaviour
{
    [Header("Trigger����")]
    public MeshRenderer triggerObject;//trigger�ĵ�������
    public Material SelectMaterial;//վ��ȥ��ı���ɫ

    [Header("3dUI����")]
    public TMP_Text countText;//��ʾ����
    public string ballOption;//��ǰtrigger������ĸ���

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
        ballOption = "";
        playerInRange = false;
    }

    private void Update()
    {
        if (playerInRange && Mouse.current.leftButton.wasPressedThisFrame)
        {
            //�����ж�
            //NumberCountManager.Instance.OnNumberSelected(ballCount);
            SequenceDiscriminationManager.Instance.OnContainerSelected(ballOption);
        }
    }

    public void SetCurrentTriggerNum(string count)
    {
        ballOption = count;
        countText.text = count.ToString();
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
