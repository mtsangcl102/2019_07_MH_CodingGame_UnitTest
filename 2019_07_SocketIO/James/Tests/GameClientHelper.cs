using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;
using Tests;

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

        yield return TestHelper.Timeout(() => handler == null, 5f );

        Assert.AreEqual(gameClient.IsConnected, true);
        Assert.AreEqual(gameClient.IsConnecting, false);		
    }


	public static IEnumerator AssertInvalidConnect( GameClient gameClient )
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

        yield return TestHelper.Timeout(() => handler == null, 5f );

        Assert.AreEqual(gameClient.IsConnecting, false);
		Assert.AreEqual(gameClient.IsConnected, false);
	}

	public static IEnumerator AssertDisconnect( GameClient gameClient )
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
        gameClient.Disconnect();

       // Assert.AreEqual(gameClient.IsDisconnecting, true);

        yield return TestHelper.Timeout(() => handler == null, 5f );

        Assert.AreEqual(gameClient.IsConnected, false);
     //   Assert.AreEqual(gameClient.IsDisconnecting, false);
	}

	public static IEnumerator AssertLeaveIfHasRoom( GameClient gameClient )
	{
		EventHandler<SocketEvent> handler = null;		
		bool hasRoom = false;
        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
			if( e.EventType == SocketEventType.GetRoom && e.IsSuccess ){
				hasRoom = true;
			}
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

		gameClient.GetRoom();

		yield return TestHelper.Timeout(() => handler == null, 5f );	

		if( hasRoom ){
			yield return AssertLeaveRoom( gameClient );
		}
	}

	public static IEnumerator AssertCreateRoom( GameClient gameClient )
	{
		EventHandler<SocketEvent> handler = null;		

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.CreateRoom);
			Assert.AreEqual(e.IsSuccess, true);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

		gameClient.CreateRoom();

		yield return TestHelper.Timeout(() => handler == null, 5f );
	}

	public static IEnumerator AssertHasRoom( GameClient gameClient )
	{
		EventHandler<SocketEvent> handler = null;		

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.GetRoom);
			Assert.AreEqual(e.IsSuccess, true);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

		gameClient.GetRoom();

		yield return TestHelper.Timeout(() => handler == null, 5f );		
	}

	public static IEnumerator AssertLeaveRoom( GameClient gameClient )
	{
		EventHandler<SocketEvent> handler = null;		

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.LeaveRoom);
			Assert.AreEqual(e.IsSuccess, true);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

		gameClient.LeaveRoom();

		yield return TestHelper.Timeout(() => handler == null, 5f );
	}

	public static IEnumerator AssertJoinRoom( GameClient gameClient,string roomId )
	{
		EventHandler<SocketEvent> handler = null;		

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.JoinRoom);
			Assert.AreEqual(e.IsSuccess, true);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

		gameClient.JoinRoom( roomId );

		yield return TestHelper.Timeout(() => handler == null, 5f );
	}

	public static IEnumerator AssertInvalidJoinRoom( GameClient gameClient, string roomId )
	{
		EventHandler<SocketEvent> handler = null;		

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.JoinRoom);
			Assert.AreEqual(e.IsSuccess, false);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
        };

		gameClient.JoinRoom( roomId );

		yield return TestHelper.Timeout(() => handler == null, 5f );
	}	

	public static IEnumerator AssertChatInRoom( GameClient gameClient, string message )
	{
		EventHandler<SocketEvent> handler = null;		

        gameClient.SocketClient.OnSocketEvent += handler = (sender, e) =>
        {
            Assert.AreEqual(e.EventType, SocketEventType.ChatInRoom);
			Assert.AreEqual(e.IsSuccess, true);
			//Assert.AreEqual(e.Message, message);
            gameClient.SocketClient.OnSocketEvent -= handler;
            handler = null;
			Debug.LogWarning("c1");
        };

		gameClient.CharInRoom( message );

		yield return TestHelper.Timeout(() => handler == null, 5f );
		Debug.LogWarning("c2");
	}		
}