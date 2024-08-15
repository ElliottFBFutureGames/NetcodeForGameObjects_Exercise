using UnityEngine;

public class EmoteEffect : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float shrink;

    private void Start()
    {
        //moves forward a bit so it doesn't clip into the player or map
        transform.position += Vector3.back * 0.6f;
    }
    private void Update()
    {
        //position and size
        transform.position += new Vector3(0, speed * Time.deltaTime);
        transform.localScale -= Vector3.one * shrink * Time.deltaTime;

        
        if(transform.localScale.x <= 0)
        {
            Destroy(gameObject);
        }
    }
}
