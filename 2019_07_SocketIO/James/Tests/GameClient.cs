using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.Transports;
using System;
using PlatformSupport.Collections.ObjectModel;
using BestHTTP.SocketIO.JsonEncoders;
using System.Linq;

public class GameClient : System.Object
{
	public string RoomId;
	public List<string> Chats = new List<string>();
	public bool IsConnected;
	public bool IsConnecting;

	public string UserId;
	public SocketClient SocketClient = new SocketClient();
	
	public event MessageDelegate OnEventMessage;

	public delegate void MessageDelegate( object sender, EventMessage message );

	private SocketOptions _options = new SocketOptions();
	private Socket _socket;
	private string Host;

	public GameClient( string host, string userId )
	{
		Host = host;
		UserId = userId;
	}

	public void Connect()
	{
		IsConnecting = true;
		_options.AdditionalQueryParams = new ObservableDictionary<string, string>();
		_options.AdditionalQueryParams.Add("userId", UserId);
		_options.ConnectWith = TransportTypes.WebSocket;
		Uri _uri = new Uri("https://blooming-fjord-24277.herokuapp.com/socket.io/");
		SocketManager _manager = new SocketManager(_uri, _options);
		_manager.Encoder = new LitJsonEncoder();
		_socket = _manager.Socket;
		_socket.On("connect", OnConnect);
		_socket.On("disconnect", OnDisconnect);
		_socket.On("error", OnError);
		_socket.On("event", OnEvent);

		_socket.On("respondGetRoom", RespondGetRoom);
		_socket.On("respondLeaveRoom", RespondLeaveRoom);
		_socket.On("respondCreateRoom", RespondCreateRoom);
		_socket.On("respondJoinRoom", RespondJoinRoom);
		_socket.On("respondChatInRoom", RespondChatInRoom);
		_socket.On("respondMessageinRoom", RespondMessageinRoom);
		Debug.Log("endConnect");
		//_socket.On("disconnect", OnDisconnect);
		/*
		Debug.Log("afterConnect");
		
		_socket.On("event", OnEvent);

		//Debug.Log("getRoom");
		_socket.Emit("getRoom"); // server: respondGetRoom

		


		//Debug.Log("leaveRoom");
		//_socket.Emit("leaveRoom"); // server: respondLeaveRoom
		//Debug.Log("createRoom");
		//_socket.Emit("createRoom"); // server: respondCreateRoom
		//Debug.Log("joinRoom");
		//_socket.Emit("joinRoom", "roomId"); // server: respondJoinRoom
		//Debug.Log("chatInRoom");
		_socket.Emit("chatInRoom", "message123"); // server: respondChatInRoom
		_socket.On("disconnect", OnDisconnect);
		*/
	}
	
	public void RespondMessageinRoom(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log( "RespondMessageinRoom " + UserId   + _PrintMessage( args ) );
		if( OnEventMessage != null ){
			OnEventMessage( null, new EventMessage( "respondMessageInRoom" ) );
		}
		Debug.Log( (string)((Dictionary<string,object>)args[ 0 ])[ "message" ] );
		Chats.Add( (string)((Dictionary<string,object>)args[ 0 ])[ "message" ] );
	}

	public void CharInRoom( string message )
	{
		_socket.Emit("chatInRoom", message );
	}

	public void CreateRoom()
	{
		_socket.Emit("createRoom");
	}

	public void JoinRoom( string roomId )
	{
		_socket.Emit("joinRoom", roomId);
	}

	public void LeaveRoom()
	{
		_socket.Emit("leaveRoom");
	}

	public void GetRoom()
	{
		_socket.Emit("getRoom");
	}

	public void Disconnect()
	{
		if( _socket == null ){
			return;
		}
		_socket.Disconnect();
		Debug.Log("EndDisconnect");
	}

	void OnConnect(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("OnConnect " + UserId   + _PrintMessage( args ));
		IsConnecting = false;
		IsConnected = true;
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.Connected ) );
	}

	void OnDisconnect(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("OnDisconnect " + UserId  + _PrintMessage( args ));
		//IsDisconnecting = false;
		IsConnected = false;
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.Disconnected ) );
	}

	void OnError(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("OnError " + UserId  +" " +_PrintMessage( args ) );
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.Error ) );
		IsConnected = false;
		IsConnecting = false;
		Disconnect();
	}

	void OnEvent(Socket socket, Packet packet, params object[] args)
	{

		Dictionary<string,object> dict = ((Dictionary<string,object>)args[ 0 ]);
		Debug.Log("OnEvent " + UserId + _PrintMessage( args ) );
		if( dict.ContainsKey( "isSuccess" ) && (bool)dict[ "isSuccess" ] && dict.ContainsKey( "message" ) )
		{
			Debug.Log( "RespondMessageinRoom " + UserId   + _PrintMessage( args ) );
			if( OnEventMessage != null ){
				OnEventMessage( null, new EventMessage( "respondMessageInRoom" ) );
			}
			Debug.Log( (string)((Dictionary<string,object>)args[ 0 ])[ "message" ] );
			Chats.Add( (string)((Dictionary<string,object>)args[ 0 ])[ "message" ] );
		}
	}

	void RespondGetRoom(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("RespondGetRoom " + UserId + _PrintMessage( args ) );
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.GetRoom, (bool)((Dictionary<string,object>)args[ 0 ])[ "isSuccess" ] ) );
	}

	void RespondLeaveRoom(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("RespondLeaveRoom " + UserId + _PrintMessage( args ) );
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.LeaveRoom, (bool)((Dictionary<string,object>)args[ 0 ])[ "isSuccess" ] ) );
	}

	void RespondCreateRoom(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("RespondCreateRoom " + UserId + _PrintMessage( args ) );
		RoomId = (string)((Dictionary<string,object>)args[ 0 ])[ "roomId" ];
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.CreateRoom, (bool)((Dictionary<string,object>)args[ 0 ])[ "isSuccess" ] ) );
	}

	void RespondJoinRoom(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("RespondJoinRoom " + UserId + _PrintMessage( args ) );
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.JoinRoom, (bool)((Dictionary<string,object>)args[ 0 ])[ "isSuccess" ] ) );
	}

	void RespondChatInRoom(Socket socket, Packet packet, params object[] args)
	{
		Debug.Log("RespondChatInRoom " + UserId + _PrintMessage( args ) );
		SocketClient.Invoke( this, new SocketEvent( SocketEventType.ChatInRoom, (bool)((Dictionary<string,object>)args[ 0 ])[ "isSuccess" ] ) );
	}

	

	string _PrintMessage( params object[] args )
	{
		if( args == null || args.Length == 0 ) return "";
		string str = " " +args.Length + " ";
		Dictionary<string,object> dict = args[ 0 ] as Dictionary<string,object>;

		if( dict == null ){
			return args[ 0 ].ToString();
		}

		string[] keys = dict.Keys.ToArray();
		object[] values = dict.Values.ToArray();
		for( int i = 0; i < values.Length; i++ )
		{
			str += keys[ i ] +" " + values[ i ] + " :: ";
		}
		return str;
	}
}

public enum SocketEventType
{
	Connected,
	Disconnected,
	Error,
	CreateRoom,
	GetRoom,
	LeaveRoom,
	JoinRoom,
	ChatInRoom,
}

public class SocketClient
{
	public event EventHandler<SocketEvent> OnSocketEvent;

	public void Invoke( object sender, SocketEvent socketEvent )
	{
		if( OnSocketEvent != null ){
			OnSocketEvent( sender, socketEvent );
		}
	}
}

public class SocketEvent
{
	public SocketEventType EventType;
	public bool IsSuccess;
	public string Message;

	public SocketEvent( SocketEventType eventType, bool isSuccess = true, string message = null )
	{
		EventType = eventType;
		IsSuccess = isSuccess;
		Message = message;
	}
}

public class EventMessage
{
	public string EventName;

	public EventMessage( string eventName ){
		EventName = eventName;
	}
}