//@ sourceURL= config.js
function getConfig(key, func) {
    $.get(basepath + "GetConfig?key=" + key, function (res) {

        $("#config" + key).prop("checked", res.Data == "1");

    });
}

$(document).ready(function () {
    getConfig("AllowAnonymousUser");
    $("#frmCAConfig").attr("action", basepath + "/UploadTempFile");
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

//文件上传
function fileSelected() {
    var file = document.getElementById('fileToUpload').files[0];
    if (file) {
        var fileSize = 0;
        if (file.size > 1024 * 1024)
            fileSize = (Math.round(file.size * 100 / (1024 * 1024)) / 100).toString() + 'MB';
        else
            fileSize = (Math.round(file.size * 100 / 1024) / 100).toString() + 'KB';
        //document.getElementById('fileName').innerHTML = 'Name: ' + file.name;
        document.getElementById('fileSize').innerHTML = 'Size: ' + fileSize;
        //document.getElementById('fileType').innerHTML = 'Type: ' + file.type;
    }
    uploadFile();

}
function uploadFile() {
    var fd = new FormData();
    fd.append("fileToUpload", document.getElementById('fileToUpload').files[0]);
    var xhr = new XMLHttpRequest();
    xhr.upload.addEventListener("progress", uploadProgress, false);
    xhr.addEventListener("load", uploadComplete, false);
    xhr.addEventListener("error", uploadFailed, false);
    xhr.addEventListener("abort", uploadCanceled, false);
    xhr.open("POST", basepath + "/UploadTempFile");//修改成自己的接口 xhr.send(fd);
    xhr.send(fd);
    $("#divLoading").show();
}
function uploadProgress(evt) {
    if (evt.lengthComputable) {
        var percentComplete = Math.round(evt.loaded * 100 / evt.total);
        document.getElementById('progressNumber').innerHTML = percentComplete.toString() + '%';
    }
    else {
        document.getElementById('progressNumber').innerHTML = 'unable to compute';
    }
}
function uploadComplete(evt) {
    /* 服务器端返回响应时候触发event事件*/
    var result = JSON.parse(evt.target.responseText);
    if (result.State == 1) {
        //显示文件名
        $("#fileInfo").show();
        $("#fileName").html("<a href=#>文件已上传</a>");
        $("#fileBottoms").hide();
        $("#divLoading").hide();
        $("#fileToUpload").val("");
        $("#fileSize").html("");
        $("#progressNumber").html("");
        //$("#fileName").html(result.data);
    }
}
function uploadFailed(evt) {
    alert("There was an error attempting to upload the file.");
}
function uploadCanceled(evt) {
    alert("The upload has been canceled by the user or the browser dropped the connection.");
}

function delCABound() {
    $("#fileToUpload").val("");
    $("#fileInfo").hide();
    $("#fileBottoms").show();
    $("#divLoading").hide();
}

function addCABound(port, filename) {

}

function genCA() {
    var hosts = prompt("请输入绑定域名，多个域名请用逗号分隔。\n(eg.: *.tmoonlight.com,cloud.kd.com)");
    if (hosts && hosts.trim() != "") {
        $("#divLoading").show();
        $.get(basepath + "GenerateCA?hosts=" + hosts,
            function (res) {
                $("#divLoading").hide();
                $("#fileInfo").show();
                $("#fileBottoms").hide();
                $("#fileName").html("<a href=#>证书已生成</a>");
                $("#hidCAFilename").val(res.data);
            });
    }
}