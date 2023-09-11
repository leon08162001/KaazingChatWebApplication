"  if (this.status == 200) {" +
    "      var blob = this.response;" +
    "      var pos = 0;" +
    "      var chunkCount = 0;" +
    "      var chunkSize = (1 << 16);" +
    "      for (pos = 0; pos < blob.size; pos += chunkSize) {" +
    "          (function(pos, blob) {" +
    "          var fr = new FileReader();" +
    "          fr.onload = function () {" +
    "              chunkCount += 1;" +
    "              const base64data = fr.result.split(',')[1];" +
    "              alert(base64data);" +
    "              if (((chunkCount - 1) * chunkSize) < blob.size) {" +
    "                  CSharp.GetBase64FromBlobData(base64data, datatype, filename, chunkCount, 'N');" +
    "              }" +
    "              else {" +
    "                  CSharp.GetBase64FromBlobData(base64data, datatype, filename, chunkCount, 'Y');" +
    "              }" +
    "          };" +
    "          fr.readAsDataURL(blob.slice(pos, pos + chunkSize));" +
    "          })(pos, blob);" +
    "      }" +
    "  }" +
    "  else if (xhr.status !== 200) {" +
    "      alert(xhr.responseText);" +
    "  }" +


    "javascript: function sliceBlobToBase64(blob, pos, chunkCount) {" +
    "                var fr = new FileReader();" +
    "                fr.onload = function () {" +
    "                   const base64data = fr.result.split(',')[1];" +
    "                   if (((chunkCount - 1) * chunkSize) < blob.size) {" +
    "                       CSharp.GetBase64FromBlobData(base64data, datatype, filename, chunkCount, 'N');" +
    "                   }" +
    "                   else {" +
    "                       CSharp.GetBase64FromBlobData(base64data, datatype, filename, chunkCount, 'Y'); " +
    "                   }" +
    "                };" +
    "                fr.readAsDataURL(blob.slice(pos, pos + chunkSize));" +
    "            };" +

    "            var datatype = document.querySelector('a[href=\"" + blobUrl + "\"]').getAttribute('datatype');" +
    "            var filename = document.querySelector('a[href=\"" + blobUrl + "\"]').getAttribute('origintext');" +
    "            var xhr = new XMLHttpRequest();" +
    "            xhr.open('GET', '" + blobUrl + "', true);" +
    "            xhr.setRequestHeader('Content-type', datatype);" +
    "            xhr.responseType = 'blob';" +
    "            xhr.onload = function(e) {" +
    "                           if (this.status == 200) {" +
    "                             var blob = this.response;" +
    "                             var pos = 0;" +
    "                             var chunkCount = 0;" +
    "                             var chunkSize = (1 << 16);" +
    "                             for (pos = 0; pos < blob.size; pos += chunkSize) {" +
    "                               chunkCount += 1;" +
    "                               sliceBlobToBase64(blob, pos, chunkCount);" +
    "                             }" +
    "                           }" +
    "                           else if (xhr.status !== 200) {" +
    "                             alert(xhr.responseText);" +
    "                           }" +
    "                         };" +
    "            xhr.send(null);"
