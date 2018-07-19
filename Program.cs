using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace myApp
{
    class Server
    {
        public class Clients
        {
            private int globalPoint;
            private Socket theSocket;
            private string clientName;
            private bool isConnected;
            private bool isInChat;

            public int getGlobalPoint()
            {
                return globalPoint;
            }

            public string getClientName()
            {
                return clientName;
            }

            public Socket getSocket()
            {
                return theSocket;
            }

            public bool getisConneted()
            {
                return isConnected;
            }

            public void makeOffline()
            {
                this.isConnected = false;
            }

            public void chatNow()
            {
                isInChat = true;
            }

            public void stopConversation()
            {
                isInChat = false;
            }

            public bool isInConversation() { return isInChat; }

            public Clients(string name, Socket temp)
            {
                clientName = name;
                theSocket = temp;
                isConnected = true;
                isInChat = false;
            }

        }

        public class Chat
        {
            public string chatter1;
            public string chatter2;
            public Chat(string p1, string p2)
            {
                chatter1 = p1;
                chatter2 = p2;
            }

            public string getNameOne()
            {
                return chatter1;
            }

            public string getNameTwo()
            {
                return chatter2;
            }

            public void setNameOne(string tempName)
            {
                chatter1 = tempName;
            }

            public void setNameTwo(string tempName)
            {
                chatter2 = tempName;
            }

        }

        public struct Invititation
        {
            public string inviter;
            public string invitee;

            public Invititation(string p1, string p2)
            {
                inviter = p1;
                invitee = p2;
            }
        }
        static List<Chat> chatList = new List<Chat>();
        static List<Clients> clientList = new List<Clients>();
        static List<Invititation> inviteList = new List<Invititation>();
        static List<Socket> socketList = new List<Socket>();
        static bool listening = false;
        static bool terminating = false;
        static bool accept = true;
        static int counter = 0;
        static Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public bool isInClientList(string tempName)
        {
            foreach (Clients tempClient in clientList)
            {
                if (tempClient.getClientName() == tempName)
                {
                    return true;
                }
            }
            return false;
        }

        public string returnNameBySocket(Socket tempSocket)
        {
            string temp = "X";
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].getSocket() == tempSocket)
                {
                    temp = clientList[i].getClientName();
                    return temp;
                }
            }
            return temp;
        }

        public string returnOpponentName(string tempName)
        {
            string temp = "X";
            for (int i = 0; i < chatList.Count; i++)
            {
                if (chatList[i].getNameOne() == tempName)
                {
                    temp = chatList[i].getNameTwo();
                    return temp;
                }
                else if (chatList[i].getNameTwo() == tempName)
                {
                    temp = chatList[i].getNameOne();
                    return temp;
                }
            }
            return temp;
        }

        public void sendMessage(Socket clientSocket, string messegeTemp) //sends message to given client
        {
            byte[] bufferTempInitial = Encoding.Default.GetBytes(messegeTemp);
            clientSocket.Send(bufferTempInitial);
        }

        public void sendMessageByName(string tempName, string message)
        {
            for (int i = 0; i < clientList.Count; i++) //to return socket of the opponent
            {
                if (clientList[i].getClientName() == tempName)
                {
                    sendMessage(clientList[i].getSocket(), message);
                }
            }
        }

        public void sendPrivateMessage(Socket clientSocket, string nameOftalker, string message)
        {
            string tempMessage = "[" + returnNameBySocket(clientSocket) + "] : " + message + "\n";
            sendMessageByName(nameOftalker, tempMessage);
        }

        public void broadcastMessage(Socket clientSocket, string message)
        {
            string tempMessage = "[" + returnNameBySocket(clientSocket) + "] : " + message + "\n";
            for (int i = 0; i < clientList.Count; i++)
            {
                sendMessageByName(clientList[i].getClientName(), tempMessage);
            }
        }
        public bool doesContain(string tempName)
        {
            for (int i = 0; i < clientList.Count; i++) //to return socket of the opponent
            {
                if (clientList[i].getClientName() == tempName)
                {
                    return true;
                }
            }
            return false;
        }

        public bool isChatting(string tempName)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].getClientName() == tempName)
                {
                    return clientList[i].isInConversation();
                }
            }
            return false;
        }

        public void startChat(string tempName)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].getClientName() == tempName)
                {
                    clientList[i].chatNow();
                }
            }
        }

        public void stopChatting(string tempName)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].getClientName() == tempName)
                {
                    clientList[i].stopConversation();
                }
            }
        }

        public void sendPlayerList(Socket clientSocket)
        {
            sendMessage(clientSocket, "Here are the players in the room : \n");
            Console.Write("Sending player list to Client : " + returnNameBySocket(clientSocket));
            foreach (Clients tempClient in clientList)
            {
                sendMessage(clientSocket, "     >> " + tempClient.getClientName() + " with global point " + tempClient.getGlobalPoint() + "\n");
            }
        }

        public void removeFromClientList(string tempName)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i].getClientName() == tempName)
                {
                    clientList.Remove(clientList[i]);
                    break;
                }
            }
        }

        public void initiateInvitation(string requester, string requested)
        {
            if (!isInClientList(requested)) //if requested players is not present in network
            {
                Console.Write("Player : " + requested + " who is requested by " + requester + " is not connected to server. \n");
            }
            else if (isChatting(requester))
            {
                Console.Write("Player : " + requester + " is already joined a Chat. \n");
            }
            else if (isChatting(requested))
            {
                Console.Write("Player : " + requested + " is already joined a Chat. \n");
            }
            else //in case of invitation; the pair of players are put in temporary list to be later added to real list if other player accepts else it is removed from the list
            {
                inviteList.Add(new Invititation(requester, requested));
            }
        }
        public int isInvited(Socket theSocket)
        {
            foreach (Invititation temp in inviteList)
            {
                if (temp.invitee == returnNameBySocket(theSocket))
                    return 0; //yes invited
                else if (temp.inviter == returnNameBySocket(theSocket))
                    return 1; //trying to invate himself
            }
            return 2; //not invated
        }

        public void clearOtherInvitations(string tempName)
        {
            foreach (Invititation temp in inviteList)
            {
                if (temp.inviter == tempName)
                    inviteList.Remove(temp);
                else if (temp.invitee == tempName)
                    inviteList.Remove(temp);
            }
        }

        public string returnOpponentinInvitation(string tempName)
        {
            foreach (Invititation temp in inviteList)
            {
                if (temp.invitee == tempName)
                    return temp.inviter;
                else if (temp.inviter == tempName)
                    return temp.invitee;
            }
            return "X";
        }

        public void inviteClient(Socket theSocket, string tempName)
        {
            Console.Write(returnNameBySocket(theSocket) + " is asking to invite : " + tempName + ". \n");
            if (returnNameBySocket(theSocket) != tempName)
            {
                if (doesContain(tempName))
                {
                    if (isChatting(tempName))
                    {
                        Console.Write(returnNameBySocket(theSocket) + " can't invite "
                           + tempName + " is already been Conversation with someone else. . \n");
                        sendMessage(theSocket, tempName + " is already been Conversation with someone else. \n");
                    }
                    else if (isChatting(returnNameBySocket(theSocket)))
                    {
                        sendMessage(theSocket, "You are already in a Chat! So you can't invite anybody. \n");
                        Console.Write(returnNameBySocket(theSocket) + " is already Conversation. \n");
                    }
                    else
                    {
                        sendMessageByName(tempName, returnNameBySocket(theSocket) + " is inviting you to play. \n");
                        sendMessage(theSocket, "Your invitation messege has been sent to " + tempName + " .\n");
                        initiateInvitation(returnNameBySocket(theSocket), tempName);
                    }
                }
                else
                {
                    Console.Write(returnNameBySocket(theSocket) + " can't invite "
                        + tempName + " because s/he is not present in lobby. \n");
                }
            }
            else
            {
                Console.Write(returnNameBySocket(theSocket) + " can't invite itself. \n");
            }
        }

        public void acceptChat(Socket theSocket)
        {
            int invitationSituation = isInvited(theSocket); //returns 0 for begin invited, 1 for inviting himself, 2 for not invited
            if (invitationSituation == 2)
            {// not invited
                Console.Write("There is no request for user: " + returnNameBySocket(theSocket) + " . \n");
                sendMessage(theSocket, "There is no request for you or request is terminated. Thus you can't accept.\n");
            }
            else if (invitationSituation == 1)
            {//inviting himself
                sendMessage(theSocket, "You can't accept your own request. \n");
                Console.Write(returnNameBySocket(theSocket) + " can't accept its own request to start Chat. \n");
            }
            else if (invitationSituation == 0)
            {
                chatList.Add(new Chat(returnOpponentinInvitation(returnNameBySocket(theSocket))
                    , returnNameBySocket(theSocket)));

                startChat(returnOpponentinInvitation(returnNameBySocket(theSocket)));
                startChat(returnNameBySocket(theSocket));

                clearOtherInvitations(returnOpponentinInvitation(returnNameBySocket(theSocket)));
                clearOtherInvitations(returnNameBySocket(theSocket));

                sendMessageByName(returnOpponentName(returnNameBySocket(theSocket)),
                    "You are in a new Chat with " + returnNameBySocket(theSocket) + ". \n");
                sendMessage(theSocket, "You are in a new Chat with "
                    + returnOpponentName(returnNameBySocket(theSocket)) + ". \n");

                Console.Write(returnNameBySocket(theSocket) + " and " +
                    returnOpponentName(returnNameBySocket(theSocket)) + " are in a new Chat. \n");
            }
        }

        public void rejectChat(Socket theSocket)
        {
            int invitationSituation = isInvited(theSocket); //returns 0 for begin invited, 1 for inviting himself, 2 for not invited
            if (invitationSituation == 2)
            {// not invited
                Console.Write("There is no request for user: " + returnNameBySocket(theSocket) + " . \n");
                sendMessage(theSocket, "There is no request for you or request is terminated. Thus you can't reject.\n");
            }
            else if (invitationSituation == 1)
            {
                sendMessage(theSocket, "You can't reject your own request. \n");
                Console.Write(returnNameBySocket(theSocket) + " can't reject its own request to start Chat. \n");
            }
            else if (invitationSituation == 0)
            {
                sendMessageByName(returnOpponentinInvitation(returnNameBySocket(theSocket)), "You have been rejected by "
                    + returnNameBySocket(theSocket) + " .\n");
                sendMessage(theSocket, "You have rejected user: " + returnOpponentinInvitation(returnNameBySocket(theSocket)) + ". \n");

                Console.Write(returnNameBySocket(theSocket) + " has rejected the request from "
                    + returnOpponentinInvitation(returnNameBySocket(theSocket)) + ". \n");

                clearOtherInvitations(returnNameBySocket(theSocket));
            }
        }

        public void sendInChatOrBroadcast(Socket n, string newmessage)
        {
            string sendMessage = newmessage.Substring(4, newmessage.Length - 4);
            if (isChatting(returnNameBySocket(n))) // If in conversation
            {
                Console.Write(returnNameBySocket(n) + " has send a message \n");
                sendPrivateMessage(n, returnNameBySocket(n), sendMessage);
                sendPrivateMessage(n, returnOpponentName(returnNameBySocket(n)), sendMessage);
            }
            else
            {
                Console.Write(returnNameBySocket(n) + " has send a message \n");
                broadcastMessage(n, sendMessage);
            }
        }

        public void finishChat(Socket theSocket) //the one in the socket is the winner
        {
            stopChatting(returnOpponentName(returnNameBySocket(theSocket)));
            stopChatting(returnNameBySocket(theSocket));
            clearOtherInvitations(returnOpponentName(returnNameBySocket(theSocket)));
            clearOtherInvitations(returnNameBySocket(theSocket));
        }


        public static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter a numeric argument.");
                System.Console.WriteLine("Usage: App <Port>");
            }
            
            Server chatServer = new Server();

            chatServer.startListen(args[0]);
        }


        public void Receive()
        {
            bool connected = true;
            Socket n = socketList[socketList.Count - 1];

            while (connected)
            {
                try
                {
                    Byte[] buffer = new byte[64];
                    int rec = n.Receive(buffer);
                    if (rec <= 0)
                    {
                        throw new SocketException();
                    }
                    string newmessage = Encoding.Default.GetString(buffer);
                    newmessage = newmessage.Substring(0, newmessage.IndexOf("\0"));
                    if (newmessage.Length > 6)
                    {
                        if (newmessage.Substring(0, 6) == "Invite")
                        {
                            inviteClient(n, newmessage.Substring(6, newmessage.Length - 6));
                        }
                    }
                    if (newmessage == "Accept")
                    {
                        acceptChat(n);
                    }
                    else if (newmessage == "Reject")
                    {
                        rejectChat(n);
                    }
                    else if (newmessage == "Surrender")
                    {
                        finishChat(n);
                    }
                    else if (newmessage == "Give")
                    {
                        sendPlayerList(n);
                    }
                    else if (newmessage.Length > 5)
                    {

                        if (newmessage.Substring(0, 4) == "Send")
                        {
                            string sendMessage = newmessage.Substring(4, newmessage.Length - 4);
                            if (isChatting(returnNameBySocket(n))) // If in conversation
                            {
                                Console.Write(returnNameBySocket(n) + " has send a message \n");
                                sendPrivateMessage(n, returnNameBySocket(n), sendMessage);
                                sendPrivateMessage(n, returnOpponentName(returnNameBySocket(n)), sendMessage);
                            }
                            else
                            {
                                Console.Write(returnNameBySocket(n) + " has send a message \n");
                                broadcastMessage(n, sendMessage);
                            }
                        }
                    }
                }
                catch
                {
                    if (!terminating) //client has choose to disconnect or a problem occured
                        Console.Write("Client has disconnected : " + returnNameBySocket(n));
                    n.Close();
                    string tempName = returnNameBySocket(n);
                    socketList.Remove(n);
                    clearOtherInvitations(tempName);
                    removeFromClientList(tempName);
                    connected = false;
                }
            }

        }

        public bool addClient(Socket clientTemp) // adding new client if the username is not previously used
        {
            string username;
            Byte[] buffer = new byte[64];
            int rec = clientTemp.Receive(buffer);
            username = Encoding.Default.GetString(buffer);
            username = username.Substring(0, username.IndexOf("\0"));
            if (doesContain(username)) //adds if user name is not used
            {
                Console.Write("New client request have been rejected. \n");
                Console.Write("Following username is already used :" + username + "\n");
                return false;
            }
            else
            {
                clientList.Add(new Clients(username, clientTemp));
                Console.Write("New client has connected : " + username + "\n");
                return true;
            }
        }

        public void Accept()
        {
            while (accept)
            {
                try
                {
                    counter += 1;
                    Socket clientTemp = server.Accept();
                    socketList.Add(clientTemp);
                    if (addClient(clientTemp)) //proceeds if user name is unique
                    {
                        Thread thrReceive;
                        thrReceive = new Thread(new ThreadStart(Receive));
                        thrReceive.IsBackground = true;
                        thrReceive.Start();
                    }
                    else //removes the socket because username is faulty
                    {
                        sendMessage(clientTemp, "Username is not valid! \n Disconnecting from server...");
                        socketList.Remove(clientTemp);
                    }

                }
                catch
                {
                    if (terminating)
                        accept = false;
                    else
                        Console.Write("Listening socket has stopped working...\n");
                }
            }
        }

        private void startListen(string tempPort)
        {
            int serverPort;
            Thread thrAccept;

            //this port will be used by clients to connect            
            serverPort = Convert.ToInt32(tempPort);

            try
            {
                server.Bind(new IPEndPoint(IPAddress.Any, serverPort));
                Console.Write("Started listening for incoming connections.\n");

                server.Listen(10); //the parameter here is maximum length of the pending connections queue
                thrAccept = new Thread(new ThreadStart(Accept));
                thrAccept.IsBackground = true;
                thrAccept.Start();
                listening = true;
            }
            catch
            {
                terminating = true;
                Console.Write("Cannot create a server with the specified port number. Check the port number and try again.\n");
                Console.Write("terminating...");
            }
        }


    }
}
