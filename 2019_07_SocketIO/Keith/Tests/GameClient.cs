
using System.Collections.Generic;
using System;
using UnityEngine;
using BestHTTP;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;
using BestHTTP.SocketIO.JsonEncoders;

public class SocketClient
{
    public EventHandler<SocketEvent> OnSocketEvent;//= new EventHandler<SocketEvent>() ;
}

public enum SocketEventType
{
    Connecting,
    Connected
}

public class SocketEvent : EventArgs
{
    public SocketEventType EventType;
}

public class GameClient
{
    public string host = "";
    public string userId = "";

    public string RoomId = "";

    //public EventHandler OnEventMessage;

    public List<string> Chats = new List<string>();

    public bool IsConnected = false;
    public bool IsConnecting = false;
    public SocketClient SocketClient = new SocketClient();

    //public EventMessage message;

    public delegate void EventMessage( string sender, Packet message );
    public event EventMessage OnEventMessage;

    public SocketEvent socketEvent = new SocketEvent();
    public Socket _socket;
    public SocketManager _socketManager;

    public GameClient( string host, string userId )
    {
        this.host = host;
        this.userId = userId;
    }

    public void Connect()
    {
        IsConnecting = true;
        IsConnected = false;
        socketEvent.EventType = SocketEventType.Connecting;

        SocketOptions _options = new SocketOptions();
        _options.AdditionalQueryParams = new ObservableDictionary<string, string>();
        _options.AdditionalQueryParams.Add("userId", userId);
        _options.ConnectWith = TransportTypes.WebSocket;

        //Uri _uri = new Uri( "https://blooming-fjord-24277.herokuapp.com/socket.io/" );
        Uri _uri = new Uri( host );
        _socketManager = new SocketManager( _uri, _options );
        _socketManager.Encoder = new LitJsonEncoder();
        _socket = _socketManager.Socket;
        _socket.On( "connect", OnConnect );
        _socket.On( "disconnect", OnDisconnect );
        //_socket.On( "disconnecting", OnDisconnecting );
        _socket.On( "error", OnError );
        _socket.On( "event", OnEvent );
    }

    public void Disconnect()
    {
        if( IsConnected || IsConnecting )
        {
            IsConnecting = false;
            IsConnected = false;
            if( _socket.IsOpen )
            {
                _socket.Off();
                _socket.Disconnect();
                _socket = null;
                _socketManager.Close();
                _socketManager = null;
            }
        }
    }

    #region -----[ Event ]-----
    public void OnConnect( Socket socket, Packet packet, object[] args )
    {
        IsConnecting = false;
        IsConnected = true;
        socketEvent.EventType = SocketEventType.Connected;
        SocketClient.OnSocketEvent( this, socketEvent );
    }

    public void OnDisconnect( Socket socket, Packet packet, object[] args )
    {
        //Debug.LogWarning( "OnDisconnect" );
        Disconnect();
    }

    public void OnDisconnecting( Socket socket, Packet packet, object[] args )
    {
        //Debug.LogWarning( "OnDisconnecting" );
    }

    public void OnError( Socket socket, Packet packet, object[] args )
    {
        //Debug.LogWarning( "OnError" );
        Disconnect();
    }

    public void OnEvent( Socket socket, Packet packet, object[] args )
    {
        socketEvent.EventType = SocketEventType.Connected;

        Dictionary<string, object> arg = args[ 0 ] as Dictionary<string, object>;
        foreach( KeyValuePair<string, object> tmp in arg )
        {
            Debug.Log( tmp );
        }

        bool isSuccess = arg.ContainsKey( "isSuccess" ) ? (bool)arg[ "isSuccess" ] : false ;
        string roomId = arg.ContainsKey( "roomId" ) ? ( string)arg[ "roomId" ] : "";
        Debug.Log( packet.EventName );
        switch( packet.EventName )
        {
            case "respondGetRoom":
                RoomId = isSuccess ? roomId : "";
                SocketClient.OnSocketEvent( this, socketEvent );
                break;
            case "respondLeaveRoom":
                if( isSuccess ) RoomId = "";
                SocketClient.OnSocketEvent( this, socketEvent );
                break;
            case "respondCreateRoom":
                if( isSuccess )
                {
                    RoomId = roomId;
                    Chats.Clear();
                }
                SocketClient.OnSocketEvent( this, socketEvent );
                break;
            case "respondJoinRoom":
                if( isSuccess )
                {
                    RoomId = roomId;
                    Chats.Clear();
                }
                SocketClient.OnSocketEvent( this, socketEvent );
                break;
            case "respondChatInRoom":
                SocketClient.OnSocketEvent( this, socketEvent );
                break;
            case "respondMessageInRoom":
                Chats.Add( (string)arg[ "message" ] );
                OnEventMessage( "", packet );
                break;
        }
    }
    #endregion

    #region -----[ Internal ]-----
    internal void GetRoom()
    {
        socketEvent.EventType = SocketEventType.Connecting;
        _socket.Emit( "getRoom" );
    }
    internal void LeaveRoom()
    {
        socketEvent.EventType = SocketEventType.Connecting;
        _socket.Emit( "leaveRoom" );
    }

    internal void CreateRoom()
    {
        socketEvent.EventType = SocketEventType.Connecting;
        _socket.Emit( "createRoom" );
    }

    internal void JoinRoom( string roomId )
    {
        socketEvent.EventType = SocketEventType.Connecting;
        _socket.Emit( "joinRoom", roomId );
    }

    internal void ChatInRoom( string message )
    {
        socketEvent.EventType = SocketEventType.Connecting;
        _socket.Emit( "chatInRoom", message );
    }
    #endregion
}