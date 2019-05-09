//@ sourceURL= user.js
(function () {
    var template = "<tr><td><input type='checkbox' value='{cbxval}'></td><td>{ID}</td><td>{Username}</td><td>{RegisterTime}</td><td>{Status}</td></tr>";

    selectUsers();
})();

function addUser() {
    $("#divAddUser").collapse('show');
}

function addUser_submit() {
    $.get(basepath + "AddUser?userid=" + $("#inputEmail1").val()
        + "&userpwd=" + $("#inputPassword1").val(),
        function (res) {
            if (res.State == 0) { alert("保存失败：" + res.Msg); return; }
            alert('保存成功');
            $("#divAddUser").collapse('hide');
            selectUsers();
        }

    );

}
function delUser() {

    $.get(basepath + "RemoveUser?id=1", function (res) {
        redAlert(res.msg);
    });

}
function selectUsers() {

    $.get(basepath + "GetUsers", function (res) {
        var data = res.Data;
        var htmlStr = "";
        var i = 0;
        for (i in data) {
            var user = jQuery.parseJSON(data[i]);
            htmlStr += "<tr>" +
                "<td> <input type='checkbox' name='cbxUserIds' value='"+ i +"'></td>" +
                "<td>" + i + "</td>" +
                "<td>" + user.userId + "</td>" +
                "<td>" + user.regTime + "</td>" +
                "<td>"+ 1 +"</td>" +
                "</tr>";
            i++;
        }
        $("#user_tb_body").html(htmlStr);
       
    });
}