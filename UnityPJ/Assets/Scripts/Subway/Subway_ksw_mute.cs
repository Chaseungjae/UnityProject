using UnityEngine;

public class Subway_ksw_mute : MonoBehaviour
{   
    public AudioSource sound;
    private float speed = -5.0f; 
    private float minX = -10.0f;
    private float maxX = 10.0f;
    public float timer;
    private bool first = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.Background.is_sound = false;
        timer = Random.Range(20f, 30f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = sound.gameObject.transform.position;

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else if (first)
        {
            first= false;
            sound.Play();
        }
        else
        {
            pos.x += speed * Time.deltaTime;
            if (pos.x > maxX)
            {
                pos.x = maxX;    
                speed = -speed;  
            }
            else if (pos.x < minX)
            {
                pos.x = minX;   
                speed = -speed; 
            }
            sound.gameObject.transform.position = pos;

        }
    }
}
