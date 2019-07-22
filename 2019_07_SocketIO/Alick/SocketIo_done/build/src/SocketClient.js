"use strict";
var __importDefault = (this && this.__importDefault) || function (mod) {
    return (mod && mod.__esModule) ? mod : { "default": mod };
};
Object.defineProperty(exports, "__esModule", { value: true });
const debug_1 = __importDefault(require("debug"));
const events_1 = require("events");
const socket_io_client_1 = __importDefault(require("socket.io-client"));
const debug = debug_1.default("app:SocketClient");
class SocketClient extends events_1.EventEmitter {
    constructor(host, userId) {
        super();
        this.isConnecting = false;
        this.isConnected = false;
        // connection
        this._socket = null;
        this._socketOptions = {
            ["connect timeout"]: 5000,
            timeout: 5000,
            transports: ["websocket"],
        };
        this._roomId = "";
        this._chats = [];
        // endregion
        // region private methods
        // standard events
        this._onConnect = () => {
            debug("_onConnect");
            this.isConnecting = false;
            this.isConnected = true;
            this.emit("connect");
        };
        this._onBroadcast = (respond) => {
            debug("received broadcast message", respond, this._userId);
        };
        this._onDisconnect = () => {
            debug("_onDisconnect");
            this.disconnect();
        };
        this._onError = (err) => {
            debug("error: " + err);
            this.emit("error", err);
            this.disconnect();
        };
        // room events
        this._onRespondCreateRoom = (respond) => {
            debug("_onRespondCreateRoom", respond);
            if (respond.isSuccess) {
                this._roomId = respond.roomId;
            }
        };
        this._onRespondLeaveRoom = (respond) => {
            debug("_onRespondLeaveRoom", respond);
            if (respond.isSuccess) {
                this._roomId = "";
            }
        };
        this._onRespondGetRoom = (respond) => {
            debug("_onRespondGetRoom", respond);
            if (respond.isSuccess) {
                this._roomId = respond.roomId;
            }
            else {
                this._roomId = "";
            }
        };
        this._onRespondJoinRoom = (respond) => {
            debug("_onRespondJoinRoom", respond);
            if (respond.isSuccess) {
                this._roomId = respond.roomId;
            }
        };
        this._onRespondChatInRoom = (respond) => {
            debug("_onRespondChatInRoom", respond);
            if (respond.isSuccess) {
                // do nothing
            }
        };
        this._onRespondMessageInRoom = (respond) => {
            debug("_onRespondMessageInRoom", respond);
            if (respond.isSuccess) {
                // do nothing
                this.emit("respondMessageInRoom", respond.message);
                this._chats.push(respond.message);
            }
        };
        this._uri = `${host}?userId=${userId}`;
        this._userId = userId;
        debug(`SocketClient. userId: ${userId}, uri: ${this._uri}`);
    }
    // region public get/set
    get hasRoom() {
        return !!(this._roomId);
    }
    get roomId() {
        return this._roomId;
    }
    get chats() {
        return this._chats;
    }
    get userId() {
        return this._userId;
    }
    // endregion
    connect() {
        return new Promise((resolve, err) => {
            if (!this.isConnected && !this.isConnecting) {
                debug("connect");
                this._socket = socket_io_client_1.default(this._uri, this._socketOptions);
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
    disconnect() {
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
    createRoom() {
        return new Promise(resolve => {
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
    getRoom() {
        return new Promise(resolve => {
            if (this._socket) {
                debug("getRoom");
                this._socket.emit("getRoom");
                this._socket.once("respondGetRoom", resolve);
                return;
            }
            resolve();
        });
    }
    leaveRoom() {
        return new Promise(resolve => {
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
    joinRoom(roomId) {
        return new Promise(resolve => {
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
    chatInRoom(message) {
        return new Promise(resolve => {
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
}
exports.SocketClient = SocketClient;
