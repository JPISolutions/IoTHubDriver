IoT Hub Driver
This is a ClearSCADA driver intended to export point data from ClearSCADA to either AWS or Azure IoT Hubs. It is built using the ClearSCADA Simple Driver Development Kit available here: http://resourcecenter.controlmicrosystems.com/display/public/CS/Driver+Development+Kit+%28DDK%29.

Versioning Notes
This project will make extensive use of branches to keep the various versions separate. Each version of ClearSCADA requires a special build of the driver in order to work correctly. 
dev and master branches are expected to be built based on the latest version of ClearSCADA available only. Each finished stable build of the driver should have several branches, one for each version of ClearSCADA. 

In the DB Module and Driver project properties we are also making use of the ClearSCADA major.minor.build versions so that we know which ClearSCADA version the driver is compiled against. 
Eg. 6.78.6553.0
6.78 is the major-minor ClearSCADA version (CS2017 R1 in this case)
6553 is the ClearSCADA build number. This one is for the November update
0 is JPI's internal release/revision number. 0 = pre-release. 1 = first release. 2 = second release and so on. 

This all might change as things progress... but should get us started for now. 

