Files in this directory match the structure of the Game Client files under UserData

User Data files are created by the game client on its own based off responses from the server. 
For example, going into CAS and creating a new avatar, the game will first send a CharBlob, then a CREATEAVATARNOTIFICATION PDU then a SetCharByID
If you respond to all of these with response packets (minus the create avatar notification pdu) the game client will dump Avatar1.dat and create a (avatarid).bkm file.
Both of these will allow you to see your sim in SAS.