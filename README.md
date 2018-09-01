

# Volvox.Helios [![Build Status](https://travis-ci.org/VolvoxCommunity/Volvox.Helios.svg?branch=master)](https://travis-ci.org/VolvoxCommunity/Volvox.Helios) [![CodeFactor](https://www.codefactor.io/repository/github/volvoxcommunity/volvox.helios/badge)](https://www.codefactor.io/repository/github/volvoxcommunity/volvox.helios) [![Discord](https://discordapp.com/api/guilds/468467000344313866/widget.png)](https://discord.gg/jReSmc3)
#### An easy to use, customizable, modular discord bot, all managed from your favorite browser.

## Getting Started
Visit the Volvox.Helios [website](http://www.volvox.tech) to get started adding the bot to your Discord server.

## Prerequisites
.NET Core ->  Latest  
NPM -> Latest

## Installation
Clone the repo

Head to the [discord developer site](https://discordapp.com/developers/applications/), and create an application.  
 _note_: Make sure to create a bot user, and require OAuth 2 grant.

 Add a redirect URL to the OAuth 2 column.
_example_: http://localhost:5000/signin-discord

Add the following application settings

```
"Discord": { 
    "Token": "BOTTOKENHERE",
    "ClientID": "BOTCLIENTIDHERE",
    "ClientSecret": "BOTCLIENTSECRETHERE"
  }
```

Next, run the following commands to build the project and install the dependencies:
```
$ npm install
$ dotnet restore
$ dotnet build
```

Then, to run the bot:  
```
$ dotnet run
```


## Contributing
Read [CONTRIBUTING.md](https://github.com/VolvoxCommunity/Volvox.Helios/blob/master/CONTRIBUTING.md) for details on how to contribute.  

## Credits
Helios was produced by Volvox, a community dedicated to learning.  
Join our [Discord](https://discord.gg/jReSmc3)!


## License
Volvox.Helios is [MIT licensed](https://github.com/VolvoxCommunity/Volvox.Helios/blob/master/LICENSE).
