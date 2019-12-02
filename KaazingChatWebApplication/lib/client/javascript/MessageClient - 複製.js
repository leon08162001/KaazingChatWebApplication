var MessageTypeEnum = {
    Topic: 1,
    Queue: 2
};

function MessageClient(uri, userName, passWord, messageType, listenName, WebUiObject) {
    this.uri = uri;
    this.userName = userName;
    this.passWord = passWord;
    this.messageType = messageType;
    this.listenName = listenName;
    this.WebUiObject = WebUiObject;
    this.messageReceivedHandlers = [];
    this.connectionStartedHandlers = [];
    this.connectionClosedHandlers = [];
}

MessageClient.prototype = (function () {
    var that;
    var connection;
    var session;
    var topicConsumer;
    var errLog = "";

    //var triggerMessageReceived = function (thisObj, msg) {
    //    var scope = thisObj || window;
    //    scope.messageReceivedHandlers.forEach(function (item) {
    //        item.call(scope, scope.WebUiObject, msg);
    //    });
    //};

    var triggerMessageReceived = function (msg) {
        var scope = this;
        this.messageReceivedHandlers.forEach(function (item) {
            item.call(scope, scope.WebUiObject, msg);
        });
    };

    var triggerConnectionStarted = function (errorMsg) {
        var scope = this;
        this.connectionStartedHandlers.forEach(function (item) {
            item.call(scope, errorMsg);
        });
    };

    var triggerConnectionClosed = function (errorMsg) {
        var scope = this;
        this.connectionClosedHandlers.forEach(function (item) {
            item.call(scope, errorMsg);
        });
    };

    var processMessage = function (message) {
        if (isJson(message.getText())) {
            var json = eval("(" + message.getText() + ")");
            var jsonObj = JSON.parse(JSON.stringify(json));
            triggerMessageReceived.call(that, jsonObj);
        }
        else {
            triggerMessageReceived.call(that, message.getText());
        }
    };

    var handleException = function (e) {
        if (e.type != "ConnectionDroppedException" && e.type != "ConnectionRestoredException") {
            errLog = "EXCEPTION: " + e;
            window.alert(errLog);
        }
    };

    return {

        start: function () {
            that = this;
            // Connect to JMS, create a session and start it.
            var jmsConnectionFactory = new JmsConnectionFactory(this.uri);
            setupSSO(jmsConnectionFactory.getWebSocketFactory(), this.userName, this.passWord);
            var listenTopicOrQueue;
            var messageType = this.messageType;
            var listenName = messageType == 1 ? "/topic/" + this.listenName : "/queue/" + this.listenName;
            try {
                var connectionFuture = jmsConnectionFactory.createConnection(this.userName, this.passWord, function () {
                    if (!connectionFuture.exception) {
                        try {
                            connection = connectionFuture.getValue();
                            connection.setExceptionListener(handleException);

                            session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
                            // *** Task 3 ***
                            // Creating topic or queue
                            if (messageType == 1) {
                                listenTopicOrQueue = session.createTopic(listenName);
                            }
                            else {
                                listenTopicOrQueue = session.createQueue(listenName);
                            }
                            //consoleLog("Topic created...");
                            // *** Task 3 ***                

                            // *** Task 4 ***
                            // Creating topic Consumer
                            topicConsumer = session.createConsumer(listenTopicOrQueue);

                            //consoleLog("Topic consumer created...");
                            // *** Task 4 ***

                            // *** Task 5 ***
                            topicConsumer.setMessageListener(processMessage);
                            // *** Task 5 ***
                            connection.start(function () {
                                // Put any callback logic here.
                                triggerConnectionStarted.call(that, "");
                            });
                        } catch (e) {
                            handleException(e);
                            triggerConnectionStarted.call(that, e);
                        }
                    } else {
                        if (loginMsg != "") {
                            handleException(loginMsg);
                            return;
                        }
                        handleException(connectionFuture.exception);
                        triggerConnectionStarted.call(that, connectionFuture.exception);
                    }
                });
            } catch (e) {
                handleException(e);
                triggerConnectionStarted.call(that, e);
            }
        },

        close: function () {
            try {
                if (topicConsumer) {
                    topicConsumer.close(null);
                }
                connection.close(function () {
                    errLog = "";
                    triggerConnectionClosed.call(that, "");
                });
            }
            catch (e) {
                handleException(e);
                triggerConnectionClosed.call(that, e);
            }
        },

        getErrorLog: function () {
            return errLog;
        },

        setMessage: function (message) {
            //triggerMessageReceived(this, message);
            triggerMessageReceived.call(this, message)
        },

        onMessageReceived: function (fn) {
            var chkExistFunc = this.messageReceivedHandlers.filter(
                function (item) {
                    if (item == fn) {
                        return item;
                    }
                }
            );
            if (chkExistFunc.length == 0) {
                this.messageReceivedHandlers.push(fn);
            }
        },

        //offMessageReceived: function (fn) {
        //    this.messageReceivedHandlers = this.messageReceivedHandlers.filter(
        //        function (item) {
        //            if (item !== fn) {
        //                return item;
        //            }
        //        }
        //    );
        //},

        onConnectionStarted: function (fn) {
            var chkExistFunc = this.connectionStartedHandlers.filter(
                function (item) {
                    if (item == fn) {
                        return item;
                    }
                }
            );
            if (chkExistFunc.length == 0) {
                this.connectionStartedHandlers.push(fn);
            }
        },

        //offConnectionStarted: function (fn) {
        //    this.connectionStartedHandlers = this.connectionStartedHandlers.filter(
        //        function (item) {
        //            if (item !== fn) {
        //                return item;
        //            }
        //        }
        //    );
        //},

        onConnectionClosed: function (fn) {
            var chkExistFunc = this.connectionClosedHandlers.filter(
                function (item) {
                    if (item == fn) {
                        return item;
                    }
                }
            );
            if (chkExistFunc.length == 0) {
                this.connectionClosedHandlers.push(fn);
            }
        },

        //offConnectionClosed: function (fn) {
        //    this.connectionClosedHandlers = this.connectionClosedHandlers.filter(
        //        function (item) {
        //            if (item !== fn) {
        //                return item;
        //            }
        //        }
        //    );
        //},

        removeAllEvents: function () {
            this.messageReceivedHandlers.length = 0;
            this.connectionStartedHandlers.length = 0;
            this.connectionClosedHandlers.length = 0;
        }
    };
})();

function isJson(str) {
    try {
        var json = eval("(" + str + ")");
        JSON.parse(JSON.stringify(json));
        return true;
    } catch (err) {
        return false;
    }
}

var maxRetries = 2;
var retry;
var loginMsg;
function setupSSO(webSocketFactory, userID, Pwd) {
    /* Respond to authentication challenges with popup login dialog */
    retry = 0;
    loginMsg = "";
    var basicHandler = new BasicChallengeHandler();
    basicHandler.loginHandler = function (callback) {
        if (retry++ >= maxRetries) {
            callback(null);       // abort authentication process if reaches max retries
            loginMsg = "UserID Or Password used to connect to Kaazing Websocket is incorrect";
            retry = 0;
        }
        else {
            login(callback, userID, Pwd);
        }
    }
    webSocketFactory.setChallengeHandler(basicHandler);
    //ChallengeHandlers.setDefault(basicHandler);
}

function login(callback, userID, Pwd) {
    var credentials = new PasswordAuthentication(userID, Pwd);
    callback(credentials);
}
