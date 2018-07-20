(function($) {

    $.fn.populateGuildChannels = function(guildId) {
        return this.each(function() {
            let selectElement = $(this);

            selectElement.empty();

            selectElement.append('<option selected="true" disabled hidden>Select a channel</option>');
            selectElement.prop('selectedIndex', 0);
            selectElement.prop('disabled', false);

            const url = `/Settings/GetGuildChannels?guildId=${guildId}`;

            $.getJSON(url, function (data) {
                $.each(data, function (key, entry) {
                    selectElement.append($('<option></option>').attr('value', entry.id).text(entry.name));
                })
            });
        });
    };

}(jQuery));