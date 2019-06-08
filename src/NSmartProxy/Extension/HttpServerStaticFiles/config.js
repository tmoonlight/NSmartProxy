//@ sourceURL= config.js
function getConfig(key, func) {
    $.get(basepath + "GetConfig?key=" + key, function (res) {

        $("#config" + key).prop("checked", res.Data == "1");

    });
}

$(document).ready(function () {
    getConfig("AllowAnonymousUser");
});

function toggleConfig(obj) {
    var cbx = $(obj).find("input[type='checkbox']");
    var key = cbx.attr("id").replace("config", "");
    var value = cbx.prop("checked") == true ? "1" : "0";
    if (window.conDelayTask) window.clearTimeout(conDelayTask);
    window.conDelayTask = window.setTimeout('setConfig("' + key + '","' + value + '")', 10);//hack
}

function setConfig(key, value) {
    $.get(basepath + "SetConfig?key=" + key + "&value=" + value, function (res) {
        //$("#config" + key).prop("checked", res.Data == "1");
        console.log("设置成功");
    });
}