<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
    <title></title>
</head>
<body>
    <style type="text/css">
        body {
            font-family: Arial;
            font-size: 10pt;
        }

        img {
            height: 100px;
            width: 100px;
            margin: 2px;
        }

        .draggable {
            filter: alpha(opacity=60);
            opacity: 0.6;
        }

        .dropped {
            position: static !important;
        }

        #dvSource {
            border: 5px solid #ccc;
            padding: 5px;
            min-height: 100px;
            width: 430px;
        }

        #dvDest, #dvDest1, #dvDest2, #dvDest3 {
            border: 5px solid #ccc;
            padding: 5px;
            min-height: 100px;
            width: 210px;
        }
        table td {
            vertical-align: top;
        }
    </style>
    <script type="text/javascript" src="https://ajax.googleapis.com/ajax/libs/jquery/1.8.3/jquery.min.js"></script>
    <script src="https://code.jquery.com/ui/1.8.24/jquery-ui.min.js" type="text/javascript"></script>
    <link href="https://code.jquery.com/ui/1.8.24/themes/blitzer/jquery-ui.css" rel="stylesheet" type="text/css" />
    <script type="text/javascript">
        $(function () {
            $("#dvSource img").draggable({
                revert: "invalid",
                refreshPositions: true,
                drag: function (event, ui) {
                    ui.helper.addClass("draggable");
                },
                stop: function (event, ui) {
                    ui.helper.removeClass("draggable");
                    var image = this.src.split("/")[this.src.split("/").length - 1];
                    if ($.ui.ddmanager.drop(ui.helper.data("draggable"), event)) {
                    }
                    else {
                    }
                }
            });
            
            $("#dvDest img").draggable({
                revert: "invalid",
                refreshPositions: true,
                drag: function (event, ui) {
                    ui.helper.addClass("draggable");
                    alert('dvDest drag');
                },
                stop: function (event, ui) {
                    ui.helper.removeClass("draggable");
                    var image = this.src.split("/")[this.src.split("/").length - 1];
                    if ($.ui.ddmanager.drop(ui.helper.data("draggable"), event)) {
                    }
                    else {
                    }
                }
            });
            $("#dvDest1 img").draggable({
                revert: "invalid",
                refreshPositions: true,
                drag: function (event, ui) {
                    ui.helper.addClass("draggable");
                },
                stop: function (event, ui) {
                    ui.helper.removeClass("draggable");
                    var image = this.src.split("/")[this.src.split("/").length - 1];
                    if ($.ui.ddmanager.drop(ui.helper.data("draggable"), event)) {
                    }
                    else {
                    }
                }
            });
            $("#dvDest2 img").draggable({
                revert: "invalid",
                refreshPositions: true,
                drag: function (event, ui) {
                    ui.helper.addClass("draggable");
                },
                stop: function (event, ui) {
                    ui.helper.removeClass("draggable");
                    var image = this.src.split("/")[this.src.split("/").length - 1];
                    if ($.ui.ddmanager.drop(ui.helper.data("draggable"), event)) {
                    }
                    else {
                    }
                }
            });
            $("#dvDest3 img").draggable({
                revert: "invalid",
                refreshPositions: true,
                drag: function (event, ui) {
                    ui.helper.addClass("draggable");
                },
                stop: function (event, ui) {
                    ui.helper.removeClass("draggable");
                    var image = this.src.split("/")[this.src.split("/").length - 1];
                    if ($.ui.ddmanager.drop(ui.helper.data("draggable"), event)) {
                    }
                    else {
                    }
                }
            });
            $("#dvSource").droppable({
                drop: function (event, ui) {
                    if ($("#dvSource img").length == 0) {
                        $("#dvSource").html("");
                    }
                    ui.draggable.addClass("dropped");
                    $("#dvSource").append(ui.draggable);
                }
            });

            $("#dvDest").droppable({
                drop: function (event, ui) {
                    if ($("#dvDest img").length == 0) {
                        $("#dvDest").html("");
                    }
                    ui.draggable.addClass("dropped");
                    $("#dvDest").append(ui.draggable);
                }
            });
            $("#dvDest1").droppable({
                drop: function (event, ui) {
                    if ($("#dvDest1 img").length == 0) {
                        $("#dvDest1").html("");
                    }
                    ui.draggable.addClass("dropped");
                    $("#dvDest1").append(ui.draggable);
                }
            });
            $("#dvDest2").droppable({
                drop: function (event, ui) {
                    if ($("#dvDest2 img").length == 0) {
                        $("#dvDest2").html("");
                    }
                    ui.draggable.addClass("dropped");
                    $("#dvDest2").append(ui.draggable);
                }
            });
            $("#dvDest3").droppable({
                drop: function (event, ui) {
                    if ($("#dvDest3 img").length == 0) {
                        $("#dvDest3").html("");
                    }
                    ui.draggable.addClass("dropped");
                    $("#dvDest3").append(ui.draggable);
                }
            });
        });
    </script>
    <div id="dvSource">
        <img id="img1" alt="" src="images/Chrysanthemum.jpg" />
        <img id="img2" alt="" src="images/Desert.jpg" />
        <img id="img3" alt="" src="images/Hydrangeas.jpg" />
        <img id="img4" alt="" src="images/Jellyfish.jpg" />
        <img id="img5" alt="" src="images/Koala.jpg" />
        <img id="img6" alt="" src="images/Lighthouse.jpg" />
        <img id="img7" alt="" src="images/Penguins.jpg" />
        <img id="img8" alt="" src="images/Tulips.jpg" />
    </div>
    <hr />

    <table>
        <tr>
            <td>
                <div id="dvDest">
                    <!---Drop here-->
                </div>
            </td>
            <td>
                <div id="dvDest1">
                    <!--Drop here-->
                </div>
            </td>
            <td>
                <div id="dvDest2">
                    <!--Drop here-->
                </div>
            </td>
            <td>
                <div id="dvDest3">
                    <!--Drop here-->
                </div>
            </td>
        </tr>
    </table>
</body>
</html>
