

# Volvox.Helios [![Build Status](https://travis-ci.org/BillChirico/Volvox.Helios.svg?branch=master)](https://travis-ci.org/BillChirico/Volvox.Helios) [![CodeFactor](https://www.codefactor.io/repository/github/billchirico/volvox.helios/badge)](https://www.codefactor.io/repository/github/billchirico/volvox.helios)
#### An easy to use, customizable, modular discord bot, all managed from your favorite browser.

## Getting Started
Visit the Volvox.Helios [website](https://volvoxhelios.azurewebsites.net/) to get started adding the bot to your Discord server.

## Prerequisits
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
Read [CONTRIBUTING.md](https://github.com/BillChirico/Volvox.Helios/blob/master/CONTRIBUTING.md) for details on how to contribute.  

## Credits
Helios was produced by Volvox, a community dedicated to learning.  
Join our [Discord](https://discord.gg/W45xA4t)


## License
Volvox.Helios is [MIT licensed](https://github.com/BillChirico/Volvox.Helios/blob/master/LICENSE).
