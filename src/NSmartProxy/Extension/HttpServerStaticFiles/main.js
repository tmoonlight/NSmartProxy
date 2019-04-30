var basepath = "";

//hashchange事件，路由是如此实现的
(function () {
    if ("onhashchange" in window) { // event supported?
        window.onhashchange = function () {
            hashChanged(window.location.hash);
        }
    } else { // event not supported:
        var storedHash = window.location.hash;
        window.setInterval(function () {
            if (window.location.hash != storedHash) {
                storedHash = window.location.hash;
                hashChanged(storedHash);
            }
        },
            100);
    }

    function hashChanged(storedHash) {
        storedHash = loadContent(storedHash);

    }

    function loadContent(storedHash) {
        if (this.location.href.indexOf('#') < 0) {this.location.href += "#dashboard";
            return;
        }
        storedHash = "#" + storedHash;
        var compName = storedHash.substring(storedHash.lastIndexOf("#") + 1);
        //发布时注释掉
        $.get(basepath + compName + ".html?"+new Date(), function (src) {
            $("#content").html(src);
            $.getScript(compName + ".js");
        });
        return storedHash;
    }

    $(document).ready(function () {
            loadContent(window.location.hash);
        }
    );
}

)();



