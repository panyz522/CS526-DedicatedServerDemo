using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum ServerStatus
{
    Idle,
    Starting,
    Waiting,
    PartiallyConnected,
    FullyConnected
}

public class ServerNetwork<TSyncToServer, TSyncToClient>
    where TSyncToServer : new()
    where TSyncToClient : new()
{
    public TSyncToClient DataToClient { get; private set; }
    public TSyncToServer[] DataFromClient { get; private set; }
    public ServerStatus Status { get; private set; } = ServerStatus.Idle;
    public bool PlayerDisconnected { get; private set; } // Override Status and Joined
    public int Joined { get; private set; }

    private Task netTask;
    private Task[] netClientTask = new Task[3]; // Assume only 3 clients
    private TcpClient[] remoteClient = new TcpClient[3];
    private NetworkStream[] remoteStream = new NetworkStream[3];
    private TcpListener listener;

    private Serializer<TSyncToClient> serializer;
    private Serializer<TSyncToServer> deserializer;

    private CancellationTokenSource cts;

    public ServerNetwork()
    {
        DataToClient = new TSyncToClient();
        DataFromClient = new TSyncToServer[3];
        serializer = new Serializer<TSyncToClient>();
        deserializer = new Serializer<TSyncToServer>();
        for (int i = 0; i < 3; i++)
        {
            DataFromClient[i] = new TSyncToServer();
        }
    }

    public void Start(int port)
    {
        Debug.Assert(Status == ServerStatus.Idle);
        Debug.Log("Starting...");
        Status = ServerStatus.Starting;

        PlayerDisconnected = false;
        cts = new CancellationTokenSource();
        listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        netTask = Task.Run(Run);
    }

    public void Stop()
    {
        Debug.Assert(Status != ServerStatus.Idle);
        Debug.Log("Stopping...");

        cts.Cancel();
        listener.Stop();
        netTask.Wait();
        for (int i = 0; i < 3; i++)
        {
            netClientTask[i]?.Wait();
            try { remoteStream[i]?.Close(); }
            catch { }
            try { remoteClient[i]?.Close(); }
            catch { }
        }
        cts.Dispose();

        Debug.Log("Successfully Stopped and Disposed");
        Status = ServerStatus.Idle;
    }

    private void Run()
    {
        try
        {
            while (Joined < 3 && !cts.IsCancellationRequested)
            {
                Debug.Log($"Waiting for connection {Joined}...");
                Status = (Joined == 0)? ServerStatus.Waiting : ServerStatus.PartiallyConnected;
                remoteClient[Joined] = listener.AcceptTcpClient();
                var player = Joined;
                netClientTask[player] = Task.Run(() => { RunForClient(remoteClient[player], player); });
                Joined++;
            }
            Status = ServerStatus.FullyConnected;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Debug.Log("Listener Stopped");

        // Assume only 3 client, don't listen anymore.
    }

    private void RunForClient(TcpClient client, int player)
    {
        try
        {
            Debug.Log($"Client {player} connected");
            byte[] dataIn = new byte[1024];
            byte[] dataOut = new byte[1024];
            remoteStream[player] = client.GetStream();
            var stream = remoteStream[player];

            while (!cts.IsCancellationRequested)
            {
                //Debug.Log("Reading input...");
                stream.ReadAsync(dataIn, 0, dataIn.Length, cts.Token).Wait();
                deserializer.DeserializeFrom(dataIn, DataFromClient[player]);
                //Debug.Log($"Read {curInput}");

                serializer.SerializeTo(DataToClient, dataOut);
                //Debug.Log($"Writing pos...");
                stream.WriteAsync(dataOut, 0, dataOut.Length, cts.Token).Wait();
                stream.Flush();
                //Debug.Log($"Wrote.");

                Thread.Sleep(10);
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Debug.Log($"Client {player} disconnected");
        PlayerDisconnected = true;
    }
}
