# Named-Pipes
Inter-process communication library using named pipes.

Full details can be found here: http://www.codeproject.com/Articles/864679/Creating-a-Server-Using-Named-Pipes

In this repository I show how I created a server and client implementation using named pipes in C# .Net 4.
I used `NamedPipeServerStream` and `NamedPipeClientStream`, but soon realized that the name "server" was confusing. `NamedPipeServerStream` could only handle one client at a time (see Named Pipe Instances topic in MSDN), but I needed a server that could handle multiple clients requests.
I could not find any online example suitable to my needs, therefore, I created my own implementation of a server using `NamedPipeServerStream` and `NamedPipeClientStream`.

## Introduction to the code
PipeServer is in charge of creating and holding the named pipe streams, which are opened for each client. 
`InternalPipeServer` is a wrapper for `NamedPipeServerStream`. 
`PipeClient` is a wrapper for `NamedPipeClientStream`.

## Main flows
1. PipeServer is created and started
    * A new pipe name is generated.
    * A new instance of InternalPipeServer is created and begins waiting for client connections. 

2. PipeClient is created and started
    * A connection is made with InternalPipeServer. 
    * InternalPipeServer fires an event to let PipeServer know a connection was made.
        * PipeServer fires its own event, to let the world know that a client has connected. It then creates a new instance of InternalPipeServer and starts it so that it will begin waiting for new connections, while the first instance communicates with the first client.
    * InternalPipeServer begins an asynchronous read operation which completes when a client has sent a message, has been disconnected or when the pipe has been closed.

3. PipeClient sends a message 
    * InternalPipeServer receives part of the message since the message is longer than its buffer size, and initiates a new asynchronous read operation.
    * InternalPipeServer receives the rest of the message, appends it to the first parts, fires an event to let PipeServer know a new message has arrived, and initiates a new asynchronous read operation to wait for new messages.
        * PipeServer fires its own event to let the world know a new message has arrived from one of the clients. 
    
4. PipeClient disconnects
    * InternalPipeServer's read operation ends with no bytes read, so InternalPipeServer assumes the client has disconnected. It fires an event to let PipeServer know about it.
    * PipeServer fires its own event to let the world know a client has been disconnected.

5. PipeServer is stopped
    * PipeServer stops all its InternalPipeServer instances 

## Using the code
If you need to communicate with another process, use this code. 
Create a `PipeServer` in one process and a `PipeClient` in another. Then use the client in order to send messages to the server.
