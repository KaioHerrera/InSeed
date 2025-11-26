using UnityEngine;

public class Seed : MonoBehaviour
{
    [Header("Seed Settings")]
    public float rotationSpeed = 50f;
    public float floatSpeed = 1f;
    public float floatAmount = 0.3f;
    
    private Vector3 startPosition;
    private bool collected = false;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        if (!collected)
        {
            // Rotação
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            
            // Flutuação
            float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }
    }
    
    public void Collect()
    {
        if (!collected)
        {
            collected = true;
            GameManager.Instance.CollectSeed();
            Destroy(gameObject);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !collected)
        {
            Collect();
        }
    }
}
