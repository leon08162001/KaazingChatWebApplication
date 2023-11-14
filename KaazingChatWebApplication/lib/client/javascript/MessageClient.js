var JmsServiceTypeEnum = {
    ActiveMQ: 1,
    TibcoEMS: 2
};
var MessageTypeEnum = {
    Topic: 1,
    Queue: 2
};

function MessageClient() {
    this.uri = "";
    this.userName = "";
    this.passWord = "";
    this.jmsServiceType = "";
    this.messageType = "";
    this.listenName = "";
    this.funcName = "";
    this.sendName = "";
    this.WebUiObject = "";
    this.messageReceivedHandlers = [];
    this.connectionStartedHandlers = [];
    this.connectionFailedHandlers = [];
    this.connectionClosedHandlers = [];
    this.clientIp = "";
    this.isShowMsgWhenOpenAndClose = true;
}

function MessageClient(uri, userName, passWord, jmsServiceType, messageType, listenName, sendName, WebUiObject) {
    this.uri = uri;
    this.userName = userName;
    this.passWord = passWord;
    this.jmsServiceType = jmsServiceType;
    this.messageType = messageType;
    this.listenName = listenName;
    this.funcName = "";
    this.sendName = sendName;
    this.WebUiObject = WebUiObject;
    this.messageReceivedHandlers = [];
    this.connectionStartedHandlers = [];
    this.connectionFailedHandlers = [];
    this.connectionClosedHandlers = [];
    this.clientIp = "";
    this.isShowMsgWhenOpenAndClose = true;
}

MessageClient.prototype = (function () {
    var that;
    var connection;
    var session;
    var topicOrQueueConsumer;
    var topicOrQueueProducer;
    var errLog = "";
    var messageObj;
    var tempArry = [];

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

    var triggerConnectionStarted = function (funcName) {
        var scope = this;
        this.connectionStartedHandlers.forEach(function (item) {
            item.call(scope, funcName);
        });
    };

    var triggerConnectionFailed = function (funcName) {
        var scope = this;
        this.connectionFailedHandlers.forEach(function (item) {
            item.call(scope, funcName);
        });
    };

    var triggerConnectionClosed = function (funcName) {
        var scope = this;
        this.connectionClosedHandlers.forEach(function (item) {
            item.call(scope, funcName);
        });
    };

    var processMessage = function (message) {
        if (message.getJMSType().toString() === "text") {
            if (isJson(message.getText())) {
                var json = eval("(" + message.getText() + ")");
                messageObj = JSON.parse(JSON.stringify(json));
                messageObj.type = "json";
                triggerMessageReceived.call(that, messageObj);
            }
            else {
                messageObj = message.getText();
                triggerMessageReceived.call(that, messageObj);
            }
        }
        else if (message.getJMSType() === "map") {
            if (message instanceof Message) {
                var count = parseInt(message.getStringProperty("N10038"));
                var tempObj = new Object();
                var props = message.getPropertyNames();
                while (props.hasMoreElements()) {
                    var key = props.nextElement();
                    var value = message.getStringProperty(key);
                    if (key === "JMSXDeliveryCount" || key === "ID" || key === "N99" || key === "N10038") {
                        continue;
                    }
                    tempObj[key] = value;
                }
                tempArry.push(tempObj);
                if (count == tempArry.length) {
                    messageObj = toObject(tempArry);
                    tempArry.splice(0, tempArry.length);
                    messageObj.type = "map";
                    messageObj.count = count;
                    triggerMessageReceived.call(that, messageObj);
                }
            }
        }
        else if (message.getJMSType().toString() === "file") {
            var seq, ttlSeq, length, arrayBuffer, uint8Buffer;
            seq = parseInt(message.getStringProperty("sequence"));
            ttlSeq = parseInt(message.getStringProperty("totalSequence"));
            length = message.getBodyLength();
            arrayBuffer = new ArrayBuffer(length);
            uint8Buffer = new Uint8Array(arrayBuffer);
            message.readBytes(uint8Buffer, length);
            if (seq === 1) {
                messageObj = new Object();
                messageObj.id = message.getStringProperty("id");
                messageObj.dataType = message.getStringProperty("datatype");
                messageObj.fileName = message.getStringProperty("filename");
                messageObj.type = "file";
                messageObj.file = arrayBuffer;
            }
            if (seq > 1 && seq <= ttlSeq) {
                messageObj.file = concatBuffers(messageObj.file, arrayBuffer);
            }
            if (seq === ttlSeq) {
                triggerMessageReceived.call(that, messageObj);
            }
        }
        else if (message.getJMSType().toString() === "stream") {
            var seq, ttlSeq, length, arrayBuffer, uint8Buffer;
            seq = parseInt(message.getStringProperty("sequence"));
            ttlSeq = parseInt(message.getStringProperty("totalSequence"));
            length = message.getBodyLength();
            arrayBuffer = new ArrayBuffer(length);
            uint8Buffer = new Uint8Array(arrayBuffer);
            message.readBytes(uint8Buffer, length);
            if (seq === 1) {
                messageObj = new Object();
                messageObj.id = message.getStringProperty("id");
                messageObj.dataType = message.getStringProperty("datatype");
                messageObj.streamName = message.getStringProperty("streamname");
                messageObj.type = "stream";
                messageObj.stream = arrayBuffer;
            }
            if (seq > 1 && seq <= ttlSeq) {
                messageObj.stream = concatBuffers(messageObj.stream, arrayBuffer);
            }
            if (seq === ttlSeq) {
                triggerMessageReceived.call(that, messageObj);
            }
        }
    };

    var handleException = function (e) {
        if (e.type == "ConnectionFailedException"){
            errLog = "離線模式使用中(EXCEPTION: " + e + ")";
            console.error(errLog);
            window.alert(errLog);
        }
        else if (e.type !== "ConnectionDroppedException" && e.type !== "ConnectionRestoredException" && e.type !== "ReconnectFailedException" && e.type !== "IllegalStateException" && e.type !== "JMSException") {
            errLog = "EXCEPTION: " + e;
            console.error(errLog);
            window.alert(errLog);
        }
    };

    return {

        start: function () {
            that = this;
            // Connect to JMS, create a session and start it.
            var browser = new window.browserDetect(window.navigator.userAgent);
            var jmsConnectionFactory = new JmsConnectionFactory(this.uri);
            setupSSO(jmsConnectionFactory.getWebSocketFactory(), this.userName, this.passWord);
            var listenTopicOrQueue;
            var sendTopicOrQueue;
            var jmsServiceType = this.jmsServiceType;
            var messageType = this.messageType;
            var userName = this.listenName;
            var listenName = messageType === 1 ? "/topic/" + this.listenName : "/queue/" + this.listenName;
            var sendName = messageType === 1 ? "/topic/" + this.sendName : "/queue/" + this.sendName;
            var funcName = this.funcName;
            var clientIp = this.clientIp;
            //var macAddr;
            try {
                var connectionFuture = jmsConnectionFactory.createConnection(null, null, null, function () {
                    if (!connectionFuture.exception) {
                        try {
                            connection = connectionFuture.getValue();
                            connection.setExceptionListener(handleException);

                            session = connection.createSession(false, Session.AUTO_ACKNOWLEDGE);
                            // *** Task 3 ***
                            // Creating topic or queue
                            if (messageType === 1) {
                                listenTopicOrQueue = session.createTopic(listenName);
                                sendTopicOrQueue = session.createTopic(sendName);
                            }
                            else {
                                listenTopicOrQueue = session.createQueue(listenName);
                                sendTopicOrQueue = session.createQueue(sendName);
                            }
                            //consoleLog("Topic created...");
                            // *** Task 3 ***                

                            // *** Task 4 ***
                            // Creating topic or queue Consumer
                            if (messageType === 1) {
                                if (jmsServiceType === 1) {
                                    topicOrQueueConsumer = session.createConsumer(listenTopicOrQueue);
                                }
                                else {
                                    //getUserIP(function (ip) { macAddr = ip; });
                                    //var durableName = listenName + "_" + macAddr;
                                    //var durableName = listenName + "_" + clientIp;
                                    //var durableName = listenName + "_" + clientIp + "_" + Date.now();
                                    //var durableName = listenName + "_" + clientIp + "_" + navigator.userAgent;
                                    //var durableName = clientIp + "_" + navigator.userAgent;
                                    //var durableName = userName + "@" + clientIp + "_" + browser.name;
                                    var durableName = userName + "@" + clientIp + "_" + navigator.userAgent;
                                    topicOrQueueConsumer = session.createDurableSubscriber(listenTopicOrQueue, durableName);
                                }
                            }
                            else {
                                topicOrQueueConsumer = session.createConsumer(listenTopicOrQueue);
                            }

                            //consoleLog("Topic consumer created...");
                            // *** Task 4 ***

                            // *** Task 5 ***
                            topicOrQueueConsumer.setMessageListener(processMessage);
                            // *** Task 5 ***

                            // *** Task 6 ***
                            // Creating topic or queue Producer
                            topicOrQueueProducer = session.createProducer(sendTopicOrQueue);
                            // *** Task 6 ***

                            connection.start(function () {
                                // Put any callback logic here.
                                triggerConnectionStarted.call(that, funcName);
                            });
                        } catch (e) {
                            handleException(e);
                            triggerConnectionFailed.call(that, funcName);
                            //triggerConnectionStarted.call(that, e);
                        }
                    } else {
                        if (loginMsg !== "") {
                            handleException(loginMsg);
                            return;
                        }
                        handleException(connectionFuture.exception);
                        triggerConnectionFailed.call(that, funcName);
                        //triggerConnectionStarted.call(that, connectionFuture.exception);
                    }
                });
            } catch (e) {
                handleException(e);
                triggerConnectionFailed.call(that, funcName);
                //triggerConnectionStarted.call(that, e);
            }
        },

        close: function () {
            var funcName = this.funcName;
            try {
                if (topicOrQueueConsumer) {
                    topicOrQueueConsumer.close(null);
                }
                if (topicOrQueueProducer) {
                    topicOrQueueProducer.close();
                }
                connection.close(function () {
                    errLog = "";
                    triggerConnectionClosed.call(that, funcName);
                });
            }
            catch (e) {
                handleException(e);
                //triggerConnectionClosed.call(that, e);
            }
        },
        reStartSender: function () {
            var jmsServiceType = this.jmsServiceType;
            var messageType = this.messageType;
            var sendName = messageType === 1 ? "/topic/" + this.sendName : "/queue/" + this.sendName;
            var sendTopicOrQueue;
            if (messageType === 1) {
                sendTopicOrQueue = session.createTopic(sendName);
            }
            else {
                sendTopicOrQueue = session.createQueue(sendName);
            }
            topicOrQueueProducer = session.createProducer(sendTopicOrQueue);
        },
        reStartListener: function () {
            var jmsServiceType = this.jmsServiceType;
            var messageType = this.messageType;
            var userName = this.listenName;
            var listenName = messageType === 1 ? "/topic/" + this.listenName : "/queue/" + this.listenName;
            var listenTopicOrQueue;
            if (messageType === 1) {
                listenTopicOrQueue = session.createTopic(listenName);
                if (jmsServiceType === 1) {
                    topicOrQueueConsumer = session.createConsumer(listenTopicOrQueue);
                }
                else {
                    var durableName = userName + "@" + clientIp + "_" + navigator.userAgent;
                    topicOrQueueConsumer = session.createDurableSubscriber(listenTopicOrQueue, durableName);
                }
            }
            else {
                listenTopicOrQueue = session.createQueue(listenName);
                topicOrQueueConsumer = session.createConsumer(listenTopicOrQueue);
            }
        },
        getErrorLog: function () {
            return errLog;
        },

        sendMessage: function (message) {
            var messageObj = session.createTextMessage(message);
            var future = topicOrQueueProducer.send(messageObj, function () {
                if (future.exception) {
                    handleException(future.exception);
                }
            });
        },
        sendFile: function (fileName, file, id) {
            var array = [];
            var reader = new FileReader();
            reader.onload = function () {
                var arrayBuffer = this.result;
                array = new Uint8Array(arrayBuffer);
                var messageObj = session.createBytesMessage();
                messageObj.writeBytes(array, 0, file.size);
                messageObj.setStringProperty("id", id);
                messageObj.setStringProperty("filename", fileName);
                messageObj.setStringProperty("datatype", file.type);
                messageObj.setJMSType("file");
                try {
                    var future = topicOrQueueProducer.send(messageObj, function () {
                        if (future.exception) {
                            handleException(future.exception);
                        }
                        else {
                            console.log("file: " + fileName + " has been sended");
                        }
                    });
                }
                catch (e) {
                    handleException(e);
                }
            };
            reader.readAsArrayBuffer(file);
        },
        //setMessage: function (message) {
        //    //triggerMessageReceived(this, message);
        //    triggerMessageReceived.call(this, message)
        //},

        onMessageReceived: function (fn) {
            var chkExistFunc = this.messageReceivedHandlers.filter(
                function (item) {
                    if (item === fn) {
                        return item;
                    }
                }
            );
            if (chkExistFunc.length === 0) {
                this.messageReceivedHandlers.push(fn);
            }
        },

        onConnectionStarted: function (fn) {
            var chkExistFunc = this.connectionStartedHandlers.filter(
                function (item) {
                    if (item === fn) {
                        return item;
                    }
                }
            );
            if (chkExistFunc.length === 0) {
                this.connectionStartedHandlers.push(fn);
            }
        },

        onConnectionFailed: function (fn) {
            var chkExistFunc = this.connectionFailedHandlers.filter(
                function (item) {
                    if (item === fn) {
                        return item;
                    }
                }
            );
            if (chkExistFunc.length === 0) {
                this.connectionFailedHandlers.push(fn);
            }
        },

        onConnectionClosed: function (fn) {
            var chkExistFunc = this.connectionClosedHandlers.filter(
                function (item) {
                    if (item === fn) {
                        return item;
                    }
                }
            );
            if (chkExistFunc.length === 0) {
                this.connectionClosedHandlers.push(fn);
            }
        },

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
    };
    webSocketFactory.setChallengeHandler(basicHandler);
    //ChallengeHandlers.setDefault(basicHandler);
}

function login(callback, userID, Pwd) {
    var credentials = new PasswordAuthentication(userID, Pwd);
    callback(credentials);
}

function _uuid() {
    var d = Date.now();
    if (typeof performance !== 'undefined' && typeof performance.now === 'function') {
        d += performance.now(); //use high-precision timer if available
    }
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = (d + Math.random() * 16) % 16 | 0;
        d = Math.floor(d / 16);
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}
//Get the user IP throught the webkitRTCPeerConnection
function getUserIP(onNewIP) { //  onNewIp - your listener function for new IPs
    //compatibility for firefox and chrome
    var myPeerConnection = window.RTCPeerConnection || window.mozRTCPeerConnection || window.webkitRTCPeerConnection;
    var pc = new myPeerConnection({
        iceServers: []
    }),
        noop = function () { },
        localIPs = {},
        ipRegex = /([0-9]{1,3}(\.[0-9]{1,3}){3}|[a-f0-9]{1,4}(:[a-f0-9]{1,4}){7})/g,
        key;

    function iterateIP(ip) {
        if (!localIPs[ip]) onNewIP(ip);
        localIPs[ip] = true;
    }

    //create a bogus data channel
    pc.createDataChannel("");

    // create offer and set local description
    pc.createOffer(function (sdp) {
        sdp.sdp.split('\n').forEach(function (line) {
            if (line.indexOf('candidate') < 0) return;
            line.match(ipRegex).forEach(iterateIP);
        });

        pc.setLocalDescription(sdp, noop, noop);
    }, noop);

    //listen for candidate events
    pc.onicecandidate = function (ice) {
        if (!ice || !ice.candidate || !ice.candidate.candidate || !ice.candidate.candidate.match(ipRegex)) return;
        ice.candidate.candidate.match(ipRegex).forEach(iterateIP);
    };
}

function concatTypedArrays(a, b) { // a, b TypedArray of same type
    var c = new (a.constructor)(a.length + b.length);
    c.set(a, 0);
    c.set(b, a.length);
    return c;
}
function concatBuffers(a, b) {
    return concatTypedArrays(
        new Uint8Array(a.buffer || a),
        new Uint8Array(b.buffer || b)
    ).buffer;
}

function toObject(arr) {
    var rv = {};
    for (var i = 0; i < arr.length; ++i)
        rv[i] = arr[i];
    return rv;
}
