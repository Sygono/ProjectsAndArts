using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Bot))]
[DefaultExecutionOrder(-1)]
public class BotController : MonoBehaviour
{
    public Bot bot;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = bot.getDirection();
        bot.transform.position+=direction*0.01f*speed;
    }
}
