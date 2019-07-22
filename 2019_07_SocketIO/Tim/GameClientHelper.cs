using System;
using System.Collections;
using Tests;
using UnityEngine;
using UnityEngine.Assertions;

public class GameClientHelper
{
    static double TIME_OUT = 5;

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

        yield return TestHelper.Timeout(() => handler == null, TIME_OUT);

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

        yield return TestHelper.Timeout(() => gameClient.IsError, TIME_OUT);

        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }

    public static IEnumerator AssertDisconnect(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.Disconnected);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

        gameClient.leaveRoom();
        gameClient.Disconnect();

        yield return TestHelper.Timeout(() => handler == null, TIME_OUT);

        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }

    public static IEnumerator AssertLeaveIfHasRoom(GameClient gameClient)
    {
        gameClient.checkRoom();
        yield return TestHelper.Timeout(() => gameClient.RoomId != null, TIME_OUT);

        if (gameClient.RoomId != "")
            gameClient.leaveRoom();
    }

    public static IEnumerator AssertHasRoom(GameClient gameClient)
    {
        gameClient.checkRoom();
        yield return TestHelper.Timeout(() => gameClient.RoomId != null, TIME_OUT);
        Assert.AreNotEqual(gameClient.RoomId, "");
    }

    public static IEnumerator AssertLeaveRoom(GameClient gameClient)
    {
        gameClient.leaveRoom();
        yield return TestHelper.Timeout(() => gameClient.RoomId == "", TIME_OUT);
        Assert.AreEqual(gameClient.RoomId, "");
    }

    public static IEnumerator AssertCreateRoom(GameClient gameClient)
    {
        gameClient.createRoom();
        yield return TestHelper.Timeout(() => gameClient.RoomId != null && gameClient.RoomId != "", TIME_OUT);
        Assert.AreNotEqual(gameClient.RoomId, "");
    }

    public static IEnumerator AssertJoinRoom(GameClient gameClient, string roomId)
    {
        gameClient.joinRoom(roomId);
        yield return TestHelper.Timeout(() => gameClient.RoomId != null && gameClient.RoomId != "", TIME_OUT);
        Assert.AreNotEqual(gameClient.RoomId, "");
    }

    public static IEnumerator AssertInvalidJoinRoom(GameClient gameClient, string roomId)
    {
        gameClient.joinRoom(roomId);
        yield return TestHelper.Timeout(() => gameClient.RoomId != null , TIME_OUT);
        Assert.AreEqual(gameClient.RoomId, "");
    }

    public static IEnumerator AssertChatInRoom(GameClient gameClient, string msg)
    {
        gameClient.chatInRoom(msg);
        yield return null;
    }

}