(function() {
    var template = "<tr><td><input type='checkbox' value='{cbxval}'></td><td>{ID}</td><td>{Username}</td><td>{RegisterTime}</td><td>{Status}</td></tr>";

    selectUsers();
})();

function addUser() { }
function delUser() {

    greenAlert("删除成功。");
}
function selectUsers() {

    $.get("GetUsers", function(res) {
   
    });
}