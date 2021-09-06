function confirmDelete(uniqueId, isDeleteClicked) {
    var deleteSpan = "deleteSpan_" + uniqueId;
    var confirmDeleteSpan = "confirmDeleteSpan_" + uniqueId;

    if (isDeleteClicked) {
        $("#" + deleteSpan).hide();
        $("#" + confirmDeleteSpan).show();
    } else {
        $("#" + deleteSpan).show();
        $("#" + confirmDeleteSpan).hide();
    }
}

//function AutoGoHome() {
//    var h2 = $("#AutoGoHome");
//    var timeout = 8;

//    var AutoGoHmoe = setInterval(SetTitle, 1000);

//    function SetTitle() {
//        timeout--;
//        if (timeout < 0) {
//            $("#GoHome")[0].click();
//            window.clearInterval(AutoGoHmoe);
//        }
//        h2[0].innerText = (timeout + "秒后自动返回首页");
//        //h2[0].innerText = (timeout + "秒后自动返回首页");
//    }
//}
