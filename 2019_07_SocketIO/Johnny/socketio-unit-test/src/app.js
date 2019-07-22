var app = require('http').createServer(handler)
var io = require('socket.io')(app);
var fs = require('fs');
var users = {};
var userRoom = {};
var roomId = 1;
var rooms = {};

app.listen(3000);

function handler (req, res) {
    fs.readFile(__dirname + '/index.html',
        function (err, data) {
            if (err) {
                res.writeHead(500);
                return res.end('Error loading index.html');
            }

            res.writeHead(200);
            res.end(data);
        });
}

io.on('connection', function (socket) {
    let userId = socket.handshake.query.userId;
    console.log(userId);

   if(users[userId]){
       users[userId].disconnect();
   }
   users[userId] = socket;

    socket.on('getRoom', function () {
        console.log('getting room');
        console.log(rooms);
        var success = true;
        if(!userRoom[userId]) {
            success = false;
        }else{
            console.log(' > ' + userId + ': ' + userRoom[userId]);
        }
        socket.emit('respondGetRoom', {roomId: userRoom[userId] ? userRoom[userId] : "", isSuccess: success});
    });

    socket.on('leaveRoom', function () {
        console.log('leaving room');
        var success = false;
        if(userRoom[userId]){
            socket.leave(userRoom[userId]);
            rooms[userRoom[userId]].limit--;
            userRoom[userId] = '';
            success = true;
        }
        socket.emit('respondLeaveRoom', {isSuccess: success});
    });

    socket.on('createRoom', function () {
        rooms[roomId] = {'limit': 1};
        userRoom[userId] = roomId;
        socket.join(userRoom[userId]);
        roomId++;
        console.log(' creating room > ' + userId + ': ' + userRoom[userId]);
        socket.emit('respondCreateRoom', {roomId: userRoom[userId], isSuccess: true});

    });

    socket.on('joinRoom', function(id){
        console.log("joining room id: " + id);
        var success = false;
        if(!userRoom[userId] && rooms[id].limit < 2){
            socket.join(id);
            userRoom[userId] = id;
            rooms[id].limit++;
            success = true;
        }else{
            console.log("invalid room");
        }
        socket.emit('respondJoinRoom', {roomId: success ? id : "", isSuccess: success});
    });

    socket.on('chatInRoom', function(msg){
        var success = false;
        if(userRoom[userId]){
            success = true;
            socket.emit('respondChatInRoom', {message: msg, isSuccess: success});
            io.in(userRoom[userId]).emit('respondMessageInRoom', {message: msg, isSuccess: success});
        }
    });

    socket.on('error', function (e) {
        console.log(e);
    });

});
