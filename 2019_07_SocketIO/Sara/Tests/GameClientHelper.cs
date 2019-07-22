using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Tests;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using BestHTTP.SocketIO.Events;
using LitJson;
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

        yield return TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);
        yield return null;
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
        yield return TestHelper.Timeout(() => handler == null);
    }
    
    public static IEnumerator AssertLeaveIfHasRoom(GameClient gameClient)
    {
        EventHandler<Packet> handler = null;

        Assert.AreEqual(gameClient.IsConnected, true);

        gameClient.OnEventMessage +=  (sender, e) =>
        {
            if (e.EventName == "respondLeaveRoom")
            {
                handler = null;                
            }
        };
        gameClient.Socket.Emit("leaveRoom");
        yield return TestHelper.Timeout(() => handler == null);

    }    
    
    public static IEnumerator AssertDisconnect(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.Disconnected);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };
        gameClient.Disconnect();
        
        yield return TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.SocketClient.SocketState, SocketEventType.Disconnected);
        yield return null;
    }

    public static IEnumerator AssertCreateRoom(GameClient gameClient)
    {
        EventHandler<Packet> handler = null;

        gameClient.OnEventMessage +=  (sender, e) =>
        {
            if (e.EventName == "respondCreateRoom")
            {
                handler = null;
                JsonData data = JsonMapper.ToObject(e.Payload);
                Assert.IsNotNull(data[1]["roomId"]);
                gameClient.RoomId = data[1]["roomId"].ToString();
            }
        };
        gameClient.Socket.Emit("createRoom");
        yield return TestHelper.Timeout(() => handler == null);
    }

    public static IEnumerator AssertHasRoom(GameClient gameClient)
    {
        EventHandler<Packet> handler = null;

        gameClient.OnEventMessage +=  (sender, e) =>
        {
            if (e.EventName == "respondGetRoom")
            {
                JsonData data = JsonMapper.ToObject(e.Payload);
                Assert.IsNotNull(data[1]["roomId"]);
                handler = null;
            }
        };
        gameClient.Socket.Emit("getRoom");
        yield return TestHelper.Timeout(() => handler == null);

    }

    public static IEnumerator AssertLeaveRoom(GameClient gameClient)
    {
        EventHandler<Packet> handler = null;

        Assert.AreEqual(gameClient.IsConnected, true);

        gameClient.OnEventMessage +=  (sender, e) =>
        {
            if (e.EventName == "respondLeaveRoom")
            {
                handler = null;                
            }
        };
        gameClient.Socket.Emit("leaveRoom");
        yield return TestHelper.Timeout(() => handler == null);

    }

    public static IEnumerator AssertJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<Packet> handler = null;

        gameClient.OnEventMessage +=  (sender, e) =>
        {
            if (e.EventName == "respondJoinRoom")
            {
                JsonData data = JsonMapper.ToObject(e.Payload);
                Debug.Log("success: " + data[1]["isSuccess"]);
                Assert.IsTrue((bool)data[1]["isSuccess"]);
                gameClient.RoomId = data[1]["roomId"].ToString();
                handler = null;
            }
        };
        gameClient.Socket.Emit("joinRoom", roomId);
        yield return TestHelper.Timeout(() => handler == null);

    }

    public static IEnumerator AssertInvalidJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<Packet> handler = null;

        gameClient.OnEventMessage +=  (sender, e) =>
        {
            if (e.EventName == "respondJoinRoom")
            {
                JsonData data = JsonMapper.ToObject(e.Payload);
                Debug.Log("success: " + data[1]["isSuccess"]);
                Assert.IsFalse((bool)data[1]["isSuccess"]);

                handler = null;
            }
        };
        gameClient.Socket.Emit("joinRoom", roomId);
        yield return TestHelper.Timeout(() => handler == null);

    }

    public static IEnumerator AssertChatInRoom(GameClient gameClient, string message)
    {
        EventHandler<Packet> handler = null;

        gameClient.OnEventMessage +=  (sender, e) =>
        {
            if (e.EventName == "respondChatInRoom")
            {
                JsonData data = JsonMapper.ToObject(e.Payload);
                Debug.Log("success: " + data[1]["isSuccess"]);
                Assert.IsTrue((bool)data[1]["isSuccess"]);
                
                handler = null;
                
            }
        };
        gameClient.Socket.Emit("chatInRoom", message);
        yield return TestHelper.Timeout(() => handler == null);
    }
}
