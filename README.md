# Rebirth
MapleStory Global v95.1 Server Emulator. Written from scratch with influences from Nexon BMS leaked server files.  
Started by Rajan (2018-2019) and continued by Erik (2019-2021).  
The custom content (emojis, security features, etc) added by Raj have been stripped from the source by his request.  
This project uses .img files for data loading and all game data files are hot (eager) loaded on start. This consumes about 0.5GB of RAM when it's all loaded. Might want to reconsider this design choice if upgrading to current version of GMS, but for low versions it's completely fine to hot load the entire thing.  
The documentation for this project is not good -- expect no less.  

## :hammer: Building
Building this bad boi is a little finicky and the python dependency will throw endless errors unless a specific install recipe is followed. See below section for python instructions.  
For this you'll need:  
* [Python 3.7.x (version is critical)](https://www.python.org/downloads/)
* [Redis for Windows](https://github.com/rgl/redis/downloads)
* [PostgreSQL 10.13](https://www.enterprisedb.com/downloads/postgres-postgresql-downloads)
* DB IDE [(I use DataGrip)](https://www.jetbrains.com/datagrip/)
* [Visual Studio 201x (I use 2019)](https://visualstudio.microsoft.com/vs/)
* [IMG Game Data](https://gitlab.com/YungMoozi/rebirthdata/)


### Setting Up Python
For this you'll need Python 3.7.x installed as your FIRST python version. If you have multiple python versions (eg 2.7, 3.7, 3.8, etc) this may or may not work. If this does not work, please uninstall all python builds, clear all python-related entries from your environment (PATH) variables, clear all python-related directories and files from your appdata folders and perform a clean install of python 3.7.x.  
Make sure you do a custom install (not the default) and check all the boxes when performing the install. The important ones here are that you:  
a) install for all users  
b) add python to the environment (PATH) variables  
c) check all the other boxes   

### Setting up DB
Install PostgreSQL and run both sql files that exist in data/sql.  

### Setting up game data/sql
Unzip and place in data/img/Data. (should be wz file structure in Data folder -> Character, Effect, Item, etc.. folders with .img files)  

## :book: Acknowledgements
* [WzTools](https://github.com/diamondo25/MapleManager/)  

## :star: Authors
* Minimum Delta  
* Rajan Grewal (Moozi/Darter)  