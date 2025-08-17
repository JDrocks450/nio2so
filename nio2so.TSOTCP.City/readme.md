# nio2so "Neo Vol2ron" Server Readme
## Certificates
Included in this project directory are two (2) certificates files: cert.pem and key.pem.
These are used to secure the connection between the client and server. The cert.pem file contains the public certificate, while the key.pem file contains the private key. These files should be kept secure and not shared with anyone.

## Running the Server
### SSL or Non-SSL?
The server will automatically search for these two certificates in the current directory. If they are not found, the server will force not using SSL.
This means that you can only run this server with The Sims Online: Pre-Alpha since it does not support SSL connections.
If you want to run The Sims Online: Play-Test or later, you must provide these two files in the current directory to enable SSL connections.
Additionally, your DataService settings must be set to use SSL connections in your VoltronServerSettings configuration option in your settings.json file.