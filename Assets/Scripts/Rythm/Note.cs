using UnityEngine;

public class Note : MonoBehaviour
{
    [SerializeField] private float noteSpeed = 400;
    
    void Update()
    {
        transform.localPosition += Vector3.right * noteSpeed * Time.deltaTime;
    }
}
