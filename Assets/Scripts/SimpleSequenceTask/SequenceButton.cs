using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class SequenceButton : MonoBehaviour
{
    [Header("Trigger设置")]
    public MeshRenderer triggerObject;
    public Material SelectMaterial;

    [Header("3DUI设置")]
    public int number;


    [Header("顺序/倒序设置")]
    public bool SequenceOrReverseOrder = true;

    private Material triggerOriginalMaterial;
    private bool playerInRange = false;
    private bool isCorrect = false;

    private void Awake()
    {
        triggerOriginalMaterial = triggerObject.material;
    }

    private void OnEnable()
    {
        ResetMaterial();
    }

    private void OnDisable()
    {
        playerInRange = false;
        isCorrect = false;
    }

    private void Update()
    {
        //顺序检测
        if (playerInRange && SequenceOrReverseOrder && Mouse.current.leftButton.wasPressedThisFrame)
        {
            SimpleSequenceManager.Instance.OnButtonClicked(number);
        }

        //倒叙检测
        if (playerInRange && !SequenceOrReverseOrder && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ReverseOrderManager.Instance.OnReverseButtonClicked(number);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (!isCorrect)
            {
                triggerObject.material = SelectMaterial;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (!isCorrect)
            {
                triggerObject.material = triggerOriginalMaterial;
            }
        }
    }

    public void SetCorrectMaterial()
    {
        isCorrect = true;
        triggerObject.material = SelectMaterial;
    }

    public void ResetMaterial()
    {
        isCorrect = false;
        triggerObject.material = triggerOriginalMaterial;
    }
}