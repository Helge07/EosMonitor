# EosMonitor 
A tethering tool for Canon DSLR- and DSLM-cameras

## Table of contents
- [Introduction](#Introduction)
- [Requirements](#Requirements)
- [Compilation_and_Installation](#Compilation_and_Installation)
- [Usage](#usage)
- [License](#license)

## Introduction
The Windows application `EosMonitor` is aimed to control Canon DSLM and DSLR cameras which are connected to a host computer via a USB cable. `EosMonitor` sets the parameters for taking pictures, can display a LiveView image and can download pictures to the host. A special feature of `EosMonitor` is the ability to take focus stacking and defocus stacking image series:
- Focus stacks are used to increase the depth of field 
- Defocus stacks are used to increase the background blur 


## Requirements
`EosMonitor` was developed with Visual Studio 2022 as a WPF-application based on Net 9.0. and the Canon development kit EDSDK 13.18.40. The complete EDSDK can be obtained from Canon, but the two files used in EosMonitor (EDSDK.dll and EdsImage.dll) are contained in the GitHub Repository (with permission of Canon).

## Compilation_and_Installation
To compile and install `EosMonitor`, follow these steps:
1. Get the repository from https://github.com/Helge07/EosMonitor/tree/master 

2. Open the project file  EosMonitor\EosMonitor.sln

3. Compile the project `EosMonitor`. This generates the directories
   'EosMonitor\bin\Debug\net9.0-windows10.0.22621.0'
   resp.
   'EosMonitor\bin\Release\net9.0-windows10.0.22621.0'
   
4. Add the following three files from the directory 'RuntimeFiles' to each of the above directories:
   - EDSDK.dll
   - EdsImage.dll
   - EosMonitor.cfg
   
5. Start EosMonitor.exe  from the Debug- resp. Release-directory 

6. Compiling the project `EosMonitor_Setup` will produce a .msi installation file which can be used to install the application as a Windows application.

## Usage
The `EosMonitor_Usermanual+Anleitung.pdf` contains the user documentation (in english and german). The documentation of the program structure can be found in `EosMonitor_structurediagrams.pdf`.
Moreover the program code contains delailed comments and explanations.  

## EosMonitor on GitHub Pages
- https://helge07.github.io/EosMonitor/

## License
EosMonitor is published under the GPL-3.0 license. See the LICENSE file for more information. The EDSDK files EDSDK.dll and EdsImage.dll are distributed with permission of Canon.



