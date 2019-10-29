//@ sourceURL= config.js
//function getClientsInfo() {
//    $.get(basepath + "GetClientsInfoJson", function (res) {
//        var data = res.Data;
//        var clientsInfo = $.parseJSON(data);

//        updatedClientsInfo(clientsInfo);
//    });
//}
//折叠表
(function ($) {
    //$(function () {
    //    initDataTable();
    //});
    function initDataTable() {
        $('.table-expandable').each(function () {
            var table = $(this);
            table.children('thead').children('tr').append('<th style="width:30px;"></th>');
            table.children('tbody').children('tr').filter(':odd').hide();
            table.children('tbody').children('tr').filter(':even').click(function () {
                var element = $(this);
                element.next('tr').toggle();
                element.find(".span-left").toggle();
                element.find(".span-down").toggle();
            });
            table.children('tbody').children('tr').filter(':even').each(function () {
                var element = $(this);
                element.append('<td><span class="span-left" data-feather="chevron-left"></span><span class="span-down" data-feather="chevron-down" display="none"></span></td>');
            });
            if (feather)
                feather.replace();

        });
    }

    var flag = true;
    function expandAll() {

        $('.table-expandable').each(function () {
            var table = $(this);
            //table.children('thead').children('tr').append('<th></th>');
            table.children('tbody').children('tr').filter(':odd').hide();
            table.children('tbody').children('tr').filter(':even').each(function () {
                var element = $(this);
                if (flag == true) {
                    element.next('tr').show();
                    element.find(".span-left").hide();
                    element.find(".span-down").show();
                } else {
                    element.next('tr').hide();
                    element.find(".span-left").show();
                    element.find(".span-down").hide();
                }
            });
        });
        flag = !flag;
    }

    function getClientsInfo() {
        $.get(basepath + "GetClientsInfoJson", function (res) {
            var data = res.Data;
            var clientsInfo = $.parseJSON(data);
            var htmlBroken = "<span data-feather='cloud-lightning' color='red'></span> ";
            var htmlGood = "<span data-feather='sun' color='orange'></span> ";
            var html = "";
            for (i in clientsInfo) {
                var co = clientsInfo[i];
                var statusHtml = "";
                statusHtml = (co.blocksCount == "1") ? htmlGood : htmlBroken;
                html += " <tr>" +
                    " <td>" + co.port + "(" + co.protocol + "," + co.host + ")</td >" +
                    "<td>" + co.clientId + "</td>" +
                    "<td>" + co.appId + "</td>" +
                    "<td>" + co.description + "</td>" +
                    "<td>" + statusHtml + "</td>" +
                    "<td>连接数：" + co.revconns.length + "，隧道数：" + co.tunnels.length + "</td>" +
                    "</tr>" +
                    "<tr>" +
                    "<td colspan='6'>";
                for (j in co.tunnels) {
                    var tunnel = co.tunnels[j];
                    if (tunnel.consumerClient == undefined) tunnel.consumerClient = "已断开";
                    if (tunnel.clientServerClient == undefined) tunnel.clientServerClient = "已断开";
                    html += "外网:" + tunnel.consumerClient + "&nbsp;";
                    html += "内网:" + tunnel.clientServerClient;
                    html += "<br />";
                }


                html += "</td>" +
                    "</tr>";
                //alert(clientObj.);
                //  clientObj.Cli
            }
            $("#connections_tb_body").html(html);
            //updatedClientsInfo(clientsInfo);
            initDataTable();
        });
    }

    $(document).ready(function () {

        getClientsInfo();
    });
    $("#btnExpandAll").click(function () { expandAll(); });
})(jQuery); 