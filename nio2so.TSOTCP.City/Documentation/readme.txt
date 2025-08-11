Hello!

Thank you for trying nio2so.
Please see me on GitHub for information on what this project is: https://github.com/JDrocks450/nio2so/

Installation Instructions:
==========================

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
   You should duplicate the installation of TSO:Pre-Alpha times 
	(1) One for the "Server" TSOClient who will host the lot. 
	(2) However many clients (visitors) you want to have open at a given time. Don't use the same client over multiple instances. This will cause problems.

4. REPLACE TSOCLIENT
   For the Server, replace TSOClient.exe with PatchedServer.exe
   For the Client(s) (visitors), replace TSOClient.exe with PatchedClient.exe
	Back up your installation *.exe just in case !

   Please note: these clients contain fixes to help them run better on your PC. The PatchedServer has ONE BYTE changed to designate it as a room Host when you SELECT A LOT ON THE CITY VIEW.

FIRST-RUN

2. START THE DATA SERVICE
   Run nio2so.DataService.API
   Be prepared for first run to create a settings file for you to customize.

   This will serve all of the required data to the other servers in the nio2so system to allow them to function correctly and track/save progress.

3. START THE TSOHTTPS SERVICE
   Run nio2so.TSOHTTPS

   This will interact with the DATA SERVICE to allow you to login to The Sims Online and see your Avatars in SAS.

4. START THE VOLTRON SERVICE
   Run nio2so.TSOTCP.Voltron.Server

   This will act as the original Voltron Server at Maxis and interface directly with the TSOClient(s) over TCP. It will connect to the Data Service to facilitate the online experience.

5. SETUP YOUR SETTINGS
   The Data Service instance should be showing you a message with where to find your settings. Edit this document to your liking and when ready, restart the server. You may need to restart
   Voltron as well.

	Note: If using Visual Studio, there is a configuration called "nio2so.City" that will run all of these in sequence for your convenience. 

NORMAL USAGE

(ensure all previously mentioned server components are running)

6. SIGN IN
   Refer to your database (accounts.json) for an account to use. You can also set a new username now by simply typing it in. Password is "asdf".

7. CAS
   You can create a new avatar now as normal. If you already have an Avatar, select it in SAS.

8. CITY
   You can buy a new lot, message others, see other owned lots, search for lots/avatars and join online rooms from this screen as normal.
   
   If joining a room, use a visitor (client) TSOClient to actually connect. You need to have a Server (host) instance opened and on the lot.

9. HOSTING
   Selecting an Avatar who OWNS A LOT ALREADY with a Server TSOClient will automatically load into your lot as a host. Your Host avatar will not appear. Your room is ready for a visitor to
   join from City View. Visitors can appear and move around. If they are a Roommate, they can build without issue. Visitors cannot build without being a roommate, and they cannot use cheats,
   either. The host however, CAN use cheats.