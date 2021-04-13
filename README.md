# ChatApp
Chat application that can behave both as client and server implemented with WebSockets

# Architecture Overview

Solution has been implemented with a .Net Core Console Application consisting of two main classes: Client and Server

In order to handle application parameters, I have used an extension called McMaster.Extensions.CommandLineUtils. This allows for arguments and application description
with usage information. This can be triggered using "ChatApp -?". It also handles parameter parsing.

At low level there is a class than handles the configuration values: ChatAppConfig. This has only two properties: ServerIP and ExitMessage. This along with the console arguments compose the whole data layer the application needs to work.

.Net Core dependency injection framework is being used. Main logic due to McMaster's extension is now on method OnExecute. Here I declare the services and instantiate the needed classes.

Also there is a Utils class with a few functions that are needed across the application.IsPortBusy and IsExitMessage.

Main classes Server and Client are instantiated depending on the port being busy or not.

In case the port is open, Server is created and run. Otherwise Client is created.

# Server

It has two main methods. StartServer and WaitForStop
StartServer creates a WebSocketServer and handles the logic when OnOpen, OnMessage and OnClose
WaitForStop listens in loop for input entry and stops when it receives the exit message
It also has different methods to send messages to all suscribers and notify the local console with events.

# Client

It has a method to perform a Login, then two main methods for two tasks that are run concurrently. ReadConsoleAndSendHandler and ReceiveFromServer
Client reads user input and checks for the exit message, sending messages the user types, and at the same time listens to the server and displays the messages it receives.
Also finalizes when server is disconnected.



