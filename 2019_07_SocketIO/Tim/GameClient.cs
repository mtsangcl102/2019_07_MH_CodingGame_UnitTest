using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.Events;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;

public enum SocketEventType { Connected=1, Disconnected, Error };

public class SocketEvent 
{
    public SocketEventType EventType;
    public string EventName;

    public SocketEvent(SocketEventType e, string name) { EventType = e;  EventName = name; }
}

public class SocketClientClass
{
    public event EventHandler<SocketEvent> OnSocketEvent;
    public Boolean IsConnected;
    public Boolean IsConnecting;
    public Boolean IsError;

    private SocketOptions _options = new SocketOptions();
    private Socket _socket;
    private SocketManager _manager;

    public void Connect(string host, string id, SocketIOCallback eventCallback)
    {
        BestHTTP.HTTPManager.MaxConnectionPerServer = 16;
        _options.AdditionalQueryParams = new ObservableDictionary<string, string>();
        _options.AdditionalQueryParams.Add("userId", id);
        _options.ConnectWith = TransportTypes.WebSocket;
        Uri _uri = new Uri(host);
        _manager = new SocketManager(_uri, _options);
        _manager.Encoder = new LitJsonEncoder();
        _socket = _manager.Socket;
        _socket.On("connect", OnConnect);
        _socket.On("disconnect", OnDisconnect);
        _socket.On("error", OnError);
        _socket.On("event", eventCallback);
        IsConnecting = true;
    }

    public void Disconnect()
    {
        _manager?.Close();
    }

    public void Emit(string eventName, params object[] args )
    {
        _socket?.Emit(eventName, args); 
    }

    private void OnError(Socket socket, Packet packet, object[] args)
    {
        SocketEvent(this, new SocketEvent(SocketEventType.Error, packet.EventName));
        IsError = true;
        IsConnecting = false;
    }

    private void OnConnect(Socket socket, Packet packet, object[] args)
    {
        SocketEvent(this, new SocketEvent(SocketEventType.Connected, packet.EventName));
        IsConnected = true;
        IsConnecting = false;
    }

    private void OnDisconnect(Socket socket, Packet packet, object[] args)
    {
        SocketEvent(this, new SocketEvent(SocketEventType.Disconnected, packet.EventName));
        IsConnected = false;
        IsConnecting = false;
    }


    public void SocketEvent(SocketClientClass sClient, SocketEvent e)
    {
        if (OnSocketEvent != null)
            OnSocketEvent(sClient, e);
    }
}

public class GameClient 
{
    public SocketClientClass SocketClient = new SocketClientClass();

    public Boolean IsConnected { get { return SocketClient.IsConnected; } set { SocketClient.IsConnected = value;  } }
    public Boolean IsConnecting { get { return SocketClient.IsConnecting; } set { SocketClient.IsConnecting = value; } }
    public Boolean IsError { get { return SocketClient.IsError; } set { SocketClient.IsError = value; } }
    public event EventHandler<SocketEvent> OnEventMessage = null ;

    public string Host { get; set; }
    public string ID { get; set; }
    public string RoomId { get; set; }
    public List<string> Chats = new List<string>() ;

    public GameClient(string host, string id)
    {
        Host = host;
        ID = id;
    }


    public void Connect()
    {
        SocketClient.Connect(Host, ID, OnEvent);
    }

    public void Disconnect()
    {
        SocketClient.Disconnect();
    }

    public void checkRoom()
    {
        SocketClient.Emit("getRoom");
    }

    public void createRoom()
    {
        SocketClient.Emit("createRoom");
    }

    public void leaveRoom()
    {
        SocketClient.Emit("leaveRoom");
    }

    public void joinRoom(string roomId)
    {
        SocketClient.Emit("joinRoom", roomId);
    }

    public void chatInRoom(string msg)
    {
        SocketClient.Emit("chatInRoom", msg);
    }

    private void OnEvent(Socket socket, Packet packet, object[] args)
    {
        if(OnEventMessage != null)
            OnEventMessage(this, new SocketEvent(0, packet.EventName));

        if ( packet.EventName == "respondGetRoom")
        {
            var data = args[0] as Dictionary<string, object>;
            RoomId = (bool)data["isSuccess"]?data["roomId"] as string:"";
        }
        else if (packet.EventName == "respondCreateRoom")
        {
            var data = args[0] as Dictionary<string, object>;
            RoomId = (bool)data["isSuccess"] ? data["roomId"] as string : "";
        }
        else if (packet.EventName == "respondJoinRoom")
        {
            var data = args[0] as Dictionary<string, object>;
            RoomId = (bool)data["isSuccess"] ? data["roomId"] as string : "";
        }
        else if( packet.EventName == "respondMessageInRoom")
        {
            var data = args[0] as Dictionary<string, object>;
            var msg = data["message"] as string;
            Chats.Add(msg);
        }

    }

}
