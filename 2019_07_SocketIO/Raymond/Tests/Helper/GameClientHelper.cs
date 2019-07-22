using System;
using System.Collections;
using Core;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class GameClientHelper
    {
        public static IEnumerator AssertInvalidConnect( GameClient gameClient )
        {
            EventHandler<SocketEventArgs> handler = null;
            Assert.AreEqual( gameClient.IsConnected , false );
            Assert.AreEqual( gameClient.IsConnecting , false );

            gameClient.SocketClient.OnSocketEvent += handler = ( sender , e ) =>
            {
                Assert.AreEqual( e.EventType , SocketEventType.Error );
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            };
            gameClient.Connect();

            Assert.AreEqual( gameClient.IsConnecting , true );

            yield return TestHelper.Timeout( () => handler == null );

            Assert.AreEqual( gameClient.IsConnected , false );
            Assert.AreEqual( gameClient.IsConnecting , false );
        }

        public static IEnumerator AssertConnect( GameClient gameClient )
        {
            EventHandler<SocketEventArgs> handler = null;
            Assert.AreEqual( gameClient.IsConnected , false );
            Assert.AreEqual( gameClient.IsConnecting , false );

            gameClient.SocketClient.OnSocketEvent += handler = ( sender , e ) =>
            {
                Assert.AreEqual( e.EventType , SocketEventType.Connected );
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            };
            gameClient.Connect();

            Assert.AreEqual( gameClient.IsConnecting , true );

            yield return TestHelper.Timeout( () => handler == null );

            Assert.AreEqual( gameClient.IsConnected , true );
            Assert.AreEqual( gameClient.IsConnecting , false );
        }

        public static IEnumerator AssertDisconnect( GameClient gameClient )
        {
            Assert.IsTrue( gameClient.IsConnected );
            gameClient.Disconnect();
            Assert.IsFalse( gameClient.IsConnected );
            Assert.IsFalse( gameClient.HasRoom );
            yield return null;
        }

        public static IEnumerator AssertHasRoom( GameClient gameClient )
        {
            yield return _AssertGameClientRequestAndRespond( gameClient , "respondGetRoom" , gameClient.GetRoom );
            Assert.IsTrue( gameClient.HasRoom );
        }

        public static IEnumerator AssertCreateRoom( GameClient gameClient )
        {
            yield return _AssertGameClientRequestAndRespond( gameClient , "respondCreateRoom" , gameClient.CreateRoom );
            Assert.IsTrue( gameClient.HasRoom );
        }

        public static IEnumerator AssertJoinRoom( GameClient gameClient , string roomId )
        {
            yield return _AssertGameClientRequestAndRespond( gameClient , "respondJoinRoom" ,
                () => { gameClient.JoinRoom( roomId ); } );
            Assert.IsTrue( gameClient.HasRoom );
        }

        public static IEnumerator AssertInvalidJoinRoom( GameClient gameClient , string roomId )
        {
            yield return _AssertGameClientRequestAndRespond( gameClient , "respondJoinRoom" ,
                () => { gameClient.JoinRoom( roomId ); } );
            Assert.IsFalse( gameClient.HasRoom );
        }

        public static IEnumerator AssertLeaveRoom( GameClient gameClient )
        {
            Assert.IsTrue( gameClient.HasRoom );
            yield return _AssertGameClientRequestAndRespond( gameClient , "respondLeaveRoom" , gameClient.LeaveRoom );
            Assert.IsFalse( gameClient.HasRoom );
        }


        public static IEnumerator AssertLeaveIfHasRoom( GameClient gameClient )
        {
            yield return _AssertGameClientRequestAndRespond( gameClient , "respondGetRoom" , gameClient.GetRoom );

            if ( gameClient.HasRoom )
            {
                // Debug.Log( $"Has room: {gameClient.RoomId}" );
                // if has room
                yield return _AssertGameClientRequestAndRespond( gameClient , "respondLeaveRoom" ,
                    gameClient.LeaveRoom );
            }
        }



        public static IEnumerator AssertChatInRoom( GameClient gameClient , string newMessage )
        {
            Assert.IsTrue( gameClient.HasRoom );
            yield return _AssertGameClientRequestAndRespond( gameClient , "respondChatInRoom" ,
                () => { gameClient.ChatInRoom( newMessage ); } );
        }


        private static IEnumerator _AssertGameClientRequestAndRespond( GameClient gameClient , string eventName ,
            Action requestAction )
        {
            EventHandler<GameClientEventArgs> handler = null;

            gameClient.OnEventMessage += handler = ( sender , e ) =>
            {
                if ( e.EventName == eventName )
                {
                    // Debug.Log( $"respond {eventName}" );
                    gameClient.OnEventMessage -= handler;
                    handler = null;
                }
            };
            requestAction?.Invoke();
            yield return TestHelper.Timeout( () => handler == null );
        }
    }
}