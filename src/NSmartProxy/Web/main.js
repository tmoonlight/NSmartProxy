//TODO 调试用
var basepath = "/";//api根地址,这里需要和配置文件一致
//var basepath = "http://localhost:12309/"; //调试时请把地址改成这个

//hashchange事件，路由是如此实现的
(function () {
    if ("onhashchange" in window) { // event supported?
        window.onhashchange = function () {
            hashChanged(window.location.hash);
        };
    } else { // event not supported:
        var storedHash = window.location.hash;
        window.setInterval(function () {
            if (window.location.hash !== storedHash) {
                storedHash = window.location.hash;
                hashChanged(storedHash);
            }
        },
            100);
    }

    function hashChanged(storedHash) {
        storedHash = loadContent(storedHash);
        if (location.pathname.toUpperCase() !== "/LOGIN.HTML") {
            if (getCookie("NSPTK").length < 1) {
                location.href = "/login.html";
            }
        }
    }

    function loadContent(storedHash) {
        if (this.location.href.indexOf("login.html") < 0 && this.location.href.indexOf('#') < 0) {
            this.location.href += "#dashboard";
            return;
        }
        storedHash = "#" + storedHash;
        var compName = storedHash.substring(storedHash.lastIndexOf("#") + 1);
        //发布时注释掉
        $.get(compName + ".html?" + new Date(), function (src) {
            $("#content").html(src);
            $.getScript(compName + ".js");//加载模板
            //加载图标
            if (feather)
                feather.replace();
        });
        $(".active").removeClass("active");
        var hrefTag = "#" + storedHash.substring(storedHash.lastIndexOf("#") + 1);
        $("a[href='" + hrefTag + "']").addClass("active");
        return storedHash;
    }

    $(document).ready(function () {
        loadContent(window.location.hash);
        //选中项
    }
    );
}

)();

function redAlert(msg) {
    $("#red_alert span:first").html(msg);
    $("#red_alert").show();
}
function greenAlert(msg) {
    $("#green_alert span:first").html(msg);
    $("#green_alert").show();
}

///处理nsp发回的统一数据，如果有错则弹框，否则返回真
function processNSPResult(res) {
    return true;
}

function getCookie(cookieName) {
    var strCookie = document.cookie;
    var arrCookie = strCookie.split("; ");
    for (var i = 0; i < arrCookie.length; i++) {
        var arr = arrCookie[i].split("=");
        if (cookieName === arr[0]) {
            return arr[1];
        }
    }
    return "";
}

function delCookie(name) {
    var exp = new Date();
    exp.setTime(exp.getTime() - 1);
    var cval = getCookie(name);
    if (cval !== null) document.cookie = name + "=" + cval + ";expires=" + exp.toGMTString();
}

function signOut() {
    delCookie("NSPTK");
    location.href = "/login.html";
}


