using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

using static NetUtils;

public class ClientNetwork
{
    private Task netTask;
    private TcpClient client;
    private NetworkStream stream;

    private Vector2 curInput;
    private Vector3[] curPoss = new Vector3[3];

    private CancellationTokenSource cts;

    public TimeSpan Delay { get; private set; }

    public void Start(string ip, int port)
    {
        cts = new CancellationTokenSource();
        client = new TcpClient(ip, port);
        stream = client.GetStream();
        netTask = Task.Run(Run);
    }

    public void Stop()
    {
        cts.Cancel();
        stream.Close();
        client.Close();
        netTask.Wait();
        cts.Dispose();
    }

    private void Run()
    {
        float[] floatsOut = new float[2];
        float[] floatsIn = new float[9];
        byte[] dataOut = new byte[2 * sizeof(float)];
        byte[] dataIn = new byte[9 * sizeof(float)];

        while (!cts.IsCancellationRequested)
        {
            //Debug.Log("Writing Input...");
            DateTime sendTime = DateTime.Now;
            FillFloatArray(floatsOut, 0, curInput);
            Buffer.BlockCopy(floatsOut, 0, dataOut, 0, dataOut.Length);
            stream.WriteAsync(dataOut, 0, dataOut.Length, cts.Token).Wait();
            stream.Flush();
            //Debug.Log("Writing Input Done");

            //Debug.Log("Reading Pos...");
            stream.ReadAsync(dataIn, 0, dataIn.Length, cts.Token).Wait();
            Buffer.BlockCopy(dataIn, 0, floatsIn, 0, dataIn.Length);
            for (int i = 0; i < 3; i++)
            {
                GetVector3FromArray(floatsIn, i * 3, out Vector3 curPos);
                curPoss[i] = curPos;
            }
            //Debug.Log("Reading Pos Done");
            Delay = DateTime.Now - sendTime;

            Thread.Sleep(10);
        }
    }

    public void SendInput(Vector2 dir)
    {
        curInput = dir;
    }

    public Vector3 GetPosition(int i)
    {
        return curPoss[i];
    }
}
