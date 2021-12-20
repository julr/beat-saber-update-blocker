# Beat Saber Update Blocker
> A small tool to block Beat Saber from updating to keep mod support.

## Preconditions
This tool does not help you downgrading Beat Saber. If your game already updated you need to downgrade first (e.g. using [this guide](https://steamcommunity.com/sharedfiles/filedetails/?id=1805934840)).

To make sure that Beat Saber is not updating set the automatic download time in Steam to some time where you can intervene first (e.g. update only between 4 and 5 am).\
Additionally you can set to only update Beat Saber on launch in Steam.

## Usage
Close Steam (if running) and start the tool. It will automatically search for the acf file and modify it. Advanced options via command line are available.

### Command line options
`-help` Display a help text\
`-steamapps [dir]` Manually specify the path to the steamapps directory\
`-manifest [id]` Manually specify the manifest id to write

## Thanks
This tool would not be possible without the [SteamCMD API](https://www.steamcmd.net) by Jona Koudijs.