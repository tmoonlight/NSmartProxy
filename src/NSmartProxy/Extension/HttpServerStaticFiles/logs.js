//@ sourceURL= dashboard.js
(function () {
    alert('test');
    getLogFiles();
}());

function getLogFiles() {
    $.get(basepath
        + "GetLogFiles",
        function (res) {
            var data = res.Data;
            var html = "<label>往期日志：</label><br />";
            for (i in data) {
                html += "<button type='button' onclick='getLogFile(\""
                    + data[i] + "\")' class='btn btn-outline-warning mb-2'><span data-feather='file'></span>"
                    + data[i]
                    + "</button> ";
                i++;
            }
            $("#divOldLog").html(html);
        }
    );
};

function getLogFile(filekey) {
    var apiUrl = basepath + "GetLogFile?filekey=" + filekey;
    window.open(apiUrl);
}
