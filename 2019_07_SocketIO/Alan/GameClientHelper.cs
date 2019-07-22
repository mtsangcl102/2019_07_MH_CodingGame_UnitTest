using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using BestHTTP;
using BestHTTP.Statistics;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;

public class GameClientHelper
{
    //public static timeout = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));

    public static IEnumerator AssertInvalidConnect(GameClient gameClient)
    {
        try
        {
            gameClient.connect();
            Assert.IsTrue(false);
        }
        catch (Exception e)
        {
            Assert.IsTrue(true);
        }

        yield return null;
    }

    public static IEnumerator AssertConnect(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);

        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual( e.EventType, SocketEventType.Connected);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.connect();
        Assert.AreEqual(gameClient.IsConnecting, true);
        yield return Tests.TestHelper.Timeout(() => handler == null, 10);

        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }

    public static IEnumerator AssertDisconnect(GameClient gameClient)
    {
        Assert.IsTrue(gameClient.IsConnected);
        gameClient.Disconnect();
        yield return Tests.TestHelper.Timeout(() => !gameClient.IsConnected);
        Assert.IsFalse(gameClient.IsConnected);
        Assert.IsFalse(gameClient.hasRoom());
    }

    public static IEnumerator AssertHasRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.GetRoom);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.getRoom();
        yield return Tests.TestHelper.Timeout(() => handler == null);
        Assert.IsTrue(gameClient.hasRoom());
    }

    public static IEnumerator AssertCreateRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.CreateRoom);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.createRoom();
        yield return Tests.TestHelper.Timeout(() => handler == null);

        Assert.IsTrue(gameClient.hasRoom());
    }

    public static IEnumerator AssertJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.JoinRoom);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.joinRoom(roomId);
        yield return Tests.TestHelper.Timeout(() => handler == null);
        Assert.IsTrue(gameClient.hasRoom());
    }

    public static IEnumerator AssertInvalidJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.JoinRoom);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.joinRoom(roomId);
        yield return Tests.TestHelper.Timeout(() => handler == null);
        Assert.IsFalse(gameClient.hasRoom());
    }

    public static IEnumerator AssertLeaveRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.LeaveRoom);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.leaveRoom();

        yield return Tests.TestHelper.Timeout(() => handler == null);
        Assert.IsFalse(gameClient.hasRoom());
    }

    public static IEnumerator AssertLeaveIfHasRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;

        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.GetRoom);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.getRoom();
        yield return Tests.TestHelper.Timeout(() => handler == null);

        handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.LeaveRoom);
            gameClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.leaveRoom();
        yield return Tests.TestHelper.Timeout(() => handler == null);

        Assert.IsFalse(gameClient.hasRoom());
    }

    public static IEnumerator AssertChatInRoom(GameClient gameClient, string message)
    {
        Assert.IsTrue(gameClient.hasRoom());
        gameClient.chatInRoom(message);
        yield return null;
    }
}
