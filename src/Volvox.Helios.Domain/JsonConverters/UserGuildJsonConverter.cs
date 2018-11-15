using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Volvox.Helios.Domain.Discord;

namespace Volvox.Helios.Domain.JsonConverters
{
    public class UserGuildJsonConverter : JsonConverter<List<UserGuild>>
    {
        public override bool CanWrite => false;

        public override bool CanRead => true;

        public override void WriteJson(JsonWriter writer, List<UserGuild> value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override List<UserGuild> ReadJson(JsonReader reader, Type objectType, List<UserGuild> existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            var userGuildsResponse = JToken.Load(reader);

            var userGuilds = new List<UserGuild>();

            foreach (var userGuild in userGuildsResponse)
            {
                var newUserGuild = new UserGuild
                {
                    Guild = new Guild()
                };

                serializer.Populate(userGuild.CreateReader(), newUserGuild);
                serializer.Populate(userGuild.CreateReader(), newUserGuild.Guild);

                // Assign the guild image URL relative to Discord
                newUserGuild.Guild.ImageUrl = $"https://cdn.discordapp.com/icons/{newUserGuild.Guild.Id}/{newUserGuild.Guild.Icon}.png";

                userGuilds.Add(newUserGuild);
            }

            return userGuilds;
        }
    }
}