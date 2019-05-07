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
            if (res.startsWith("error:")) { alert("保存失败：" + res); }
            alert('保存成功');
            $("#divAddUser").collapse('hide');
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

    });
}