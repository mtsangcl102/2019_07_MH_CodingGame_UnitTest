using System;
using System.Collections;
using BestHTTP.SocketIO;
using BestHTTP;
using NUnit.Compatibility;
using UnityEngine;
using NUnit.Framework;

public class GameClientHelper
{
    public static IEnumerator AssertConnect( GameClient gameClient )
    {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual( gameClient.IsConnected, false );
        Assert.AreEqual( gameClient.IsConnecting, false );
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.Connect();

        Assert.AreEqual( gameClient.IsConnecting, true );

        yield return Tests.TestHelper.Timeout( () => handler == null );

        Assert.AreEqual( gameClient.IsConnected, true );
        Assert.AreEqual( gameClient.IsConnecting, false );
    }

    public static IEnumerator AssertInvalidConnect( GameClient gameClient )
    {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual( gameClient.IsConnected, false );
        Assert.AreEqual( gameClient.IsConnecting, false );

        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.Connect();

        Assert.AreEqual( gameClient.IsConnecting, true );

        yield return Tests.TestHelper.Timeout( () => handler == null || ( !gameClient.IsConnecting && !gameClient.IsConnecting ) );

        Assert.AreEqual( gameClient.IsConnected, false );
        Assert.AreEqual( gameClient.IsConnecting, false );
    }

    public static IEnumerator AssertLeaveIfHasRoom( GameClient gameClient )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.GetRoom();

        yield return Tests.TestHelper.Timeout( () => handler == null );

        // No room
        if( gameClient.RoomId == "" )
        {
            Assert.AreEqual( gameClient.RoomId == "", true );
        }
        // Have room
        else
        {
            EventHandler<SocketEvent> handler2 = null;
            gameClient.SocketClient.OnSocketEvent += handler2 = ( sender, e ) =>
            {
                Assert.AreEqual( e.EventType, SocketEventType.Connected );
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler2 = null;
            };

            gameClient.LeaveRoom();

            yield return Tests.TestHelper.Timeout( () => handler2 == null );

            Assert.AreEqual( gameClient.RoomId == "", true );
        }
    }

    public static IEnumerator AssertCreateRoom( GameClient gameClient )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.CreateRoom();

        yield return Tests.TestHelper.Timeout( () => handler == null );

        Assert.AreEqual( gameClient.RoomId != "", true );
    }

    public static IEnumerator AssertDisconnect( GameClient gameClient )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        Assert.AreEqual( gameClient.IsConnected, true );

        gameClient.Disconnect();

        yield return Tests.TestHelper.Timeout( () => handler == null || ( !gameClient.IsConnecting && !gameClient.IsConnecting ) );

        Assert.AreEqual( gameClient.IsConnected, false );
        Assert.AreEqual( gameClient.IsConnecting, false );

        yield return null;
    }

    public static IEnumerator AssertHasRoom( GameClient gameClient )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.GetRoom();

        yield return Tests.TestHelper.Timeout( () => handler == null );

        Assert.AreEqual( gameClient.RoomId != "", true );
    }

    public static IEnumerator AssertLeaveRoom( GameClient gameClient )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.LeaveRoom();

        yield return Tests.TestHelper.Timeout( () => handler == null );

        Assert.AreEqual( gameClient.RoomId != "", false );
    }

    public static IEnumerator AssertJoinRoom( GameClient gameClient, string roomId )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.JoinRoom( roomId );

        yield return Tests.TestHelper.Timeout( () => handler == null );

        Assert.AreEqual( gameClient.RoomId == roomId, true );
    }

    public static IEnumerator AssertInvalidJoinRoom( GameClient gameClient, string roomId )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.JoinRoom( roomId );

        yield return Tests.TestHelper.Timeout( () => handler == null );

        Assert.AreEqual( gameClient.RoomId != roomId, true );
    }

    public static IEnumerator AssertChatInRoom( GameClient gameClient, string message )
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = ( sender, e ) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected );
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.ChatInRoom( message );

        yield return Tests.TestHelper.Timeout( () => handler == null );

        Assert.IsTrue( true );
    }
}