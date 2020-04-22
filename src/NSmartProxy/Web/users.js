//@ sourceURL= user.js
(function () {
    var template =
        "<tr><td><input type='checkbox' value='{cbxval}'></td><td>{ID}</td><td>{Username}</td><td>{RegisterTime}</td><td>{Status}</td></tr>";

    selectUsers();
    $(document).ready(function () {
        initValidate();
        //加载json编辑器
    });

})();
var isEdit = "0";
var m_oldUsername = "";
function addUser() {
    $("#inputPassword").val("");
    $("#inputUserName").val("");
    //$("#inputUserName").attr("disabled", "");
    $("#divAddUser").collapse('toggle');
    //$("#btnAddUser").unbind("click").bind("click", function () { addUser_submit(); });
    document.getElementById("btnAddUser").onclick = function () { addUser_submit(); };
    isEdit = "0";
}

function editUser(oldUserName, isAdmin) {
    //TODO 待修改，需要赋值，并且mask密码,取消adduser认证
    $("#inputPassword").val("XXXXXXXX");
    $("#inputUserName").val(oldUserName);
    //$("#inputUserName").attr("disabled", "disabled"); 
    $("#divAddUser").collapse('show');
    document.getElementById("btnAddUser").onclick = function () { editUser_submit(oldUserName); };
    //$("#btnAddUser").unbind("click").bind("click", function () { editUser_submit(oldUserName); });
    $("#cbxIsAdmin").prop("checked", isAdmin == "1");
    isEdit = "1";
    m_oldUsername = oldUserName;
}

function addUser_submit() {
    var validator = $('#divAddUser').data('bootstrapValidator');
    validator.validate();
    //alert(validator.isValid());
    if (validator.isValid()) {
        $.get(basepath +
            "AddUserV2?username=" +
            $("#inputUserName").val() +
            "&userpwd=" +
            $("#inputPassword").val() +
            "&isadmin=" +
            ($("#cbxIsAdmin").prop("checked") ? 1 : 0),
            function (res) {
                if (res.State == 0) {
                    alert("保存失败：" + res.Msg);
                    return;
                }
                alert('保存成功');
                $("#divAddUser").collapse('hide');
                selectUsers();
                $("#inputPassword").val("");
                $("#inputUserName").val("");
            }
        );
    }
}

function editUser_submit(oldUserName) {
    var newUserName = $("#inputUserName").val();
    $.get(basepath +
        "UpdateUser?oldusername=" + oldUserName +
        "&newusername=" +
        newUserName +
        "&userpwd=" +
        $("#inputPassword").val() +
        "&isadmin=" +
        ($("#cbxIsAdmin").prop("checked") ? 1 : 0),
        function (res) {
            if (res.State == 0) {
                alert("编辑失败：" + res.Msg);
                return;
            }
            alert('编辑成功');
            $("#divAddUser").collapse('hide');
            selectUsers();
            $("#inputPassword").val("");
            $("#inputUserName").val("");
        }
    );
    //}
}

function formatConfig() {
    $('#inputConfig').val(formatJson($('#inputConfig').val()));
}

function delUser() {

    var ids = [];
    var userNames = [];
    $('input[name="cbxUserIds"]:checked').each(function () {//遍历每一个名字为interest的复选框，其中选中的执行函数    
        ids.push($(this).val());//将选中的值添加到数组chk_value中    
        userNames.push($(this).closest("tr").find(".td_username").html());
    });
    if (ids.length == 0) return;
    if (!confirm('是否删除')) {
        return;
    }

    $.get(basepath + "RemoveUser?id=" + ids.join(',') + '&usernames=' + userNames.join(','), function (res) {
        if (res.State == 0) {
            alert("操作失败：" + res.Msg);
            return;
        }
        alert('删除成功');
        selectUsers();
    });
}


function delOneUser(userIndex, userName) {
    if (!confirm('是否删除')) {
        return;
    }

    $.get(basepath + "RemoveUser?id=" + userIndex + '&usernames=' + userName, function (res) {
        if (res.State == 0) {
            alert("操作失败：" + res.Msg);
            return;
        }
        alert('删除成功');
        selectUsers();
    });
}
function selectUsers() {

    $.get(basepath + "GetUsers", function (res) {
        var data = res.Data;
        var htmlStr = "";
        var i = 0;
        var htmlIsBanned = "<span data-feather='zap-off' color='red'></span> ";
        var htmlIsConnected = "<span data-feather='activity' color='green'></span> ";
        var htmlIsAdmin = "<span data-feather='star' color='orange'></span> ";
        for (i in data) {
            var user = $.parseJSON(data[i]);
            htmlStr += "<tr>" +
                "<td> <input type='checkbox' style='zoom:150%;' name='cbxUserIds' value='" + i + "'></td>" +
                "<td>" + i + "</td>" +
                "<td>" +
                dropDownButtonHtml(user, i) +
                "</td>" +
                "<td class='td_userid'>" + user.userId + "</td>" +
                "<td class='td_username'>" + user.userName + "</td>" +
                "<td>" + user.regTime + "</td>" +
                "<td>";
            if (user.isAdmin == "1") htmlStr += htmlIsAdmin;
            if (user.isOnline == "true") htmlStr += htmlIsConnected;
            if (user.isBanned == "true") htmlStr += htmlIsBanned;

            htmlStr += "</td>" +
                "<td class='td-ports'>" + user.boundPorts + "</td>" +

                "</tr>";
            //alert(user.isBanned == "true");
            i++;
        }
        $("#user_tb_body").html(htmlStr);
        if (feather)
            feather.replace();

    });
}

function dropDownButtonHtml(user, userIndex) {
    var html = "<div class=\"btn-group\" '>" +
        "<button class=\"btn btn-primary btn-sm dropdown-toggle\" type=\"button\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">" +
        "操作</button>\r\n      <div class=\"dropdown-menu\" x-placement=\"bottom-start\" style=\"position: absolute; will-change: transform; top: 0px; left: 0px; transform: translate3d(0px, 31px, 0px);\">" +
        "<a class=\"dropdown-item\" href=\"javascript:changeBind('" + user.boundPorts + "','" + user.userId + "')\">端口绑定</a>";
    if (user.isBanned == "true") {
        html += "<a class=\"dropdown-item\" href=\"javascript:unBanOneUser('" + user.userId + "')\">恢复断开</a>";
    } else {
        html += "<a class=\"dropdown-item\" href=\"javascript:banOneUser('" + user.userId + "')\">断开用户</a>";
    }
    //user.username user.
    html += "<div class=\"dropdown-divider\"></div>" +
        "<a class=\"dropdown-item\" href=\"javascript:editUser('" + user.userName + "','" + user.isAdmin + "')\">编辑用户</a>" +
        "<a class=\"dropdown-item\" href=\"javascript:delOneUser('" + userIndex + "','" + user.userName + "')\">删除用户</a>";
    html += "<div class=\"dropdown-divider\"></div>" +
        "<a class=\"dropdown-item\" href=\"javascript:expandConfigPanel('" + user.userName + "')\">修改配置</a>" +
        "</div></div>";
    return html;
}

function changeBind(ports, userId) {
    var ports = prompt("请输入即将绑定的端口，逗号分隔", ports);
    if (ports == null) {
        return;
    }
    if (ports.length > 256) {
        alert("输入的信息过多");
        return;
    }
    //BindUserToPort
    $.get(basepath + "BindUserToPort?userId=" + userId + "&ports=" + ports, function (res) {
        // if (res.State == 0) {
        alert(res.Data);
        selectUsers();
        return;
        // }
    });
}


function initValidate() {
    $('#divAddUser').bootstrapValidator({
        feedbackIcons: {
            valid: 'glyphicon glyphicon-ok',
            invalid: 'glyphicon glyphicon-remove',
            validating: 'glyphicon glyphicon-refresh'
        },
        submitButtons: '#btnAddUser',
        fields: {
            inputUserName: {
                validators: {
                    notEmpty: {
                        message: 'The user name is required and cannot be empty.'
                    },
                    regexp: {
                        regexp: /^[^0-9]+/,
                        message: 'The username cannot start with number.'
                    },
                    remote: {
                        delay: 2000,
                        url: basepath + 'ValidateUserName',
                        type: "GET",
                        message: 'This user already exists.',
                        data: function (validator) {
                            return {
                                p1: isEdit,//is edit
                                p2: m_oldUsername//old user
                                //p2: $("#inputUserName").val(),//new user


                            };
                        }
                    }

                }
            },
            inputPassword: {
                validators: {
                    notEmpty: {
                        message: 'The password is required and cannot be empty.'
                    },
                    regexp:
                    {
                        regexp: /^[a-zA-Z0-9_\.]+$/,
                        message: 'The password format is incorrect. Only supports number and english characters.'
                    }
                }
            }
        }
    });
}

function banUsers() {
    var addToBanlist = "0";

    var ids = [];
    // var userNames = [];
    $('input[name="cbxUserIds"]:checked').each(function () {//遍历每一个名字为interest的复选框，其中选中的执行函数    
        ids.push($(this).closest("tr").find(".td_userid").html());
    });

    if (ids.length == 0) return;
    if (confirm('是否同时将这些用户加入黑名单？')) {
        addToBanlist = "1";
    }
    $.get(basepath + "BanUsers?clientIdStr=" + ids.join(',') + "&addToBanlist=" + addToBanlist, function (res) {
        if (res.State == 0) {
            alert("操作失败：" + res.Msg);
            return;
        }
        alert('操作成功');
        selectUsers();
    });
}

function banOneUser(userId) {
    if (userId == "") return;
    var addToBanlist = "0";
    if (confirm('是否同时将用户加入黑名单？')) {
        addToBanlist = "1";
    }

    $.get(basepath + "BanUsers?clientIdStr=" + userId + "&addToBanlist=" + addToBanlist, function (res) {
        if (res.State == 0) {
            alert("操作失败：" + res.Msg);
            return;
        }
        alert('操作成功');
        selectUsers();
    });
}

function unBanUsers() {
    var ids = [];
    // var userNames = [];
    $('input[name="cbxUserIds"]:checked').each(function () {//遍历每一个名字为interest的复选框，其中选中的执行函数    
        ids.push($(this).closest("tr").find(".td_userid").html());
    });

    if (ids.length == 0) return;

    $.get(basepath + "UnBanUsers?clientIdStr=" + ids.join(','), function (res) {
        if (res.State == 0) {
            alert("操作失败：" + res.Msg);
            return;
        }
        alert('操作成功');
        selectUsers();
    });
}

function unBanOneUser(userId) {

    $.get(basepath + "UnBanUsers?clientIdStr=" + userId, function (res) {
        if (res.State == 0) {
            alert("操作失败：" + res.Msg);
            return;
        }
        alert('操作成功');
        selectUsers();
    });
}

function expandConfigPanel(userId) {
    $(hidUserId).val(userId);
    $('#lblConfig').html("<span style='color:#007bff'>" + userId + "</span>/appsettings.json");
    $('#divConfig').collapse('show');
    $('#inputConfig').val("");
    getServerClientConfig(userId);//异步
}

function setClientConfig() {
    var userId = $('#hidUserId').val();
    var config = $("#inputConfig").val();
    var checkResult = isJSON(config);
    if (checkResult!="") {
        alert("JSON格式不正确，请检查后重试。\n详细错误: "+checkResult);
        return;
    }
    $.get(basepath +
        "SetServerClientConfig?userid=" + userId +
        "&config=" +
        config,
        function (res) {
            if (res.State == 0) {
                alert("配置失败：" + res.Msg);
                return;
            }
            alert('配置成功');
            $("#divConfig").collapse('hide');
            selectUsers();
            $("#inputConfig").val("");
            //$("#inputUserName").val("");
        }
    );
}

function getServerClientConfig(userId) {
    $.get(basepath + "GetServerClientConfig?userId=" + userId, function (res) {
        if (res.State == 0) {
            alert("操作失败：" + res.Msg);
            return;
        }
        if (res.Data) {
            $('#inputConfig').val(JSON.stringify(res.Data, null, 2));
        }

        //显示用户配置

    });
}

//格式化json
function formatJson(json, options) {
    var reg = null,
        formatted = '',
        pad = 0,
        PADDING = '    '; // one can also use '\t' or a different number of spaces
    // optional settings
    options = options || {};
    // remove newline where '{' or '[' follows ':'
    options.newlineAfterColonIfBeforeBraceOrBracket = (options.newlineAfterColonIfBeforeBraceOrBracket === true) ? true : false;
    // use a space after a colon
    options.spaceAfterColon = (options.spaceAfterColon === false) ? false : true;

    // begin formatting...

    // make sure we start with the JSON as a string
    if (typeof json !== 'string') {
        json = JSON.stringify(json);
    }
    // parse and stringify in order to remove extra whitespace
    json = JSON.parse(json);
    json = JSON.stringify(json);

    // add newline before and after curly braces
    reg = /([\{\}])/g;
    json = json.replace(reg, '\r\n$1\r\n');

    // add newline before and after square brackets
    reg = /([\[\]])/g;
    json = json.replace(reg, '\r\n$1\r\n');

    // add newline after comma
    reg = /(\,)/g;
    json = json.replace(reg, '$1\r\n');

    // remove multiple newlines
    reg = /(\r\n\r\n)/g;
    json = json.replace(reg, '\r\n');

    // remove newlines before commas
    reg = /\r\n\,/g;
    json = json.replace(reg, ',');

    // optional formatting...
    if (!options.newlineAfterColonIfBeforeBraceOrBracket) {
        reg = /\:\r\n\{/g;
        json = json.replace(reg, ':{');
        reg = /\:\r\n\[/g;
        json = json.replace(reg, ':[');
    }
    if (options.spaceAfterColon) {
        reg = /\:/g;
        json = json.replace(reg, ': ');
    }

    $.each(json.split('\r\n'), function (index, node) {
        var i = 0,
            indent = 0,
            padding = '';

        if (node.match(/\{$/) || node.match(/\[$/)) {
            indent = 1;
        } else if (node.match(/\}/) || node.match(/\]/)) {
            if (pad !== 0) {
                pad -= 1;
            }
        } else {
            indent = 0;
        }

        for (i = 0; i < pad; i++) {
            padding += PADDING;
        }
        formatted += padding + node + '\r\n';
        pad += indent;
    });
    return formatted;
}

function isJSON(str) {
    if (str == "") return "";

    if (typeof str == 'string') {
        try {
            var obj = JSON.parse(str);
            if (typeof obj == 'object' && obj) {
                return "";
            } else {
                return "无法转换";
            }

        } catch (e) {
            console.log('error：' + str + '!!!' + e);
            return e;
        }
    }
}
