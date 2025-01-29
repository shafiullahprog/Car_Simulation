StreamingAssets Configuration Guide

Base URL Configuration

Inside the StreamingAssets folder, there is a file named baseurl containing the following JSON structure:

json
{
    "ip": "http://127.0.0.1:3000",
    "displayname": "build pc"
}


# IP Configuration

- Modify the ip field to specify the server IP where you want the client to connect.
- This is essential for establishing communication between the server and the client.

 System Setup

The build runs on two systems:

1. Server: Runs both Unity and Node.js.
2. Client: Requires modification of the IP field in the `baseurl` file to connect to the server.

 Known Issues

- There were minor issues during the IP modification process.
- Sometimes, animations do not work properly after changing the IP.
