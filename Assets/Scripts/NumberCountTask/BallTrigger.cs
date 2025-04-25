using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class BallTrigger : MonoBehaviour
{
    public event Action<GameObject> OnBallTriggered;
    private bool isTriggered = false;

    private void Start()
    {
        // 确保球体有触发器碰撞体
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;
        
        if (other.gameObject.CompareTag("MouseFollower"))
        {
            isTriggered = true;
            OnBallTriggered?.Invoke(gameObject);
        }
    }
}