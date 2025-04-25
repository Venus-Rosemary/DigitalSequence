using UnityEngine;
using System;

public class BallClick : MonoBehaviour
{
    public event Action<GameObject> OnBallClicked;
    
    private void OnMouseDown()
    {
        OnBallClicked?.Invoke(gameObject);
    }
}