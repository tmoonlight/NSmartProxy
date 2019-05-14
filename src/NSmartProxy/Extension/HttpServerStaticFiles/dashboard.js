/* globals Chart:false, feather:false */

(function() {
    //'use strict'
    // Graphs
    var ctx1 = document.getElementById('myChart');
    var ctx2 = document.getElementById('myChart2');
    var ctx3 = document.getElementById('myChart3');

    // eslint-disable-next-line no-unused-vars
    //仪表盘
    var myChart = new Chart(ctx1,
        {
            type: 'line',
            data: {
                labels: [
                    '','','','', '', ''],

                animationSteps: 15,
                datasets: [
                    {
                        data: [
                           
                        ],
                        lineTension: 0,
                        backgroundColor: 'transparent',
                        borderColor: '#007bff',
                        borderWidth: 4,
                        pointBackgroundColor: '#007bff',

                    }
                ]
            },
            options: {
                scales: {
                    yAxes: [
                        {
                            ticks: {
                                beginAtZero: false
                            }
                        }
                    ]
                },
                legend: {
                    display: false
                },
                title: {
                    display: true,
                    text: '连接'
                }
            }
        });
    

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
                    text: '内存'
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

    setInterval(function () {


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
        }, 1000
    );
}());
