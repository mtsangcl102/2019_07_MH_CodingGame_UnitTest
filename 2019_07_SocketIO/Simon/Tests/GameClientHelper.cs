using System;
using System.Collections;
using System.Collections.Generic;
using Tests;
using UnityEngine.Assertions;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;
using Socket = BestHTTP.SocketIO.Socket;

public class SocketEvent
{
    public int EventType;
    public string EventName;
    public bool IsSuccess;
    public string Message;

    public SocketEvent(int type, bool isSuccess, string message = "" )
    {
        EventType = type;
        IsSuccess = isSuccess;
        Message = message;
        if (EventType == SocketEventType.Message)
        {
            EventName = "respondMessageInRoom";
        }
    }
}

public class SocketEventType
{
    public const int Connected = 1;
    public const int Disconnected = 2;
    public const int Error = 3;
    public const int GetRoom = 4;
    public const int CreateRoom = 5;
    public const int LeaveRoom = 6;
    public const int JoinRoom = 7;
    public const int Message = 8;
}

public class SocketClient
{
    public EventHandler<SocketEvent> OnSocketEvent ;
}

public class GameClient
{
    public string Host;
    public string Name;
    public string RoomId;
    public bool IsConnected;
    public bool IsConnecting;

    public SocketClient SocketClient;
    public EventHandler<SocketEvent> OnEventMessage;
    
    public List<string> Chats = new List<string>();

    private SocketManager _manager;
    private SocketOptions _options;
    private Socket _socket;
    private int _currentEvent;

    public GameClient(string host, string name)
    {
        Host = host;
        Name = name;
        RoomId = "";
        
        SocketClient = new SocketClient();
        
        _options = new SocketOptions();
        _options.AdditionalQueryParams = new ObservableDictionary<string, string>();
        _options.AdditionalQueryParams.Add("userId", name);
        _options.AutoConnect = false;
        _options.ConnectWith = TransportTypes.WebSocket;
    }

    private void OnConnect(Socket socket, Packet packet, params object[] args)
    {
        IsConnected = true;
        IsConnecting = false;
        Debug.Log(Name +" OnConnect");
        if (SocketClient.OnSocketEvent!= null)
        {
            SocketClient.OnSocketEvent(this, new SocketEvent( SocketEventType.Connected, true ));
        }
    }
    private void OnDisconnect(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log(Name +" OnDisconnect");
        IsConnected = false;
        if (SocketClient.OnSocketEvent!= null)
        {
            SocketClient.OnSocketEvent(this, new SocketEvent( SocketEventType.Disconnected, true ));
        }
    }
    private void OnError(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log(Name +" OnError");
        IsConnecting = false;
        IsConnected = false;
        if (SocketClient.OnSocketEvent!= null)
        {
            SocketClient.OnSocketEvent(this, new SocketEvent( SocketEventType.Error, false ));
        }
    }
    private void OnEvent(Socket socket, Packet packet, params object[] args)
    {
        Debug.Log(Name + " OnEvent");

        var dictionary = (Dictionary<string, object>) args[0]; 
        foreach (var key in dictionary.Keys )
        {
            Debug.Log( key + ":" + dictionary[key] );
        }
         
        if (dictionary.ContainsKey("isSuccess"))
        {
            bool isSuccess = (bool) dictionary["isSuccess"];
            string message = "";
             
            if (dictionary.ContainsKey("roomId"))
            {
                RoomId = (string) dictionary["roomId"];
            }
            if (dictionary.ContainsKey("message"))
            {
                message = (string) dictionary["message"];
                Chats.Add( message );
            }

            if (_currentEvent == SocketEventType.GetRoom && !isSuccess )
            {
                RoomId = "";
            }
            if (_currentEvent == SocketEventType.LeaveRoom && isSuccess )
            {
                RoomId = "";
            }
             
            if (OnEventMessage!= null)
            {
                OnEventMessage(this, new SocketEvent( _currentEvent, isSuccess, message ));
            }
        }
    }
    public void Connect()
    {
        Uri _uri = new Uri(Host);
        _manager = new SocketManager(_uri, _options);
        _manager.Encoder = new LitJsonEncoder();
        _socket = _manager.Socket;
        _socket.On("connect", OnConnect);
        _socket.On("disconnect", OnDisconnect);
        _socket.On("error", OnError);
        _socket.On("event", OnEvent);
        
        IsConnecting = true;
        Debug.Log(Name + " SendConnect");
        _manager.Open();
    }

    public void Disconnect()
    {
        Debug.Log(Name +" SendDisconnect");
        _manager?.Close();
        _manager = null;
    }

    public void SendGetRoom()
    {
        _currentEvent = SocketEventType.GetRoom;
        Debug.Log(Name +" Send Get Room");
        _socket.Emit("getRoom");
    }
    
    public void SendLeaveRoom()
    {
        _currentEvent = SocketEventType.LeaveRoom;
        Debug.Log(Name +" Send Leave Room");
        _socket.Emit("leaveRoom");
    }
    
    public void SendCreateRoom()
    {
        _currentEvent = SocketEventType.CreateRoom;
        Debug.Log(Name +" Send Create Room");
        _socket.Emit("createRoom");
    }
    
    public void SendJoinRoom( string roomId )
    {
        _currentEvent = SocketEventType.JoinRoom;
        Debug.Log(Name +" Send Join Room " + roomId );
        _socket.Emit("joinRoom", roomId);
    }
    
    public void SendChatInRoom( string message )
    {
        _currentEvent = SocketEventType.Message;
        Debug.Log(Name +" Send Chat " + message );
        _socket.Emit("chatInRoom", message);
    }
}

public class GameClientHelper
{
    public static IEnumerator AssertConnect(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.Connected);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.Connect();

        Assert.AreEqual(gameClient.IsConnecting, true);

        yield return TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }
    
    public static IEnumerator AssertDisconnect(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.Disconnected);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.Disconnect();
        yield return TestHelper.Timeout(() => handler == null);
    }
    
    public static IEnumerator AssertInvalidConnect(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.Error);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.Connect();

        Assert.AreEqual(gameClient.IsConnecting, true);

        yield return TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }
    
    public static IEnumerator AssertCreateRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnEventMessage += handler = (sender, e) =>
        {
            gameClient.OnEventMessage -= handler;
            handler = null;
        };
        gameClient.SendCreateRoom();
        yield return TestHelper.Timeout(() => handler == null);
        AssertHasRoom(gameClient);
    }
    
    public static IEnumerator AssertJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnEventMessage += handler = (sender, e) =>
        {
            gameClient.OnEventMessage -= handler;
            handler = null;
        };
        gameClient.SendJoinRoom( roomId );
        yield return TestHelper.Timeout(() => handler == null);
        Assert.AreEqual(gameClient.RoomId, roomId );
    }
    
    public static IEnumerator AssertInvalidJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnEventMessage += handler = (sender, e) =>
        {
            gameClient.OnEventMessage -= handler;
            handler = null;
        };
        gameClient.SendJoinRoom( roomId );
        yield return TestHelper.Timeout(() => handler == null);
        Assert.AreNotEqual(gameClient.RoomId, roomId);
    }
    
    public static IEnumerator AssertHasRoom(GameClient gameClient)
    {
        Assert.AreNotEqual(gameClient.RoomId, "");
        yield return null;
    }
    
    public static IEnumerator AssertLeaveRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnEventMessage += handler = (sender, e) =>
        {
            gameClient.OnEventMessage -= handler;
            handler = null;
        };
        gameClient.SendLeaveRoom();
        yield return TestHelper.Timeout(() => handler == null);
        
        Assert.AreEqual(gameClient.RoomId, "");
    }
    
    public static IEnumerator AssertLeaveIfHasRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnEventMessage += handler = (sender, e) =>
        {
            gameClient.OnEventMessage -= handler;
            handler = null;
        };
        gameClient.SendGetRoom();
        yield return TestHelper.Timeout(() => handler == null);

        if (gameClient.RoomId != "")
        {
            yield return AssertLeaveRoom( gameClient );
        }

        Assert.AreEqual(gameClient.RoomId, "");
    }
    
    public static IEnumerator AssertChatInRoom(GameClient gameClient, string message)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnEventMessage += handler = (sender, e) =>
        {
            gameClient.OnEventMessage -= handler;
            handler = null;
        };
        gameClient.SendChatInRoom( message );
        yield return TestHelper.Timeout(() => handler == null);
    }
}
