<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Kaazing_2015100601.aspx.cs" Inherits="KaazingTestWebApplication.Kaazing_2015100601" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Kaazing Web Socket Test</title>
    <script src="lib/client/javascript/jquery-1.9.1.min.js" type="text/javascript"></script>
    <script src="lib/client/javascript/StompJms.js" type="text/javascript"></script>
    <script src="lib/client/javascript/MessageClient.js" type="text/javascript"></script>
    <script type="text/javascript">
        // Variables you can change
        //
        var MY_WEBSOCKET_URL = "ws://host.leonlee.com:8001/jms";
        var LISTEN_TOPIC = "/topic/DEMO.NUOMS.JefferiesReport.Resp";
        var SEND_TOPIC = "DEMO.NUOMS.JefferiesReport.Resp";
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
                    //$("#logMsgs").html(screenMsg);
                    document.getElementById('logMsgs').innerHTML = screenMsg;
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
                document.getElementById("openMessageClient").disabled = true;
                document.getElementById("closeMessageClient").disabled = false;
                document.getElementById("divMsg").innerHTML += "Connect to Kaazing WebSocket success!" + "</br>";
            }
            else
            {
                document.getElementById("divMsg").innerHTML += errMsg + "</br>";
            }
        }

        var handleConnectClosed = function (errMsg) {
            if (errMsg == "") {
                document.getElementById("closeMessageClient").disabled = true;
                document.getElementById("openMessageClient").disabled = false;
                document.getElementById("divMsg").innerHTML += "Close Kaazing WebSocket success!" + "</br>";
            }
            else {
                document.getElementById("divMsg").innerHTML += errMsg + "</br>";
            }
        }

        var bindMessageToUI = function (uiObj, value) {
            uiObj.innerHTML += value;
        }

        var clickHandler = function (item) {
            log.add("fired: " + item);
        };

        //var messageClient = new MessageClient(MY_WEBSOCKET_URL, "leon", "880816", "DEMO.NUOMS.JefferiesReport.Resp", document.getElementById("divMsg"));
        var messageClient = new MessageClient();

        var openMessageClient = function () {
            try
            {
                messageClient.uri = MY_WEBSOCKET_URL;
                messageClient.userName = document.getElementById("userID").value;
                messageClient.passWord = document.getElementById("pwd").value;
                messageClient.listenName = "DEMO.NUOMS.JefferiesReport.Resp";
                messageClient.WebUiObject = document.getElementById("divMsg");
                messageClient.onMessageReceived(handleMessage);
                messageClient.onConnectionStarted(handleConnectStarted);
                messageClient.onConnectionClosed(handleConnectClosed);
                //var objMessage = [{ partyCode: "A000001", partyName: "leonlee" }, { partyCode: "A000002", partyName: "johnwang" }, "2"];
                //messageClient.setMessage(objMessage);
                messageClient.start();
            }
            catch(e)
            {
                document.getElementById("divMsg").innerHTML += e + "</br>";
            }
        };

        var closeMessageClient = function () {
            try{
                messageClient.close();
            }
            catch (e) {
                document.getElementById("divMsg").innerHTML += e + "</br>";
            }
        }
        //../KaazingWebService/KaazingService.asmx/SendMessage
        var sendAjaxMessage = function () {
            $.ajax({
                url: "https://host.leonlee.com/KaazingWebService/KaazingService.asmx/SendMessage",
                data: "{ 'message': '" + document.getElementById("message").value +
                      "', 'times': '" + document.getElementById("times").value +
                      "', 'sendTopic': '" + SEND_TOPIC + "' }",
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
                },
                error: function (xhr, textStatus, errorThrown) {
                    var err = JSON.parse(xhr.responseText);
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
        message:<textarea id="message" rows="10" cols="80">How are you?</textarea></br>
        Times:<input type="text" name="times" id="times" value="10" /></br>
        </div>
    </form>
    <button id="openMessageClient" type="button" onclick="openMessageClient();">open MessageClient</button>
    <button id="closeMessageClient" type="button" onclick="closeMessageClient();">close MessageClient</button>
    <button id="sendMessage" type="button" onclick="sendAjaxMessage();">send ajax message</button>
    <div id="divMsg"></div>
</body>
</html>
