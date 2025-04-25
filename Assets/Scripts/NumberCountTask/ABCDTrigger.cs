using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ABCDTrigger : MonoBehaviour
{
    [Header("Trigger设置")]
    public MeshRenderer triggerObject;//trigger的地面物体
    public Material SelectMaterial;//站上去后改变颜色

    [Header("3dUI设置")]
    public TMP_Text countText;//显示个数
    [SerializeField] private int ballCount;//当前trigger所代表的个数

    //私有变量
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

            Debug.Log($"ballCount：{ballCount}");
            //进行判断
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
