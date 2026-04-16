<p align="center"> 
  <img src="/nio2so.TSOView2/tsoview2logo.png" height="200"> 
</center>

# TSOView2 for nio2so

This program is a utility that has many functions relative to working on reverse-engineering and exploring The Sims Online.

It has many functions which some are documented [in the wiki](https://github.com/JDrocks450/nio2so/wiki/TSOView2).

## Scope of Functions

<img width="2020" height="1521" alt="image" src="https://github.com/user-attachments/assets/2a6157de-529c-41fe-9c60-5e5d1537222e" />

_The start page in TSOView2_

### File Menu

TSOView2 supports the following utilities and functions in the File menu: 

* **Emulating UI Scripts** to visualize how certain UI Elements would've looked like without a running game client. [Read more here](https://github.com/JDrocks450/nio2so/wiki/TSOView2#ui-script-uis-viewer)
* **Rendering 3D City geometry** using Pre-Alpha _(like)_ terrain tile rendering algorithm _(emulated)._ [Jump](https://github.com/JDrocks450/nio2so/tree/master/nio2so.TSOView2#Viewing-3D-Geometry)
* **Open the TSODataDefinition.dat file** and allow you to explore its contents. [Jump](https://github.com/JDrocks450/nio2so/tree/master/nio2so.TSOView2#viewing-tsodatadefinitiondat)
* **Compress/Decompress a RefPack stream** - [What is RefPack?](http://wiki.niotso.org/RefPack) - [Jump](https://github.com/JDrocks450/nio2so/tree/master/nio2so.TSOView2#compressing--decompressing-refpack)
* **Visualize nio2so server packet activty** to help you in your research
* **Read String (*.CST) Files** and entire directories.
* **Open FAR Archives** - FAR3, FAR1 & FARV1B Archives are supported for viewing and extraction.
* **Open Targa Images** - The Sims Online uses these sometimes.

### View Menu

<img width="893" height="252" alt="image" src="https://github.com/user-attachments/assets/ba680522-de6e-4036-81ef-088b21e32ff4" />

TSOView2 supports the following functions in the View menu:

* **Viewing an interactive library** of the currently reverse-engineered Maxis Protocol for The Sims Online: Pre-Alpha using nio2so's implementation. [Jump](https://github.com/JDrocks450/nio2so/tree/master/nio2so.TSOView2#Viewing-the-Maxis-Protocol) - [Full Spec](https://github.com/JDrocks450/nio2so/wiki/The-Sims-Online:-Pre%E2%80%90Alpha-%E2%80%90-Voltron-Specification)
* **View Constants** found in the Executable file, search, copy values, etc.

### Plugin Menu

<img width="752" height="205" alt="image" src="https://github.com/user-attachments/assets/153bdbb2-5433-4a5a-9df8-ba81cf8cf266" />

TSOView2 has the following plug-ins in the Plugin menu:

* **Format a table of constants** formatted in one format to one compatible with C#
* **Take a Play-Test version or later City Folder and make it compatible with The Sims Online: Pre-Alpha** (City Transmogrifier)
* **Utility to paste Hex Data from Edith** to have it formatted, dumped or visualized


## Compressing / Decompressing RefPack

<img width="981" height="486" alt="image" src="https://github.com/user-attachments/assets/320a0385-7280-4cc4-9f6d-ada9e103e628" />

Using nio2so, you can sniff through some dumped network activty to/from the nio2so NeoVol2ron Server (Voltron Emulator Server) (Voltron is the Server for The Sims Online)
and come across a payload that is a compressed RefPack stream. 

### Decompression
You can dump this stream of bytes to a separate isolated file, and drag it into the **left (Blue) box**. TSOView2 will dump a decompressed stream!

### Compression
You can (re)compress data by feeding a file containing bytes to compress into the **right (Orange) box**.

## Viewing 3D Geometry

<img width="2027" height="1465" alt="image" src="https://github.com/user-attachments/assets/84fecfaa-504b-4056-9b22-b15f4a77a4d9" />

<img width="2020" height="1463" alt="image" src="https://github.com/user-attachments/assets/5ffdf367-3b46-42e2-8a87-dd0940b8cc94" />

_3D Geometry being viewed using TSOView2_

You can view 3D visualization of a given city by doing the following:

* Visit **File** > **Pre-Alpha City Map** or **New & Improved City Map**
* Select a City Folder
* Wait...
* Right-Click and Drag to move, Scroll to Zoom
* **File > Close Page** to exit

## Viewing TSODataDefinition.dat

<img width="1569" height="1127" alt="image" src="https://github.com/user-attachments/assets/8ea809f3-0869-4eba-bac1-5ea832566c74" />

_You can use open any TSODataDefinition file using the File Menu in the TSOView2 Data Definition Viewer._

Clicking on a struct will list its properties. Using the Find Menu in the toolbar will allow you to apply a text filter.

### Level One Structs

These are the simplest data types, which can be used in a Level Two data type. They represent a basic, reusable struct data type.

### Level Two Structs

These encompass basic and Level One data types to make more complex data structures.

### Derived Types

These take a Level Two data type and apply a Keep/Remove Mask to the type to optimize data types to lower bandwidth, etc. 

### Strings

This menu lists all strings found in the document.

## Viewing the Maxis Protocol

<img width="2019" height="1575" alt="image" src="https://github.com/user-attachments/assets/df5e1081-7c36-45ca-9428-9489495d487e" />

_Using the Interactive Maxis Protocol viewer in TSOView2._

TSOView2 uses C# Reflection to visualize the codebase. nio2so Voltron uses a reverse-engineered Maxis Protocol programmed in C#. This utility will visualize the structure of the 
Network Data Sent to/from The Sims Online Client. This happens to mimic the structure of the C# code document due to how the project's code is written.

This is guaranteed to match the structure, and order, of the data the Client will send/receive because this deeply connects to the **VoltronSerializer** system.

The **VoltronSerializer** has a secondary functionality which will generate a **SerializationGraph**, optionally with runtime values. This **SerializationGraph** is then visualized by this utility.

_NOTE: Using the nio2so Network Activity utility will show runtime values for a network frame._

[View the Full Specification here](https://github.com/JDrocks450/nio2so/wiki/The-Sims-Online:-Pre%E2%80%90Alpha-%E2%80%90-Voltron-Specification)

### Left - Navigation Menu

This is a tree-graph representation of the how certain structures build off simpler ones.

### Center-Top - Data Structure

* Items in Square Brackets are a serializer-hint, telling the serializer to handle that type differently.
* Green Types are classes
* Blue Types are basic types (int, bool)
* Arrays are denoted by two square brackets after the Type Name

### Bottom - MD Generator

Copy & Paste MD into GitHub
