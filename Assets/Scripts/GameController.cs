using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public Text TxtDelay;
    public GameObject BallPrefab;
    public bool IsServer;
    public string Ip;
    ServerNetwork server;
    ClientNetwork client;

    GameObject[] ballObjs = new GameObject[3];
    GameObject[] freeBallObjs = new GameObject[48];
    Rigidbody[] ballRbs = new Rigidbody[3];
    Transform[] ballTrs = new Transform[3];
    Rigidbody[] freeBallRbs = new Rigidbody[48];
    Transform[] freeBallTrs = new Transform[48];

    private void Awake()
    {
        if (IsServer)
            Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        int iball = 0;
        // Generate free balls
        for (int i = 1; i <= 4; i++)
        {
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(-2 * i, 1, -2), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(2 * i, 1, -2), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(-2 * i, 1, -4), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(2 * i, 1, -4), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(-2 * i, 1, -6), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(2 * i, 1, -6), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(-2 * i, 1, 2), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(2 * i, 1, 2), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(-2 * i, 1, 4), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(2 * i, 1, 4), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(-2 * i, 1, 6), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
            freeBallObjs[iball] = Instantiate(BallPrefab, new Vector3(2 * i, 1, 6), Quaternion.identity);
            freeBallTrs[iball] = freeBallObjs[iball].GetComponent<Transform>();
            iball++;
        }

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

            for (int i = 0; i < 48; i++)
            {
                freeBallRbs[i] = freeBallObjs[i].GetComponent<Rigidbody>();
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

            for (int i = 0; i < 48; i++)
            {
                Destroy(freeBallObjs[i].GetComponent<Rigidbody>());
                Destroy(freeBallObjs[i].GetComponent<SphereCollider>());
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
                var input = server.DataFromClient[i].Inputs;
                ballRbs[i].velocity = new Vector3(input.x, ballRbs[i].velocity.y, input.y); // TODO: Change to force
                server.DataToClient.Positions[i] = ballTrs[i].position;
            }

            for (int i = 0; i < 48; i++)
            {
                server.DataToClient.FreeBallPosition[i] = freeBallTrs[i].position;
            }
        }
        else
        {
            if (!client.DataReceived)
            {
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                ballTrs[i].position = client.DataFromServer.Positions[i];
            }

            for (int i = 0; i < 48; i++)
            {
                freeBallTrs[i].position = client.DataFromServer.FreeBallPosition[i];
            }

            Vector2 dir = new Vector2();
            dir.x -= Input.GetKey(KeyCode.LeftArrow) ? 1 : 0;
            dir.x += Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
            dir.y += Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
            dir.y -= Input.GetKey(KeyCode.DownArrow) ? 1 : 0;
            client.DataToServer.Inputs = dir * 4;

            TxtDelay.text = client.Delay.ToString();
        }

    }

    private void OnApplicationQuit()
    {
        if (IsServer)
        {
            server?.Stop();
        }
        {
            client?.Stop();
        }
    }
}
