function getClientsInfo() {
    $.get(basepath + "GetClientsInfoJson", function (res) {
        var data = res.Data;
        var clientsInfo = $.parseJSON(data);

        updatedClientsInfo(clientsInfo);
    });
}