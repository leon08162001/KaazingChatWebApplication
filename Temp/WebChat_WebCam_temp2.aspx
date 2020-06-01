<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebChat_WebCam.aspx.cs" Inherits="KaazingChatWebApplication.WebChat_WebCam" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Kaazing Web Socket Test</title>
    <link href="https://afeld.github.io/emoji-css/emoji.css" rel="stylesheet" />
    <link href='https://unpkg.com/emoji.css/dist/emoji.min.css' rel='stylesheet' />
    <link rel="stylesheet" href="css/bootstrap.css" />
    <link rel="stylesheet" href="css/buttons.css" />
    <script src="lib/client/javascript/jquery-1.9.1.min.js" type="text/javascript"></script>
    <script src="lib/client/javascript/moment.min.js" type="text/javascript"></script>
    <script src="lib/client/javascript/browser-detect.umd.js" type="text/javascript"></script>
    <script src="js/bootstrap.min.js" type="text/javascript"></script>
    <!--<script src="lib/client/javascript/StompJms.js" type="text/javascript"></script>-->
    <script src="lib/client/javascript/WebSocket.js" type="text/javascript"></script>
    <script src="lib/client/javascript/JmsClient.js" type="text/javascript"></script>
    <script src="lib/client/javascript/MediaStreamRecorder.min.js" type="text/javascript"></script>
    <script src="lib/client/javascript/MessageClient.js" type="text/javascript"></script>
    <script type="text/javascript">
        // Variables you can change
        var ajaxMessageTypeEnum = {
            read: 1,
            file: 2,
            stream: 3
        };
        //儲存web server變數置網頁
        var clientIp = "<%= ClientIp %>";                                                                           //Client IP
        var MY_WEBSOCKET_URL = "<%= KaazingJmsSvc %>";                                                              //WebSocket Url
        var isSaveVideoStreamToServer = <%= IsSaveVideoStreamToServer.ToString().ToLower() %>;                      //是否將視訊會議串流資料儲存到server

        var ajaxProgress = null;
        var IN_DEBUG_MODE = true;
        var DEBUG_TO_SCREEN = true;
        var runningOnJSFiddle = true;
        var screenMsg = "";
        var readedHtml = "<span class=\"tabbed\">已讀</span>";

        // WebSocket,JMS相關變數
        var messageClient;
        var jmsServiceType = JmsServiceTypeEnum.ActiveMQ;
        var messageType = MessageTypeEnum.Queue;
        var defaultMessageType = messageType;
        var failOverReconnectSecs = 15;

        //視訊,音訊,瀏覽器資訊等相關變數
        var mediaSourceList = [];
        var multiStreamRecorder = null;
        var mediaStream = null;
        var browser = browserDetect();

        //上傳檔案及接收已讀相關變數
        var allReceivedNum;
        var reader = new FileReader();
        var fileName;

        //Web API相關 Url
        //var MY_WEBSOCKET_URL = "wss://192.168.43.114:9001/jms";
        //var messageTalkServiceUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebService/ChatService.asmx/SendTalkMessageToServer";
        //var messageTalkServiceUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebApi/api/WebChat/SendTalkMessageToServer";
        //var messageTalkServiceUrl = "Asmx/ChatService.asmx/SendTalkMessageToServer";
        var messageTalkServiceUrl = "api/WebChat/SendTalkMessageToServer";
        var chkWebSocketLoadBalancerUrl = "api/WebChat/GetWebSocketLoadBalancerUrl";

        //var messageReadServiceUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebService/ChatService.asmx/SendReadMessageToServer";
        //var messageReadServiceUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebApi/api/WebChat/SendReadMessageToServer";
        //var messageReadServiceUrl = "Asmx/ChatService.asmx/SendReadMessageToServer";
        var messageReadServiceUrl = "api/WebChat/SendReadMessageToServer";

        //因android 瀏覽器執行下列上傳檔案asmx會出現error,故改用呼叫ashx方式進行(暫查不出原因,因PC上瀏覽器執行上傳檔案asmx沒有問題)
        //var messageUploadFileUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebService/ChatService1.asmx/UploadFile";

        //WebSocketUploadFile
        //var messageUploadFileUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebService/UploadFile.ashx";
        //MQUploadFile
        //var messageUploadFileUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebService/UploadFile1.ashx";
        //EMSUploadFile
        //var messageUploadFileUrl = "https://leonpc.asuscomm.com/KaazingChatWebService/UploadFile2.ashx";

        //WebSocketUploadFile
        //var messageUploadFileUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebApi/api/WebChat/UploadFile";
        //MQUploadFile
        //var messageUploadFileUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebApi/api/WebChat/UploadFile1";
        //EMSUploadFile
        //var messageUploadFileUrl = "https://leonpc.asuscomm.com:1443/KaazingChatWebApi/api/WebChat/UploadFile2";

        //WebSocketUploadFile
        //var messageUploadFileUrl = "Ashx/UploadFile.ashx";
        //MQUploadFile
        //var messageUploadFileUrl = "Ashx/UploadFile1.ashx";
        //EMSUploadFile
        //var messageUploadFileUrl = "Ashx/UploadFile2.ashx";

        //WebSocketUploadFile
        var messageUploadFileUrl = "api/WebChat/UploadFile";
        //MQUploadFile
        //var messageUploadFileUrl = "api/WebChat/UploadFile1";
        //EMSUploadFile
        //var messageUploadFileUrl = "api/WebChat/UploadFile2";

        //WebSocketUploadStream
        var messageUploadStreamUrl = "api/WebChat/UploadStream";

        // Used for development and debugging. All logging can be turned
        // off by modifying this function.
        //
        if (MY_WEBSOCKET_URL.length == 0) {
            window.alert("WebSocket 服務尚未啟動!");
        }

        setInterval(getWebSocketLoadBalancerUrl, failOverReconnectSecs * 1000);

        var consoleLog = function (text) {
            if (IN_DEBUG_MODE) {
                if (runningOnJSFiddle || DEBUG_TO_SCREEN) {
                    // Logging to the screen
                    screenMsg = screenMsg + text + "<br>";
                    $("#logMsgs").html(screenMsg);
                } else {
                    // Logging to the browser console
                    console.log(text);
                }
            }
        };

        var handleException = function (e) {
            consoleLog("EXCEPTION: " + e);
        };

        var handleMessage = function (uiObj, message) {
            if (typeof message == "string") {
                var hasReadedHtml = message.indexOf(readedHtml) == -1 ? false : true;
                if (hasReadedHtml) {
                    bindMessageToUI(uiObj, "<span style=\"background-color: yellow;\">" + message + "</span><br>");
                }
                else {
                    sendAjaxMessage(message + readedHtml, ajaxMessageTypeEnum.read);
                    bindMessageToUI(uiObj, message + "<br>")
                }
            }
            else if (Object.prototype.toString.call(message) == '[object Array]') {
                for (var key in message) {
                    handleMessage(uiObj, message[key]);
                }
            }
            else if (typeof message == "object") {
                var sMessage = "";
                if (message.hasOwnProperty('file')) {
                    var messageTime = getNowFormatDate();
                    var brTag = document.createElement('br');
                    //playDownloadVideoOrAudioFile(message);
                    var link = createDownloadFileLink(message);
                    var playLink = playLinkForVideoOrAudioFile(message);
                    var spanTag = document.createElement('span');
                    var timeSpanTag = document.createElement('span');
                    spanTag.innerText = message.id + "：";
                    timeSpanTag.innerText = "(" + messageTime + ")";
                    uiObj.insertBefore(brTag, uiObj.firstChild);
                    uiObj.insertBefore(timeSpanTag, uiObj.firstChild);
                    if (playLink != null) {
                        uiObj.insertBefore(playLink, uiObj.firstChild);
                        $("body").on("click", "#" + playLink.id, function () {
                            if (event.target.text.indexOf("視訊") != -1) {
                                var video3 = $("#video3")[0];
                                video3.onended = function () {
                                    this.style.display = 'none';
                                };
                                var audio = $("#audio")[0];
                                audio.pause();
                                audio.src = "";
                                audio.style.display = 'none';
                                video3.src = mediaSourceList.find(x => x.id === event.target.id).url;
                                video3.style.display = 'block';
                                video3.load();
                                video3.play();
                            }
                            else if (event.target.text.indexOf("音訊") != -1) {
                                var audio = $("#audio")[0];
                                audio.onended = function () {
                                    this.style.display = 'none';
                                };
                                var video3 = $("#video3")[0];
                                video3.pause();
                                video3.src = "";
                                video3.style.display = 'none';
                                audio.src = mediaSourceList.find(x => x.id === event.target.id).url;
                                audio.style.display = 'block';
                                audio.load();
                                audio.play();
                            }
                        });
                    }
                    uiObj.insertBefore(link, uiObj.firstChild);
                    uiObj.insertBefore(spanTag, uiObj.firstChild);
                }
                else if (message.hasOwnProperty('stream')) {
                    playStream(message);
                }
                else {
                    for (var field in message) {
                        sMessage += field.toString() + "=" + message[field] + "<br>";
                    }
                    bindMessageToUI(uiObj, sMessage)
                }
            }
        }

        var handleConnectStarted = function (funcName) {
            $('#openMessageClient').attr('disabled', true);
            $('#btnUploadFile').attr('disabled', false);
            $('#closeMessageClient').attr('disabled', false);
            $("#sendMessage").attr('disabled', false);
            if (funcName == "聊天") {
                $('#startLiveVideo').attr('disabled', false);
                $('#closeLiveVideo').attr('disabled', true);
            }
            else if (funcName == "視訊") {
                $('#startLiveVideo').attr('disabled', true);
                $('#closeLiveVideo').attr('disabled', false);
            }
            window.alert(funcName + "已啟動!");
        }

        var handleConnectClosed = function (funcName) {
            $('#btnUploadFile').attr('disabled', true);
            $("#closeMessageClient").attr('disabled', true);
            $("#sendMessage").attr('disabled', true);
            $("#openMessageClient").attr('disabled', false);
            if (funcName == "聊天") {
                $('#startLiveVideo').attr('disabled', true);
                $('#closeLiveVideo').attr('disabled', true);
            }
            else if (funcName == "視訊") {
                $('#startLiveVideo').attr('disabled', false);
                $('#closeLiveVideo').attr('disabled', true);
            }
            window.alert(funcName + "已關閉!");
        }

        var bindMessageToUI = function (uiObj, value) {
            allReceivedNum += 1;
            if (value.toString().indexOf(readedHtml) > 0) {
                if (value.toString().indexOf('id') > -1) {
                    var messageID = $(value).find("span")[0].getAttribute("id");
                    //傳送一筆時(單人及多人適用)
                    if ($("[id='" + messageID + "']").length == 1) {
                        //找不到已讀
                        if ($("#" + messageID).html().indexOf("已讀") == -1) {
                            $("#" + messageID).html($("#" + messageID).html() + readedHtml);
                        }
                        //找得到已讀
                        else {
                            var num = $("#" + messageID).html().match("已讀(.*)</span>")[1];
                            if (!$.isNumeric(num)) {
                                $("#" + messageID).html($("#" + messageID).html().replace("已讀", "已讀2"));
                            }
                            else {
                                var iNum = parseInt(num) + 1;
                                $("#" + messageID).html($("#" + messageID).html().replace("已讀" + num, "已讀" + iNum.toString()));
                            }
                        }
                    }
                    //傳送多筆時(單人及多人適用)
                    else {
                        //找不到已讀
                        if ($("[id='" + messageID + "']").html().indexOf("已讀") == -1) {
                            $("[id='" + messageID + "']").html($("[id='" + messageID + "']").html() + readedHtml);
                        }
                        //找得到已讀
                        else {
                            var times = $.isNumeric($("#times").val()) ? parseInt($("#times").val()) : 0;
                            var num = $("[id='" + messageID + "']").html().match("已讀(.*)</span>")[1];
                            var iNum = parseInt(allReceivedNum / times);
                            if (iNum > 1) {
                                $("[id='" + messageID + "']").html($("[id='" + messageID + "']").html().replace("已讀" + num, "已讀" + iNum.toString()));
                            }
                        }
                    }
                }
            }
            else {
                var helper = document.createElement('div');
                helper.innerHTML = value;
                uiObj.insertBefore(helper, uiObj.firstChild);
            }
        }

        var bindLinkToUI = function (uiObj, link) {
            uiObj.innerHTML = link.outerHTML + "<br>" + uiObj.innerHTML;
        }

        var clickHandler = function (item) {
            log.add("fired: " + item);
        };

        var openMessageClient = function (funcName) {
            try {
                if (!$.trim($("#talkTo").val()) || !$.trim($("#listenFrom").val())) {
                    alert('My Name & TalkTo must key in');
                    return;
                }
                messageClient = new MessageClient();
                messageClient.uri = MY_WEBSOCKET_URL;
                messageClient.clientIp = clientIp;
                messageClient.userName = $("#userID").val();
                messageClient.passWord = $("#pwd").val();
                messageClient.WebUiObject = $("#divMsg")[0];
                messageClient.jmsServiceType = jmsServiceType;
                messageClient.messageType = messageType;
                messageClient.listenName = ("webchat." + $.trim($("#listenFrom").val())).toUpperCase();
                messageClient.funcName = funcName;
                //messageClient.sendName = $.trim($("#talkTo").val()).split(/[^a-zA-Z-]+/g).filter(v => v).join(',').toUpperCase();
                messageClient.sendName = $.trim($("#talkTo").val()).split(/[^a-zA-Z1-9-_]+/g).filter(function (x) { return x }).map(function (y) { return "webchat." + y }).join(',').toUpperCase();
                messageClient.onMessageReceived(handleMessage);
                messageClient.onConnectionStarted(handleConnectStarted);
                messageClient.onConnectionClosed(handleConnectClosed);
                messageClient.start();
                if (event && event.target.id == "openMessageClient") {
                    getChatToday();
                    getChatHistory();
                }
            }
            catch (e) {
                window.alert(e);
                //$("#divMsg1").append(e + "<br>");
            }
        };

        var closeMessageClient = function () {
            try {
                if (event && multiStreamRecorder != null) {
                    alert("視訊開啟中，請先關閉視訊!")
                    return;
                }
                if (messageClient) {
                    messageClient.close();
                    if ($("#divMsg").html().length > 0) {
                        var chat = getChat();
                        chatUpdate(chat, true);
                    }
                }
            }
            catch (e) {
                $("#divMsg").append(e + "<br>");
            }
        }

        var sendMessage = function () {
            if ($.trim($("#message").val()).length == 0) {
                return false;
            }
            var uuid = getUuid();
            var messageTime = getNowFormatDate();
            $("#divMsg").html("<span style=\"background-color: yellow;\"><pre>" + $.trim($("#listenFrom").val()).toUpperCase() + "：" + $("#message").val().replace(/\n/g, '<br>') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span></span><br>" + $("#divMsg").html());
            messageClient.sendMessage(JSON.stringify("<pre>" + $.trim($("#listenFrom").val()).toUpperCase() + "：" + $("#message").val().replace(/\n/g, '<br>') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span>"));
        }
        //../KaazingChatWebService/ChatService.asmx/SendMessageToServer
        //https://leonpc.asuscomm.com:1443/KaazingChatWebService/ChatService.asmx/SendTalkMessageToServer
        
        var chatUpdate = function (chat, isAsync) {
            var chatUpdateServiceUrl = "api/WebChat/ChatUpdate";
            if (isAsync) {
                CallAjax(chatUpdateServiceUrl, chat,
                    function (result) {
                        if (result || result.d) {
                        }
                        else if (!result || !result.d) {
                            console.log("ChatUpdate fail!");
                            window.alert("ChatUpdate fail!");
                        }
                        ajaxProgress = null;
                    },
                    function (xhr, textStatus, errorThrown) {
                        if (xhr.readyState == 0) {
                            console.log(xhr.statusText);
                            window.alert(xhr.statusText);
                        }
                        else {
                            console.log(xhr.responseText);
                            window.alert(xhr.responseText);
                        }
                    });
            }
            else {
                if (navigator.sendBeacon) {
                    chatUpdateServiceUrl = "api/WebChat/ChatUpdateWhenExit";
                    var data = new FormData();
                    data.append('id', chat.id);
                    data.append('name', chat.name);
                    data.append('receiver', chat.receiver);
                    data.append('htmlMessage', chat.htmlMessage);
                    data.append('date', chat.date);
                    data.append('oprTime', chat.oprTime);
                    data.append('oprIpAddress', chat.oprIpAddress);
                    navigator.sendBeacon(chatUpdateServiceUrl, data);
                }
                else {
                    CallSyncAjax(chatUpdateServiceUrl, chat,
                        function (result) {
                            if (result || result.d) {
                            }
                            else if (!result || !result.d) {
                                console.log("ChatUpdate fail!");
                            }
                        },
                        function (xhr, textStatus, errorThrown) {
                            if (xhr.readyState == 0) {
                                console.log(xhr.statusText);
                            }
                            else {
                                console.log(xhr.responseText);
                            }
                        });
                }
            }
        }

        var sendAjaxTalkMessage1 = function () {
            var uuid = getUuid();
            var messageTime = getNowFormatDate();
            var data = {};
            data.message = $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\">" + $("#message").val().replace(/\n/g, '<br>') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span>";
            data.times = Number($("#times").val());
            data.topicOrQueueName = messageClient.sendName;
            data.messageType = Number(messageClient.messageType);
            data.mqUrl = messageClient.uri;
            $("#sendMessage").attr('disabled', true);
            for (var i = 0; i < Number($("#times").val()); i++) {
                $("#divMsg").html("<span style=\"background-color: yellow;\">" + $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\">" + $("#message").val().replace(/\n/g, '<br>') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span></span><br>" + $("#divMsg").html());
            }
            CallAjax(messageTalkServiceUrl, data,
                function (result) {
                    if (result || result.d) {
                        $("#message").val("");
                        //window.alert("send ajax message finish!");
                    }
                    else if (!result || !result.d) {
                        window.alert("send ajax message fail!");
                    }
                    $("#sendMessage").attr('disabled', false);
                    ajaxProgress = null;
                },
                function (xhr, textStatus, errorThrown) {
                    //var err = JSON.parse(xhr.responseText);
                    $("#sendMessage").attr('disabled', false);
                    if (xhr.readyState == 0) {
                        console.log(xhr.statusText);
                        window.alert(xhr.statusText);
                    }
                    else {
                        console.log(xhr.responseText);
                        window.alert(xhr.responseText);
                    }
                    //window.alert(err.Message);
                });
        }

        var sendAjaxTalkMessage = function () {
            allReceivedNum = 0;
            if ($.trim($("#message").val()).length == 0) {
                return false;
            }
            var uuid = getUuid();
            var messageTime = getNowFormatDate();
            var data = {};
            if ($("#message").val().indexOf("https://") == 0 || $("#message").val().indexOf("http://") == 0) {
                data.message = $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\"><a href=\"" + $("#message").val() + "\" target=\"_blank\">" + $("#message").val().replace(/\n/g, '<br>') + "</a></pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span>";
            }
            else if ($("#message").val().indexOf("<a href=https://") == 0 ||
                $("#message").val().indexOf("<a href=http://") == 0 ||
                $("#message").val().indexOf("<a href=\"https://") == 0 ||
                $("#message").val().indexOf("<a href=\"http://") == 0 ||
                $("#message").val().indexOf("<a href='https://") == 0 ||
                $("#message").val().indexOf("<a href='http://") == 0) {
                data.message = $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\">" + $("#message").val().replace('<a href', '<a target=_blank href') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span>";
            }
            else {
                data.message = $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\">" + $("#message").val().replace(/\n/g, '<br>') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span>";
            }
            data.times = Number($("#times").val());
            data.topicOrQueueName = messageClient.sendName;
            data.messageType = Number(messageClient.messageType);
            data.mqUrl = messageClient.uri;
            $("#sendMessage").attr('disabled', true);
            if ($("#message").val().indexOf("https://") == 0 || $("#message").val().indexOf("http://") == 0) {
                for (var i = 0; i < Number($("#times").val()); i++) {
                    $("#divMsg").html("<span style=\"background-color: yellow;\">" + $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\"><a href=\"" + $("#message").val() + "\" target=\"_blank\">" + $("#message").val().replace(/\n/g, '<br>') + "</a></pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span></span><br>" + $("#divMsg").html());
                }
            }
            else if ($("#message").val().indexOf("<a href=https://") == 0 ||
                $("#message").val().indexOf("<a href=http://") == 0 ||
                $("#message").val().indexOf("<a href=\"https://") == 0 ||
                $("#message").val().indexOf("<a href=\"http://") == 0 ||
                $("#message").val().indexOf("<a href='https://") == 0 ||
                $("#message").val().indexOf("<a href='http://") == 0) {
                $("#divMsg").html("<span style=\"background-color: yellow;\">" + $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\">" + $("#message").val().replace('<a href', '<a target=_blank href') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span></span><br>" + $("#divMsg").html());
            }
            else {
                for (var i = 0; i < Number($("#times").val()); i++) {
                    $("#divMsg").html("<span style=\"background-color: yellow;\">" + $.trim($("#listenFrom").val()).toUpperCase() + "：<pre class=\"defaultfont\" style=\"display: inline;\">" + $("#message").val().replace(/\n/g, '<br>') + "</pre><span class=\"tabbed\" id=\"" + uuid + "\">(" + messageTime + ")</span></span><br>" + $("#divMsg").html());
                }
            }
            ajaxProgress = $.ajax({
                url: messageTalkServiceUrl,
                data: JSON.stringify(data),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                    if (result || result.d) {
                        $("#message").val("");
                        var chat = getChat();
                        chatUpdate(chat, true);
                    }
                    else if (!result || !result.d) {
                        console.log("send ajax talk message fail!");
                        window.alert("send ajax talk message fail!");
                    }
                    ajaxProgress = null;
                },
                error: function (xhr, textStatus, errorThrown) {
                    if (xhr.readyState == 0) {
                        console.log(xhr.statusText);
                        window.alert(xhr.statusText);
                    }
                    else {
                        console.log(xhr.responseText);
                        window.alert(xhr.responseText);
                    }
                },
                complete: function (XHR, TS) {
                    $("#sendMessage").attr('disabled', false);
                    XHR = null;
                }
            });
        }

        var getChatToday = function () {
            var serviceUrl = "api/WebChat/GetChatToday";
            var chat = {};
            chat.id = messageClient ? messageClient.listenName.replace(/webchat./ig, "") : "";
            chat.receiver = messageClient ? messageClient.sendName.replace(/webchat./ig, "") : "";
            chat.date = getLocalDate().substring(0, 10);
            $("#divMsg").html("");
            CallAjax(serviceUrl, chat,
                function (data) {
                    if (data || data.d) {
                        $.each(data, function () {
                            $("#divMsg").html($("#divMsg").html() + this.htmlMessage);
                        });
                        $("a").each(function () {
                            var a = $(this)[0];
                            $(this).on('click', function () {
                                setTimeout(function () {
                                    if (a.text.indexOf("(已點擊下載)") == -1) {
                                        if (a.href.indexOf("blob:") != -1 || a.getAttribute("origintext")) {
                                            a.removeAttribute("href");
                                            a.text = a.getAttribute("origintext") + "(已點擊下載)";
                                        }
                                        if ($("#divMsg").html().length > 0) {
                                            chat = getChat();
                                            chatUpdate(chat, true);
                                        }
                                    }
                                }, 150);
                            });
                        });
                    }
                    else if (!data || !data.d) {
                        console.log("getChatToday fail!");
                        window.alert("getChatToday fail!");
                    }
                },
                function (xhr, textStatus, errorThrown) {
                    if (xhr.readyState == 0) {
                        console.log(xhr.statusText);
                        window.alert(xhr.statusText);
                    }
                    else {
                        console.log(xhr.responseText);
                        window.alert(xhr.responseText);
                    }
                });
        }

        var getChatHistory = function () {
            var serviceUrl = "api/WebChat/GetChatHistory";
            var chat = {};
            chat.id = messageClient ? messageClient.listenName.replace(/webchat./ig, "") : "";
            chat.name = messageClient ? messageClient.listenName.replace(/webchat./ig, "") : "";
            chat.receiver = messageClient ? messageClient.sendName.replace(/webchat./ig, "") : "";
            chat.htmlMessage = "";
            chat.date = getLocalDate().substring(0, 10);
            chat.oprTime = getLocalDate();
            chat.oprIpAddress = messageClient.clientIp;
            $("#divMsgHis").html("");
            CallAjax(serviceUrl, chat,
                function (data) {
                    if (data || data.d) {
                        $.each(data, function () {
                            $("#divMsgHis").html($("#divMsgHis").html() + "<span class=\"Rounded\">" + this.date.substring(0, 10) + "</span>");
                            $("#divMsgHis").html($("#divMsgHis").html() + this.htmlMessage);
                        });
                    }
                    else if (!data || !data.d) {
                        console.log("getChatHistory fail!");
                        window.alert("getChatHistory fail!");
                    }
                },
                function (xhr, textStatus, errorThrown) {
                    if (xhr.readyState == 0) {
                        console.log(xhr.statusText);
                        window.alert(xhr.statusText);
                    }
                    else {
                        console.log(xhr.responseText);
                        window.alert(xhr.responseText);
                    }
                });
        }

        var sendAjaxMessage = function (message, ajaxMessageType) {
            var data = {};
            data.message = message;
            //data.topicOrQueueName = messageClient.sendName;
            if (ajaxMessageType == ajaxMessageTypeEnum.read) {
                data.topicOrQueueName = messageClient.sendName.indexOf(",") > -1 ? ("webchat." + message.substr(0, message.indexOf("："))).toUpperCase() : messageClient.sendName;
            }
            else {
                data.topicOrQueueName = $.trim($("#talkTo").val()).split(/[^a-zA-Z1-9-_]+/g).filter(function (x) { return x }).map(function (y) { return "webchat." + y }).join(',').toUpperCase();
            }
            data.messageType = Number(messageClient.messageType);
            data.mqUrl = messageClient.uri;
            ajaxProgress = $.ajax({
                url: messageReadServiceUrl,
                data: JSON.stringify(data),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                error: function (xhr, textStatus, errorThrown) {
                    if (xhr.readyState == 0) {
                        console.log(xhr.statusText);
                        window.alert(xhr.statusText);
                    }
                    else {
                        var err = JSON.parse(xhr.responseText);
                        console.log(err.Message);
                        window.alert(err.Message);
                    }
                },
                complete: function (XHR, TS) {
                    XHR = null;
                }
            });
        }

        var b64toBlob = function (b64Data, contentType, sliceSize) {
            contentType !== undefined ? contentType : '';
            sliceSize !== undefined ? sliceSize : 512;
            sliceSize = 512;
            var byteCharacters = atob(b64Data);
            var byteArrays = [];

            for (var offset = 0; offset < byteCharacters.length; offset += sliceSize) {
                var slice = byteCharacters.slice(offset, offset + sliceSize);

                var byteNumbers = new Array(slice.length);
                for (var i = 0; i < slice.length; i++) {
                    byteNumbers[i] = slice.charCodeAt(i);
                }

                var byteArray = new Uint8Array(byteNumbers);
                byteArrays.push(byteArray);
            }
            var blob = new Blob(byteArrays, { type: contentType });
            return blob;
        };

        var getWebSocketLoadBalancerUrlOld = function () {
            var ajaxProgress = $.ajax({
                type: "POST",
                url: chkWebSocketLoadBalancerUrl,
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                    if (MY_WEBSOCKET_URL != result && MY_WEBSOCKET_URL.length > 0) {
                        MY_WEBSOCKET_URL = result;
                        $('#closeMessageClient').click();
                        $('#openMessageClient').click();
                    }
                    ajaxProgress = null;
                    if (MY_WEBSOCKET_URL.length == 0) {
                        console.log("WebSocket 服務尚未啟動!");
                        window.alert("WebSocket 服務尚未啟動!");
                    }
                },
                complete: function (XHR, TS) {
                    XHR = null;
                }
            });
        };

        var getWebSocketLoadBalancerUrl = function () {
            var ajaxProgress = $.ajax({
                type: "POST",
                url: chkWebSocketLoadBalancerUrl,
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                    if ((result.length > 0 && MY_WEBSOCKET_URL.length > 0 && result.indexOf(MY_WEBSOCKET_URL) == -1) || (result.length > 0 && MY_WEBSOCKET_URL.length == 0)) {
                        MY_WEBSOCKET_URL = result[0];
                        //$('#closeMessageClient').click();
                        $('#openMessageClient').click();
                    }
                    else if (result.length == 0) {
                        MY_WEBSOCKET_URL = "";
                    }
                    if (MY_WEBSOCKET_URL.length == 0) {
                        $('#btnUploadFile').attr('disabled', true);
                        $("#closeMessageClient").attr('disabled', true);
                        $("#sendMessage").attr('disabled', true);
                        $("#openMessageClient").attr('disabled', false);
                        console.log("WebSocket 服務尚未啟動!");
                        window.alert("WebSocket 服務尚未啟動!");
                    }
                    ajaxProgress = null;
                },
                complete: function (XHR, TS) {
                    XHR = null;
                }
            });
        };

        function sleep(milliseconds) {
            var start = new Date().getTime();
            for (var i = 0; i < 1e7; i++) {
                if ((new Date().getTime() - start) > milliseconds) {
                    break;
                }
            }
        }

        function getNowFormatDate() {
            var date = new Date();
            var seperator1 = "-";
            var seperator2 = ":";
            var month = date.getMonth() + 1;
            var strDate = date.getDate();
            var strHour = date.getHours();
            var strMinute = date.getMinutes();
            var strSecond = date.getSeconds();
            if (month >= 1 && month <= 9) {
                month = "0" + month;
            }
            if (strDate >= 0 && strDate <= 9) {
                strDate = "0" + strDate;
            }
            if (strHour >= 0 && strHour <= 9) {
                strHour = "0" + strHour;
            }
            if (strMinute >= 0 && strMinute <= 9) {
                strMinute = "0" + strMinute;
            }
            if (strSecond >= 0 && strSecond <= 9) {
                strSecond = "0" + strSecond;
            }
            var currentdate = month + seperator1 + strDate
                + " " + strHour + seperator2 + strMinute
                + seperator2 + strSecond;
            return currentdate;
        }

        function getUuid() {
            var d = Date.now();
            if (typeof performance !== 'undefined' && typeof performance.now == 'function') {
                d += performance.now(); //use high-precision timer if available
            }
            return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = (d + Math.random() * 16) % 16 | 0;
                d = Math.floor(d / 16);
                return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
            });
        }

        function createDownloadFileLink(obj) {
            var a = document.createElement('a');
            a.id = getUuid();
            a.text = obj.fileName;
            a.download = obj.fileName;
            a.setAttribute("origintext", a.text);
            var blob = new Blob([obj.file], { type: obj.dataType });
            //for IE
            if (window.navigator.msSaveOrOpenBlob) {
                a.href = "#";
                a.addEventListener('click', function () {
                    setTimeout(function () {
                        if (a.text.indexOf("(已點擊下載)") == -1) {
                            if (a.href.indexOf("blob:") != -1 || a.getAttribute("origintext")) {
                                window.navigator.msSaveOrOpenBlob(blob, obj.fileName);
                                a.removeAttribute("href");
                                a.text = a.getAttribute("origintext") + "(已點擊下載)";
                            }
                            if ($("#divMsg").html().length > 0) {
                                var chat = getChat();
                                chatUpdate(chat, true);
                            }
                        }
                    }, 150);
                });
            }
            else {
                var blobUrl = URL.createObjectURL(blob);
                a.href = blobUrl;
                a.addEventListener('click', function () {
                    setTimeout(function () {
                        if (a.href.indexOf("blob:") != -1 || a.getAttribute("origintext")) {
                            URL.revokeObjectURL(a.href);
                            a.removeAttribute("href");
                            a.text = a.getAttribute("origintext") + "(已點擊下載)";
                        }
                        if ($("#divMsg").html().length > 0) {
                            var chat = getChat();
                            chatUpdate(chat, true);
                        }
                    }, 150);
                });
            }
            return a;
        }

        function playDownloadVideoOrAudioFile(obj) {
            if (obj.dataType.toUpperCase().indexOf('MP4') != -1 || obj.dataType.toUpperCase().indexOf('OGG') != -1 || obj.dataType.toUpperCase().indexOf('WEBM') != -1) {
                var blob = new Blob([obj.file], { type: obj.dataType });
                var blobUrl = URL.createObjectURL(blob);
                var video3 = $("#video3")[0];
                video3.onended = function () {
                    this.style.display = 'none';
                };
                video3.src = blobUrl;
                video3.style.display = 'block';
                video3.load();
                video3.play();
            }
            else if (obj.dataType.toUpperCase().indexOf('MPEG') != -1 || obj.dataType.toUpperCase().indexOf('WAV') != -1) {
                var blob = new Blob([obj.file], { type: obj.dataType });
                var blobUrl = URL.createObjectURL(blob);
                var audio = $("#audio")[0];
                audio.src = blobUrl;
                audio.load();
                audio.play();
            }
        }
        
        function playLinkForVideoOrAudioFile(obj) {
            if (obj.dataType.toUpperCase().indexOf('MP4') != -1 || obj.dataType.toUpperCase().indexOf('OGG') != -1 ||
                obj.dataType.toUpperCase().indexOf('WEBM') != -1 || obj.dataType.toUpperCase().indexOf('MPEG') != -1 ||
                obj.dataType.toUpperCase().indexOf('WAV') != -1) {
                var blob = new Blob([obj.file], { type: obj.dataType });
                var blobUrl = URL.createObjectURL(blob);
                var a = document.createElement('a');
                a.id = getUuid();
                a.setAttribute("origintext", a.text);
                a.href = "#";
                a.blobUrl = blobUrl;
                var mediaSource = { "id": a.id, "url": blobUrl };
                mediaSourceList.push(mediaSource);
                if (obj.dataType.toUpperCase().indexOf('MP4') != -1 || obj.dataType.toUpperCase().indexOf('OGG') != -1 || obj.dataType.toUpperCase().indexOf('WEBM') != -1) {
                    a.text = "(播放視訊)"
                    //a.addEventListener('click', function () {
                    //    var video3 = $("#video3")[0];
                    //    video3.onended = function () {
                    //        this.style.display = 'none';
                    //    };
                    //    var audio = $("#audio")[0];
                    //    audio.pause();
                    //    audio.src = "";
                    //    audio.style.display = 'none';
                    //    video3.src = blobUrl;
                    //    video3.style.display = 'block';
                    //    video3.load();
                    //    video3.play();
                    //});
                }
                else if (obj.dataType.toUpperCase().indexOf('MPEG') != -1 || obj.dataType.toUpperCase().indexOf('WAV') != -1) {
                    a.text = obj.fileName.toUpperCase().indexOf('MP3') != -1 ||
                        obj.fileName.toUpperCase().indexOf('WAV') != -1 ? "(播放音訊)" : "";
                    //a.addEventListener('click', function () {
                    //    var audio = $("#audio")[0];
                    //    audio.onended = function () {
                    //        this.style.display = 'none';
                    //    };
                    //    var video3 = $("#video3")[0];
                    //    video3.pause();
                    //    video3.src = "";
                    //    video3.style.display = 'none';
                    //    audio.src = blobUrl;
                    //    audio.style.display = 'block';
                    //    audio.load();
                    //    audio.play();
                    //});
                }
                return a;
            }
            else {
                return null;
            }
        }

        function playStream(obj) {
            if ($('#startLiveVideo').prop('disabled') && (obj.dataType.toUpperCase().indexOf('WEBM') != -1 || obj.dataType.toUpperCase().indexOf('MP4') != -1)) {
                var blob = new Blob([obj.stream], { type: obj.dataType });
                var blobUrl = URL.createObjectURL(blob);
                var video2 = $("#video2")[0];
                video2.width = 1280;
                video2.height = 720;
                video2.src = blobUrl;
                video2.style.display = 'block';
                video2.controls = false;
                video2.load();
                video2.play();
            }
        }

        function resetFileUploadText() {
            $('[id*=fileUpload]').next(".custom-file-label").attr('data-content', "未選擇任何檔案");
            $('[id*=fileUpload]').next(".custom-file-label").text("Choose files");
        }

        function downloadFileLinkBase64(obj) {
            var blob = b64toBlob(obj.file, obj.dataType);
            var blobUrl = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.href = blobUrl;
            a.download = obj.fileName;
            a.click();
            window.URL.revokeObjectURL(blobUrl);
        }

        function createDownloadFileLinkBase64(obj) {
            var blob = b64toBlob(obj.file, obj.dataType);
            var blobUrl = URL.createObjectURL(blob);
            var a = document.createElement('a');
            a.id = getUuid();
            a.href = blobUrl;
            a.text = obj.fileName;
            a.setAttribute("origintext", a.text);
            a.download = obj.fileName;
            a.addEventListener('click', function () {
                setTimeout(function () {
                    URL.revokeObjectURL(a.href); a.removeAttribute("href"); a.text = a.getAttribute("origintext") + "(已點擊下載)";
                }, 150);
            });
            return a;
        }

        function CallAjax(url, data, okFunc, failFunc) {
            ajaxProgress = $.ajax({
                url: url,
                data: JSON.stringify(data),
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: okFunc,
                error: failFunc,
                complete: function (XHR, TS) {
                    XHR = null;
                }
            });
        }

        function CallSyncAjax(url, data, okFunc, failFunc) {
            ajaxProgress = $.ajax({
                url: url,
                data: JSON.stringify(data),
                dataType: "json",
                type: "POST",
                async: false,
                contentType: "application/json; charset=utf-8",
                success: okFunc,
                error: failFunc,
                complete: function (XHR, TS) {
                    XHR = null;
                }
            });
        }

        function getLocalDate() {
            var date = new Date();
            var localDateString = new Date(date.getTime() - (date.getTimezoneOffset() * 60000)).toJSON();
            return localDateString;
        }

        function getChat() {
            var chat = {};
            chat.id = messageClient.listenName.replace(/webchat./ig, "");
            chat.name = messageClient.listenName.replace(/webchat./ig, "");
            chat.receiver = messageClient.sendName.replace(/webchat./ig, "");
            chat.htmlMessage = $("#divMsg").html();
            chat.date = getLocalDate().substring(0, 10);
            chat.oprTime = getLocalDate();
            chat.oprIpAddress = messageClient.clientIp;
            return chat;
        }

        function startLiveVideo() {
            navigator.getUserMedia = navigator.getUserMedia || navigator.webkitGetUserMedia || navigator.mozGetUserMedia;
            if (navigator.getUserMedia) {
                navigator.getUserMedia({ audio: true, video: { width: 1280, height: 720 } },
                    function (stream) {
                        mediaStream = stream;
                        $('#startLiveVideo').attr('disabled', true);
                        $('#closeLiveVideo').attr('disabled', false);
                        mediaStream.stop = function () {
                            this.getAudioTracks().forEach(function (track) {
                                track.stop();
                            });
                            this.getVideoTracks().forEach(function (track) { //in case... :)
                                track.stop();
                            });
                        };
                        var video1 = document.querySelector('#video1');
                        try {
                            video1.srcObject = stream;
                        } catch (error) {
                            video1.src = window.URL.createObjectURL(stream);
                        }
                        video1.onloadedmetadata = function (e) {
                            //if (multiStreamRecorder && multiStreamRecorder.stream) return;
                            //if (browser.name != 'firefox') {
                            //    multiStreamRecorder = new MultiStreamRecorder([stream]);
                            //    multiStreamRecorder.mimeType = 'video/webm';
                            //    multiStreamRecorder.stream = stream;
                            //}
                            //else {
                            //    multiStreamRecorder = new MediaStreamRecorder(stream);
                            //    multiStreamRecorder.mimeType = 'video/webm;codecs=vp9';
                            //    multiStreamRecorder.stream = stream;
                            //}
                            multiStreamRecorder = new MediaStreamRecorder(stream);
                            multiStreamRecorder.mimeType = 'video/webm';
                            multiStreamRecorder.stream = stream;
                            multiStreamRecorder.ondataavailable = function (blob) {
                                //using ajax send media stream
                                var videoStreamName = "video_" + messageClient.listenName.replace(/webchat./ig, "") + "_" + messageClient.sendName.replace(/webchat./ig, "") + "_" + moment().format("YYYYMMDDhhmmss") + ".webm";
                                var data = new FormData();
                                data.append("sender", messageClient.listenName.replace(/webchat./ig, ""));
                                data.append("topicOrQueueName", messageClient.sendName);
                                data.append("messageType", messageClient.messageType.toString());
                                data.append("mqUrl", messageClient.uri);
                                data.append("mimetype", multiStreamRecorder.mimeType);
                                data.append("stream", blob);
                                if (isSaveVideoStreamToServer) {
                                    data.append("videoname", videoStreamName);
                                }
                                var messageTime = getNowFormatDate();
                                //$("#divMsg").html("<span style=\"background-color: yellow;\">" + messageClient.listenName.replace(/webchat./ig, "") + "：傳送串流中，請稍後...(" + messageTime + ")</span><br>" + $("#divMsg").html());
                                //sendAjaxMessage(messageClient.listenName.replace(/webchat./ig, "") + "：傳送串流中，請稍後...(" + messageTime + ")", ajaxMessageTypeEnum.stream);
                                setTimeout(function () {
                                    var ajaxProgress = $.ajax({
                                        type: "POST",
                                        url: messageUploadStreamUrl,
                                        data: data,
                                        contentType: false,
                                        processData: false,
                                        success: function () {
                                        },
                                        error: function (jqXHR, textStatus, errorThrown) {
                                            messageTime = getNowFormatDate();
                                            var uiObj = $("#divMsg")[0];
                                            var brTag = document.createElement('br');
                                            var spanTag = document.createElement('span');
                                            spanTag.setAttribute("style", "background-color:yellow");
                                            spanTag.innerHTML = messageClient.listenName.replace(/webchat./ig, "") + "：串流傳送失敗:" + textStatus + "(" + messageTime + "):" + jqXHR.responseText;
                                            uiObj.insertBefore(brTag, uiObj.firstChild);
                                            uiObj.insertBefore(spanTag, uiObj.firstChild);
                                            sendAjaxMessage(messageClient.listenName.replace(/webchat./ig, "") + "：串流傳送失敗:" + textStatus + "(" + messageTime + "):" + jqXHR.responseText, ajaxMessageTypeEnum.file);
                                            //alert('串流傳送失敗');
                                        },
                                        complete: function (XHR, TS) {
                                            XHR = null;
                                        }
                                    });
                                }, 0);
                            };

                            //get blob after specific time interval
                            multiStreamRecorder.start(32000);
                            video1.width = 1280;
                            video1.height = 720;
                            video1.style.display = 'block';
                            video1.play();
                        };
                        closeMessageClient();
                        messageType = MessageTypeEnum.Topic;
                        openMessageClient("視訊");
                    },
                    function (err) {
                        console.log("The following error occurred: " + err.message);
                        window.alert("The following error occurred: " + err.message);
                    }
                );
            }
            else {
                console.log("getUserMedia not supported");
            }
        }

        function closeLiveVideo() {
            var video1 = document.querySelector('#video1');
            var video2 = document.querySelector('#video2');
            video1.style.display = 'none';
            video2.style.display = 'none';
            $('#startLiveVideo').attr('disabled', false);
            $('#closeLiveVideo').attr('disabled', true);
            multiStreamRecorder.stop();
            multiStreamRecorder.stream.stop();
            mediaStream.stop();
            multiStreamRecorder = null;
            mediaStream = null;
        }

        $(document).ready(function () {
            var video2 = document.querySelector('#video2');

            video2.onended = (event) => {
                video2.pause();
            };

            $(window).on("beforeunload", function () {
                if ($("#divMsg").html().length > 0) {
                    var chat = getChat();
                    chatUpdate(chat, false);
                    return null;
                }
            });

            $('[id*=fileUpload]').change(function () {
                var fieldVal = $(this).val();

                // Change the node's value by removing the fake path (Chrome)
                fieldVal = fieldVal.replace("C:\\fakepath\\", "");

                if (fieldVal != "") {
                    $(this).next(".custom-file-label").attr('data-content', fieldVal);
                    $(this).next(".custom-file-label").text(fieldVal);
                }
                else {
                    $(this).next(".custom-file-label").text("Choose files");
                }

                if (typeof (FileReader) != "undefined") {
                    var files = $("#fileUpload")[0].files;
                    if (files[0] != null) {
                        fileName = files[0].name;
                        reader.readAsDataURL(files[0]);
                    }
                } else {
                    alert("This browser does not support HTML5 FileReader.");
                }
            });

            $("#message").on('keypress', function (e) {
                if ((e.keyCode == 10 || e.keyCode == 13) && e.ctrlKey) {
                    $("#sendMessage").click();
                    //$("#message").val($("#message").val() + "\n");
                }
                else if ((e.keyCode == 10 || e.keyCode == 13) && !e.ctrlKey) {
                    //$("#sendMessage").click();
                }
            });

            $('#talkTo').change(function () {
                if (messageClient) {
                    var chat = getChat();
                    chatUpdate(chat, true);
                    $("#divMsg").html("");
                    //messageClient.sendName = $.trim($(this).val()).split(/[^a-zA-Z-]+/g).filter(function (v) {return v }).join(',').toUpperCase();
                    messageClient.sendName = $.trim($(this).val()).split(/[^a-zA-Z1-9-_]+/g).filter(function (x) { return x }).map(function (y) { return "webchat." + y }).join(',').toUpperCase();
                    getChatToday();
                    getChatHistory();
                }
            });

            $('#btnUploadFile').on('click', function () {
                var data = new FormData();
                var files = $("#fileUpload")[0].files;
                var fileNames = "";
                if (!messageClient) {
                    window.alert("聊天尚未啟動");
                    return;
                }
                if (files.length == 0) {
                    window.alert("尚未指定傳送的檔案");
                    return;
                }

                //javascript端傳送多個檔案檔案程式碼(因目前以迴圈方式傳送多個檔案會造成底層JmsClient.js內部錯誤故註解)
                //for (var i = 0; i < files.length; i++) {
                //    messageClient.sendFile(files[i].name, files[i], messageClient.listenName);
                //}

                data.append("sender", messageClient.listenName.replace(/webchat./ig, ""));
                data.append("topicOrQueueName", messageClient.sendName);
                data.append("messageType", messageClient.messageType.toString());
                data.append("mqUrl", messageClient.uri);
                for (var i = 0; i < files.length; i++) {
                    data.append("files", files[i]);
                    fileNames += files[i].name + "，";
                }
                if (fileNames.length > 0) {
                    fileNames = fileNames.substring(0, fileNames.length - 1);
                }
                // Make Ajax request with the contentType = false, and procesDate = false
                var messageTime = getNowFormatDate();
                $("#divMsg").html("<span style=\"background-color: yellow;\">" + messageClient.listenName.replace(/webchat./ig, "") + "：傳送檔案中，請稍後...(" + messageTime + ")</span><br>" + $("#divMsg").html());

                $("#fileUpload").attr('disabled', true);
                $('#btnUploadFile').attr('disabled', true);
                sendAjaxMessage(messageClient.listenName.replace(/webchat./ig, "") + "：傳送檔案中，請稍後...(" + messageTime + ")", ajaxMessageTypeEnum.file);

                setTimeout(function () {
                    var ajaxProgress = $.ajax({
                        type: "POST",
                        url: messageUploadFileUrl,
                        data: data,
                        contentType: false,
                        processData: false,
                        success: function () {
                            messageTime = getNowFormatDate();
                            resetFileUploadText();
                            var uiObj = $("#divMsg")[0];
                            var brTag = document.createElement('br');
                            var spanTag = document.createElement('span');
                            spanTag.setAttribute("style", "background-color:yellow");
                            spanTag.innerHTML = messageClient.listenName.replace(/webchat./ig, "") + "：" + fileNames + "(檔案傳送完成)(" + messageTime + ")";
                            uiObj.insertBefore(brTag, uiObj.firstChild);
                            uiObj.insertBefore(spanTag, uiObj.firstChild);
                            $("#fileUpload").val('');
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            messageTime = getNowFormatDate();
                            var uiObj = $("#divMsg")[0];
                            var brTag = document.createElement('br');
                            var spanTag = document.createElement('span');
                            spanTag.setAttribute("style", "background-color:yellow");
                            spanTag.innerHTML = messageClient.listenName.replace(/webchat./ig, "") + "：檔案傳送失敗(" + messageTime + "):" + jqXHR.responseText;
                            uiObj.insertBefore(brTag, uiObj.firstChild);
                            uiObj.insertBefore(spanTag, uiObj.firstChild);
                            sendAjaxMessage(messageClient.listenName.replace(/webchat./ig, "") + "：檔案傳送失敗(" + messageTime + "):" + jqXHR.responseText, ajaxMessageTypeEnum.file);
                            //alert('檔案傳送失敗');
                        },
                        complete: function (XHR, TS) {
                            $("#fileUpload").attr('disabled', false);
                            $('#btnUploadFile').attr('disabled', false);
                            XHR = null;
                        }
                    });
                }, 1000);
            });

            $('#startLiveVideo').bind("click", function () {
                if (!$.trim($("#talkTo").val()) || !$.trim($("#listenFrom").val())) {
                    alert('My Name & TalkTo must key in');
                    return;
                }
                startLiveVideo();
            });

            $('#closeLiveVideo').bind("click", function () {
                if (!$.trim($("#talkTo").val()) || !$.trim($("#listenFrom").val())) {
                    alert('My Name & TalkTo must key in');
                    return;
                }
                closeLiveVideo();
                closeMessageClient();
                messageType = defaultMessageType;
                openMessageClient("聊天");
            });
        });
        </script>
    <style>
        body {
            padding: 5px;
        }

        @media (max-width: 980px) {
            body {
                padding: 5px;
            }
        }

        .tabbed {
            padding-left: 4.00em;
        }

        .defaultfont {
            font-size: medium;
            font-family: 標楷體, TimesNewRoman, "Times New Roman", Times, Arial, Georgia;
        }

        .Rounded {
            -moz-border-radius: 10px 10px 10px 10px;
            border-radius: 10px 10px 10px 10px;
            border: solid 1px #000;
            background-color: #acf;
            padding: 2px;
            text-align: center;
            display: block;
        }
    </style>
</head>
<body>
    <div id="logMsgs"></div>
    <form id="form1" runat="server">
        <div class="form-group">
            <input type="hidden" name="userID" id="userID" value="leon" />
            <input type="hidden" name="pwd" id="pwd" value="880816" />
            <table style="width: 100%">
                <tr>
                    <td style="width: 5%">
                        <label for="listenFrom" class="text-nowrap">My Name:</label>
                    </td>
                    <td style="width: 95%">
                        <input type="text" name="listenFrom" id="listenFrom" class="form-control" style="width: 10em; height: 1.5em" value="" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="talkTo">TalkTo:</label>
                    </td>
                    <td>
                        <input type="text" name="talkTo" id="talkTo" class="form-control" style="width: 10em; height: 1.5em" value="" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="message">Message:</label>
                    </td>
                    <td>
                        <textarea id="message" class="form-control" style='line-height: 1.5em; font-family: 標楷體, TimesNewRoman, "Times New Roman", Times, Arial, Georgia;' rows="6" placeholder="Write something here..."></textarea>
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="times">Times:</label>
                    </td>
                    <td>
                        <input type="number" name="times" id="times" class="form-control" style="width: 5em; height: 1.7em" value="1" />
                    </td>
                </tr>
                <tr>
                    <td>
                        <label for="message">Files:</label>
                    </td>
                    <td>
                        <div class="input-group">
                            <div class="custom-file">
                                <input type="file" class="custom-file-input" id="fileUpload"
                                    aria-describedby="inputGroupFileAddon01" style="width: 100%" multiple />
                                <label class="custom-file-label" for="inputGroupFile01">Choose files</label>
                            </div>
                            <button id="btnUploadFile" type="button" disabled="disabled" class="btn btn-primary">傳檔</button>
                        </div>
                    </td>
                </tr>
            </table>
        </div>
    </form>
    <div style="text-align: center">
        <button id="openMessageClient" class="blue button" type="button" onclick="openMessageClient('聊天');">啟動聊天</button>&nbsp;
       
        <button id="closeMessageClient" class="blue button" type="button" disabled="disabled" onclick="closeMessageClient();">結束聊天</button>&nbsp;
       
        <button id="sendMessage" class="blue button" type="button" disabled="disabled" onclick="sendAjaxTalkMessage();">傳送訊息</button>&nbsp;
       
        <button id="startLiveVideo" class="blue button" type="button" disabled="disabled">開啟即時視訊</button>&nbsp;
       
        <button id="closeLiveVideo" class="blue button" type="button" disabled="disabled">關閉即時視訊</button>&nbsp;
       
        <%--        <button id="sendClientMessage" class="blue button" type="button" disabled="disabled" onclick="sendMessage();">傳送訊息(javascript)</button>--%>
    </div>
    <br />
    <div id="mediaZone" style="display: inline">
        <video id="video1" style="display: none; margin: auto; position: relative; top: 0px; left: 0px; bottom: 0px; right: 0px; max-width: 100%; max-height: 100%;" autoplay="">
            您的瀏覽器不支援<code>video</code>標籤!
       
        </video>
        <video id="video2" style="display: none; margin: auto; position: relative; top: 0px; left: 0px; bottom: 0px; right: 0px; max-width: 100%; max-height: 100%;" autoplay="">
            您的瀏覽器不支援<code>video</code>標籤!
       
        </video>
        <video id="video3" style="display: none; margin: auto; position: relative; top: 0px; left: 0px; bottom: 0px; right: 0px; max-width: 100%; max-height: 100%;" autoplay="" controls="controls">
            您的瀏覽器不支援<code>video</code>標籤!
       
        </video>
        <audio id="audio" style="display: none;" controls="controls">您的瀏覽器不支援audio標籤!</audio>
    </div>
    <div id="divMsg" class="defaultfont"></div>
    <div id="divMsgHis" class="defaultfont"></div>
</body>
</html>
