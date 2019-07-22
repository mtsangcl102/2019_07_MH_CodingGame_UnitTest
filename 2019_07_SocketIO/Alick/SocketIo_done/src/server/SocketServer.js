let debug = require('debug')('SocketServer');
let app = require('express')();
let http = require('http').createServer(app);
let io = require('socket.io')(http);

// Model
let User = require('./Models/User');
let Room = require('./Models/Room');

// Enum
let EventTypeServerToClient = require('./Enums/EventTypeServerToClient');

/* @type {User[]}  */
let userList = {};
/* @type {Room[]}  */
let roomList = {};

io.on('connection', function (socket) {
  let userId = +socket.handshake.query.userId;

  if (+socket.handshake.query.userId) {
    if (userList[userId] && !!userList[userId].userId) {
      userList[userId].socket.disconnect();
      userList[userId].socket = socket;
    } else {
      userList[userId] = new User(socket)
    }

    socket.user = userList[userId];

    if (socket.user.roomId) {
      socket.join(socket.user.roomId);
    }

    io.emit('connect', 'connection started');
  } else {
    socket.disconnect();
  }

  socket.on('getRoom', function (msg) {
    let data = {roomId: 0};
    if (socket.user.roomId) data = {roomId: socket.user.roomId};
    socket.user.responseSuccess(
      EventTypeServerToClient.RespondGetRoom,
      data
    )
  });

  socket.on('leaveRoom', function () {
    if (socket.user.roomId) {
      roomList[socket.user.roomId].leaveRoom(socket);
    }

    socket.user.responseSuccess(
      EventTypeServerToClient.RespondLeaveRoom,
      {roomId: socket.user.roomId}
    )
  });

  socket.on('createRoom', function (msg) {
    let room = new Room();
    room.newRoomAndJoin(socket);
    roomList[room.roomId] = room;
    socket.user.responseSuccess(
      EventTypeServerToClient.RespondCreateRoom,
      {roomId: room.roomId}
    )
  });

  socket.on('joinRoom', function (targetRoomId) {
    if (roomList[targetRoomId]) {
      roomList[targetRoomId].joinRoom(socket);
    }

    socket.user.responseSuccess(
      EventTypeServerToClient.RespondJoinRoom,
      {roomId: socket.user.roomId}
    )
  });

  socket.on('chatInRoom', function (message) {
    if (socket.user.roomId) {
      socket.user.responseSuccess(EventTypeServerToClient.RespondChatInRoom);
      socket.user.broadcastToRoom(
        io,
        EventTypeServerToClient.RespondMessageInRoom,
        message
      )
    }
  });
});

http.listen(8000, function () {
  console.log('listening on *:80');
});
