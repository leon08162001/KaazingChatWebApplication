<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Kaazing1.aspx.cs" Inherits="KaazingTestWebApplication.Kaazing1" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Kaazing Web Socket Test</title>
    <link href="https://afeld.github.io/emoji-css/emoji.css" rel="stylesheet" />
    <link href='https://unpkg.com/emoji.css/dist/emoji.min.css' rel='stylesheet' />
    <script src="lib/client/javascript/jquery-1.9.1.min.js" type="text/javascript"></script>
    <!--<script src="lib/client/javascript/StompJms.js" type="text/javascript"></script>-->
    <script src="lib/client/javascript/WebSocket.js" type="text/javascript"></script>
    <script src="lib/client/javascript/JmsClient.js" type="text/javascript"></script>
    <script src="lib/client/javascript/MessageClient.js" type="text/javascript"></script>
    <script type="text/javascript">
        // Variables you can change
        //
        var MY_WEBSOCKET_URL = "wss://SI1374.asuscomm.com:9001/jms";
        var messageServiceUrl = "https://SI1374.asuscomm.com:1443/KaazingChatWebService/ChatService.asmx/SendMessageToServer";
        var messageType = MessageTypeEnum.Topic;
        var LISTEN_NAME = "DEMO.NUOMS.JefferiesReport.Resp";
        var SEND_NAME = "DEMO.NUOMS.JefferiesReport.Req";
        var IN_DEBUG_MODE = true;
        var DEBUG_TO_SCREEN = true;

        // WebSocket and JMS variables
        //
        var connection;
        var session;
        var wsUrl;

        // JSFiddle-specific variables
        //
        var runningOnJSFiddle = true;
        var WEBSOCKET_URL = (runningOnJSFiddle ? MY_WEBSOCKET_URL : "ws://" + window.location.hostname + ":" + window.location.port + "/jms");

        // Variable for log messages
        //
        var screenMsg = "";

        // Used for development and debugging. All logging can be turned
        // off by modifying this function.
        //
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
                bindMessageToUI(uiObj, message + "</br>")
            }
            else if (Object.prototype.toString.call(message) === '[object Array]') {
                for (var key in message) {
                    handleMessage(uiObj, message[key]);
                }
            }
            else if (typeof message == "object") {
                var sMessage = "";
                for (var field in message) {
                    sMessage += field.toString() + "=" + message[field] + "</br>";
                }
                //window.alert(sMessage + "has be received");
                bindMessageToUI(uiObj, sMessage)
            }
        }

        var handleConnectStarted = function (errMsg)
        {
            if (errMsg == "") {
                $('#openMessageClient').attr('disabled', true);
                $('#closeMessageClient').attr('disabled', false);
                window.alert("Connect to Kaazing WebSocket success!");
                //$("#divMsg").html("Connect to Kaazing WebSocket success!" + "</br>");
            }
            else
            {
                window.alert(errMsg);
                //$("#divMsg").append(errMsg + "</br>");
            }
        }

        var handleConnectClosed = function (errMsg) {
            if (errMsg == "") {
                $("#closeMessageClient").attr('disabled', true);
                $("#openMessageClient").attr('disabled', false);
                window.alert("Close Kaazing WebSocket success!");
                //$("#divMsg").append("Close Kaazing WebSocket success!" + "</br>");
            }
            else {
                window.alert(errMsg);
                //$("#divMsg").append(errMsg + "</br>");
            }
        }

        var bindMessageToUI = function (uiObj, value) {
            uiObj.innerHTML = value + uiObj.innerHTML;
            //uiObj.innerHTML = uiObj.innerHTML=="" ? "1" : parseInt(uiObj.innerHTML) + 1;
        }

        var clickHandler = function (item) {
            log.add("fired: " + item);
        };

        //var messageClient = new MessageClient(MY_WEBSOCKET_URL, "leon", "880816", "DEMO.NUOMS.JefferiesReport.Resp", document.getElementById("divMsg"));
        var messageClient;
        var openMessageClient = function () {
            try
            {
                messageClient = new MessageClient();
                //var messageClient = new MessageClient(MY_WEBSOCKET_URL,document.getElementById("userID").value,document.getElementById("pwd").value,LISTEN_NAME,document.getElementById("divMsg1"));
                messageClient.uri = MY_WEBSOCKET_URL;
                messageClient.userName = $("#userID").val();
                messageClient.passWord = $("#pwd").val();
                messageClient.WebUiObject = $("#divMsg1")[0];
                messageClient.messageType = messageType;
                messageClient.listenName = LISTEN_NAME;
                messageClient.sendName = SEND_NAME;
                messageClient.onMessageReceived(handleMessage);
                messageClient.onConnectionStarted(handleConnectStarted);
                messageClient.onConnectionClosed(handleConnectClosed);
                //var objMessage = [{ partyCode: "A000001", partyName: "leonlee" }, { partyCode: "A000002", partyName: "johnwang" }, "2"];
                //messageClient.setMessage(objMessage);
                messageClient.start();
            }
            catch(e)
            {
                $("#divMsg1").append(e + "</br>");
            }
        };

        var closeMessageClient = function () {
            try{
                messageClient.close();
            }
            catch (e) {
                $("#divMsg").append(e + "</br>");
            }
        }
        var sendMessage = function () {
            messageClient.sendMessage(JSON.stringify($("#message").val()));
        }
        //../KaazingChatWebService/ChatService.asmx/SendMessageToServer
        //https://TWM004.tw-moneysq.local/KaazingChatWebService/ChatService.asmx/SendMessageToServer
        var ajaxProgress = null;
        var sendAjaxTalkMessage = function () {
            $("#sendMessage").attr('disabled', true);
            ajaxProgress = $.ajax({
                url: messageServiceUrl,
                data: "{ 'jsonMessage': " + JSON.stringify($("#message").val()) +
                      ", 'times': '" + $("#times").val() +
                      "', 'topicOrQueueName': '" + SEND_NAME +
                      "', 'messageType': '" + messageClient.messageType + "' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (result) {
                    if (result.d) {
                        //window.alert("send ajax message finish!");
                    }
                    else if (!result.d) {
                        window.alert("send ajax message fail!");
                    }
                    $("#sendMessage").attr('disabled', false);
                    ajaxProgress = null;
                },
                error: function (xhr, textStatus, errorThrown) {
                    var err = JSON.parse(xhr.responseText);
                    $("#sendMessage").attr('disabled', false);
                    window.alert(err.Message);
                }
            });
        }
    </script>
</head>
<body>
    <div id="logMsgs"></div>
    <form id="form1" runat="server">
        <div>
        userID:<input type="text" name="userID" id="userID" value="leon" /></br>
        password:<input type="password" name="pwd" id="pwd" value="880816" /></br>
        message:<textarea id="message" rows="10" cols="80"></textarea></br>
        Times:<input type="text" name="times" id="times" value="1" /></br>
        </div>
    </form>
    <button id="openMessageClient" type="button" onclick="openMessageClient();">open MessageClient</button>
    <button id="closeMessageClient" type="button" disabled="disabled" onclick="closeMessageClient();">close MessageClient</button>
    <button id="sendMessage" type="button" onclick="sendAjaxTalkMessage();">send ajax message</button>
    <button id="sendClientMessage" type="button" onclick="sendMessage();">send client message</button>
    <div id="divMsg"></div>
    <div id="divMsg1"></div>
</body>
</html>
