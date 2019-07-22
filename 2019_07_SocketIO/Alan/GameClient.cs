using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP;
using BestHTTP.Statistics;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;

public class SocketEvent : EventArgs
{
    public Socket socket { get; set; }
    public Packet packet { get; set; }
    public object[] args { get; set; }
    public String EventName
    {
        get
        {
            return packet.EventName;
        }
    }

    public String EventType { get; set; }
}

public class GameClient
{
    public event EventHandler<SocketEvent> OnSocketEvent;
    public string _host;
    public string _userId;
    public SocketManager _socketManger;
    SocketOptions _options;
    public bool IsConnected = false;
    public bool IsConnecting = false;
    public string RoomId = "";
    public List<string> Chats = new List<string>();

    public GameClient(string host, string userId)
    {
        _host = host;
        _userId = userId;
        RoomId = "";
        IsConnected = false;
        IsConnecting = false;

        // Change an option to show how it should be done
        _options = new SocketOptions();
        _options.AdditionalQueryParams = new ObservableDictionary<string, string>();
        _options.AdditionalQueryParams.Add("userId", userId);
        _options.ConnectWith = TransportTypes.WebSocket;
    }

    public void connect()
    {
        if (!IsConnected && !IsConnecting)
        {
            // Create the Socket.IO 
            _socketManger = new SocketManager(new Uri(_host), _options);
            _socketManger.Encoder = new BestHTTP.SocketIO.JsonEncoders.LitJsonEncoder();

            IsConnecting = true;

            // standard events
            Socket _socket = _socketManger.Socket;
            _socket.On("connect", _onConnect);
            _socket.On("event", _onEventCallBack);
            _socket.Once("disconnect", _onDisconnect);
        }
    }

    public bool hasRoom()
    {
        return !(RoomId == "");
    }

    public string userId()
    {
        return _userId;
    }

    void _onConnect(Socket socket, Packet packet, params object[] args)
    {
        IsConnecting = false;
        IsConnected = true;
        SendOnSocketEvent(SocketEventType.Connected, socket, packet, args);
    }

    void _onDisconnect(Socket socket, Packet packet, params object[] args)
    {
        Disconnect();
    }

    void _onError(Socket socket, Packet packet, params object[] args)
    {
        Debug.LogError(string.Format("--ERROR - {0}", args[0].ToString()));
        Disconnect();
    }

    public void Disconnect()
    {
        if (IsConnecting || IsConnected)
        {
            _socketManger.Socket.Off("event");
            _socketManger.Socket.On("disconnect", _onDisconnect);
            _socketManger.Socket.Disconnect();

            RoomId = "";
            IsConnecting = false;
            IsConnected = false;
            Chats.Clear();
        }
    }

    void _onEventCallBack(Socket socket, Packet packet, params object[] args)
    {
        if (args.Length > 0 && args[0] is Dictionary<string, object>)
        {
            Dictionary<string, object> tempArgs = (Dictionary<string, object>)args[0];

            foreach (KeyValuePair<string, object> _entry in tempArgs)
            {
                switch (_entry.Key)
                {
                    case "message":
                        if (packet.EventName == SocketEventType.MessageInRoom)
                            Chats.Add(_entry.Value.ToString());
                        break;

                    case "isSuccess":
                        break;

                    case "roomId":
                        RoomId = _entry.Value.ToString();
                        break;

                    default:
                        Debug.Log(_entry.Key + " : " + _entry.Value);
                        break;
                }
            }

            switch (packet.EventName)
            {
                case SocketEventType.Broadcast:

                    break;

                default:
                    SendOnSocketEvent(packet.EventName, socket, packet, args);
                    break;
            }
        }
    }

    public void createRoom()
    {
        if (_socketManger.Socket != null)
        {
            _socketManger.Socket.Emit(SocketEmitType.CreateRoom);
        }
    }

    public void getRoom()
    {
        if (_socketManger.Socket != null)
        {
            _socketManger.Socket.Emit(SocketEmitType.GetRoom);
        }
    }

    public void leaveRoom()
    {
        if (_socketManger.Socket != null)
        {
            RoomId = "";
            _socketManger.Socket.Emit(SocketEmitType.LeaveRoom);
        }
    }

    public void joinRoom(string roomId)
    {
        if (_socketManger.Socket != null)
        {
            _socketManger.Socket.Emit(SocketEmitType.JoinRoom, roomId);
        }
    }

    public void chatInRoom(string message)
    {
        if (IsConnected & hasRoom())
        {
            _socketManger.Socket.Emit(SocketEmitType.ChatInRoom, message);
        }
    }

    private void SendOnSocketEvent(string eventType, Socket socket, Packet packet, params object[] args)
    {
        SocketEvent tempSocketEvent = new SocketEvent();
        tempSocketEvent.socket = socket;
        tempSocketEvent.packet = packet;
        tempSocketEvent.args = args;
        tempSocketEvent.EventType = eventType;

        OnSocketEvent?.Invoke(this, tempSocketEvent);
    }
}