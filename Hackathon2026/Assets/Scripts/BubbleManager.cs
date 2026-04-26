using UnityEngine;

public class BubbleManager : MonoBehaviour
{
    public ParticleSystem bubble1;
    public ParticleSystem bubble2;
    public ParticleSystem bubble3;
    static BubbleManager instance;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void StartBubbles()
    {
        instance.bubble1.Play();
        instance.bubble2.Play();
        instance.bubble3.Play();
    }
}
