using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using NUnit.Framework;
using Tests;
using static Tests.SocketEvent;

public class GameClientHelper {


    public static IEnumerator AssertConnect(GameClient gameClient) {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            Assert.AreEqual(e.EventType, SocketNameType.Connected);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.Connect();

        Assert.AreEqual(gameClient.IsConnecting, true);
        yield return TestHelper.Timeout(() => handler == null, 10f);
        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }


    public static IEnumerator AssertDisconnect(GameClient gameClient) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            Debug.Log("AssertDisconnect:: " + e.EventType);
            Assert.AreEqual(e.EventType, SocketNameType.Disconnected);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        Assert.IsTrue(gameClient.IsConnected);
        gameClient.Disconnect();
        yield return TestHelper.Timeout(() => handler == null);
    }


    public static IEnumerator AssertHasRoom(GameClient gameClient) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            Assert.AreEqual(e.EventType, SocketNameType.GetRoom);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.GetRoom();
        yield return TestHelper.Timeout(() => handler == null);
        Assert.IsTrue(gameClient.HasRoom());
    }


    public static IEnumerator AssertCreateRoom(GameClient gameClient) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            Debug.Log("AssertCreateRoom:: " + e.EventType);
            switch(e.EventType) {
                case SocketNameType.CreateRoom:
                    gameClient.SocketClient.OnSocketEvent -= handler;
                    handler = null;
                break;
                default:
                break;
            }
        };
        gameClient.CreateRoom();
        yield return TestHelper.Timeout(() => handler == null);
    }


    public static IEnumerator AssertInvalidConnect(GameClient gameClient) {
        EventHandler<SocketEvent> handler = null;
        Assert.AreEqual(gameClient.IsConnected, false);
        try {
            gameClient.Connect();
            Assert.IsTrue(false);
        } catch {
            Assert.IsTrue(true);
        }
        yield return null;
    }


    public static IEnumerator AssertJoinRoom(GameClient gameClient, string roomId) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            switch(e.EventType) {
                case SocketNameType.JoinRoom:
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
                break;
                default:
                break;
            }
        };
        gameClient.JoinRoom(roomId);
        yield return TestHelper.Timeout(() => handler == null);
    }


    public static IEnumerator AssertInvalidJoinRoom(GameClient gameClient, string roomId) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            switch(e.EventType) {
                case SocketNameType.JoinRoom:
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
                break;
                default:
                break;
            }
        };
        gameClient.JoinRoom(roomId);
        yield return TestHelper.Timeout(() => handler == null);
        Assert.IsFalse(gameClient.HasRoom());
    }


    public static IEnumerator AssertLeaveRoom(GameClient gameClient) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        Assert.IsTrue(gameClient.HasRoom());
        gameClient.LeaveRoom();
        //Assert.IsFalse(gameClient.HasRoom());
        yield return TestHelper.Timeout(() => handler == null);

    }


    public static IEnumerator AssertLeaveIfHasRoom(GameClient gameClient) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            Debug.Log("AssertLeaveIfHasRoom:: " + e.EventType);
            switch(e.EventType) {
                case SocketNameType.GetRoom:
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
                break;
                case SocketNameType.LeaveRoom:
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
                break;
                default:
                break;
            }
        };
        gameClient.GetRoom();
        yield return TestHelper.Timeout(() => handler == null, 10f);
        //Assert.IsTrue(gameClient.HasRoom());

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            Debug.Log("AssertLeaveIfHasRoom2:: " + e.EventType);
            switch(e.EventType) {
                case SocketNameType.GetRoom:
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
                break;
                case SocketNameType.LeaveRoom:
                gameClient.SocketClient.OnSocketEvent -= handler;
                handler = null;
                break;
                default:
                break;
            }
        };
        gameClient.LeaveRoom();
        yield return TestHelper.Timeout(() => handler == null, 10f);
        Assert.IsFalse(gameClient.HasRoom());
    }


    public static IEnumerator AssertChatInRoom(GameClient gameClient, string newMessage) {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) => {
            Debug.Log("AssertChatInRoom:: " + e.EventType);
            switch(e.EventType) {
                case SocketNameType.MessageInRoom:
                gameClient.SocketClient.OnSocketEvent -= handler;

                handler = null;
                break;
                default:
                break;
            }
        };
        gameClient.ChatInRoom(newMessage);
        yield return TestHelper.Timeout(() => handler == null);

    }
}