//@ sourceURL= dashboard.js
/* globals Chart:false, feather:false */

//var clientsInfo;



(function () {

    function updatedClientsInfo(data) {
        var disco = 0;
        var semico = 0;
        var co = 0;

        for (var i = 0; i < data.length; i++) {
            for (var j = 0; j < data[i].revconns.length; j++) {
                var constate = 0;
                if (data[i].revconns[j].hasOwnProperty("lEndPoint")) {
                    constate += 0.5;
                }
                if (data[i].revconns[j].hasOwnProperty("rEndPoint")) {
                    constate += 0.5;
                }
                if (constate == 0) disco++;
                if (constate == 0.5) semico++;
                if (constate == 1) co++;
            }
        }
        myChart.data.datasets[0].data[0] = disco;
        myChart.data.datasets[0].data[1] = semico;
        myChart.data.datasets[0].data[2] = co;
        myChart.update();

    }

    function getClientsInfo() {
        $.get(basepath + "GetClientsInfoJson", function (res) {
            var data = res.Data;
            var clientsInfo = $.parseJSON(data);

            updatedClientsInfo(clientsInfo);
        });
    }



    //'use strict'
    // Graphs
    var ctx1 = document.getElementById('myChart');
    var ctx2 = document.getElementById('myChart2');
    var ctx3 = document.getElementById('myChart3');

    // eslint-disable-next-line no-unused-vars
    //仪表盘
    var myChart = new Chart(ctx1,
        {
            type: 'doughnut',
            data: {
                labels: ['已关闭', '半连接', '全连接'],
                datasets: [
                    {
                        label: '内存占用',
                        data: [0, 0, 0],
                        backgroundColor: [
                            "rgb(255, 99, 132)", "rgb(54, 162, 235)", "rgb(255, 205, 86)"
                        ]
                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: '连接'
                }
            }
        });
    getLogFileTable(10);
    getClientsInfo();

    var myChart2 = new Chart(ctx2,
        {
            type: 'doughnut',
            data: {
                labels: ['NSmart内存', '其他应用内存'],
                datasets: [
                    {
                        label: '内存占用',
                        data: [12, 19],
                        backgroundColor: [
                            "rgb(255, 99, 132)", "rgb(54, 162, 235)", "rgb(255, 205, 86)"
                        ]
                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: '连接历史'
                }
            }
        });
    var myChart3 = new Chart(ctx3,
        {
            type: 'doughnut',
            data: {
                labels: ['活跃用户', '离线用户'/*, 'Yellow', 'Green', 'Purple', 'Orange'*/],
                datasets: [
                    {
                        label: '活跃用户',
                        data: [12, 19],
                        backgroundColor: [
                            "rgb(255, 99, 132)", "rgb(54, 162, 235)", "rgb(255, 205, 86)"
                        ]
                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: '在线用户'
                }
            }

        });

    $("#myChart").click(
        function (evt) {
            var url = "连接管理";
            alert(url);
        }
    );

    $("#myChart2").click(
        function (evt) {
            var url = " ";
            alert(url);
        }
    );

    $("#myChart3").click(
        function (evt) {
            var url = "用户管理";
            alert(url);
        }
    );
    //定时更新数据
    if (window.intevalId) {
        window.clearInterval(window.intevalId);
    }
    window.intevalId = setInterval(function () {

        getClientsInfo();
        getLogFileTable(10);
        //myChart2.data.datasets.pop();
        //更新数据
        myChart2.data.datasets[0].data[1] += 3;
        //myChart2.data.datasets[1] = 10;
        //myChart2.data.datasets.push({
        //label: label,
        // backgroundColor: color,
        //  data: [12, 19]
        //});
        myChart2.update();
    }, 5000
    );
}());

function getLogFileTable(lines) {
    var apiUrl = basepath + "GetLogFileInfo?lastLines=" + lines;
    $.get(apiUrl,
        function (res) {
            var data = res.Data;
            var logText = "";
            for (i in data) {
                logText += " <tr><td>" + i + "</td> <td>" + data[i] + "</td></tr> ";
            }
            $("#tbodyLogs").html(logText);
        }
    );
}
