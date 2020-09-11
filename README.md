# UMI3D-SDK (Unity)
UMI3D is a web protocol that enables the creation of 3D media in which users of any AR/VR device can collaborate in real time. The 3D media is created once and hosted on a server or on a local computer. Any AR/VR device can display and interact with it remotely thanks to a dedicated UMI3D browser. 

For more information about UMI3D, visit the [UMI3D Consortium's website](https://umi3d-consortium.org)

### Version And Documentation

The Current UMI3D-SDK version is 2.0.
The documentation can be found [https://umi3d.github.io/UMI3D-SDK/index.html](here)

### UMI3D Browser & Samples

* [Virtual Worlds Samples](https://github.com/UMI3D/UMI3D-Samples)
* [UMI3D desktop browser](https://github.com/UMI3D/UMI3D-Desktop-Browser)
* UMI3D SteamVR browser (Coming soon)

### Installation through .unitypackage

* [UMI3D Virtual World Development Kit](/Packages/edk.unitypackage)
* [UMI3D Browser Development Kit](/Packages/cdk.unitypackage)

### Installation through Unity's Package Manager

Open Your project manifest ('.\Packages\manifest.json')

For the UMI3D Virtual World Development Kit add the following lines at the top of the 'dependencies' array:
>	"com.gfi-innovaton.umi3d.dependencies":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Dependencies#development",
>	"com.gfi-innovaton.umi3d.common.core":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/Core#development",
>	"com.gfi-innovaton.umi3d.common.interaction-system":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/InteractionSystem#development",
>	"com.gfi-innovaton.umi3d.common.user-capture":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/UserCapture#development",
>	"com.gfi-innovaton.umi3d.common.collaboration":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/Collaboration#development",
>	"com.gfi-innovaton.umi3d.edk.core":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/EnvironmentDevelopmentKit/Core#development",
>	"com.gfi-innovaton.umi3d.edk.interaction-system":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/EnvironmentDevelopmentKit/InteractionSystem#development",
>	"com.gfi-innovaton.umi3d.edk.user-capture":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/EnvironmentDevelopmentKit/UserCapture#development",
>	"com.gfi-innovaton.umi3d.edk.collaboration":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/EnvironmentDevelopmentKit/Collaboration#development",

For the UMI3D Browser Development Kit add the following lines at the top of the 'dependencies' array:
>	"com.gfi-innovaton.umi3d.dependencies":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Dependencies#development",
>	"com.gfi-innovaton.umi3d.common.core":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/Core#development",
>	"com.gfi-innovaton.umi3d.common.interaction-system":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/InteractionSystem#development",
>	"com.gfi-innovaton.umi3d.common.user-capture":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/UserCapture#development",
>	"com.gfi-innovaton.umi3d.common.collaboration":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/Common/Collaboration#development",
>	"com.gfi-innovaton.umi3d.cdk.core":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/ClientDevelopmentKit/Core#development",
>	"com.gfi-innovaton.umi3d.cdk.interaction-system":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/ClientDevelopmentKit/InteractionSystem#development",
>	"com.gfi-innovaton.umi3d.cdk.user-capture":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/ClientDevelopmentKit/UserCapture#development",
>	"com.gfi-innovaton.umi3d.cdk.collaboration":"https://github.com/Gfi-Innovation/UMI3D-SDK.git?path=/UMI3D-SDK/Assets/ClientDevelopmentKit/Collaboration#development",
