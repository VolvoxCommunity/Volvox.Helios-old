(function($) {

  $.fn.populateGuildChannels = function(guildId) {
    return this.each(function() {
      const selectElement = $(this);

      selectElement.empty();

      selectElement.append('<option selected="true" disabled hidden>Select a channel</option>');
      selectElement.prop("selectedIndex", 0);
      selectElement.prop("disabled", false);

      const url = `/api/GetGuildChannels?guildId=${guildId}`;

      $.getJSON(url, function (data) {
        $.each(data, function (key, entry) {
          selectElement.append($("<option></option>").attr("value", entry.id).text(entry.name));
        });
      });
    });
  };

  $.fn.populateAnalytics = function () {
    return this.each(function () {
      const element = $(this);

      const url = "/api/GetUserAdminGuilds?inGuild=true";

      $.getJSON(url, function (data) {
        element.empty();

        $.each(data, function (key, entry) {
          element.append(generateGuildDropdownItem(entry.id, entry.name, entry.icon, `/Analytics/${entry.id}`));
        });
      });
    });
  };

  $.fn.populateSettings = function () {
    return this.each(function () {
      const element = $(this);

      const url = "/api/GetUserAdminGuilds";

      $.getJSON(url, function (data) {
        element.empty();

        $.each(data, function (key, entry) {
          element.append(generateGuildDropdownItem(entry.id, entry.name, entry.icon, `/Settings/${entry.id}`));
        });
      });
    });
  };

  function generateGuildDropdownItem(guildId, guildName, guildIcon, href) {

    // guildIcon will be null if the guild doesn't have an icon. Therefore set src to default error icon.
    const iconUrl = guildIcon === null ? "/images/error.png" : `https://cdn.discordapp.com/icons/${guildId}/${guildIcon}.png`;

    return (`
      <div>
        <a class="dropdown-item dropdown-item-container" href="${href}">
            <span>
                <img class="dropdown-image-small" src="${iconUrl}">
            </span>
            <span>
                ${guildName}
            </span>
        </a>
    </div>
    `)
  }

}(jQuery));
