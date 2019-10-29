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



    function updatedServerStatus(data) {

        myChart2.data.datasets[0].data[0] = data.totalReceivedBytes;
        myChart2.data.datasets[0].data[1] = data.totalSentBytes;
        //myChart.data.datasets[0].data[2] = co;
        myChart2.update();
    }

    function updatedUserStatus(data) {

        myChart3.data.datasets[0].data[0] = data.onlineUsersCount;
        myChart3.data.datasets[0].data[1] = data.offlineUsersCount;
        myChart3.data.datasets[0].data[2] = data.banUsersCount;
        //myChart.data.datasets[0].data[2] = co;
        myChart3.update();
    }

    function getClientsInfo() {
        $.get(basepath + "GetClientsInfoJson", function (res) {
            var data = res.Data;
            var clientsInfo = $.parseJSON(data);

            updatedClientsInfo(clientsInfo);
        });
    }

    function getServerStatus() {
        var apiUrl = basepath + "GetServerStatus";
        $.get(apiUrl,
            function (res) {
                var data = res.Data;
                updatedServerStatus(data);
            }
        );
    }

    function getUserStatus() {
        var apiUrl = basepath + "GetUserStatus";
        $.get(apiUrl,
            function (res) {
                var data = res.Data;
                updatedUserStatus(data);
            }
        );
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
    getServerStatus();
    getUserStatus();

    var myChart2 = new Chart(ctx2,
        {
            type: 'doughnut',
            data: {
                labels: ['输入', '输出'],
                datasets: [
                    {
                        label: '内存占用',
                        data: [0, 0],
                        backgroundColor: [
                            "rgb(255, 99, 132)", "rgb(54, 162, 235)", "rgb(255, 205, 86)"
                        ]
                    }
                ]
            },
            options: {
                title: {
                    display: true,
                    text: '传输'
                }
            }
        });
    var myChart3 = new Chart(ctx3,
        {
            type: 'doughnut',
            data: {
                labels: ['活跃用户', '离线用户', '黑名单用户'],
                datasets: [
                    {
                        label: '活跃用户',
                        data: [0, 0],
                        backgroundColor: [
                            "rgb(75, 192, 192)", "rgb(255, 99, 132)", "rgb(201, 203, 207)"
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
    //red: 'rgb(255, 99, 132)',
    //    orange: 'rgb(255, 159, 64)',
    //    yellow: 'rgb(255, 205, 86)',
    //    green: 'rgb(75, 192, 192)',
    //    blue: 'rgb(54, 162, 235)',
    //    purple: 'rgb(153, 102, 255)',
    //    grey: 'rgb(201, 203, 207)'
    //定时更新数据
    if (window.intevalId) {
        window.clearInterval(window.intevalId);
    }
    window.intevalId = setInterval(function () {

        getClientsInfo();
        getServerStatus();
        getLogFileTable(10);
        getUserStatus();
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


