class User {
  constructor(socket, roomId = 0) {
    this.socket = socket;
    this.roomId = roomId;
    this.userId = +socket.handshake.query.userId;
  }

  responseSuccess(eventName, data = {}) {
    data.isSuccess = true;
    this.socket.emit(eventName, data)
  }

  broadcastToRoom(io, eventName, message) {
    let data = {
      isSuccess: true,
      message
    };
    io.in(this.roomId).emit(eventName, data);
  }
}

module.exports = User;