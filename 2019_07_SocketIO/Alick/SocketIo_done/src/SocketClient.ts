import Debug from "debug";
import {EventEmitter} from "events";
import SocketIOClient from "socket.io-client";

const debug = Debug("app:SocketClient");

type Callback = () => void;
interface IRespond {
  isSuccess: boolean;
  roomId: string;
  userIds: string[];
  message: string;
}

class SocketClient extends EventEmitter {
  public isConnecting: boolean = false;
  public isConnected: boolean = false;

  // connection
  private _socket: SocketIOClient.Socket | null = null;
  private _socketOptions = {
    ["connect timeout"]: 5000,
    timeout: 5000,
    transports: ["websocket"],
  };

  // data
  private _uri: string;
  private _userId: string;
  private _roomId: string = "";
  private _chats: string[] = [];

  constructor(host: string, userId: string) {
    super();

    this._uri = `${host}?userId=${userId}`;
    this._userId = userId;

    debug(`SocketClient. userId: ${userId}, uri: ${this._uri}`);
  }

  // region public get/set

  public get hasRoom(): boolean {
    return !!(this._roomId);
  }

  public get roomId() {
    return this._roomId;
  }

  public get chats() {
    return this._chats;
  }

  public get userId() {
    return this._userId;
  }

  // endregion

  public connect(): Promise<void> {
    return new Promise<void>((resolve, err) => {
      if (!this.isConnected && !this.isConnecting) {
        debug("connect");

        this._socket = SocketIOClient(this._uri, this._socketOptions);
        this.isConnecting = true;

        // standard events
        this._socket.on("connect", this._onConnect);
        this._socket.on("connect_error", this._onError);
        this._socket.on("connect_timeout", this._onError);
        this._socket.on("broadcast", this._onBroadcast);
        this._socket.on("disconnect", this._onDisconnect);
        this._socket.on("error", this._onError);

        // room events
        this._socket.on("respondJoinRoom", this._onRespondJoinRoom);
        this._socket.on("respondCreateRoom", this._onRespondCreateRoom);
        this._socket.on("respondLeaveRoom", this._onRespondLeaveRoom);
        this._socket.on("respondGetRoom", this._onRespondGetRoom);
        this._socket.on("respondChatInRoom", this._onRespondChatInRoom);
        this._socket.on("respondMessageInRoom", this._onRespondMessageInRoom);

        this.once("connect", resolve);
        this.once("error", err);
        return;
      }

      resolve();
    });
  }

  public disconnect() {
    if (this.isConnecting || this.isConnected) {
      debug("disconnect");

      if (this._socket) {
        this._socket.removeAllListeners();
        this._socket.disconnect();
        this._socket = null;
      }

      this._roomId = "";
      this.isConnecting = false;
      this.isConnected = false;
      this.emit("disconnect");
    }
  }

  // region public methods

  public createRoom(): Promise<void> {
    return new Promise<void>(resolve => {
      if (this._socket) {
        if (!this.hasRoom) {
          debug("createRoom");
          this._socket.emit("createRoom");
          this._socket.once("respondCreateRoom", resolve);
          return;
        }
      }

      resolve();
    });
  }

  public getRoom() {
    return new Promise<void>(resolve => {
      if (this._socket) {
        debug("getRoom");
        this._socket.emit("getRoom");
        this._socket.once("respondGetRoom", resolve);
        return;
      }

      resolve();
    });
  }

  public leaveRoom() {
    return new Promise<void>(resolve => {
      if (this.hasRoom) {
        if (this._socket) {
          debug("leaveRoom");
          this._socket.emit("leaveRoom");
          this._socket.once("respondLeaveRoom", resolve);
          return;
        }
      }
      resolve();
    });
  }

  public joinRoom(roomId: string) {
    return new Promise<void>(resolve => {
      if (!this.hasRoom) {
        if (this._socket) {
          debug("joinRoom");
          this._socket.emit("joinRoom", roomId);
          this._socket.once("respondJoinRoom", resolve);
          return;
        }
      }
      resolve();
    });
  }

  public chatInRoom(message: string) {
    return new Promise<void>(resolve => {
      if (this.hasRoom) {
        if (this._socket) {
          debug("chatInRoom", message);
          this._socket.emit("chatInRoom", message);
          this._socket.once("respondChatInRoom", resolve);
          return;
        }
      }
      resolve();
    });
  }

  // endregion

  // region private methods

  // standard events

  private _onConnect = () => {
    debug("_onConnect");

    this.isConnecting = false;
    this.isConnected = true;
    this.emit("connect");
  }

  private _onBroadcast = (respond: IRespond) => {
    debug("received broadcast message", respond, this._userId);
  }

  private _onDisconnect = () => {
    debug("_onDisconnect");

    this.disconnect();
  }

  private _onError = (err: Error) => {
    debug("error: " + err);

    this.emit("error", err);
    this.disconnect();
  }

// room events

  private _onRespondCreateRoom = (respond: IRespond) => {
    debug("_onRespondCreateRoom", respond);

    if (respond.isSuccess) {
      this._roomId = respond.roomId;
    }
  }

  private _onRespondLeaveRoom = (respond: IRespond) => {
    debug("_onRespondLeaveRoom", respond);

    if (respond.isSuccess) {
      this._roomId = "";
    }
  }

  private _onRespondGetRoom = (respond: IRespond) => {
    debug("_onRespondGetRoom", respond);

    if (respond.isSuccess) {
      this._roomId = respond.roomId;
    } else {
      this._roomId = "";
    }
  }

  private _onRespondJoinRoom = (respond: IRespond) => {
    debug("_onRespondJoinRoom", respond);

    if (respond.isSuccess) {
      this._roomId = respond.roomId;
    }
  }

  private _onRespondChatInRoom = (respond: IRespond) => {
    debug("_onRespondChatInRoom", respond);

    if (respond.isSuccess) {
      // do nothing
    }
  }

  private _onRespondMessageInRoom = (respond: IRespond) => {
    debug("_onRespondMessageInRoom", respond);

    if (respond.isSuccess) {
      // do nothing
      this.emit("respondMessageInRoom", respond.message);
      this._chats.push(respond.message);
    }
  }

// endregion
}

export {SocketClient};