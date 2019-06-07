//@ sourceURL= user.js
(function () {
    var template =
        "<tr><td><input type='checkbox' value='{cbxval}'></td><td>{ID}</td><td>{Username}</td><td>{RegisterTime}</td><td>{Status}</td></tr>";

    selectUsers();
    $(document).ready(function () {
        initValidate();
    });

})();

function addUser() {
    $("#inputPassword").val("");
    $("#inputUserName").val("");
    $("#divAddUser").collapse('show');
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

function delUser() {
    if (!confirm('是否删除')) {
        return;
    }
    var ids = [];
    var userNames = [];
    $('input[name="cbxUserIds"]:checked').each(function () {//遍历每一个名字为interest的复选框，其中选中的执行函数    
        ids.push($(this).val());//将选中的值添加到数组chk_value中    
        userNames.push($(this).closest("tr").find(".td_username").html());
    });

    $.get(basepath + "RemoveUser?id=" + ids.join(',') + '&usernames=' + userNames.join(','), function (res) {
        alert('删除成功');
        selectUsers();
    });

}
function selectUsers() {

    $.get(basepath + "GetUsers", function (res) {
        var data = res.Data;
        var htmlStr = "";
        var i = 0;
        for (i in data) {
            var user = $.parseJSON(data[i]);
            htmlStr += "<tr>" +
                "<td> <input type='checkbox' name='cbxUserIds' value='" + i + "'></td>" +
                "<td>" + i + "</td>" +
                "<td>" + user.userId + "</td>" +
                "<td class='td_username'>" + user.userName + "</td>" +
                "<td>" + user.regTime + "</td>" +
                "<td>" + 1 + "</td>" +
                "<td class='td-ports'>" + user.boundPorts + "</td>" +
                "<td>" +
                dropDownButtonHtml(user.boundPorts, user.userId) +
                //"<button type=\"button\" onclick='changeBind(" + user.userId + ")' class=\"btn btn-primary btn-sm\">绑定</button>" +
                //"&nbsp;<button type=\"button\" onclick='changeBind(" + user.userId + ")' class=\"btn btn-primary btn-sm\">断开</button>" +
                "</td>" +
                "</tr>";
            i++;
        }
        $("#user_tb_body").html(htmlStr);

    });
}

function dropDownButtonHtml(ports, userId) {
    return "<div class=\"btn-group\">" +
        "<button class=\"btn btn-primary btn-sm dropdown-toggle\" type=\"button\" data-toggle=\"dropdown\" aria-haspopup=\"true\" aria-expanded=\"false\">" +
        "操作</button>\r\n      <div class=\"dropdown-menu\" x-placement=\"bottom-start\" style=\"position: absolute; will-change: transform; top: 0px; left: 0px; transform: translate3d(0px, 31px, 0px);\">" +
        "<a class=\"dropdown-item\" href=\"javascript:changeBind('" + ports + "','" + userId + "')\">端口绑定</a>" +
        "<a class=\"dropdown-item\" href=\"#\">断开用户</a>" +
        "<div class=\"dropdown-divider\"></div>" +
        "<a class=\"dropdown-item\" href=\"#\">编辑用户</a>" +
        "<a class=\"dropdown-item\" href=\"#\">删除用户</a>" +
        "</div></div>";
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
                        message: 'This user already exists.'
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
                        message: 'The password format is incorrect.'
                    }
                }
            }
        }
    });
}