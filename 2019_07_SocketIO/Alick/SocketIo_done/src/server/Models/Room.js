class Room {
  constructor(roomId = null) {
    this.roomId = roomId;
    this.memberList = [];
  }

  newRoomAndJoin(socket) {
    this.roomId = Room.getNewId(socket.user.userId);
    this.joinRoom(socket);
  }

  joinRoom(socket) {
    if (this.memberList.length < 2) {
      this.memberList.push(socket.user.userId);
      socket.join(this.roomId);
      socket.user.roomId = this.roomId;
    }
  }

  leaveRoom(socket) {
    if (socket.user.roomId) {
      this.memberList = this.memberList.filter(x => x !== socket.user.userId)
      socket.leave(socket.user.roomId);
      socket.user.roomId = 0;
    }
  }

  static getNewId(userId) {
    return userId + ":" + Date.now();
  }
}

module.exports = Room;