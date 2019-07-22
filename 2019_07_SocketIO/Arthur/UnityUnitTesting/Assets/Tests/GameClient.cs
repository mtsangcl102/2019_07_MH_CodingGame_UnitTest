using System.Collections;
using System.Collections.Generic;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.Transports;
using BestHTTP.SocketIO.JsonEncoders;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;
using System;

public enum SocketEventType
{
    Connected,
    Disconnected,
    Error,
    GetRoom,
    CreateRoom,
    JoinRoom,
    LeaveRoom,
}
public struct SocketClient
{
    public EventHandler<SocketEvent> OnSocketEvent;
}

public struct SocketEvent
{
    public SocketEventType EventType;
}

public class GameClient
{
    public bool IsConnecting, IsConnected;
    public string RoomId = "";
    public List<string> Chats = new List<string>();

    public SocketClient SocketClient;
    public Action<Socket, Packet> OnEventMessage;
    
    
    private string _host, _userId;
    private SocketManager _manager;
    private Socket _socket;
    
    
    
    public GameClient(string host, string userId)
    {
        _host = host;
        _userId = userId;
    }
    


    void OnConnect(Socket socket, Packet packet, params object[] args)
    {
        IsConnecting = false;
        IsConnected = true;

        if(SocketClient.OnSocketEvent != null)
            SocketClient.OnSocketEvent(socket, new SocketEvent(){EventType = SocketEventType.Connected});
    }

    void OnDisconnect(Socket socket, Packet packet, params object[] args)
    {
        IsConnecting = false;
        IsConnected = false;
        
        if(SocketClient.OnSocketEvent != null)
            SocketClient.OnSocketEvent(socket, new SocketEvent(){EventType = SocketEventType.Disconnected});
    }
    
    void OnError(Socket socket, Packet packet, params object[] args)
    {
        IsConnecting = false;
        
        if(SocketClient.OnSocketEvent != null)
            SocketClient.OnSocketEvent(socket, new SocketEvent(){EventType = SocketEventType.Error});
    }
    
    void OnEvent(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log(packet.Payload);
        Dictionary<string, object> dic;
        switch (packet.DecodeEventName())
        {
            case "respondGetRoom":
                dic = args[0] as Dictionary<string, object>;
                if (dic.ContainsKey("isSuccess"))
                {
                    if (dic.ContainsKey("roomId"))
                        RoomId = dic["roomId"].ToString();
                    
                    if(SocketClient.OnSocketEvent != null)
                        SocketClient.OnSocketEvent(socket, new SocketEvent(){EventType = SocketEventType.GetRoom});
                }
                break;
            case "respondCreateRoom":
                dic = args[0] as Dictionary<string, object>;
                if (dic.ContainsKey("isSuccess"))
                {
                    if (dic.ContainsKey("roomId"))
                        RoomId = dic["roomId"].ToString();
                    
                    if(SocketClient.OnSocketEvent != null)
                        SocketClient.OnSocketEvent(socket, new SocketEvent(){EventType = SocketEventType.CreateRoom});
                }
                break;
            case "respondJoinRoom":
                dic = args[0] as Dictionary<string, object>;
                if (dic.ContainsKey("isSuccess"))
                {
                    if ((bool) dic["isSuccess"])
                    {
                        if (dic.ContainsKey("roomId"))
                            RoomId = dic["roomId"].ToString();
                    }
                    else
                    {
                        RoomId = "";
                    }
                    
                    if(SocketClient.OnSocketEvent != null)
                        SocketClient.OnSocketEvent(socket, new SocketEvent(){EventType = SocketEventType.JoinRoom});
                }
                break;
            case "respondMessageInRoom":
                dic = args[0] as Dictionary<string, object>;
                if (dic.ContainsKey("isSuccess"))
                {
                    if (dic.ContainsKey("message"))
                        Chats.Add(dic["message"].ToString());
                    OnEventMessage(socket, packet);
                }
                break;
            case "respondLeaveRoom":
                dic = args[0] as Dictionary<string, object>;
                if (dic.ContainsKey("isSuccess"))
                {
                    if ((bool) dic["isSuccess"])
                    {
                        RoomId = "";
                        
                        if(SocketClient.OnSocketEvent != null)
                            SocketClient.OnSocketEvent(socket, new SocketEvent(){EventType = SocketEventType.LeaveRoom});
                    }
                }
                break;
        }
    }
    
    
    
    
    public void Connect()
    {
        IsConnecting = true;
        
        Uri uri = new Uri(_host);
        SocketOptions options = new SocketOptions();
        options.AdditionalQueryParams = new ObservableDictionary<string, string>();
        options.AdditionalQueryParams.Add("userId", _userId);
        options.ConnectWith = TransportTypes.WebSocket;
        
        _manager = new SocketManager(uri, options);
        _manager.Encoder = new LitJsonEncoder();
        
        _socket = _manager.Socket;
        _socket.On("connect", OnConnect);
        _socket.On("disconnect", OnDisconnect);
        _socket.On("error", OnError);
        _socket.On("event", OnEvent);
        
        _manager.Open();
    }
    public void Disconnect()
    {
        if(_manager != null)
            _manager.Close();
    }

    public void GetRoom()
    {
        _socket.Emit("getRoom");
    }

    public void CreateRoom()
    {
        _socket.Emit("createRoom");
    }
    
    public void JoinRoom(string targetRoomId)
    {
        _socket.Emit("joinRoom", targetRoomId);
    }

    public void LeaveRoom()
    {
        _socket.Emit("leaveRoom");
    }

    public void LeaveIfHasRoom()
    {
        EventHandler<SocketEvent> handler = null;
        
        SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            if (e.EventType == SocketEventType.GetRoom)
            {
                if (RoomId != "")
                    LeaveRoom();
                else
                {
                    if(SocketClient.OnSocketEvent != null)
                        SocketClient.OnSocketEvent(sender, new SocketEvent(){EventType = SocketEventType.LeaveRoom});
                }
                
                SocketClient.OnSocketEvent -= handler;
                handler = null;
            }
        };
        
        GetRoom();
    }

    public void SendChat(string msg)
    {
        _socket.Emit("chatInRoom", msg);
    }
}
