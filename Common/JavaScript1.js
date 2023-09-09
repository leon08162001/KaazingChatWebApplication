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
