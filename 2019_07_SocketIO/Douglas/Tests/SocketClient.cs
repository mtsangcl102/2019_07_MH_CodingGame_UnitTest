using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tests {
    public class SocketClient {
        public EventHandler<SocketEvent> OnSocketEvent;
    }

    public class SocketEvent {
        private SocketNameType _eventType = SocketNameType.None;
        private string _message;

        public SocketEvent(SocketNameType eventType, string message = "") {
            _eventType = eventType;
            _message = message;
        }


        public SocketNameType EventType { get { return _eventType; } }
        public enum SocketNameType {
            Connected,
            Disconnected,
            None,
            GetRoom,
            LeaveRoom,
            HasRoom,
            CreateRoom,
            JoinRoom,
            ChatInRoom,
            MessageInRoom,
        }
    }

    public class SocketMessage {
        public string EventName { get; internal set; }
        public string _message;
        public SocketMessage(string _eventName, string message) {
            EventName = _eventName;
            _message = message;
        }

    }
}