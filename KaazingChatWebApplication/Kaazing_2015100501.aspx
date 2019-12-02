<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Kaazing_2015100501.aspx.cs" Inherits="KaazingTestWebApplication.Kaazing_2015100501" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title>Kaazing Web Socket Test</title>
    <script src="http://demo.kaazing.com/lib/client/javascript/StompJms.js" type="text/javascript"></script>
    <script src="lib/client/javascript/jquery-1.9.1.min.js" type="text/javascript"></script>
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

        // *** Task 6a ***
        var handleTopicMessage = function (message) {
            // *** Task 8b ***
            document.getElementById("divMsg").innerHTML = message.getText();
            // *** Task 8b ***
        };
        // *** Task 6a ***

        // *** Task 7a ***
        // Send a message to the topic.
        //
        var doSend = function (message, times) {
            //window.alert(typeof(second) == "undefined");
            if (typeof (times) == "undefined") {
                topicProducer.send(null, message, DeliveryMode.NON_PERSISTENT, 3, 1, function () {
                    //yourCallBack();
                });
            }
            else {
                if (!isNaN(parseInt(times))) {
                    var tempTimes = times - 1;
                    if (tempTimes >= 0) {
                        document.getElementById('sendMessage').disabled = true;
                        topicProducer.send(null, message, DeliveryMode.NON_PERSISTENT, 3, 1, function () {
                            doSend(message, tempTimes);
                        });
                    }
                    else {
                        document.getElementById('sendMessage').disabled = false;
                        window.alert("Sending Message is finished");
                    }
                }
                else {
                    consoleLog("Input a number,Please!");
                }
            }
            //consoleLog("Message sent: " + message.getText());
        };
        // *** Task 7a ***

        // Connecting...
        //
        var handleMessage = function (uiObj, message) {
            if (typeof message == "string") {
                //window.alert(message + "\n" + "has be received");
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

        var bindMessageToUI = function (uiObj, value) {
            uiObj.innerHTML += value;
        }

        var doConnect = function () {
            // Connect to JMS, create a session and start it.
            //
            var userID = document.getElementById('userID');
            var pwd = document.getElementById('pwd');
            var stompConnectionFactory = new StompConnectionFactory(WEBSOCKET_URL);

            try {
                var connectionFuture = stompConnectionFactory.createConnection(userID.value, pwd.value, function () {
                    if (!connectionFuture.exception) {
                        try {
                            connection = connectionFuture.getValue();
                            connection.setExceptionListener(handleException);
                            consoleLog("Connected to " + WEBSOCKET_URL);

                            session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);

                            // *** Task 3 ***
                            // Creating topic
                            var myTopic = session.createTopic(LISTEN_TOPIC);
                            consoleLog("Topic created...");
                            // *** Task 3 ***

                            // *** Task 4 ***
                            // Creating topic Producer
                            topicProducer = session.createProducer(myTopic);
                            consoleLog("Topic producer created...");
                            // *** Task 4 ***

                            // *** Task 5 ***
                            // Creating topic Consumer
                            topicConsumer = session.createConsumer(myTopic);
                            consoleLog("Topic consumer created...");
                            // *** Task 5 ***

                            // *** Task 6b ***
                            topicConsumer.setMessageListener(handleTopicMessage);
                            // *** Task 6b ***
                            connection.start(function () {
                                // Put any callback logic here.
                                //
                                consoleLog("JMS session created");
                                // *** Task 7b ***
                                //doSend(session.createTextMessage("Hello world..."));
                                // *** Task 7b ***
                            });
                        } catch (e) {
                            handleException(e);
                        }
                    } else {
                        handleException(connectionFuture.exception);
                    }
                });
            } catch (e) {
                handleException(e);
            }
        };

        var doDisconnect = function () {
            try {
                connection.close(function () { consoleLog("Web Socket Connection closed"); });
            }
            finally {
                connection = null;
            }
        };

        var clickHandler = function (item) {
            log.add("fired: " + item);
        };

        //var messageClient = new MessageClient(MY_WEBSOCKET_URL, "leon", "880816", "DEMO.NUOMS.JefferiesReport.Resp", document.getElementById("divMsg"));
        var messageClient = new MessageClient();

        var openMessageClient = function () {
            messageClient.uri = MY_WEBSOCKET_URL;
            messageClient.userName = "leon";
            messageClient.passWord = "880816";
            messageClient.listenName = "DEMO.NUOMS.JefferiesReport.Resp";
            messageClient.WebUiObject = document.getElementById("divMsg");
            messageClient.onMessageReceived(handleMessage);
            var objMessage = [{ partyCode: "A000001", partyName: "leonlee" }, { partyCode: "A000002", partyName: "johnwang" }, "2"];
            messageClient.setMessage(objMessage);
            messageClient.start();
        };

        var closeMessageClient = function () {
            messageClient.close();
        }

        var sendAjaxMessage = function () {
            $.ajax({
                url: "http://localhost/KaazingWebService/KaazingService.asmx/SendMessage",
                data: "{ 'message': '" + document.getElementById("message").value +
                    "', 'sendTopic': '" + SEND_TOPIC + "' }",
                dataType: "json",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (!data.d){
                        window.alert("send ajax message fail!");
                    }
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
    <button id="connection" type="button" onclick="doConnect();">Connect Web Socket</button>
    <button id="disconnection" type="button" onclick="doDisconnect();">Disconnect Web Socket</button>
    <button id="sendMessage" type="button" onclick="doSend(session.createTextMessage(message.value));">Send a message</button>
    <button id="open MessageClient" type="button" onclick="openMessageClient();">open MessageClient</button>
    <button id="close MessageClient" type="button" onclick="closeMessageClient();">close MessageClient</button>
    <button id="sendMessage" type="button" onclick="sendAjaxMessage();">send ajax message</button>
    <div id="divMsg"></div>
</body>
</html>
