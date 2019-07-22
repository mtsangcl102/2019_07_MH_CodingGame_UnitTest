using System;
using System.Collections;
using BestHTTP.SocketIO.Events;
using NUnit.Framework;
using Tests;

public class GameClientHelper
{

    public static IEnumerator AssertConnect(GameClient gameClient)
    {
        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                                {
                                                    Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                    gameClient.OnSocketEvent -= handler;
                                                    handler = null;
                                                };
        gameClient.Connect();

        Assert.AreEqual(gameClient.IsConnecting, true);

        yield return TestHelper.Timeout(() => handler == null);

        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);
    }

    public static IEnumerator AssertInvalidConnect(GameClient gameClient)
    {
        yield return null;
    }

    public static IEnumerator AssertLeaveIfHasRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                              {
                                                  Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                  gameClient.OnSocketEvent -= handler;
                                                  handler = null;
                                              };
        gameClient.GetRoom();
        yield return TestHelper.Timeout(() => handler == null);
        
        if (gameClient.RoomId == "")
        {
            Assert.IsTrue(true);
        }
        else
        {
            gameClient.LeaveRoom();
            gameClient.OnSocketEvent += handler = (sender, e) =>
                                                  {
                                                      Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                      gameClient.OnSocketEvent -= handler;
                                                      handler = null;
                                                  };
            yield return TestHelper.Timeout(() => handler == null);
            Assert.IsTrue(gameClient.RoomId == "");
        }
    }

    public static IEnumerator AssertCreateRoom(GameClient gameClient)
    {
        // TODO
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                              {
                                                  Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                  gameClient.OnSocketEvent -= handler;
                                                  handler = null;
                                              };
        gameClient.CreateRoom();
        yield return TestHelper.Timeout(() => handler == null);
        Assert.IsTrue(gameClient.RoomId != "");
    }

    public static IEnumerator AssertHasRoom(GameClient gameClient)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                              {
                                                  Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                  gameClient.OnSocketEvent -= handler;
                                                  handler = null;
                                              };
        gameClient.GetRoom();
        yield return TestHelper.Timeout(() => handler == null);
        Assert.IsTrue(gameClient.RoomId != "");
    }

    public static IEnumerator AssertLeaveRoom(GameClient gameClient)
    {
        // TODO
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                              {
                                                  Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                  gameClient.OnSocketEvent -= handler;
                                                  handler = null;
                                              };         
        gameClient.LeaveRoom();                                
        yield return TestHelper.Timeout(() => handler == null); 
        Assert.IsTrue(gameClient.RoomId == "");                 
    }

    public static IEnumerator AssertDisconnect(GameClient gameClient)
    {
        // TODO
        gameClient.Disconnect();
        Assert.AreEqual(gameClient.IsConnected, false);
        Assert.AreEqual(gameClient.IsConnecting, false);
        yield return null;
    }
    public static IEnumerator AssertJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                              {
                                                  Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                  gameClient.OnSocketEvent -= handler;
                                                  handler = null;
                                              };
        gameClient.JoinRoom(roomId);                                
        yield return TestHelper.Timeout(() => handler == null); 
        Assert.IsTrue(gameClient.RoomId == roomId);         
    }

    public static IEnumerator AssertInvalidJoinRoom(GameClient gameClient, string roomId)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                              {
                                                  Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                  gameClient.OnSocketEvent -= handler;
                                                  handler = null;
                                              };
        gameClient.JoinRoom(roomId);                                
        yield return TestHelper.Timeout(() => handler == null); 
        Assert.IsTrue(gameClient.RoomId != roomId);   
    }

    public static IEnumerator AssertChatInRoom(GameClient gameClient, string message)
    {
        EventHandler<SocketEvent> handler = null;
        gameClient.OnSocketEvent += handler = (sender, e) =>
                                              {
                                                  Assert.AreEqual(e.EventType, SocketEventType.Connected);
                                                  gameClient.OnSocketEvent -= handler;
                                                  handler = null;
                                              };
        gameClient.ChatInRoom(message);
        yield return TestHelper.Timeout(() => handler == null); 
    }
}