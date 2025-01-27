const { debug } = require('console');
const server = require('http').createServer();
const io = require('socket.io')(server);

let connectedClients = [];
let serversocketID = null;
io.on('connection', socket => 
{
  connectedClients.push(socket.id);
  
  socket.emit('connection', socket.id);
  io.emit('connectedClients', connectedClients);
  
  console.log(connectedClients);
  if(serversocketID === null)
  {
    socket.on('firstServer', () =>{
      serversocketID = socket.id;
      socket.emit('socketType', 'server');
      socket.emit('firstServer', socket.id);
      console.log('assign as server', socket.id);
    });
  }
  else
  {
    socket.emit('socketType', 'client');
    console.log('assign as client', socket.id);
  }

  socket.on('disconnect', () => {
    console.log('Disconnected');
      connectedClients = connectedClients.filter(id => id !== socket.id);
      console.log(connectedClients);
      io.emit('connectedClients', connectedClients);
      if(serversocketID === socket.id)
      {
        if(connectedClients.length>0)
        {
          //serversocketID = connectedClients[0];
          //console.log('Reassigning', serversocketID, 'as server');
          //io.to(serversocketID).emit('socketType', 'server');
          console.log('server disconnect');
          io.emit('disconnectAllClients', 'Disconnect clients');
        } 
        else
        {
          serversocketID = null;
        }
      }
  });

  socket.on('SendMessage', (data) => {
    console.log('SendMessage');
    io.emit('SendMessage', data);
  });
  socket.on('ClientToClient', (data) => {
    console.log('ClientToClient');
    io.emit('ClientToClient', data);
  });
  socket.on('restAreClients', ()=>{
    if(connectedClients.length>1)
    {
      //connectedClients.push(socket.id);
      socket.emit('socketType', 'server');
      console.log('assign old server as server', socket.id);

      //io.to(serversocketID).emit('socketType', 'client');
      //console.log('assign new server as client', serversocketID);

      io.emit('connectedClients', connectedClients);
      serversocketID = socket.id;
    }
  });

  socket.on('playpausevideo', (data)=>{
    console.log('pause all video', data);
    io.emit('playpausevideo', data);
  });
});

server.listen(3000, () => {
  console.log('listening on *:' + 3000);
});
