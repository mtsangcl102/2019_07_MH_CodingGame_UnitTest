using System;
using System.Collections;
using NUnit.Framework;
using BestHTTP.SocketIO;
using UnityEngine;

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

        yield return Tests.TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);
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

        yield return Tests.TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }

    public static IEnumerator AssertLeaveIfHasRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            if (e.EventType == SocketEventType.LeaveRoom)
            {
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            }
        };
        gameClient.LeaveIfHasRoom();

        yield return Tests.TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.RoomId, "");
    }
    
    public static IEnumerator AssertCreateRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            if (e.EventType == SocketEventType.CreateRoom)
            {
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            }
        };
        gameClient.CreateRoom();

        yield return Tests.TestHelper.Timeout(() => handler == null);

        Assert.AreNotEqual(gameClient.RoomId, "");
    }
    
    public static IEnumerator AssertDisconnect(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual(gameClient.IsConnected, true);
        
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.Disconnected);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.Disconnect();

        yield return Tests.TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }
    
    public static IEnumerator AssertHasRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            if (e.EventType == SocketEventType.GetRoom)
            {
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            }
        };
        gameClient.GetRoom();

        yield return Tests.TestHelper.Timeout(() => handler == null);
        
        Assert.AreNotEqual(gameClient.RoomId, "");
    }
    
    public static IEnumerator AssertLeaveRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            if (e.EventType == SocketEventType.LeaveRoom)
            {
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            }
        };
        gameClient.LeaveRoom();

        yield return Tests.TestHelper.Timeout(() => handler == null);
        
        Assert.AreEqual(gameClient.RoomId, "");
    }
    
    public static IEnumerator AssertJoinRoom(GameClient gameClient, string roomID)
    {
        EventHandler<SocketEvent> handler = null;
        
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            if (e.EventType == SocketEventType.JoinRoom)
            {
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            }
        };
        gameClient.JoinRoom(roomID);

        yield return Tests.TestHelper.Timeout(() => handler == null);
        Assert.AreEqual(gameClient.RoomId, roomID);
    }
    
    public static IEnumerator AssertInvalidJoinRoom(GameClient gameClient, string roomID)
    {
        EventHandler<SocketEvent> handler = null;
        
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            if (e.EventType == SocketEventType.JoinRoom)
            {
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
            }
        };
        gameClient.JoinRoom(roomID);

        yield return Tests.TestHelper.Timeout(() => handler == null);
        Assert.AreEqual(gameClient.RoomId, "");
    }
    
    public static IEnumerator AssertChatInRoom(GameClient gameClient, string newMsg)
    {
        gameClient.SendChat(newMsg);
        
        yield return null;
    }
}