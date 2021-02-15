function Encrypt(plainText, keyStr, ivStr) {
    var key = CryptoJS.enc.Utf8.parse(keyStr);
    var iv = CryptoJS.enc.Utf8.parse(ivStr);
    var srcs = CryptoJS.enc.Utf8.parse(word);
    var encrypted = CryptoJS.AES.encrypt(plainText, key, { iv: iv, mode: CryptoJS.mode.CBC });
    return encrypted.toString();
}
function Decrypt(cipherText, keyStr, ivStr) {
    var key = CryptoJS.enc.Utf8.parse(keyStr);
    var iv = CryptoJS.enc.Utf8.parse(ivStr);
    var decrypt = CryptoJS.AES.decrypt(cipherText, key, { iv: iv, mode: CryptoJS.mode.CBC });
    return CryptoJS.enc.Utf8.stringify(decrypt).toString();
}