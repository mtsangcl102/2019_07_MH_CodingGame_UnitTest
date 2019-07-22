using System;
using System.Collections.Generic;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;
using UnityEngine;

namespace Tests {
    public class GameClient {
        public SocketClient SocketClient;
        public bool IsConnected = false;
        public bool IsConnecting = false;
        public EventHandler<SocketMessage> OnEventMessage;
        public string RoomId { get; set; }

        public List<string> Chats = new List<string>();

        private string _host;
        private string _userId;
        private SocketOptions _options;
        private SocketManager _manager;
        public BestHTTP.SocketIO.Socket _socket;
        public GameClient(string host, string userId) {
            this._host = host;
            this._userId = userId;
            IsConnected = false;
            IsConnecting = false;
            SocketClient = new SocketClient();


        }

        public void Disconnect() {
            RoomId = "";
            IsConnected = false;
            IsConnecting = false;

            _socket?.On("disconnect", OnDisconnect);
            _socket?.Off("event");
            _manager?.Close();
            SocketClient.OnSocketEvent?.Invoke(this, new SocketEvent(SocketEvent.SocketNameType.Disconnected));
        }

        public void Connect() {
            _options = new SocketOptions();
            _options.AdditionalQueryParams = new ObservableDictionary<string, string>();
            _options.AdditionalQueryParams.Add("userId", _userId);
            _options.ConnectWith = TransportTypes.WebSocket;
            Uri _uri = new Uri(_host);
            _manager = new SocketManager(_uri, _options);
            _manager.Encoder = new LitJsonEncoder();
            _socket = _manager.Socket;

            _socket.On("connect", OnConnect);
            _socket.On("disconnect", OnDisconnect);
            _socket.On("error", OnError);
            _socket.On("event", OnEvent);
            _manager.Open();
            IsConnected = false;
            IsConnecting = true;

        }

        internal void LeaveRoom() { _socket.Emit("leaveRoom"); }
        internal void GetRoom() { _socket.Emit("getRoom"); }
        internal void CreateRoom() { _socket.Emit("createRoom"); }
        internal void JoinRoom(string roomId) { _socket.Emit("joinRoom", roomId); }
        internal void ChatInRoom(string message) { _socket.Emit("chatInRoom", message); }
        public bool HasRoom() { return !String.IsNullOrEmpty(RoomId); }


        private void OnEvent(BestHTTP.SocketIO.Socket socket, Packet packet, object[] args) {
            Dictionary<string, object> dict = (args[0] as Dictionary<string, object>);
            foreach(KeyValuePair<string, object> kvp in dict) {
                Debug.Log(kvp);
            }

            switch(packet.EventName) {
                case "respondGetRoom":
                if((bool) dict["isSuccess"]) {
                    RoomId = (string) dict["roomId"];
                }
                SocketClient.OnSocketEvent?.Invoke(this, new SocketEvent(SocketEvent.SocketNameType.GetRoom));
                break;

                case "respondLeaveRoom":
                if((bool) dict["isSuccess"])
                    RoomId = "";
                SocketClient.OnSocketEvent?.Invoke(this, new SocketEvent(SocketEvent.SocketNameType.LeaveRoom));
                break;

                case "respondCreateRoom":
                if((bool) dict["isSuccess"] && RoomId == "")
                    RoomId = (string) dict["roomId"];
                SocketClient.OnSocketEvent?.Invoke(this, new SocketEvent(SocketEvent.SocketNameType.CreateRoom));
                break;

                case "respondJoinRoom":
                if((bool) dict["isSuccess"] && RoomId == "") {
                    RoomId = (string) dict["roomId"];
                }else{
                    RoomId = "";
                }
                SocketClient.OnSocketEvent?.Invoke(this, new SocketEvent(SocketEvent.SocketNameType.JoinRoom));
                break;

                case "respondChatInRoom":
                SocketClient.OnSocketEvent?.Invoke(this, new SocketEvent(SocketEvent.SocketNameType.ChatInRoom));
                break;

                case "respondMessageInRoom":
                if((string) dict["message"] != "") { Chats.Add((string) dict["message"]); }
                SocketClient.OnSocketEvent?.Invoke(this, new SocketEvent(SocketEvent.SocketNameType.MessageInRoom, (string) dict["message"]));
                OnEventMessage?.Invoke(this, new SocketMessage(packet.EventName, (string) dict["message"]));
                break;
            }
        }

        private void OnError(BestHTTP.SocketIO.Socket socket, Packet packet, object[] args) { }


        private void OnDisconnect(BestHTTP.SocketIO.Socket socket, Packet packet, object[] args) {
            Disconnect();
        }

        private void OnConnect(BestHTTP.SocketIO.Socket socket, Packet packet, object[] args) {
            IsConnected = true;
            IsConnecting = false;
            SocketClient.OnSocketEvent?.Invoke( this, new SocketEvent(SocketEvent.SocketNameType.Connected) );
        }
    }
}