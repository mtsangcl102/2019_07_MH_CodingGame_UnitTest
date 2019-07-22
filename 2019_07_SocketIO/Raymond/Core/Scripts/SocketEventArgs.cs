// Copyright (c) Mad Head Limited All Rights Reserved

using System;

namespace Core
{
    public class SocketEventArgs : EventArgs
    {
        public SocketEventType EventType;

        public SocketEventArgs( SocketEventType eventType )
        {
            EventType = eventType;
        }
    }

    public class GameClientEventArgs : SocketEventArgs
    {
        public string EventName;
        public object[] Args;

        public GameClientEventArgs( SocketEventType eventType , string eventName , object[] args ) : base( eventType )
        {
            EventName = eventName;
            Args = args;
        }
    }
}