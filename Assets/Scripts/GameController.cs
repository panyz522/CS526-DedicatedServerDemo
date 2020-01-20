using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text TxtDelay;
    public bool IsServer;
    public string Ip;
    ServerNetwork server;
    ClientNetwork client;

    GameObject[] ballObjs = new GameObject[3];
    Rigidbody[] ballRbs = new Rigidbody[3];
    Transform[] ballTrs = new Transform[3];

    // Start is called before the first frame update
    void Start()
    {
        // Get balls
        for (int i = 0; i < 3; i++)
        {
            ballObjs[i] = GameObject.Find("Ball" + i);
            ballTrs[i] = ballObjs[i].GetComponent<Transform>();
        }

        // Init server or client
        if (IsServer)
        {
            //Destroy(GameObject.Find("Main Camera"));
            //Destroy(ballObj.GetComponent<MeshRenderer>());
            for (int i = 0; i < 3; i++)
            {
                ballRbs[i] = ballObjs[i].GetComponent<Rigidbody>();
            }

            server = new ServerNetwork();
            server.Start(10000);
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                Destroy(ballObjs[i].GetComponent<Rigidbody>());
                Destroy(ballObjs[i].GetComponent<SphereCollider>());
            }

            client = new ClientNetwork();
            client.Start(Ip, 10000);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsServer)
        {
            for (int i = 0; i < 3; i++)
            {
                var input = server.GetInput(i);
                ballRbs[i].velocity = new Vector3(input.x, ballRbs[i].velocity.y, input.y); // TODO: Change to force
                server.SendPosition(i, ballTrs[i].position);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                ballTrs[i].position = client.GetPosition(i);
            }

            Vector2 dir = new Vector2();
            dir.x -= Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
            dir.x += Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
            dir.y += Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
            dir.y -= Input.GetKey(KeyCode.DownArrow) ? 1 : 0;
            client.SendInput(dir * 4);

            TxtDelay.text = client.Delay.ToString();
        }

    }
}
