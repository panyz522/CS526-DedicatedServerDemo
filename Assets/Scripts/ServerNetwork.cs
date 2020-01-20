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

public class ServerNetwork
{
    private Task netTask;
    private Task[] netClientTask = new Task[3]; // Assume only 3 clients
    private TcpClient[] remoteClient = new TcpClient[3];
    private NetworkStream[] remoteStream = new NetworkStream[3];
    private int joined;
    private TcpListener listener;

    private Vector2[] curInputs = new Vector2[3];
    private Vector3[] curPoss = new Vector3[3];

    private CancellationTokenSource cts;
    

    public void Start(int port)
    {
        Debug.Log("Starting...");
        cts = new CancellationTokenSource();
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        netTask = Task.Run(Run);
    }

    public void Stop()
    {
        cts.Cancel();
        //remoteStream.Close(); // TODO
        //remoteClient.Close();
        netTask.Wait();
        for (int i = 0; i < 3; i++)
        {
            netClientTask[i]?.Wait();
        }
        cts.Dispose();
    }

    private void Run()
    {

        while (joined < 3 && !cts.IsCancellationRequested)
        {
            Debug.Log($"Waiting for connection {joined}...");
            remoteClient[joined] = listener.AcceptTcpClient();
            var player = joined;
            netClientTask[player] = Task.Run(() => { RunForClient(remoteClient[player], player); });
            joined++;
        }

        // Assume only 3 client, don't listen anymore.
    }

    private void RunForClient(TcpClient client, int player)
    {
        Debug.Log("Client connected");
        float[] floatsIn = new float[2];
        float[] floatsOut = new float[9];
        byte[] dataIn = new byte[2 * sizeof(float)];
        byte[] dataOut = new byte[9 * sizeof(float)];
        remoteStream[player] = client.GetStream();
        var stream = remoteStream[player];

        while (!cts.IsCancellationRequested)
        {
            //Debug.Log("Reading input...");
            stream.ReadAsync(dataIn, 0, dataIn.Length, cts.Token).Wait();
            Buffer.BlockCopy(dataIn, 0, floatsIn, 0, dataIn.Length);
            GetVector2FromArray(floatsIn, 0, out Vector2 curInput);
            curInputs[player] = curInput;
            //Debug.Log($"Read {curInput}");

            for (int i = 0; i < 3; i++)
            {
                FillFloatArray(floatsOut, i * 3, curPoss[i]);
            }
            Buffer.BlockCopy(floatsOut, 0, dataOut, 0, dataOut.Length);
            //Debug.Log($"Writing pos...");
            stream.WriteAsync(dataOut, 0, dataOut.Length, cts.Token).Wait();
            stream.Flush();
            //Debug.Log($"Wrote.");

            Thread.Sleep(10);
        }
    }

    public Vector2 GetInput(int i)
    {
        return curInputs[i];
    }

    public void SendPosition(int i, Vector3 pos)
    {
        curPoss[i] = pos;
    }
}
