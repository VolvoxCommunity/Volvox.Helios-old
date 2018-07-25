

# Volvox.Helios
#### An easy to use, customizable, modular discord bot, managed from your favorite browser.

## Build Status
[![Build Status](https://travis-ci.org/BillChirico/Volvox.Helios.svg?branch=master)](https://travis-ci.org/BillChirico/Volvox.Helios)


## Getting Started
Visit the Volvox.Helios [website](http://volvoxhelios.azurewebsites.net/) to get started.

## Prerequisits
.NET Core ->  2.1 or above.  
NPM -> Latest

## Installation
Clone the repo

Head to the [discord developer site](https://discordapp.com/developers/applications/), and create an application.  
 _note_: Make sure to make a bot user, and require OAuth 2 grant.

 Add a redirect URL to the Oauth 2 column.  
_example_: http://localhost:5000/signin-discord

Add these setting _after_ the redirect URL

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
`$ dotnet run`


## Contributing

## Credits
Helios was produced by Volvox, a community dedicated to learning.  
Join our [Discord](https://discord.gg/W45xA4t)


## License
Volvox.Helios is [MIT licensed](https://github.com/BillChirico/Volvox.Helios/blob/master/LICENSE).
