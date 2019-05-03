(function() {
    var template = "<tr><td><input type='checkbox' value='{cbxval}'></td><td>{ID}</td><td>{Username}</td><td>{RegisterTime}</td><td>{Status}</td></tr>";

    selectUsers();
})();

function addUser() { }
function delUser() {

    $.get(basepath + "RemoveUser?id=1", function (res) {
        redAlert(res.msg);
    });
   
}
function selectUsers() {

    $.get(basepath+"GetUsers", function(res) {
   
    });
}