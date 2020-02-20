# UMI3D-SDK
UMI3D is a web protocol that enables the creation of 3D media in which users of any AR/VR device can collaborate in real time. The 3D media is created once and hosted on a server or on a local computer. Any AR/VR device can display and interact with it remotely thanks to a dedicated UMI3D browser. 

For more information about UMI3D, visit the UMI3D Consortium's website: https://umi3d-consortium.org

The Current UMI3D-SDK version is the 1.1.b.200219.
There is currently no documentation or tutorial online. Those should be uploaded with upcomming UMI3D-SDK version 1.0.

The Git contain two unity projects.
The first one, "UMI3D-Desktop-Browser", is an example of a UMI3D desktop browser with only the CDK part of the SDK.
The second one, "UMI3D-SDK-Unity", is a Unity project with the complet SDK (EDK and SDK) with an example scene.


To have a quick view on UMI3D unity sdk:
	The recomanded Unity version is 2019.1.14f1 (This should be working with other version but why tempting fate):
	
	- Launch the UMI3D-SDK-Unity project.
	- Open the example scene in UMI3D which can be find at "[Assets]/TestRooms/Interactions/Scenes/TestNewInteractionSystem.unity".
	- Start the scene by clicking on the play button.
	- Note the Ip and Port which can be found on the "WebSocketUMI3DServer" script on the "UMI3D" node.
	
	- Launch the UMI3D-Desktop-Browser project.
	- Open the Connection scene which can be found at "[Assets]/Project/Scenes/Connection.unity".
	- Start the scene by clicking on the play button.
	- Fill the Ip and Port fields.
	- Click on "Connection".
