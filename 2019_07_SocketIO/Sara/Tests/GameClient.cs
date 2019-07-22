using System;
using System.Collections.Generic;
using BestHTTP.ServerSentEvents;
using BestHTTP.SignalR.Messages;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using LitJson;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;

public class GameClient
{
    private SocketOptions _options = new SocketOptions();
    private SocketManager _manager;
    public Socket Socket;
    private SocketClient _socketClient;

    private string userId;
    private string host;
    public GameClient(string host, string userId)
    {
        this.host = host;
        this.userId = userId;
        
        _socketClient = new SocketClient();

    }


    public bool IsConnecting => _socketClient.SocketState== SocketEventType.Connecting;
    public bool IsConnected => _socketClient.SocketState == SocketEventType.Connected;
    public SocketClient SocketClient
    {
        get => _socketClient;
    }

    public string RoomId { get; set; }
    public List<string> Chats = new List<string>();

    public void Disconnect()
    {
        _manager?.Close();
    }

    public void Connect()
    {
        Uri _uri = new Uri(host);
        _manager = new SocketManager(_uri, _options);
        _manager.Encoder = new LitJsonEncoder();
        
        _options.AdditionalQueryParams = new ObservableDictionary<string, string>();
        _options.AdditionalQueryParams.Add("userId", userId);
        _options.ConnectWith = TransportTypes.WebSocket;
        
        Socket = _manager.Socket;
        Socket.On("connect", OnConnect);
        Socket.On("connecting", OnConnecting);

        Socket.On("disconnect", OnDisconnect);
        Socket.On("error", OnError);
        Socket.On("event", OnEvent);
        
        _socketClient.SocketState = SocketEventType.Connecting;
        _manager.Open();
    }

    private void OnConnecting(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("Connecting"); 
    }
    
    private void OnConnect(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("Connect");
        _socketClient.OnSocketEvent.Invoke(socket, new SocketEvent(SocketEventType.Connected));
    }
    
    private void OnDisconnect(Socket socket, Packet packet, object[] args)
    {
        Debug.Log("Disconnect");
        _socketClient?.OnSocketEvent?.Invoke(socket, new SocketEvent(SocketEventType.Disconnected));
    }
    
    private void OnError (Socket socket, Packet packet, object[] args)
    {
        Debug.Log("Error");
        Debug.Log(_manager.Options.AdditionalQueryParams["userId"]);
        Debug.Log(packet?.Payload);
        _socketClient?.OnSocketEvent?.Invoke(socket, new SocketEvent(SocketEventType.Error));
    }
    
    private void OnEvent (Socket socket, Packet packet, object[] args)
    {
        OnEventMessage?.Invoke(socket, packet);
        if (packet.EventName == "respondMessageInRoom")
        {
            JsonData data = JsonMapper.ToObject(packet.Payload);
            Chats.Add(data[1]["message"].ToString());
        }
        _socketClient?.OnSocketEvent?.Invoke(socket, new SocketEvent(SocketEventType.Event));
    }

    public event Action<Socket, Packet> OnEventMessage;
}

public class SocketClient
{
    public SocketEventType SocketState = SocketEventType.Disconnected;
    public EventHandler<SocketEvent> OnSocketEvent { get; set; }

    public SocketClient()
    {
        OnSocketEvent += (sender, e) =>
        {
            if (e.EventType == SocketEventType.Connected || e.EventType == SocketEventType.Disconnected ||
                e.EventType == SocketEventType.Connecting)
            {
                SocketState = e.EventType;
            }
        };
    }
}

public class SocketEvent
{
    public SocketEventType EventType { get; set; }

    public SocketEvent(SocketEventType type)
    {
        EventType = type;
    }
}

public enum SocketEventType
{
    Connected,
    Disconnected,
    Error,
    Event,
    Connecting
}
