using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum ClientStatus
{
    Idle,
    Starting,
    Connected,
    Disconnected
}

public class ClientNetwork<TSyncToServer, TSyncToClient>
    where TSyncToServer: new()
    where TSyncToClient: new()
{
    public TSyncToClient DataFromServer { get; private set; }
    public TSyncToServer DataToServer { get; private set; }
    public ClientStatus Status { get; private set; }
    public bool DataReceived { get; private set; }

    private Task netTask;
    private TcpClient client;
    private NetworkStream stream;

    private Serializer<TSyncToServer> serializer;
    private Serializer<TSyncToClient> deserializer;

    private CancellationTokenSource cts;

    public TimeSpan Delay { get; private set; }

    public ClientNetwork()
    {
        DataFromServer = new TSyncToClient();
        DataToServer = new TSyncToServer();
        serializer = new Serializer<TSyncToServer>();
        deserializer = new Serializer<TSyncToClient>();
    }

    public void Start(string ip, int port)
    {
        Debug.Assert(Status == ClientStatus.Idle);
        Debug.Log("Starting...");
        DataReceived = false;
        Status = ClientStatus.Starting;
        cts = new CancellationTokenSource();
        client = new TcpClient(ip, port);
        stream = client.GetStream();
        netTask = Task.Run(Run);
    }

    public void Stop()
    {
        Debug.Assert(Status != ClientStatus.Idle);
        Debug.Log("Stopping...");

        cts.Cancel();
        try { stream.Close(); }
        catch { }
        try { client.Close(); }
        catch { }
        netTask.Wait();
        cts.Dispose();

        Debug.Log("Successfully Stopped and Disposed");
        Status = ClientStatus.Idle;
    }

    private void Run()
    {
        try
        {
            Debug.Log("Connected");
            Status = ClientStatus.Connected;
            byte[] dataOut = new byte[1024];
            byte[] dataIn = new byte[1024];

            // Get length for data
            byte[] test = new byte[1024];
            Debug.Log($"Length of data to server: {serializer.SerializeTo(DataToServer, test)}");
            Debug.Log($"Length of data from server: {deserializer.SerializeTo(DataFromServer, test)}");

            while (!cts.IsCancellationRequested)
            {
                //Debug.Log("Writing Input...");
                DateTime sendTime = DateTime.Now;
                serializer.SerializeTo(DataToServer, dataOut);
                stream.WriteAsync(dataOut, 0, dataOut.Length, cts.Token).Wait();
                stream.Flush();
                //Debug.Log("Writing Input Done");

                //Debug.Log("Reading Pos...");
                stream.ReadAsync(dataIn, 0, dataIn.Length, cts.Token).Wait();
                deserializer.DeserializeFrom(dataIn, DataFromServer);
                DataReceived = true;
                //Debug.Log("Reading Pos Done");
                Delay = DateTime.Now - sendTime;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Debug.Log("Disconnected");
        Status = ClientStatus.Disconnected;
    }
}
