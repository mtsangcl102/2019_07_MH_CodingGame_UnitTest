using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;
using BestHTTP.SocketIO.JsonEncoders;
using System;

namespace Tests
{
    // return format from server
    //{isSuccess: boolean, roomId: string, message: string}
    public class SocketEvent : EventArgs
    {
        public SocketEventType EventType;
    }
    
    public enum SocketEventType
    {
        Connecting,
        Connected,
    }
    
    public class GameClient
    {
        private SocketOptions _options = new SocketOptions();

        public bool IsConnecting = false;
        public bool IsConnected = false;

        public delegate void EventMessage(string sender, Packet message);
        public event EventMessage OnEventMessage;

        public EventHandler<SocketEvent> OnSocketEvent;
        public SocketEvent _socketEvent = new SocketEvent();

        private string _host = "";
        private string _userId = "";
        public string RoomId = "";
        public List<string> Chats = new List<string>();

        private Socket _socket;

        public GameClient(string host, string userId)
        {
            _host = host;
            _userId = userId;
        }

        public void Connect()
        {
            IsConnecting = true;
            IsConnected = false;

            _socketEvent.EventType = SocketEventType.Connecting;

            _options.AdditionalQueryParams = new ObservableDictionary<string, string>();
            _options.AdditionalQueryParams.Add("userId", _userId);
            _options.ConnectWith = TransportTypes.WebSocket;

            Uri _uri = new Uri(_host);
            SocketManager _manager = new SocketManager(_uri, _options);
            _manager.Encoder = new LitJsonEncoder();
            _socket = _manager.Socket;
            _socket.On("connect", OnConnect);
            _socket.On("disconnect", OnDisconnect);
            _socket.On("error", OnError);
            _socket.On("event", OnEvent);
            
            
        }

        public void Disconnect()
        {
            if (IsConnected || IsConnecting)
            {
                if (_socket?.IsOpen ?? false)
                {
                    _socket.Off();
                    _socket.Disconnect();
                    _socket = null;
                }
                IsConnected = false;
                IsConnecting = false;
            }
        }

        public void OnConnect(Socket socket, Packet packet, params object[] args)
        {
            // TODO
            _socketEvent.EventType = SocketEventType.Connected;
            OnSocketEvent(this, _socketEvent);
            IsConnecting = false;
            IsConnected = true;
        }

        public void OnDisconnect(Socket socket, Packet packet, params object[] args)
        {
            Disconnect();
        }

        public void OnError(Socket socket, Packet packet, params object[] args)
        {
            // TODO
            Disconnect();
        }

        public void OnEvent(Socket socket, Packet packet, params object[] args)
        {
            // TODO
            Dictionary<string, object> response = (Dictionary<string, object>) args[0];
            _socketEvent.EventType = SocketEventType.Connected; 
//            OnSocketEvent(this, _socketEvent);
            switch (packet.EventName)
            {
                case "respondGetRoom":
                    if ((bool) response["isSuccess"])
                    {
                        RoomId = (string) response["roomId"];
                    }
                    else
                    {
                        RoomId = "";
                    }
                    OnSocketEvent(this, _socketEvent);
                    break;
                case "respondLeaveRoom":
                    if ((bool) response["isSuccess"])
                    {
                        RoomId = "";
                    }
                    OnSocketEvent(this, _socketEvent);
                    break;
                case "respondCreateRoom":
                    if ((bool) response["isSuccess"])
                    {
                        RoomId = (string) response["roomId"];
                        Chats.Clear();
                    }
                    OnSocketEvent(this, _socketEvent);
                    break;
                case "respondJoinRoom":
                    if ((bool) response["isSuccess"])
                    {
                        RoomId = (string) response["roomId"];
                        Chats.Clear();
                    }
                    OnSocketEvent(this, _socketEvent);
                    break;
                case "respondChatInRoom":
                    Debug.Log("respondChatInRoom");
                    OnSocketEvent(this, _socketEvent);
                    break;
                case "respondMessageInRoom":
                    Debug.Log("respondMessageinRoom");
                    Chats.Add((string) response["message"]);
                    OnEventMessage("", packet);
                    break;
            }
        }

        public void GetRoom()
        {
            _socketEvent.EventType = SocketEventType.Connecting;
            _socket.Emit("getRoom");
        }

        public void LeaveRoom()
        {
            _socketEvent.EventType = SocketEventType.Connecting;
            _socket.Emit("leaveRoom");
        }

        public void CreateRoom()
        {
            _socketEvent.EventType = SocketEventType.Connecting;
            _socket.Emit("createRoom");
        }

        public void JoinRoom(string roomId)
        {
            _socketEvent.EventType = SocketEventType.Connecting;
            _socket.Emit("joinRoom", roomId);
        }

        public void ChatInRoom(string message)
        {
            _socketEvent.EventType = SocketEventType.Connecting;
            _socket.Emit("chatInRoom", message);
        }
    }
}
