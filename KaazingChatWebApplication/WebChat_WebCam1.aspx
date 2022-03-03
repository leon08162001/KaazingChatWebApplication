<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WebChat_WebCam1.aspx.cs" Inherits="KaazingChatWebApplication.WebChat_WebCam1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>WebSocket網頁聊天室</title>
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
    <script src="lib/client/javascript/core-min.js" type="text/javascript"></script>
    <script src="lib/client/javascript/aes.js" type="text/javascript"></script>
    <script src="lib/client/javascript/AesHelper.js" type="text/javascript"></script>
    <script src="lib/client/javascript/MediaStreamRecorder.min.js" type="text/javascript"></script>
    <script src="lib/client/javascript/MessageClient.js" type="text/javascript"></script>
    <script type="text/javascript">
        var clientIp = "<%= ClientIp %>";                                                                           //Client IP
        var MY_WEBSOCKET_URL = "<%= KaazingJmsSvc %>";                                                              //WebSocket Url
        var isSaveVideoStreamToServer = <%= IsSaveVideoStreamToServer.ToString().ToLower() %>;                      //是否將視訊會議串流資料儲存到server
        </script>
    <script src="lib/client/javascript/WebChat1.js" type="text/javascript"></script>
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
            <input type="hidden" name="userID" id="userID" value="<%= EnCryptWebSocketUID %>" />
            <input type="hidden" name="pwd" id="pwd" value="<%= EnCryptWebSocketPWD %>" />
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
                        <label for="chatRecords">chatRecords:</label>
                    </td>
                    <td>
                        <asp:DropDownList ID="chatRecords" runat="server" class="form-control form-control-sm" Height="2.2em" Width="12.3em" Font-Size="Small">
                            <asp:ListItem Selected="True" Value="0">所有紀錄</asp:ListItem>
                            <asp:ListItem Value="1">最近一個月</asp:ListItem>
                            <asp:ListItem Value="2">最近二個月</asp:ListItem>
                            <asp:ListItem Value="3">最近三個月</asp:ListItem>
                            <asp:ListItem Value="6">最近六個月</asp:ListItem>
                            <asp:ListItem Value="12">一年</asp:ListItem>
                            <asp:ListItem Value="24">二年</asp:ListItem>
                        </asp:DropDownList>
                    </td>
                </tr>


                <tr>
                    <td>
                        <label for="dataType">dataType:</label>
                    </td>
                    <td>
                        <asp:DropDownList ID="dataType" runat="server" class="form-control form-control-sm" Height="2.2em" Width="12.3em" Font-Size="Small">
                            <asp:ListItem Selected="True" Value="1">一般文字</asp:ListItem>
                            <asp:ListItem Value="2">JSON文字</asp:ListItem>
                        </asp:DropDownList>
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
        <button id="openMessageClient" class="blue button" type="button" onclick="openMessageClient('聊天', true);">啟動聊天</button>&nbsp;
       
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
    <div id="divToday" class="defaultfont" style="display:none;"><span class="Rounded">今日</span></div>
    <div id="divMsg" class="defaultfont"></div>
    <div id="divMsgHis" class="defaultfont"></div>
</body>
</html>
