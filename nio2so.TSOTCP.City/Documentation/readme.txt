Hello!

Thank you for trying nio2so.
Please see me on GitHub for information on what this project is: https://github.com/JDrocks450/nio2so/

Installation Instructions:
==========================

BEFORE DOING ANYTHING

Install .NET 9.0 runtime or later.
https://dotnet.microsoft.com/en-us/download/dotnet/10.0/runtime

PRE-WORK

1. EDIT YOUR HOSTS FILE:
   It should look like this:

	127.0.0.1 www.ea.com       # AuthLogin for TSO
  	127.0.0.1 xo.max.ad.ea.com # The Sims Online: Pre-Alpha
  	127.0.0.1 tsocs.tso.ea.com # The Sims Online
	
   Please ensure accuracy. The Sims Online: Pre-Alpha hardcodes the address xo.max.ad.ea.com to its executable.

2. INSTALL THE SIMS ONLINE: PRE-ALPHA
   You should install The Sims Online: Pre-Alpha. Check Archive.org.

3. DUPLICATE THE INSTALLATION DIRECTORY
   You should duplicate the installation of TSO:Pre-Alpha two times 
	(1) One for the "Server" TSOClient.exe -- this will host the lot. 
	(2+) However many clients (visitors) TSOClient.exe(s) you want to have open at a given time. Don't use the same client over multiple instances. This will cause problems over fighting for resources (files are saved to UserData which can be locked or overwritten using the same directory)

4. REPLACE TSOCLIENT.EXE WITH PATCHED VERSIONS
   To make a patched TSOClient to host a server, or be a Client, first copy the TSOClient.exe and back it up.
   Use HxD to patch the application, I cannot distribute pre-patched ones for you.
   
   FILE OFFSET 0x1856 is the value in the .data section of the application storing the client type:
   Change it to: 0x01 for HouseSimServer or 0x02 for TSOClient

   For the Server, set the byte above to 0x01, save and use that to host your lot server.
   For the Client(s) (visitors), replace TSOClient.exe with the patch byte being 0x02, like above.
	Again, Back up your installation *.exe(s) just in case !

   Please note: The Patched Server has ONE BYTE changed to designate it as a room Host when you LOAD INTO A LOT THIS AVATAR OWNS (AND IS OFFLINE) (OR PRESS JOIN HOUSE IN SELECT A SIM) (WHY ARE WE YELLING?).

   Do NOT use the same client to both Host and Join. Copy the ENTIRE installation directory for Server patch, and Client patch. As in, you should have ONE installation solely for Client, and one for Server. Failing to do this, both clients as they are operating will fight over saving/loading the same resources to the UserData folder, causing crashes and hangs. (See above)

5. TRUST THE HTTPS CERTIFICATE
   Run the Adjust Certificates script that has been included for you. Select option 1 to trust the certificate. Your server WILL NOT work without doing this.



FIRST-RUN !

(a batch file called "run nio2so.bat" has been bundled for you (you're welcome) to run all servers for you with the URLs they are expected at)
This batch file will set the URLs of your services. Changing the data-service URL will require you to change it with other services using it.

...if not using the batch file...

1. START THE DATA SERVICE
   Run nio2so.DataService.API
   Be prepared for first run to create a settings file for you to customize.

   This will serve all of the required data to the other servers in the nio2so system to allow them to function correctly and track/save progress.

   ...this is required!

2. START THE TSOHTTPS SERVICE
   Run nio2so.TSOHTTPS

   This will interact with the DATA SERVICE to allow you to login to The Sims Online and see your Avatars in SAS.
  
   TSOHTTPs has appconfig.json that has the APIAddress field in it if you change your data service address to a new one.

3. START THE VOLTRON SERVICE
   Run nio2so.TSOTCP.Voltron.Server

   This will act as a stand-in for the original Voltron Server over at Maxis and interface directly with the TSOClient(s) over TCP. It will connect to the Data Service to facilitate the online experience.

   Voltron has a settings file called nio2so.TSOTCP.Voltron.Server.dll.config containing the APIAddress and VoltronSettingsURL fields for use if you change the URL of your dataservice or move 
   it to a new address.

   ... and IT SUPPORTS SSL! BUT, you don't need SSL if using Pre-Alpha, only for all later versions. Set the UseSSL property to true in server settings stored in the Data-Service (next step)

4. SETUP YOUR SETTINGS
   The Data Service instance should be showing you a message with where to find your settings. Edit this document to your liking and when ready, restart the server. You may need to restart Voltron as well.

	Note: If using Visual Studio, there is a configuration called "Cluster" that will run all of these in sequence for your convenience (see readme.md on GitHub) 

NORMAL USAGE

(ensure all previously mentioned server components are running)

1. SIGN IN
   Refer to your database (accounts.json) for an account to use. You can also set a new username now by simply typing it in. Password is "asdf" -- no it's not required, you could even just hit OK with no password.

2. CAS
   You can create a new avatar now as normal. If you already have an Avatar, select it in SAS.

3. CITY
   You can buy a new lot, message others, see other owned lots, search for lots/avatars and join online rooms from this screen as normal.
   
   If joining a room, use a visitor (client) TSOClient to actually connect and play. You need to have a Server (host) instance opened and on the lot -- do this first before using a Patched Client as a visitor! (see next step)

4. HOSTING
   Selecting an Avatar who OWNS A LOT ALREADY with a Server TSOClient will automatically load into your lot as a host. Your Host avatar will not appear. Your room is ready for a visitor to join from City View. Visitors can appear and move around. If they are a Roommate, they can build without issue. 

Visitors cannot build without being a roommate, and they cannot use cheats, either. The host CAN use cheats however.