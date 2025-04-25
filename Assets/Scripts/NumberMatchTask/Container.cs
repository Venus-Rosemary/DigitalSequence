using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class Container : MonoBehaviour
{
    [Header("球的设置")]
    public GameObject ballPrefab;
    public Color containerColor;
    public float containerWidth = 3f;
    public float containerHeight = 3f;
    public List<Transform> GenerationPoint=new List<Transform>();

    [Header("Trigger设置")]
    public MeshRenderer triggerObject;//trigger的地面物体
    public Material SelectMaterial;//站上去后改变颜色

    //私有变量
    private List<GameObject> currentBalls = new List<GameObject>();//生成的球存放
    private Material triggerOriginalMaterial;
    private bool playerInRange = false;



    private void Awake()
    {
        triggerOriginalMaterial= triggerObject.material;
    }

    private void OnEnable()
    {
        triggerObject.material = triggerOriginalMaterial;
    }

    private void Update()
    {
        if (playerInRange && Mouse.current.leftButton.wasPressedThisFrame)
        {
            //进行判断
            NumberMatchManager.Instance.OnContainerSelected(currentBalls.Count);
        }
    }

    public void GenerateBalls(int count)
    {
        // 清除现有的球
        foreach (var ball in currentBalls)
        {
            Destroy(ball);
        }
        currentBalls.Clear();
        
        // 生成新的球
        for (int i = 0; i < count; i++)
        {           
            GameObject ball = Instantiate(ballPrefab, GenerationPoint[i].position, Quaternion.identity, transform);
            ball.GetComponent<Renderer>().material.color = containerColor;
            currentBalls.Add(ball);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            triggerObject.material= SelectMaterial;
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