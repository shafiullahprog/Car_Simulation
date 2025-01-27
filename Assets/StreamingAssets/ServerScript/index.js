const { debug } = require('console');
const server = require('http').createServer();
const io = require('socket.io')(server);
 
let connectedClients = [];
let clientsDisplayName = [];
let serversocketID = null;

io.on('connection', socket =>   //Invoke when any device connects
{
  socket.on('displayname',(data)=>{
    clientsDisplayName.push(data);
    io.emit('displayname', clientsDisplayName);  //Send display msg to all the connected device
  });

  connectedClients.push(socket.id);  //list updated
  socket.emit('connection', socket.id);  //send msg to the current device(only 1)
  io.emit('connectedClients', connectedClients);  //send list msg to all the connected clients 
  
  console.log(connectedClients);
  if(serversocketID === null)
  {
    //Assign as server when 1st device connect
    socket.on('firstServer', () =>{
      serversocketID = socket.id;
      socket.emit('socketType', 'server');
      socket.emit('firstServer', socket.id);
      console.log('assign as server', socket.id);
    });
  }
  else
  {
    //Assign as client when 2nd or more than that device connect
    socket.emit('socketType', 'client');
    console.log('assign as client', socket.id);
  }

  //"removename" has been received from a device When a device gets disconnected then it will update the list
  socket.on('removename', (data)=>{
    console.log('disconnect data: ', data);
    clientsDisplayName = clientsDisplayName.filter(id => id !== data)
    console.log(clientsDisplayName);
    io.emit('displayname', clientsDisplayName);  //Sends a list to all the connected deivces
  });

  //on receving "disconnect" from a device it will disconnect the user and update the list
  socket.on('disconnect', () => {
    console.log('Disconnected');
      connectedClients = connectedClients.filter(id => id !== socket.id);
      io.emit('connectedClients', connectedClients);

      //If a server disconnects, it will send msg to all the connected clients to get disconnected
      if(serversocketID === socket.id)
      {
        if(connectedClients.length>0)
        {
          console.log('server disconnect');
          io.emit('disconnectAllClients', 'Disconnect clients');
        } 
        else
        {
          serversocketID = null;
        }
      }
  });

  //on receiving "SendMessage" from a device it sends a message from server to client
  socket.on('SendMessage', (data) => {
    console.log('SendMessage');
    io.emit('SendMessage', data);
  });

  //on receiving "ClientToClient" from a device it sends a message from client to client
  socket.on('ClientToClient', (data) => {
    console.log('ClientToClient');
    io.emit('ClientToClient', data);
  });

  //on receiving "restAreClients" when an old server re-enter into the socket it becomes as server and every other device reassign as client
  socket.on('restAreClients', ()=>{
    if(connectedClients.length>1)
    {
      socket.emit('socketType', 'server');
      console.log('assign old server as server', socket.id);

      io.emit('connectedClients', connectedClients);
      io.emit('displayname', clientsDisplayName);
      serversocketID = socket.id;
    }
  });

  //control media of a specific device
  socket.on('playpausevideo', (data)=>{
    console.log('pause all video', data);
    io.emit('playpausevideo', data);
  });
});

server.listen(3000, () => {
  console.log('listening on *:' + 3000); //3000 is a port used in unity socket as well where sever and client use.
});
