# nio2so
A continuation of niotso (The Sims Online)

### Original Project
The original project can be found archived here: https://github.com/daddesio/niotso

Additionally, the mashuga client is here: https://github.com/daddesio/mashuga

Here is some helpful reading: http://wiki.niotso.org/Maxis_Protocol

## Summary
nio2so (and niotso) are/were projects aimed to restore The Sims Online in its unaltered state by emulating the server the game interacted with back when the game was online.

Disclaimer: nio2so currently is used for research purposes for discovery about The Sims Online: Pre-Alpha. It has not been tested/designed for any other version, currently.

### Sitemap
nio2so has the following components:
 * **nio2so.TSOHTTP(S)** - This handles the all HTTP(S) communication with the game client. These use the exact same schema, just one is configured for HTTP, and the other is HTTPS.
 * **nio2so.TSOProtocol** - This is the schema used for HTTP and HTTPS communcation.
 * **nio2so.TSOTCP.City** - This emulates the TSO: Pre-Alpha City server. This server uses TCP as the transport layer and it was based on Cadence, Aries and Voltron at Maxis.
 * **TCOQuaZar** - A TCP server framework I designed for use in various applications: https://github.com/JDrocks450/TCPQuaZar

## Setting up
You do need a copy of The Sims Online: Pre-Alpha installed on your system.

I recommend using DxWnd as this software is quite buggy nowadays.

### Clone the Repository
Clone the repository, ensure you have .NET Framework installed on your system and the latest version of Visual Studio.

![image](https://github.com/JDrocks450/nio2so/assets/16988651/d0b3ec30-25e1-491b-9afd-ef4df962da18)

I would highly recommend editing your StartUp Project Settings to match mine:

![image](https://github.com/JDrocks450/nio2so/assets/16988651/fe7ef991-f7a4-4669-b2ef-a34c870a03e3)

Build & Run, ensure there are no build errors.

### Boot-up Sims Online Pre-Alpha
This was designed to run with an unmodified client. However, you can also use FatBag's patch, detailed more thoroughly in this wiki article: http://wiki.niotso.org/Maxis_Protocol

This is where you have to make a choice. If you're trying to experiment with the City server, skip to the next step. Otherwise if you're looking to try to host a room, continue.

OK. Now, you have to actually have to have the aforementioned patch. This is called the HouseSimServer patch. The Mashuga client is used to facilitate the ability to host a room.

You must run FatBag's patched client with the following runtime arguments (DxWnd helps here): `-w -debug_objects -nosound -hsb_mode -playerid:1200 -vport:49`

This instance you just opened will be the HOST. Now, make a client that will connect to the host.

Run another instance with these runtime arguments: `-w -debug_objects`

Next step.

### Logging in
OK, made it this far. Now you gotta log in. Let's begin.

Run the nio2so StartUp Project configuration I showed you above. It will launch three servers. The `TSOHTTP`, `TSOHTTPS`, and `TSOTCP.City` servers. They all are required.

Type in your username and password. Just in case you forgot I made this, your username must be `bloaty`. You can definitely change this if you want. Password can't be blank! I thought of that, too. (AuthLogin.cs)

First, The Sims Online will connect on TSOHTTPS. Then, it will ping TSOHTTP. If both of these aren't running, you won't get far.

### SAS and beyond
Welcome to the research area. Anything from here we have to discover.

The City Server is a daunting animal. Let me give you some insight below.

## Voltron? Cadence? Aries? What are you talking about?
I will tell you a little bit about these technologies.
 * **Cadence** - Cadence & CadenceClient are the lowest level transport for TCP communication. They're libraries facilitating the transport of data using TCP. This isn't too important.
 * **Aries** - Aries is the next level up on top of Cadence. Aries has AriesPackets, which have a Type, timestamp, and size. [Read up about it more here](http://wiki.niotso.org/Maxis_Protocol#Aries_packets)http://wiki.niotso.org/Maxis_Protocol#Aries_packets.
Aries, in truth, isn't really crucial either since the Sims Online development team made Voltron packets which do a similar thing as Aries.
 * **Voltron** - It's the protocol The Sims Online used to create the gameplay experience. Every action the Client can perform is largely dictated using Voltron Packets (Pre-Alpha calls these PDUs). PDUs are sent and received from Regulators (controllers, basically). A Regulator handles a certain aspect of the Engine powering the game. For example, there is a LoginRegulator which handles... logging in!

## Voltron Packets & PDUs
Voltron as a framework uses Voltron Packets wrapped inside of Aries Packets to get the job done. [The structure of a Voltron Packet can be found here.](http://wiki.niotso.org/Maxis_Protocol#Voltron_packets)

In summary, Voltron Packets have:
 * Voltron Packet Type (PDU ID)
 * Payload Size

Hey developers! Check out TSOVoltronPacket.cs in TSOTCP.City! Wanna see how it's serialized? TSOVoltronPacket.MakeBodyFromProperties();

[Here's a list of all TSO Pre-Alpha PDU IDs.](http://niotso.org/files/prealpha_pdu_tables.txt)

Don't want to use that link? There is an enum for you to use in code at TSOVoltronEnum.cs

## Regulators & Protocols
Yes, regulators are controllers, basically. Each Regulator has a protocol that it uses. The protocol is the set of PDUs it can send/receive and how to interpret them. There are the following Protocols in The Sims Online: Pre-Alpha:
 * LoginProtocol
 * cGameMasterProtocol
 * RoommateProtocol
 * CitySelectorProtocol
 * SessionProtocol
 * FriendshipProtocol
 * Top100Protocol
 * BookmarkProtocol
 * MessageProtocol
 * SearchProtocol
 * VersionProtocol

Regulators are state-machines. They have a state they're currently in, and their state can transition to another one dictated by how they're programmed. 

Regulators will see the incoming PDUs each frame. If it's a message that Regulator can interpret, it will handle it accordingly. If not, it is passed up the chain. 

The developers referred to Regulators in the source code as cTSORegulator.cpp

## Areas of Operation
This project is a lot of Reverse-Engineering. Here's some helpful tips on what some assemblies generally do.

 * **cTSOServiceClientD.dll** - Database Operations
 * **cTSOVoltronDMServiceD.dll** - Voltron/City Server
 * **Aries.dll** - Packet Send/Receive
 * **Cadence.dll** - Packet Send/Receive
