using System;
using BestHTTP.SocketIO;
using BestHTTP.SocketIO.JsonEncoders;
using BestHTTP.SocketIO.Transports;
using PlatformSupport.Collections.ObjectModel;

namespace Core
{
    public class SocketClient
    {
        private string _userId;
        private Uri _uri;
        private SocketOptions _options;
        private SocketManager _manager;
        private Socket _socket;

        public bool IsConnecting;
        public bool IsConnected;
        
        public event EventHandler<SocketEventArgs> OnSocketEvent;

        public SocketClient( string urlString , string userId )
        {
            _options = new SocketOptions();
            _options.AdditionalQueryParams = new ObservableDictionary<string , string>();
            _options.AdditionalQueryParams.Add( "userId" , userId );
            _options.ConnectWith = TransportTypes.WebSocket;
            
            _uri = new Uri( urlString );
        }

        #region =====[ Private ] =====
        
        private void OnEvent( Socket socket , Packet packet , object[] args )
        {
            // Debug.LogWarning( "receive payload: " + packet.Payload);
            OnSocketEvent?.Invoke( this , new GameClientEventArgs( SocketEventType.Event  , packet.EventName , args ) );
        }

        private void OnError( Socket socket , Packet packet , object[] args )
        {
            IsConnected = false;
            IsConnecting = false;
            OnSocketEvent?.Invoke( this , new SocketEventArgs( SocketEventType.Error ) );
        }

        private void OnDisconnect( Socket socket , Packet packet , object[] args )
        {
            IsConnected = false;
            IsConnecting = false;
            OnSocketEvent?.Invoke( this , new SocketEventArgs( SocketEventType.Disconnected ) );
        }

        private void OnConnect( Socket socket , Packet packet , object[] args )
        {
            IsConnected = true;
            IsConnecting = false;
            OnSocketEvent?.Invoke( this , new SocketEventArgs( SocketEventType.Connected ) );
        }


        #endregion

        #region ===== [ Public ] =====

        public void Emit( string eventName , object arguments )
        {
            _socket.Emit( eventName , arguments );
        }

        public void Connect()
        {
            if ( IsConnected || IsConnecting )
                return;
            
            _manager = new SocketManager( _uri , _options );
            _manager.Encoder = new LitJsonEncoder();
            _socket = _manager.Socket;
            _socket.On( "connect" , OnConnect );
            _socket.On( "disconnect" , OnDisconnect );
            _socket.On( "error" , OnError );
            _socket.On( "event" , OnEvent );

            IsConnecting = true;
        }

        public void Disconnect()
        {
            _manager?.Close();
            _manager = null;

            IsConnecting = false;
            IsConnected = false;
        }        

        #endregion

    }
}