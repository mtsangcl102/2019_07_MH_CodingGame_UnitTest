using System;
using System.Collections.Generic;

namespace Core
{
    public class GameClient
    {
        public SocketClient SocketClient;
        public string RoomId;
        public List<string> Chats = new List<string>();
        public bool IsConnecting => SocketClient.IsConnecting;
        public bool IsConnected => SocketClient.IsConnected;
        public bool HasRoom => ! string.IsNullOrEmpty(RoomId );
        
        public event EventHandler<GameClientEventArgs> OnEventMessage;
        
        public GameClient( string host , string userId )
        {
            SocketClient = new SocketClient( host , userId );
            SocketClient.OnSocketEvent += OnSocketEvent;
        }

        private void OnSocketEvent( object sender , SocketEventArgs e )
        {
            if ( e is GameClientEventArgs gameClientEventArgs )
            {
                // Debug.Log( $"event name: {gameClientEventArgs.EventName}." );

                switch ( gameClientEventArgs.EventName )
                {
                    case "respondCreateRoom":
                    case "respondJoinRoom":
                    case "respondGetRoom":
                    {
                        var dict = gameClientEventArgs.Args[0] as Dictionary<string , object>; 
                        if ( dict["isSuccess"] is bool isSuccess && isSuccess )
                            RoomId = dict["roomId"].ToString();
                        else
                            RoomId = null;
                    }
                        break;
                    case "respondLeaveRoom":
                    {
                        var dict = gameClientEventArgs.Args[0] as Dictionary<string , object>; 
                        RoomId = null;
                    }
                        break;
                    case "respondMessageInRoom":
                    {
                        var dict = gameClientEventArgs.Args[0] as Dictionary<string , object>; 
                        if ( dict["isSuccess"] is bool isSuccess && isSuccess )
                            Chats.Add( dict["message"].ToString() );
                    }
                        break;
                }
                
                OnEventMessage?.Invoke( this , gameClientEventArgs );
            }
        }


        public void Connect()
        {
            SocketClient.Connect();
        }

        public void Disconnect()
        {
            SocketClient.Disconnect();
            RoomId = null;
            Chats.Clear();
        }

        public void GetRoom()
        {
            SocketClient.Emit( "getRoom" , null );
        }

        public void CreateRoom()
        {
            SocketClient.Emit( "createRoom" , null );
        }

        public void LeaveRoom()
        {
            SocketClient.Emit( "leaveRoom" , null );
        }

        public void JoinRoom( string roomId )
        {
            SocketClient.Emit( "joinRoom" , roomId );
        }
        
        public void ChatInRoom( string message )
        {
            SocketClient.Emit( "chatInRoom" , message );
        }
    }
}