/**
 * Copyright (c) 2007-2012, Kaazing Corporation. All rights reserved.
 */

var browser=null;
if (typeof (ActiveXObject) != "undefined") {
    if ((navigator.userAgent.indexOf("MSIE 10") != -1) || (navigator.userAgent.indexOf("Trident/7") != -1 && navigator.userAgent.indexOf("rv:11") != -1)) {
        browser = "chrome"
}else{browser="ie"
}}else{if(Object.prototype.toString.call(window.opera)=="[object Opera]"){browser="opera"
}else{if(navigator.vendor.indexOf("Apple")!=-1){browser="safari";
if(navigator.userAgent.indexOf("iPad")!=-1||navigator.userAgent.indexOf("iPhone")!=-1){browser.ios=true
}}else{if(navigator.vendor.indexOf("Google")!=-1){if(navigator.userAgent.indexOf("Android")!=-1){browser="android"
}else{browser="chrome"
}}else{if(navigator.product=="Gecko"&&window.find&&!navigator.savePreferences){browser="firefox"
}else{throw new Error("couldn't detect browser")
}}}}}switch(browser){case"ie":(function(){if(document.createEvent===undefined){var a=function(){};
a.prototype.initEvent=function(h,i,g){this.type=h;
this.bubbles=i;
this.cancelable=g
};
document.createEvent=function(g){if(g!="Events"){throw new Error("Unsupported event name: "+g)
}return new a()
}
}document._w_3_c_d_o_m_e_v_e_n_t_s_createElement=document.createElement;
document.createElement=function(g){var i=this._w_3_c_d_o_m_e_v_e_n_t_s_createElement(g);
if(i.addEventListener===undefined){var h={};
i.addEventListener=function(k,l,j){i.attachEvent("on"+k,l);
return e(h,k,l,j)
};
i.removeEventListener=function(k,l,j){return d(h,k,l,j)
};
i.dispatchEvent=function(j){return f(h,j)
}
}return i
};
if(window.addEventListener===undefined){var b=document.createElement("div");
var c=(typeof(postMessage)==="undefined");
window.addEventListener=function(h,i,g){if(c&&h=="message"){b.addEventListener(h,i,g)
}else{window.attachEvent("on"+h,i)
}};
window.removeEventListener=function(h,i,g){if(c&&h=="message"){b.removeEventListener(h,i,g)
}else{window.detachEvent("on"+h,i)
}};
window.dispatchEvent=function(g){if(c&&g.type=="message"){b.dispatchEvent(g)
}else{window.fireEvent("on"+g.type,g)
}}
}function e(i,h,k,g){if(g){throw new Error("Not implemented")
}var j=i[h]||{};
i[h]=j;
j[k]=k
}function d(i,h,k,g){if(g){throw new Error("Not implemented")
}var j=i[h]||{};
delete j[k]
}function f(i,k){var g=k.type;
var j=i[g]||{};
for(var h in j){if(typeof(j[h])=="function"){try{j[h](k)
}catch(l){}}}}})();
break;
case"chrome":case"android":case"safari":if(typeof(window.postMessage)==="undefined"&&typeof(window.dispatchEvent)==="undefined"&&typeof(document.dispatchEvent)==="function"){window.dispatchEvent=function(a){document.dispatchEvent(a)
};
var addEventListener0=window.addEventListener;
window.addEventListener=function(b,c,a){if(b==="message"){document.addEventListener(b,c,a)
}else{addEventListener0.call(window,b,c,a)
}};
var removeEventListener0=window.removeEventListener;
window.removeEventListener=function(b,c,a){if(b==="message"){document.removeEventListener(b,c,a)
}else{removeEventListener0.call(window,b,c,a)
}}
}break;
case"opera":var addEventListener0=window.addEventListener;
window.addEventListener=function(b,d,a){var c=d;
if(b==="message"){c=function(f){if(f.origin===undefined&&f.uri!==undefined){var e=new URI(f.uri);
delete e.path;
delete e.query;
delete e.fragment;
f.origin=e.toString()
}return d(f)
};
d._$=c
}addEventListener0.call(window,b,c,a)
};
var removeEventListener0=window.removeEventListener;
window.removeEventListener=function(b,d,a){var c=d;
if(b==="message"){c=d._$
}removeEventListener0.call(window,b,c,a)
};
break
}function URI(h){h=h||"";
var b=0;
var e=h.indexOf("://");
if(e!=-1){this.scheme=h.slice(0,e);
b=e+3;
var d=h.indexOf("/",b);
if(d==-1){d=h.length;
h+="/"
}var f=h.slice(b,d);
this.authority=f;
b=d;
this.host=f;
var c=f.indexOf(":");
if(c!=-1){this.host=f.slice(0,c);
this.port=parseInt(f.slice(c+1),10);
if(isNaN(this.port)){throw new Error("Invalid URI syntax")
}}}var g=h.indexOf("?",b);
if(g!=-1){this.path=h.slice(b,g);
b=g+1
}var a=h.indexOf("#",b);
if(a!=-1){if(g!=-1){this.query=h.slice(b,a)
}else{this.path=h.slice(b,a)
}b=a+1;
this.fragment=h.slice(b)
}else{if(g!=-1){this.query=h.slice(b)
}else{this.path=h.slice(b)
}}}(function(){var a=URI.prototype;
a.toString=function(){var e=[];
var d=this.scheme;
if(d!==undefined){e.push(d);
e.push("://");
e.push(this.host);
var c=this.port;
if(c!==undefined){e.push(":");
e.push(c.toString())
}}if(this.path!==undefined){e.push(this.path)
}if(this.query!==undefined){e.push("?");
e.push(this.query)
}if(this.fragment!==undefined){e.push("#");
e.push(this.fragment)
}return e.join("")
};
var b={http:80,ws:80,https:443,wss:443};
URI.replaceProtocol=function(c,e){var d=c.indexOf("://");
if(d>0){return e+c.substr(d)
}else{return""
}}
})();
(function(){Base64={};
Base64.encode=function(g){var f=[];
var h;
var e;
var d;
while(g.length){switch(g.length){case 1:h=g.shift();
f.push(c[(h>>2)&63]);
f.push(c[((h<<4)&48)]);
f.push("=");
f.push("=");
break;
case 2:h=g.shift();
e=g.shift();
f.push(c[(h>>2)&63]);
f.push(c[((h<<4)&48)|((e>>4)&15)]);
f.push(c[(e<<2)&60]);
f.push("=");
break;
default:h=g.shift();
e=g.shift();
d=g.shift();
f.push(c[(h>>2)&63]);
f.push(c[((h<<4)&48)|((e>>4)&15)]);
f.push(c[((e<<2)&60)|((d>>6)&3)]);
f.push(c[d&63]);
break
}}return f.join("")
};
Base64.decode=function(j){if(j.length===0){return[]
}if(j.length%4!==0){throw new Error("Invalid base64 string (must be quads)")
}var o=[];
for(var d=0;
d<j.length;
d+=4){var l=j.charAt(d);
var h=j.charAt(d+1);
var f=j.charAt(d+2);
var e=j.charAt(d+3);
var n=a[l];
var m=a[h];
var k=a[f];
var g=a[e];
o.push(((n<<2)&252)|((m>>4)&3));
if(f!="="){o.push(((m<<4)&240)|((k>>2)&15));
if(e!="="){o.push(((k<<6)&192)|(g&63))
}}}return o
};
var c="ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/".split("");
var a={"=":0};
for(var b=0;
b<c.length;
b++){a[c[b]]=b
}if(typeof(window.btoa)==="undefined"){window.btoa=function(f){var d=f.split("");
for(var e=0;
e<d.length;
e++){d[e]=(d[e]).charCodeAt()
}return Base64.encode(d)
};
window.atob=function(d){var e=Base64.decode(d);
for(var f=0;
f<e.length;
f++){e[f]=String.fromCharCode(e[f])
}return e.join("")
}
}})();
var postMessage0=(function(){var g=new URI((browser=="ie")?document.URL:location.href);
var u={http:80,https:443};
if(g.port==null){g.port=u[g.scheme];
g.authority=g.host+":"+g.port
}var y=g.scheme+"://"+g.authority;
var s="/.kr";
if(typeof(postMessage)!=="undefined"){return function(D,C,i){if(typeof(C)!="string"){throw new Error("Unsupported type. Messages must be strings")
}if(i==="null"){i="*"
}switch(browser){case"ie":case"opera":case"firefox":setTimeout(function(){D.postMessage(C,i)
},0);
break;
default:D.postMessage(C,i);
break
}}
}else{function v(i){this.sourceToken=d(Math.floor(Math.random()*(Math.pow(2,32)-1)),8);
this.iframe=i;
this.bridged=false;
this.lastWrite=0;
this.lastRead=0;
this.lastReadIndex=2;
this.lastSyn=0;
this.lastAck=0;
this.queue=[];
this.escapedFragments=[]
}var w=v.prototype;
w.attach=function(H,C,D,i,G,F){this.target=H;
this.targetOrigin=C;
this.targetToken=D;
this.reader=i;
this.writer=G;
this.writerURL=F;
try{this._lastHash=i.location.hash;
this.poll=e
}catch(E){this._lastDocumentURL=i.document.URL;
this.poll=c
}if(H==parent){b(this,true)
}};
w.detach=function(){this.poll=function(){};
delete this.target;
delete this.targetOrigin;
delete this.reader;
delete this.lastFragment;
delete this.writer;
delete this.writerURL
};
w.poll=function(){};
function e(){var i=this.reader.location.hash;
if(this._lastHash!=i){l(this,i.substring(1));
this._lastHash=i
}}function c(){var C=this.reader.document.URL;
if(this._lastDocumentURL!=C){var i=C.indexOf("#");
if(i!=-1){l(this,C.substring(i+1));
this._lastDocumentURL=C
}}}w.post=function(F,E,i){p(this,F);
var H=1000;
var C=escape(E);
var G=[];
while(C.length>H){var D=C.substring(0,H);
C=C.substring(H);
G.push(D)
}G.push(C);
this.queue.push([i,G]);
if(this.writer!=null&&this.lastAck>=this.lastSyn){b(this,false)
}};
function p(N,M){if(N.lastWrite<1&&!N.bridged){if(M.parent==window){var C=N.iframe.src;
var G=C.split("#");
var Q=null;
var R=document.getElementsByTagName("meta");
for(var H=0;
H<R.length;
H++){if(R[H].name=="kaazing:resources"){alert('kaazing:resources is no longer supported. Please refer to the Administrator\'s Guide section entitled "Configuring a Web Server to Integrate with Kaazing Gateway"')
}}var E=y;
var K=E.toString()+s+"?.kr=xsp&.kv=10.05";
if(Q){var J=new URI(E.toString());
var G=Q.split(":");
J.host=G.shift();
if(G.length){J.port=G.shift()
}K=J.toString()+s+"?.kr=xsp&.kv=10.05"
}for(var H=0;
H<R.length;
H++){if(R[H].name=="kaazing:postMessageBridgeURL"){var F=R[H].content;
var L=new URI(F);
var D=new URI(location.toString());
if(!L.authority){L.host=D.host;
L.port=D.port;
L.scheme=D.scheme;
if(F.indexOf("/")!=0){var P=D.path.split("/");
P.pop();
P.push(F);
L.path=P.join("/")
}}n.BridgeURL=L.toString()
}}if(n.BridgeURL){K=n.BridgeURL
}var O=["I",E,N.sourceToken,escape(K)];
if(G.length>1){var I=G[1];
O.push(escape(I))
}G[1]=O.join("!");
setTimeout(function(){M.location.replace(G.join("#"))
},200);
N.bridged=true
}}}function q(D,C){var i=D.writerURL+"#"+C;
D.writer.location.replace(i)
}function x(i){return parseInt(i,16)
}function d(D,i){var C=D.toString(16);
var E=[];
i-=C.length;
while(i-->0){E.push("0")
}E.push(C);
return E.join("")
}function b(I,J){var G=I.queue;
var M=I.lastRead;
if((G.length>0||J)&&I.lastSyn>I.lastAck){var D=I.lastFrames;
var C=I.lastReadIndex;
if(x(D[C])!=M){D[C]=d(M,8);
q(I,D.join(""))
}}else{if(G.length>0){var K=G.shift();
var E=K[0];
if(E=="*"||E==I.targetOrigin){I.lastWrite++;
var L=K[1];
var F=L.shift();
var H=3;
var D=[I.targetToken,d(I.lastWrite,8),d(M,8),"F",d(F.length,4),F];
var C=2;
if(L.length>0){D[H]="f";
I.queue.unshift(K)
}if(I.resendAck){var i=[I.targetToken,d(I.lastWrite-1,8),d(M,8),"a"];
D=i.concat(D);
C+=i.length
}q(I,D.join(""));
I.lastFrames=D;
I.lastReadIndex=C;
I.lastSyn=I.lastWrite;
I.resendAck=false
}}else{if(J){I.lastWrite++;
var D=[I.targetToken,d(I.lastWrite,8),d(M,8),"a"];
var C=2;
if(I.resendAck){var i=[I.targetToken,d(I.lastWrite-1,8),d(M,8),"a"];
D=i.concat(D);
C+=i.length
}q(I,D.join(""));
I.lastFrames=D;
I.lastReadIndex=C;
I.resendAck=true
}}}}function l(F,I){var i=I.substring(0,8);
var L=x(I.substring(8,16));
var E=x(I.substring(16,24));
var G=I.charAt(24);
if(i!=F.sourceToken){throw new Error("postMessage emulation tampering detected")
}var J=F.lastRead;
var H=J+1;
if(L==H){F.lastRead=H
}if(L==H||L==J){F.lastAck=E
}if(L==H||(L==J&&G=="a")){switch(G){case"f":var D=I.substr(29,x(I.substring(25,29)));
F.escapedFragments.push(D);
b(F,true);
break;
case"F":var C=I.substr(29,x(I.substring(25,29)));
if(F.escapedFragments!==undefined){F.escapedFragments.push(C);
C=F.escapedFragments.join("");
F.escapedFragments=[]
}var K=unescape(C);
B(K,F.target,F.targetOrigin);
b(F,true);
break;
case"a":if(I.length>25){l(F,I.substring(25))
}else{b(F,false)
}break;
default:throw new Error("unknown postMessage emulation payload type: "+G)
}}}function B(D,E,C){var i=document.createEvent("Events");
i.initEvent("message",false,true);
i.data=D;
i.origin=C;
i.source=E;
dispatchEvent(i)
}var k={};
var A=[];
function f(){for(var E=0,C=A.length;
E<C;
E++){var D=A[E];
D.poll()
}setTimeout(f,20)
}function o(F){if(F==parent){return k.parent
}else{if(F.parent==window){var E=document.getElementsByTagName("iframe");
for(var C=0;
C<E.length;
C++){var D=E[C];
if(F==D.contentWindow){return m(D)
}}}else{throw new Error("Generic peer postMessage not yet implemented")
}}}function m(D){var C=D._name;
if(C===undefined){C="iframe$"+String(Math.random()).substring(2);
D._name=C
}var i=k[C];
if(i===undefined){i=new v(D);
k[C]=i
}return i
}function n(E,D,i){if(typeof(D)!="string"){throw new Error("Unsupported type. Messages must be strings")
}if(E==window){if(i=="*"||i==y){B(D,window,y)
}}else{var C=o(E);
C.post(E,D,i)
}}n.attach=function(H,C,E,i,G,F){var D=o(H);
D.attach(H,C,E,i,G,F);
A.push(D)
};
var a=function(D){var E=new URI((browser=="ie")?document.URL:location.href);
var F;
var R={http:80,https:443};
if(E.port==null){E.port=R[E.scheme];
E.authority=E.host+":"+E.port
}var I=unescape(E.fragment||"");
if(I.length>0){var C=I.split(",");
var N=C.shift();
var i=C.shift();
var T=C.shift();
var K=E.scheme+"://"+document.domain+":"+E.port;
var Q=E.scheme+"://"+E.authority;
var G=N+"/.kr?.kr=xsc&.kv=10.05";
var M=document.location.toString().split("#")[0];
var J=G+"#"+escape([K,i,escape(M)].join(","));
if(typeof(ActiveXObject)!="undefined"){F=new ActiveXObject("htmlfile");
F.open();
try{F.parentWindow.opener=window
}catch(P){if(D){F.domain=D
}F.parentWindow.opener=window
}F.write("<html>");
F.write("<body>");
if(D){F.write("<script>CollectGarbage();document.domain='"+D+"';<\/script>")
}F.write('<iframe src="'+G+'"></iframe>');
F.write("</body>");
F.write("</html>");
F.close();
var H=F.body.lastChild;
var O=F.parentWindow;
var U=parent;
var L=U.parent.postMessage0;
if(typeof(L)!="undefined"){H.onload=function(){var V=H.contentWindow;
V.location.replace(J);
L.attach(U,N,T,O,V,G)
}
}}else{var H=document.createElement("iframe");
H.src=J;
document.body.appendChild(H);
var O=window;
var S=H.contentWindow;
var U=parent;
var L=U.parent.postMessage0;
if(typeof(L)!="undefined"){L.attach(U,N,T,O,S,G)
}}}window.onunload=function(){try{var W=window.parent.parent.postMessage0;
if(typeof(W)!="undefined"){W.detach(U)
}}catch(V){}if(typeof(F)!=="undefined"){F.parentWindow.opener=null;
F.open();
F.close();
F=null;
CollectGarbage()
}}
};
n.__init__=function(C,D){var i=a.toString();
C.URI=URI;
C.browser=browser;
if(!D){D=""
}C.setTimeout("("+i+")('"+D+"')",0)
};
n.bridgeURL=false;
n.detach=function(E){var C=o(E);
for(var D=0;
D<A.length;
D++){if(A[D]==C){A.splice(D,1)
}}C.detach()
};
if(window!=top){k.parent=new v();
function h(){var F=new URI((browser=="ie")?document.URL:location.href);
var J=F.fragment||"";
if(document.body!=null&&J.length>0&&J.charAt(0)=="I"){var N=unescape(J);
var G=N.split("!");
if(G.shift()=="I"){var i=G.shift();
var E=G.shift();
var K=unescape(G.shift());
var H=y;
if(i==H){try{parent.location.hash
}catch(C){document.domain=document.domain
}}var I=G.shift()||"";
switch(browser){case"firefox":location.replace([location.href.split("#")[0],I].join("#"));
break;
default:location.hash=I;
break
}var D=o(parent);
D.targetToken=E;
var O=D.sourceToken;
var M=K+"#"+escape([H,E,O].join(","));
var L;
L=document.createElement("iframe");
L.src=M;
L.style.position="absolute";
L.style.left="-10px";
L.style.top="10px";
L.style.visibility="hidden";
L.style.width="0px";
L.style.height="0px";
document.body.appendChild(L);
return
}}setTimeout(h,20)
}h()
}var j=document.getElementsByTagName("meta");
for(var t=0;
t<j.length;
t++){if(j[t].name==="kaazing:postMessage"){if("immediate"==j[t].content){var r=function(){var F=document.getElementsByTagName("iframe");
for(var D=0;
D<F.length;
D++){var E=F[D];
if(E.style.KaaPostMessage=="immediate"){E.style.KaaPostMessage="none";
var C=m(E);
p(C,E.contentWindow)
}}setTimeout(r,20)
};
setTimeout(r,20)
}break
}}for(var t=0;
t<j.length;
t++){if(j[t].name==="kaazing:postMessagePrefix"){var z=j[t].content;
if(z!=null&&z.length>0){if(z.charAt(0)!="/"){z="/"+z
}s=z
}}}setTimeout(f,20);
return n
}})();
var XMLHttpRequest0=(function(){var e=new URI((browser=="ie")?document.URL:location.href);
var g={http:80,https:443};
if(e.port==null){e.port=g[e.scheme];
e.authority=e.host+":"+e.port
}var b={};
var a={};
var c=0;
function n(){}var h=n.prototype;
h.readyState=0;
h.responseText="";
h.status=0;
h.statusText="";
h.timeout=0;
h.onreadystatechange;
h.onerror;
h.onload;
h.onprogress;
h.open=function(t,o,q){if(!q){throw new Error("Asynchronous is required for cross-origin XMLHttpRequest emulation")
}switch(this.readyState){case 0:case 4:break;
default:throw new Error("Invalid ready state")
}var s=l(this);
var p=j(this,o);
p.attach(s);
this._pipe=p;
this._requestHeaders=[];
this._method=t;
this._location=o;
this._responseHeaders={};
this.readyState=1;
this.status=0;
this.statusText="";
this.responseText="";
var r=this;
setTimeout(function(){r.readyState=1;
m(r)
},0)
};
h.setRequestHeader=function(o,p){if(this.readyState!==1){throw new Error("Invalid ready state")
}this._requestHeaders.push([o,p])
};
h.send=function(p){if(this.readyState!==1){throw new Error("Invalid ready state")
}var o=this;
setTimeout(function(){o.readyState=2;
m(o)
},0);
k(this,p)
};
h.abort=function(){var o=this._pipe;
if(o!==undefined){o.post(["a",this._id].join(""));
o.detach(this._id)
}};
h.getResponseHeader=function(o){if(this.status==0){throw new Error("Invalid ready state")
}var p=this._responseHeaders;
return p[o]
};
h.getAllResponseHeaders=function(){if(this.status==0){throw new Error("Invalid ready state")
}return this._responseHeaders
};
function m(o){if(typeof(o.onreadystatechange)!=="undefined"){o.onreadystatechange()
}switch(o.readyState){case 3:if(typeof(o.onprogress)!=="undefined"){o.onprogress()
}break;
case 4:if(o.status<100||o.status>=500){if(typeof(o.onerror)!=="undefined"){o.onerror()
}}else{if(typeof(o.onprogress)!=="undefined"){o.onprogress()
}if(typeof(o.onload)!=="undefined"){o.onload()
}}break
}}function l(o){var p=i(c++,8);
a[p]=o;
o._id=p;
return p
}function k(r,t){if(typeof(t)!=="string"){t=""
}var o=r._method.substring(0,10);
var u=r._location;
var q=r._requestHeaders;
var s=i(r.timeout,4);
var v=(r.onprogress!==undefined)?"t":"f";
var x=["s",r._id,o.length,o,i(u.length,4),u,i(q.length,4)];
for(var p=0;
p<q.length;
p++){var w=q[p];
x.push(i(w[0].length,4));
x.push(w[0]);
x.push(i(w[1].length,4));
x.push(w[1])
}x.push(i(t.length,8),t,i(s,4),v);
r._pipe.post(x.join(""))
}function j(v,y){var p=new URI(y);
var q=(p.scheme!=null&&p.authority!=null);
var x=q?p.scheme:e.scheme;
var B=q?p.authority:e.authority;
if(B!=null&&p.port==null){B=p.host+":"+g[x]
}var t=x+"://"+B;
var r=b[t];
if(r!==undefined){if(!("iframe" in r&&"contentWindow" in r.iframe&&typeof r.iframe.contentWindow=="object")){r=b[t]=undefined
}}if(r===undefined){var s=document.createElement("iframe");
s.style.position="absolute";
s.style.left="-10px";
s.style.top="10px";
s.style.visibility="hidden";
s.style.width="0px";
s.style.height="0px";
var A=new URI(t);
A.query=".kr=xs";
A.path="/";
s.src=A.toString();
function z(C){this.buffer.push(C)
}function u(D){var C=this.attached[D];
if(C===undefined){C={};
this.attached[D]=C
}if(C.timerID!==undefined){clearTimeout(C.timerID);
delete C.timerID
}}function w(E){var C=this.attached[E];
if(C!==undefined&&C.timerID===undefined){var D=this;
C.timerID=setTimeout(function(){delete D.attached[E];
var F=a[E];
if(F._pipe==r){delete a[E];
delete F._id;
delete F._pipe
}postMessage0(r.iframe.contentWindow,["d",E].join(""),r.targetOrigin)
},0)
}}r={targetOrigin:t,iframe:s,buffer:[],post:z,attach:u,detach:w,attached:{count:0}};
b[t]=r;
function o(){var C=s.contentWindow;
if(!C){setTimeout(o,20)
}else{postMessage0(C,"I",t)
}}r.handshakeID=setTimeout(function(){b[t]=undefined;
r.post=function(C){v.readyState=4;
v.status=0;
m(v)
};
if(r.buffer.length>0){r.post()
}},30000);
document.body.appendChild(s);
if(typeof(postMessage)==="undefined"){o()
}}return r
}function d(D){var I=D.origin;
var E={http:":80",https:":443"};
var y=I.split(":");
if(y.length===2){I+=E[y[0]]
}var C=b[I];
if(C!==undefined&&C.iframe!==undefined&&D.source==C.iframe.contentWindow){if(D.data=="I"){clearTimeout(C.handshakeID);
var x;
while((x=C.buffer.shift())!==undefined){postMessage0(C.iframe.contentWindow,x,C.targetOrigin)
}C.post=function(M){postMessage0(C.iframe.contentWindow,M,C.targetOrigin)
}
}else{var x=D.data;
if(x.length>=9){var J=0;
var p=x.substring(J,J+=1);
var z=x.substring(J,J+=8);
var r=a[z];
if(r!==undefined){switch(p){case"r":var q={};
var G=f(x.substring(J,J+=2));
for(var F=0;
F<G;
F++){var u=f(x.substring(J,J+=4));
var t=x.substring(J,J+=u);
var s=f(x.substring(J,J+=4));
var A=x.substring(J,J+=s);
q[t]=A
}var B=f(x.substring(J,J+=4));
var L=f(x.substring(J,J+=2));
var H=x.substring(J,J+=L);
switch(B){case 301:case 302:case 307:var w=q.Location;
var z=l(r);
var C=j(r,w);
C.attach(z);
r._pipe=C;
r._method="GET";
r._location=w;
r._redirect=true;
break;
case 403:r.status=B;
m(r);
break;
default:r._responseHeaders=q;
r.status=B;
r.statusText=H;
break
}break;
case"p":var o=parseInt(x.substring(J,J+=1));
if(r._id===z){r.readyState=o;
var K=f(x.substring(J,J+=8));
var v=x.substring(J,J+=K);
if(v.length>0){r.responseText+=v
}m(r)
}else{if(r._redirect){r._redirect=false;
k(r,"")
}}if(o==4){C.detach(z)
}break;
case"e":if(r._id===z){r.status=0;
r.statusText="";
r.readyState=4;
m(r)
}C.detach(z);
break;
case"t":if(r._id===z){r.status=0;
r.statusText="";
r.readyState=4;
if(typeof(r.ontimeout)!=="undefined"){r.ontimeout()
}}C.detach(z);
break
}}}}}else{}}function f(o){return parseInt(o,16)
}function i(q,o){var p=q.toString(16);
var r=[];
o-=p.length;
while(o-->0){r.push("0")
}r.push(p);
return r.join("")
}window.addEventListener("message",d,false);
return n
})();
ByteOrder=function(){};
(function(){var g=ByteOrder.prototype;
g.toString=function(){throw new Error("Abstract")
};
var d=function(m){return(m&255)
};
var i=function(m){return(m&128)?(m|-256):m
};
var c=function(m){return[((m>>8)&255),(m&255)]
};
var l=function(m,n){return(i(m)<<8)|(n&255)
};
var b=function(m,n){return((m&255)<<8)|(n&255)
};
var e=function(m,n,o){return((m&255)<<16)|((n&255)<<8)|(o&255)
};
var j=function(m){return[((m>>16)&255),((m>>8)&255),(m&255)]
};
var k=function(m,n,o){return((m&255)<<16)|((n&255)<<8)|(o&255)
};
var f=function(m){return[((m>>24)&255),((m>>16)&255),((m>>8)&255),(m&255)]
};
var h=function(p,m,n,o){return(i(p)<<24)|((m&255)<<16)|((n&255)<<8)|(o&255)
};
var a=function(r,m,o,q){var n=b(r,m);
var p=b(o,q);
return(n*65536+p)
};
ByteOrder.BIG_ENDIAN=(function(){var n=function(){};
n.prototype=new ByteOrder();
var m=n.prototype;
m._toUnsignedByte=d;
m._toByte=i;
m._fromShort=c;
m._toShort=l;
m._toUnsignedShort=b;
m._toUnsignedMediumInt=e;
m._fromMediumInt=j;
m._toMediumInt=k;
m._fromInt=f;
m._toInt=h;
m._toUnsignedInt=a;
m.toString=function(){return"<ByteOrder.BIG_ENDIAN>"
};
return new n()
})();
ByteOrder.LITTLE_ENDIAN=(function(){var n=function(){};
n.prototype=new ByteOrder();
var m=n.prototype;
m._toByte=i;
m._toUnsignedByte=d;
m._fromShort=function(o){return c(o).reverse()
};
m._toShort=function(o,p){return l(p,o)
};
m._toUnsignedShort=function(o,p){return b(p,o)
};
m._toUnsignedMediumInt=function(o,p,q){return e(q,p,o)
};
m._fromMediumInt=function(o){return j(o).reverse()
};
m._toMediumInt=function(r,s,t,o,p,q){return k(q,p,o,t,s,r)
};
m._fromInt=function(o){return f(o).reverse()
};
m._toInt=function(r,o,p,q){return h(q,p,o,r)
};
m._toUnsignedInt=function(r,o,p,q){return a(q,p,o,r)
};
m.toString=function(){return"<ByteOrder.LITTLE_ENDIAN>"
};
return new n()
})()
})();
function ByteBuffer(a){this.array=a||[];
this._mark=-1;
this.limit=this.capacity=this.array.length;
this.order=ByteOrder.BIG_ENDIAN
}(function(){ByteBuffer.allocate=function(f){var g=new ByteBuffer();
g.capacity=f;
g.limit=f;
return g
};
ByteBuffer.wrap=function(f){return new ByteBuffer(f)
};
var a=ByteBuffer.prototype;
a.autoExpand=true;
a.capacity=0;
a.position=0;
a.limit=0;
a.order=ByteOrder.BIG_ENDIAN;
a.array=[];
a.mark=function(){this._mark=this.position;
return this
};
a.reset=function(){var f=this._mark;
if(f<0){throw new Error("Invalid mark")
}this.position=f;
return this
};
a.compact=function(){this.array.splice(0,this.position);
this.limit-=this.position;
this.position=0;
return this
};
a.duplicate=function(){var f=new ByteBuffer(this.array);
f.position=this.position;
f.limit=this.limit;
f.capacity=this.capacity;
return f
};
a.fill=function(f){d(this,f);
while(f-->0){this.put(0)
}return this
};
a.fillWith=function(f,g){d(this,g);
while(g-->0){this.put(f)
}return this
};
a.indexOf=function(f){var g=this.limit;
var j=this.array;
for(var h=this.position;
h<g;
h++){if(j[h]==f){return h
}}return -1
};
a.put=function(f){d(this,1);
this.putAt(this.position++,f);
return this
};
a.putAt=function(g,f){b(this,g,1);
this.array[g]=this.order._toUnsignedByte(f);
return this
};
a.putUnsigned=function(f){d(this,1);
this.putUnsignedAt(this.position,f&255);
this.position+=1;
return this
};
a.putUnsignedAt=function(g,f){b(this,g,1);
this.putAt(g,f&255);
return this
};
a.putShort=function(f){d(this,2);
this.putShortAt(this.position,f);
this.position+=2;
return this
};
a.putShortAt=function(g,f){b(this,g,2);
this.putBytesAt(g,this.order._fromShort(f));
return this
};
a.putUnsignedShort=function(f){d(this,2);
this.putUnsignedShortAt(this.position,f&65535);
this.position+=2;
return this
};
a.putUnsignedShortAt=function(g,f){b(this,g,2);
this.putShortAt(g,f&65535);
return this
};
a.putMediumInt=function(f){d(this,3);
this.putMediumIntAt(this.position,f);
this.position+=3;
return this
};
a.putMediumIntAt=function(g,f){this.putBytesAt(g,this.order._fromMediumInt(f));
return this
};
a.putInt=function(f){d(this,4);
this.putIntAt(this.position,f);
this.position+=4;
return this
};
a.putIntAt=function(g,f){b(this,g,4);
this.putBytesAt(g,this.order._fromInt(f));
return this
};
a.putUnsignedInt=function(f){d(this,4);
this.putUnsignedIntAt(this.position,f&4294967295);
this.position+=4;
return this
};
a.putUnsignedIntAt=function(g,f){b(this,g,4);
this.putIntAt(g,f&4294967295);
return this
};
a.putString=function(f,g){g.encode(f,this);
return this
};
a.putPrefixedString=function(g,h,i){if(typeof(i)==="undefined"||typeof(i.encode)==="undefined"){throw new Error("ByteBuffer.putPrefixedString: character set parameter missing")
}if(g===0){return this
}d(this,g);
var f=h.length;
switch(g){case 1:this.put(f);
break;
case 2:this.putShort(f);
break;
case 4:this.putInt(f);
break
}i.encode(h,this);
return this
};
a.putBytes=function(f){d(this,f.length);
this.putBytesAt(this.position,f);
this.position+=f.length;
return this
};
a.putBytesAt=function(l,h){b(this,l,h.length);
for(var i=0,g=l,f=h.length;
i<f;
i++,g++){this.putAt(g,h[i])
}return this
};
a.putBuffer=function(f){this.putBytes(f.array.slice(f.position,f.limit));
return this
};
a.putBufferAt=function(g,f){this.putBytesAt(g,f.array.slice(f.position,f.limit));
return this
};
a.get=function(){e(this,1);
return this.getAt(this.position++)
};
a.getAt=function(f){c(this,f,1);
return this.order._toByte(this.array[f])
};
a.getUnsigned=function(){e(this,1);
var f=this.getUnsignedAt(this.position);
this.position+=1;
return f
};
a.getUnsignedAt=function(f){c(this,f,1);
return this.order._toUnsignedByte(this.array[f])
};
a.getBytes=function(h){e(this,h);
var f=new Array();
for(var g=0;
g<h;
g++){f.push(this.order._toByte(this.array[g+this.position]))
}this.position+=h;
return f
};
a.getBytesAt=function(g,j){c(this,g,j);
var f=new Array();
this.position=g;
for(var h=0;
h<j;
h++){f.push(this.order._toByte(this.array[h+this.position]))
}this.position+=j;
return f
};
a.getShort=function(){e(this,2);
var f=this.getShortAt(this.position);
this.position+=2;
return f
};
a.getShortAt=function(f){c(this,f,2);
var g=this.array;
return this.order._toShort(g[f++],g[f++])
};
a.getUnsignedShort=function(){e(this,2);
var f=this.getUnsignedShortAt(this.position);
this.position+=2;
return f
};
a.getUnsignedShortAt=function(f){c(this,f,2);
var g=this.array;
return this.order._toUnsignedShort(g[f++],g[f++])
};
a.getUnsignedMediumInt=function(){var f=this.array;
return this.order._toUnsignedMediumInt(f[this.position++],f[this.position++],f[this.position++])
};
a.getMediumInt=function(){var f=this.getMediumIntAt(this.position);
this.position+=3;
return f
};
a.getMediumIntAt=function(f){var g=this.array;
return this.order._toMediumInt(g[f++],g[f++],g[f++])
};
a.getInt=function(){e(this,4);
var f=this.getIntAt(this.position);
this.position+=4;
return f
};
a.getIntAt=function(f){c(this,f,4);
var g=this.array;
return this.order._toInt(g[f++],g[f++],g[f++],g[f++])
};
a.getUnsignedInt=function(){e(this,4);
var f=this.getUnsignedIntAt(this.position);
this.position+=4;
return f
};
a.getUnsignedIntAt=function(f){c(this,f,4);
var g=this.array;
return this.order._toUnsignedInt(g[f++],g[f++],g[f++],g[f++]);
return val
};
a.getPrefixedString=function(g,h){var f=0;
switch(g||2){case 1:f=this.getUnsigned();
break;
case 2:f=this.getUnsignedShort();
break;
case 4:f=this.getInt();
break
}if(f===0){return""
}var i=this.limit;
try{this.limit=this.position+f;
return h.decode(this)
}finally{this.limit=i
}};
a.getString=function(g){var f=this.position;
var h=this.limit;
var i=this.array;
while(f<h&&i[f]!==0){f++
}try{this.limit=f;
return g.decode(this)
}finally{if(f!=h){this.limit=h;
this.position=f+1
}}};
a.slice=function(){return new ByteBuffer(this.array.slice(this.position,this.limit))
};
a.flip=function(){this.limit=this.position;
this.position=0;
this._mark=-1;
return this
};
a.rewind=function(){this.position=0;
this._mark=-1;
return this
};
a.clear=function(){this.position=0;
this.limit=this.capacity;
this._mark=-1;
return this
};
a.remaining=function(){return(this.limit-this.position)
};
a.hasRemaining=function(){return(this.limit>this.position)
};
a.skip=function(f){this.position+=f;
return this
};
a.getHexDump=function(){var l=this.array;
var k=this.position;
var f=this.limit;
if(k==f){return"empty"
}var j=[];
for(var g=k;
g<f;
g++){var h=(l[g]||0).toString(16);
if(h.length==1){h="0"+h
}j.push(h)
}return j.join(" ")
};
a.toString=a.getHexDump;
a.expand=function(f){return this.expandAt(this.position,f)
};
a.expandAt=function(g,h){var f=g+h;
if(f>this.capacity){this.capacity=f
}if(f>this.limit){this.limit=f
}return this
};
function d(g,f){if(g.autoExpand){g.expand(f)
}return g
}function e(h,g){var f=h.position+g;
if(f>h.limit){throw new Error("Buffer underflow")
}return h
}function c(i,g,h){var f=g+h;
if(g<0||f>i.limit){throw new Error("Index out of bounds")
}return i
}function b(i,g,h){var f=g+h;
if(g<0||f>i.limit){throw new Error("Index out of bounds")
}return i
}})();
function Charset(){}(function(){var a=Charset.prototype;
a.decode=function(b){};
a.encode=function(b){};
Charset.UTF8=(function(){function d(){}d.prototype=new Charset();
var c=d.prototype;
c.decode=function(h){var j=h.remaining()<10000;
var g=[];
while(h.hasRemaining()){var i=h.remaining();
var f=h.getUnsigned();
var k=b(f);
if(i<k){h.skip(-1);
break
}var e=null;
switch(k){case 1:e=f;
break;
case 2:e=((f&31)<<6)|(h.getUnsigned()&63);
break;
case 3:e=((f&15)<<12)|((h.getUnsigned()&63)<<6)|(h.getUnsigned()&63);
break;
case 4:e=((f&7)<<18)|((h.getUnsigned()&63)<<12)|((h.getUnsigned()&63)<<6)|(h.getUnsigned()&63);
break
}if(j){g.push(e)
}else{g.push(String.fromCharCode(e))
}}if(j){return String.fromCharCode.apply(null,g)
}else{return g.join("")
}};
c.encode=function(h,f){for(var g=0;
g<h.length;
g++){var e=h.charCodeAt(g);
if(e<128){f.put(e)
}else{if(e<2048){f.put((e>>6)|192);
f.put((e&63)|128)
}else{if(e<65536){f.put((e>>12)|224);
f.put(((e>>6)&63)|128);
f.put((e&63)|128)
}else{if(e<1114112){f.put((e>>18)|240);
f.put(((e>>12)&63)|128);
f.put(((e>>6)&63)|128);
f.put((e&63)|128)
}else{throw new Error("Invalid UTF-8 string")
}}}}}};
function b(e){if((e&128)===0){return 1
}if((e&32)===0){return 2
}if((e&16)===0){return 3
}if((e&8)===0){return 4
}throw new Error("Invalid UTF-8 bytes")
}return new d()
})()
})();
(function(){var q="StompJms";
var l=function(u){this._name=u;
this._level=l.Level.INFO
};
(function(){l.Level={OFF:8,SEVERE:7,WARNING:6,INFO:5,CONFIG:4,FINE:3,FINER:2,FINEST:1,ALL:0};
var A;
var C=document.getElementsByTagName("meta");
for(var x=0;
x<C.length;
x++){if(C[x].name==="kaazing:logging"){A=C[x].content;
break
}}l._logConf={};
if(A){var z=A.split(",");
for(var x=0;
x<z.length;
x++){var v=z[x].split("=");
l._logConf[v[0]]=v[1]
}}var u={};
l.getLogger=function(E){var D=u[E];
if(D===undefined){D=new l(E);
u[E]=D
}return D
};
var y=l.prototype;
y.setLevel=function(D){if(D&&D>=l.Level.ALL&&D<=l.Level.OFF){this._level=D
}};
y.isLoggable=function(F){for(var E in l._logConf){if(this._name.match(E)){var D=l._logConf[E];
if(D){return(l.Level[D]<=F)
}}}return(this._level<=F)
};
var B=function(){};
var w={};
w[l.Level.OFF]=B;
w[l.Level.SEVERE]=(window.console)?(console.error||console.log||B):B;
w[l.Level.WARNING]=(window.console)?(console.warn||console.log||B):B;
w[l.Level.INFO]=(window.console)?(console.info||console.log||B):B;
w[l.Level.CONFIG]=(window.console)?(console.info||console.log||B):B;
w[l.Level.FINE]=(window.console)?(console.debug||console.log||B):B;
w[l.Level.FINER]=(window.console)?(console.debug||console.log||B):B;
w[l.Level.FINEST]=(window.console)?(console.debug||console.log||B):B;
w[l.Level.ALL]=(window.console)?(console.log||B):B;
y.config=function(E,D){this.log(l.Level.CONFIG,E,D)
};
y.entering=function(F,D,G){if(this.isLoggable(l.Level.FINER)){if(browser=="chrome"||browser=="safari"){F=console
}var E=w[l.Level.FINER];
if(G){if(typeof(E)=="object"){E("ENTRY "+D,G)
}else{E.call(F,"ENTRY "+D,G)
}}else{if(typeof(E)=="object"){E("ENTRY "+D)
}else{E.call(F,"ENTRY "+D)
}}}};
y.exiting=function(G,D,F){if(this.isLoggable(l.Level.FINER)){var E=w[l.Level.FINER];
if(browser=="chrome"||browser=="safari"){G=console
}if(F){if(typeof(E)=="object"){E("RETURN "+D,F)
}else{E.call(G,"RETURN "+D,F)
}}else{if(typeof(E)=="object"){E("RETURN "+D)
}else{E.call(G,"RETURN "+D)
}}}};
y.fine=function(E,D){this.log(l.Level.FINE,E,D)
};
y.finer=function(E,D){this.log(l.Level.FINER,E,D)
};
y.finest=function(E,D){this.log(l.Level.FINEST,E,D)
};
y.info=function(E,D){this.log(l.Level.INFO,E,D)
};
y.log=function(G,F,E){if(this.isLoggable(G)){var D=w[G];
if(browser=="chrome"||browser=="safari"){F=console
}if(typeof(D)=="object"){D(E)
}else{D.call(F,E)
}}};
y.severe=function(E,D){this.log(l.Level.SEVERE,E,D)
};
y.warning=function(E,D){this.log(l.Level.WARNING,E,D)
}
})();
var c=l.getLogger("com.kaazing.gateway.client.loader.Utils");
var j=function(y){c.entering(this,"Utils.getMetaValue",y);
var w=document.getElementsByTagName("meta");
for(var x=0;
x<w.length;
x++){if(w[x].name===y){var u=w[x].content;
c.exiting(this,"Utils.getMetaValue",u);
return u
}}c.exiting(this,"Utils.getMetaValue")
};
var g=function(w){c.entering(this,"Utils.arrayCopy",w);
var u=[];
for(var v=0;
v<w.length;
v++){u.push(w[v])
}return u
};
var p=function(y,x){c.entering(this,"Utils.arrayFilter",{array:y,callback:x});
var u=[];
for(var w=0;
w<y.length;
w++){var v=y[w];
if(x(v)){u.push(y[w])
}}return u
};
var d=function(w,u){c.entering(this,"Utils.indexOf",{array:w,searchElement:u});
for(var v=0;
v<w.length;
v++){if(w[v]==u){c.exiting(this,"Utils.indexOf",v);
return v
}}c.exiting(this,"Utils.indexOf",-1);
return -1
};
var n=function(z){c.entering(this,"Utils.decodeByteString",z);
var u=[];
for(var y=0;
y<z.length;
y++){u.push(z.charCodeAt(y)&255)
}var x=new ByteBuffer(u);
var w=i(x,Charset.UTF8);
c.exiting(this,"Utils.decodeByteString",w);
return w
};
var h=function(y){c.entering(this,"Utils.decodeArrayBuffer",y);
var v=new Uint8Array(y);
var u=[];
for(var w=0;
w<v.length;
w++){u.push(v[w])
}var v=new ByteBuffer(u);
var x=i(v,Charset.UTF8);
c.exiting(this,"Utils.decodeArrayBuffer",x);
return x
};
var m=function(x){c.entering(this,"Utils.decodeArrayBuffer2ByteBuffer");
var v=new Uint8Array(x);
var u=[];
for(var w=0;
w<v.length;
w++){u.push(v[w])
}c.exiting(this,"Utils.decodeArrayBuffer2ByteBuffer");
return new ByteBuffer(u)
};
var o=String.fromCharCode(127);
var s=String.fromCharCode(0);
var k="\n";
var t=function(x){c.entering(this,"Utils.encodeEscapedByte",x);
var u=[];
while(x.remaining()){var z=x.getUnsigned();
var y=String.fromCharCode(z);
switch(y){case o:u.push(o);
u.push(o);
break;
case s:u.push(o);
u.push("0");
break;
case k:u.push(o);
u.push("n");
break;
default:u.push(y)
}}var w=u.join("");
c.exiting(this,"Utils.encodeEscapedBytes",w);
return w
};
var r=function(v,w){c.entering(this,"Utils.encodeByteString",{buf:v,requiresEscaping:w});
if(w){return t(v)
}else{var u=[];
while(v.remaining()){var y=v.getUnsigned();
u.push(String.fromCharCode(y))
}var x=u.join("");
c.exiting(this,"Utils.encodeByteString",x);
return x
}};
var i=function(v,w){var u=v.position;
var x=v.limit;
var y=v.array;
while(u<x){u++
}try{v.limit=u;
return w.decode(v)
}finally{if(u!=x){v.limit=x;
v.position=u+1
}}};
var b=window.WebSocket;
var a=(function(){var u=l.getLogger("WebSocketNativeProxy");
var D=function(){this.parent;
this._listener
};
var w=(browser=="safari"&&typeof(b.CLOSING)=="undefined");
var B=D.prototype;
B.connect=function(H,K){u.entering(this,"WebSocketNativeProxy.<init>",{location:H,protocol:K});
if(typeof(b)==="undefined"){C(this);
return
}if(H.indexOf("javascript:")==0){H=H.substr("javascript:".length)
}var I=H.indexOf("?");
if(I!=-1){if(!/[\?&]\.kl=Y/.test(H.substring(I))){H+="&.kl=Y"
}}else{H+="?.kl=Y"
}if(w&&this.parent._isBinary){H+="&encoding=utf8"
}this._balanced=false;
this._sendQueue=[];
try{if(K){this._requestedProtocol=K;
this._delegate=new b(H,K)
}else{this._delegate=new b(H)
}this._delegate.binaryType="arraybuffer"
}catch(J){u.severe(this,"WebSocketNativeProxy.<init> "+J);
C(this);
return
}y(this)
};
B.onerror=function(){};
B.onmessage=function(){};
B.onopen=function(){};
B.onclose=function(){};
B.close=function(){u.entering(this,"WebSocketNativeProxy.close");
this._delegate.close()
};
B.send=function(H){u.entering(this,"WebSocketNativeProxy.send",H);
if(this._balanced==true){F(this,H)
}else{this._sendQueue.push(H)
}};
B.setListener=function(H){this._listener=H
};
function F(K,J){u.entering(this,"WebSocketNativeProxy.doSend",J);
if(typeof(J)=="string"){K._delegate.send(J)
}else{if(J.constructor==ByteBuffer){if(w){var L=r(J);
K._delegate.send(L)
}else{var H=new Uint8Array(J.remaining());
for(var I=0;
I<H.byteLength;
I++){H[I]=J.getUnsigned()
}K._delegate.send(H.buffer)
}}else{u.severe(this,"WebSocketNativeProxy.doSend called with unkown type "+typeof(J));
throw new Error("Cannot call send() with that type")
}}}function C(I,H){u.entering(this,"WebSocketNativeProxy.doError",H);
setTimeout(function(){I._listener.connectionFailed(I.parent)
},0)
}function x(K,J){var H;
if(typeof J.data.byteLength!=="undefined"){H=m(J.data)
}else{H=ByteBuffer.allocate(J.data.length);
if(K.parent._isBinary&&K.parent._balanced){for(var I=0;
I<J.data.length;
I++){H.put(J.data.charCodeAt(I))
}}else{H.putString(J.data,Charset.UTF8)
}H.flip()
}return H
}function G(N,M){u.entering(this,"WebSocketNativeProxy.messageHandler",M);
if(N._balanced==true){if(typeof M.data.byteLength!=="undefined"){M.decoder=h
}N._listener.messageReceived(N.parent,x(N,M))
}else{var L=M.data;
if(typeof M.data.byteLength!=="undefined"){L=h(M.data)
}if(L.match("^\uf0ff")=="\uf0ff"){var J=L.substring(1);
if(J.match("^R")=="R"){var I=J.substring(1);
if(I&&I!=""){u.finest(this,"WebSocketNativeProxy.messageHandler: redirectLoc = "+I);
var K=I.indexOf("?");
if(K!=-1){I+="&.kl=Y"
}else{I+="?.kl=Y"
}if(w&&N.parent._isBinary){I+="&encoding=utf8"
}E(N);
N.close();
N._delegate=new b(I);
N._delegate.binaryType="arraybuffer";
y(N)
}else{u.warning(this,"WebSocketNativeProxy.messageHandler: No balancees");
N.close()
}}else{if(J.match("^N$")=="N"){u.finest(this,"WebSocketNativeProxy.messageHandler: Not balancer - service gateway");
N._balanced=true;
N._listener.connectionOpened(N.parent,N.parent.protocol);
var H;
while(H=N._sendQueue.shift()){F(N,H)
}}else{u.warning(this,"WebSocketNativeProxy.messageHandler: Unknown balancer control frame command "+M.data);
N._balanced=true;
N._listener.messageReceived(N.parent,x(N,M))
}}}else{u.warning(this,"WebSocketNativeProxy.messageHandler: Unknown balancer control frame "+M.data);
N._balanced=true;
N._listener.messageReceived(N.parent,x(N,M))
}}}function v(I,H){u.entering(this,"WebSocketNativeProxy.closeHandler",H);
E(I);
I._listener.connectionClosed(I.parent)
}function A(I,H){u.entering(this,"WebSocketNativeProxy.errorHandler",H);
E(I);
I._listener.connectionFailed(I.parent)
}function z(I,H){u.entering(this,"WebSocketNativeProxy.openHandler",H);
I.parent.protocol=I._delegate.protocol;
if(browser=="safari"){I.parent.protocol=I._requestedProtocol
}}function y(I){u.entering(this,"WebSocketNativeProxy.bindHandlers");
var H=I._delegate;
H.onopen=function(J){z(I,J)
};
H.onmessage=function(J){G(I,J)
};
H.onclose=function(J){v(I,J)
};
H.onerror=function(J){A(I,J)
};
I.readyState=function(){return H.readyState
}
}function E(I){u.entering(this,"WebSocketNativeProxy.unbindHandlers");
var H=I._delegate;
H.onmessage=undefined;
H.onclose=undefined;
H.onopen=undefined;
H.onerror=undefined;
I.readyState=3
}return D
})();
var f=(function(){var x=l.getLogger("WebSocketEmulatedFlashProxy");
var y=function(){this.parent;
this._listener
};
var u=y.prototype;
u.connect=function(A,C){x.entering(this,"WebSocketEmulatedFlashProxy.<init>",A);
this.URL=A;
try{v(this,A,C)
}catch(B){x.severe(this,"WebSocketEmulatedFlashProxy.<init> "+B);
w(this,B)
}this.constructor=y;
x.exiting(this,"WebSocketEmulatedFlashProxy.<init>")
};
u.setListener=function(A){this._listener=A
};
y._flashBridge={};
y._flashBridge.readyWaitQueue=[];
y._flashBridge.failWaitQueue=[];
y._flashBridge.flashHasLoaded=false;
y._flashBridge.flashHasFailed=false;
u.URL="";
u.readyState=0;
u.bufferedAmount=0;
u.connectionOpened=function(B,C){var C=C.split("\n");
for(var A=0;
A<C.length;
A++){var D=C[A].split(":");
B.responseHeaders[D[0]]=D[1]
}this._listener.connectionOpened(B,"")
};
u.connectionClosed=function(A){this._listener.connectionClosed(A)
};
u.connectionFailed=function(A){this._listener.connectionFailed(A)
};
u.messageReceived=function(A,B){this._listener.messageReceived(A,B)
};
u.redirected=function(B,A){this._listener.redirected(B,A)
};
u.authenticationRequested=function(C,A,B){this._listener.authenticationRequested(C,A,B)
};
u.send=function(C){x.entering(this,"WebSocketEmulatedFlashProxy.send",C);
switch(this.readyState){case 0:x.severe(this,"WebSocketEmulatedFlashProxy.send: readyState is 0");
throw new Error("INVALID_STATE_ERR");
break;
case 1:if(C===null){x.severe(this,"WebSocketEmulatedFlashProxy.send: Data is null");
throw new Error("data is null")
}if(typeof(C)=="string"){y._flashBridge.sendText(this._instanceId,C)
}else{if(typeof(C.array)=="object"){var D;
var B=[];
var A;
while(C.remaining()){A=C.get();
B.push(String.fromCharCode(A))
}var D=B.join("");
y._flashBridge.sendByteString(this._instanceId,D);
return
}else{x.severe(this,"WebSocketEmulatedFlashProxy.send: Data is on invalid type "+typeof(C));
throw new Error("Invalid type")
}}z(this);
return true;
break;
case 2:return false;
break;
default:x.severe(this,"WebSocketEmulatedFlashProxy.send: Invalid readyState "+this.readyState);
throw new Error("INVALID_STATE_ERR")
}};
u.close=function(){x.entering(this,"WebSocketEmulatedFlashProxy.close");
switch(this.readyState){case 1:case 2:y._flashBridge.disconnect(this._instanceId);
break
}};
u.disconnect=u.close;
var z=function(A){x.entering(this,"WebSocketEmulatedFlashProxy.updateBufferedAmount");
A.bufferedAmount=y._flashBridge.getBufferedAmount(A._instanceId);
if(A.bufferedAmount!=0){setTimeout(function(){z(A)
},1000)
}};
var v=function(D,B,F){x.entering(this,"WebSocketEmulatedFlashProxy.registerWebSocket",B);
var A=function(I,H){H[I]=D;
D._instanceId=I
};
var G=function(){w(D)
};
var E=[];
if(D.parent.requestHeaders&&D.parent.requestHeaders.length>0){for(var C=0;
C<D.parent.requestHeaders.length;
C++){E.push(D.parent.requestHeaders[C].label+":"+D.parent.requestHeaders[C].value)
}}y._flashBridge.registerWebSocketEmulated(B,E.join("\n"),A,G)
};
function w(B,A){x.entering(this,"WebSocketEmulatedFlashProxy.doError",A);
setTimeout(function(){B._listener.connectionFailed(B.parent)
},0)
}return y
})();
var e=(function(){var x=l.getLogger("WebSocketRtmpFlashProxy");
var z=function(){this.parent;
this._listener
};
var u=z.prototype;
u.connect=function(A,C){x.entering(this,"WebSocketRtmpFlashProxy.<init>",A);
this.URL=A;
try{v(this,A,C)
}catch(B){x.severe(this,"WebSocketRtmpFlashProxy.<init> "+B);
w(this,B)
}this.constructor=z;
x.exiting(this,"WebSocketRtmpFlashProxy.<init>")
};
u.setListener=function(A){this._listener=A
};
f._flashBridge={};
f._flashBridge.readyWaitQueue=[];
f._flashBridge.failWaitQueue=[];
f._flashBridge.flashHasLoaded=false;
f._flashBridge.flashHasFailed=false;
u.URL="";
u.readyState=0;
u.bufferedAmount=0;
u.connectionOpened=function(B,C){var C=C.split("\n");
for(var A=0;
A<C.length;
A++){var D=C[A].split(":");
B.responseHeaders[D[0]]=D[1]
}this._listener.connectionOpened(B,"")
};
u.connectionClosed=function(A){this._listener.connectionClosed(A)
};
u.connectionFailed=function(A){this._listener.connectionFailed(A)
};
u.messageReceived=function(A,B){this._listener.messageReceived(A,B)
};
u.redirected=function(B,A){this._listener.redirected(B,A)
};
u.authenticationRequested=function(C,A,B){this._listener.authenticationRequested(C,A,B)
};
u.send=function(C){x.entering(this,"WebSocketRtmpFlashProxy.send",C);
switch(this.readyState){case 0:x.severe(this,"WebSocketRtmpFlashProxy.send: readyState is 0");
throw new Error("INVALID_STATE_ERR");
break;
case 1:if(C===null){x.severe(this,"WebSocketRtmpFlashProxy.send: Data is null");
throw new Error("data is null")
}if(typeof(C)=="string"){f._flashBridge.sendText(this._instanceId,C)
}else{if(typeof(C.array)=="object"){var D;
var B=[];
var A;
while(C.remaining()){A=C.get();
B.push(String.fromCharCode(A))
}var D=B.join("");
f._flashBridge.sendByteString(this._instanceId,D);
return
}else{x.severe(this,"WebSocketRtmpFlashProxy.send: Data is on invalid type "+typeof(C));
throw new Error("Invalid type")
}}y(this);
return true;
break;
case 2:return false;
break;
default:x.severe(this,"WebSocketRtmpFlashProxy.send: Invalid readyState "+this.readyState);
throw new Error("INVALID_STATE_ERR")
}};
u.close=function(){x.entering(this,"WebSocketRtmpFlashProxy.close");
switch(this.readyState){case 1:case 2:f._flashBridge.disconnect(this._instanceId);
break
}};
u.disconnect=u.close;
var y=function(A){x.entering(this,"WebSocketRtmpFlashProxy.updateBufferedAmount");
A.bufferedAmount=f._flashBridge.getBufferedAmount(A._instanceId);
if(A.bufferedAmount!=0){setTimeout(function(){y(A)
},1000)
}};
var v=function(D,B,F){x.entering(this,"WebSocketRtmpFlashProxy.registerWebSocket",B);
var A=function(I,H){H[I]=D;
D._instanceId=I
};
var G=function(){w(D)
};
var E=[];
if(D.parent.requestHeaders&&D.parent.requestHeaders.length>0){for(var C=0;
C<D.parent.requestHeaders.length;
C++){E.push(D.parent.requestHeaders[C].label+":"+D.parent.requestHeaders[C].value)
}}f._flashBridge.registerWebSocketRtmp(B,E.join("\n"),A,G)
};
function w(B,A){x.entering(this,"WebSocketRtmpFlashProxy.doError",A);
setTimeout(function(){B._listener.connectionFailed(B.parent)
},0)
}return z
})();
(function(){var v=l.getLogger("com.kaazing.gateway.client.loader.FlashBridge");
var u={};
f._flashBridge.registerWebSocketEmulated=function(x,A,B,y){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.registerWebSocketEmulated",{location:x,callback:B,errback:y});
var z=function(){var C=f._flashBridge.doRegisterWebSocketEmulated(x,A);
B(C,u)
};
if(f._flashBridge.flashHasLoaded){if(f._flashBridge.flashHasFailed){y()
}else{z()
}}else{this.readyWaitQueue.push(z);
this.failWaitQueue.push(y)
}v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.registerWebSocketEmulated")
};
f._flashBridge.doRegisterWebSocketEmulated=function(x,z){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.doRegisterWebSocketEmulated",{location:x,headers:z});
var y=f._flashBridge.elt.registerWebSocketEmulated(x,z);
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.doRegisterWebSocketEmulated",y);
return y
};
f._flashBridge.registerWebSocketRtmp=function(x,A,B,y){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.registerWebSocketRtmp",{location:x,callback:B,errback:y});
var z=function(){var C=f._flashBridge.doRegisterWebSocketRtmp(x,A);
B(C,u)
};
if(f._flashBridge.flashHasLoaded){if(f._flashBridge.flashHasFailed){y()
}else{z()
}}else{this.readyWaitQueue.push(z);
this.failWaitQueue.push(y)
}v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.registerWebSocketEmulated")
};
f._flashBridge.doRegisterWebSocketRtmp=function(x,z){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.doRegisterWebSocketRtmp",{location:x,protocol:z});
var y=f._flashBridge.elt.registerWebSocketRtmp(x,z);
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.doRegisterWebSocketRtmp",y);
return y
};
f._flashBridge.onready=function(){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.onready");
var y=f._flashBridge.readyWaitQueue;
for(var x=0;
x<y.length;
x++){var z=y[x];
z()
}v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.onready")
};
f._flashBridge.onfail=function(){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.onfail");
var z=f._flashBridge.failWaitQueue;
for(var y=0;
y<z.length;
y++){var x=z[y];
x()
}v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.onfail")
};
f._flashBridge.connectionOpened=function(x,y){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.connectionOpened",x);
u[x].readyState=1;
u[x].connectionOpened(u[x].parent,y);
w();
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.connectionOpened")
};
f._flashBridge.connectionClosed=function(x){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.connectionClosed",x);
u[x].readyState=2;
u[x].connectionClosed(u[x].parent);
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.connectionClosed")
};
f._flashBridge.connectionFailed=function(x){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.connectionFailed",x);
u[x].connectionFailed(u[x].parent);
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.connectionFailed")
};
f._flashBridge.messageReceived=function(A,B){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.messageReceived",{key:A,data:B});
var z=u[A];
if(z.readyState==1){var x=ByteBuffer.allocate(B.length);
for(var y=0;
y<B.length;
y++){x.put(B[y])
}x.flip();
z.messageReceived(z.parent,x)
}v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.messageReceived")
};
f._flashBridge.redirected=function(z,x){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.redirected",{key:z,data:x});
var y=u[z];
y.redirected(y.parent,x);
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.redirected")
};
f._flashBridge.authenticationRequested=function(A,x,z){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.authenticationRequested",{key:A,data:x});
var y=u[A];
y.authenticationRequested(y.parent,x,z);
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.authenticationRequested")
};
var w=function(){v.entering(this,"WebSocketEmulatedFlashProxy.killLoadingBar");
if(browser==="firefox"){var x=document.createElement("iframe");
x.style.display="none";
document.body.appendChild(x);
document.body.removeChild(x)
}};
f._flashBridge.sendText=function(x,y){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.sendText",{key:x,message:y});
this.elt.processTextMessage(x,escape(y));
setTimeout(w,200)
};
f._flashBridge.sendByteString=function(x,y){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.sendByteString",{key:x,message:y});
this.elt.processBinaryMessage(x,escape(y));
setTimeout(w,200)
};
f._flashBridge.disconnect=function(x){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.disconnect",x);
this.elt.processClose(x)
};
f._flashBridge.getBufferedAmount=function(y){v.entering(this,"WebSocketEmulatedFlashProxy._flashBridge.getBufferedAmount",y);
var x=this.elt.getBufferedAmount(y);
v.exiting(this,"WebSocketEmulatedFlashProxy._flashBridge.getBufferedAmount",x);
return x
}
})();
(function(){var x=function(av){var ax=this;
var ap=3000;
var au="Loader";
var ak=false;
var at=-1;
ax.elt=null;
var aq=function(){var aC=new RegExp(".*"+av+".*.js$");
var ay=document.getElementsByTagName("script");
for(var aA=0;
aA<ay.length;
aA++){if(ay[aA].src){var az=(ay[aA].src).match(aC);
if(az){az=az.pop();
var aB=az.split("/");
aB.pop();
if(aB.length>0){return aB.join("/")+"/"
}else{return""
}}}}};
var ar=aq();
var ao=ar+"Loader.swf";
ax.loader=function(){var aA="flash";
var ay=document.getElementsByTagName("meta");
for(var az=0;
az<ay.length;
az++){if(ay[az].name==="kaazing:upgrade"){aA=ay[az].content
}}if(aA!="flash"||!al([9,0,115])){aj()
}else{at=setTimeout(aj,ap);
am()
}};
ax.clearFlashTimer=function(){clearTimeout(at);
at="cleared";
setTimeout(function(){aw(ax.elt.handshake(av))
},0)
};
var aw=function(ay){if(ay){f._flashBridge.flashHasLoaded=true;
f._flashBridge.elt=ax.elt;
f._flashBridge.onready()
}else{aj()
}window.___Loader=undefined
};
var aj=function(){f._flashBridge.flashHasLoaded=true;
f._flashBridge.flashHasFailed=true;
f._flashBridge.onfail()
};
var an=function(){var az=null;
if(typeof(ActiveXObject)!="undefined"){try{ak=true;
var aB=new ActiveXObject("ShockwaveFlash.ShockwaveFlash");
var ay=aB.GetVariable("$version");
var aC=ay.split(" ")[1].split(",");
az=[];
for(var aA=0;
aA<aC.length;
aA++){az[aA]=parseInt(aC[aA])
}}catch(aE){ak=false
}}if(typeof navigator.plugins!="undefined"){if(typeof navigator.plugins["Shockwave Flash"]!="undefined"){var ay=navigator.plugins["Shockwave Flash"].description;
ay=ay.replace(/\s*r/g,".");
var aC=ay.split(" ")[2].split(".");
az=[];
for(var aA=0;
aA<aC.length;
aA++){az[aA]=parseInt(aC[aA])
}}}var aD=navigator.userAgent;
if(az!==null&&az[0]===10&&az[1]===0&&aD.indexOf("Windows NT 6.0")!==-1){az=null
}if(aD.indexOf("MSIE 6.0")==-1&&aD.indexOf("MSIE 7.0")==-1){az=null
}return az
};
var al=function(aA){var ay=an();
if(ay==null){return false
}for(var az=0;
az<Math.max(ay.length,aA.length);
az++){var aB=ay[az]-aA[az];
if(aB!=0){return(aB>0)?true:false
}}return true
};
var am=function(){if(ak){var ay=document.createElement("div");
document.body.appendChild(ay);
ay.outerHTML='<object classid="clsid:D27CDB6E-AE6D-11cf-96B8-444553540000" height="0" width="0" id="'+au+'"><param name="movie" value="'+ao+'"></param></object>';
ax.elt=document.getElementById(au)
}else{var ay=document.createElement("object");
ay.setAttribute("type","application/x-shockwave-flash");
ay.setAttribute("width",0);
ay.setAttribute("height",0);
ay.setAttribute("id",au);
ay.setAttribute("data",ao);
document.body.appendChild(ay);
ax.elt=ay
}};
ax.attachToOnload=function(ay){if(window.addEventListener){window.addEventListener("load",ay,true)
}else{if(window.attachEvent){window.attachEvent("onload",ay)
}else{onload=ay
}}};
if(document.readyState==="complete"){ax.loader()
}else{ax.attachToOnload(ax.loader)
}};
var P=(function(){var aj=function(ak){this.HOST=new aj(0);
this.USERINFO=new aj(1);
this.PORT=new aj(2);
this.PATH=new aj(3);
this.ordinal=ak
};
return aj
})();
var ag=(function(){var aj=function(){};
aj.getRealm=function(al){var an=al.authenticationParameters;
if(an==null){return null
}var am=/realm=(\"(.*)\")/i;
var ak=am.exec(an);
return(ak!=null&&ak.length>=3)?ak[2]:null
};
return aj
})();
function C(){this.Keys=new Array()
}var ab=(function(){var ak=function(al){this.weakKeys=al;
this.elements=[];
this.dictionary=new C()
};
var aj=ak.prototype;
aj.getlength=function(){return this.elements.length
};
aj.getItemAt=function(al){return this.dictionary[this.elements[al]]
};
aj.get=function(am){var al=this.dictionary[am];
if(al==undefined){al=null
}return al
};
aj.remove=function(an){for(var am=0;
am<this.elements.length;
am++){var al=(this.weakKeys&&(this.elements[am]==an));
var ao=(!this.weakKeys&&(this.elements[am]===an));
if(al||ao){this.elements.remove(am);
this.dictionary[this.elements[am]]=undefined;
break
}}};
aj.put=function(al,am){this.remove(al);
this.elements.push(al);
this.dictionary[al]=am
};
aj.isEmpty=function(){return this.length==0
};
aj.containsKey=function(an){for(var am=0;
am<this.elements.length;
am++){var al=(this.weakKeys&&(this.elements[am]==an));
var ao=(!this.weakKeys&&(this.elements[am]===an));
if(al||ao){return true
}}return false
};
aj.keySet=function(){return this.elements
};
aj.getvalues=function(){var al=[];
for(var am=0;
am<this.elements.length;
am++){al.push(this.dictionary[this.elements[am]])
}return al
};
return ak
})();
var N=(function(){var ak=function(){this.name="";
this.kind="";
this.values=[];
this.children=new ab()
};
var aj=ak.prototype;
aj.getWildcardChar=function(){return"*"
};
aj.addChild=function(am,an){if(am==null||am.length==0){throw new ArgumentError("A node may not have a null name.")
}var al=ak.createNode(am,this,an);
this.children.put(am,al);
return al
};
aj.hasChild=function(al,am){return null!=this.getChild(al)&&am==this.getChild(al).kind
};
aj.getChild=function(al){return this.children.get(al)
};
aj.getDistanceFromRoot=function(){var al=0;
var am=this;
while(!am.isRootNode()){al++;
am=am.parent
}return al
};
aj.appendValues=function(){if(this.isRootNode()){throw new ArgumentError("Cannot set a values on the root node.")
}if(this.values!=null){for(var al=0;
al<arguments.length;
al++){var am=arguments[al];
this.values.push(am)
}}};
aj.removeValue=function(am){if(this.isRootNode()){return
}for(var al=0;
al<this.values.length;
al++){if(this.values[al]==am){this.values.splice(al,1)
}}};
aj.getValues=function(){return this.values
};
aj.hasValues=function(){return this.values!=null&&this.values.length>0
};
aj.isRootNode=function(){return this.parent==null
};
aj.hasChildren=function(){return this.children!=null&&this.children.getlength()>0
};
aj.isWildcard=function(){return this.name!=null&&this.name==this.getWildcardChar()
};
aj.hasWildcardChild=function(){return this.hasChildren()&&this.children.containsKey(this.getWildcardChar())
};
aj.getFullyQualifiedName=function(){var al=new String();
var an=[];
var ao=this;
while(!ao.isRootNode()){an.push(ao.name);
ao=ao.parent
}an=an.reverse();
for(var am=0;
am<an.length;
am++){al+=an[am];
al+="."
}if(al.length>=1&&al.charAt(al.length-1)=="."){al=al.slice(0,al.length-1)
}return al.toString()
};
aj.getChildrenAsList=function(){return this.children.getvalues()
};
aj.findBestMatchingNode=function(aq,ap){var ao=this.findAllMatchingNodes(aq,ap);
var al=null;
var ar=0;
for(var am=0;
am<ao.length;
am++){var an=ao[am];
if(an.getDistanceFromRoot()>ar){ar=an.getDistanceFromRoot();
al=an
}}return al
};
aj.findAllMatchingNodes=function(at,ar){var av=[];
var al=this.getChildrenAsList();
for(var aq=0;
aq<al.length;
aq++){var an=al[aq];
var ao=an.matches(at,ar);
if(ao<0){continue
}if(ao>=at.length){do{if(an.hasValues()){av.push(an)
}if(an.hasWildcardChild()){var am=an.getChild(this.getWildcardChar());
if(am.kind!=this.kind){an=null
}else{an=am
}}else{an=null
}}while(an!=null)
}else{var au=an.findAllMatchingNodes(at,ao);
for(var ap=0;
ap<au.length;
ap++){av.push(au[ap])
}}}return av
};
aj.matches=function(am,al){if(al<0||al>=am.length){return -1
}if(this.matchesToken(am[al])){return al+1
}if(!this.isWildcard()){return -1
}else{if(this.kind!=am[al].kind){return -1
}do{al++
}while(al<am.length&&this.kind==am[al].kind);
return al
}};
aj.matchesToken=function(al){return this.name==al.name&&this.kind==al.kind
};
ak.createNode=function(al,an,am){var ao=new ak();
ao.name=al;
ao.parent=an;
ao.kind=am;
return ao
};
return ak
})();
var w=(function(){var aj=function(ak,al){this.kind=al;
this.name=ak
};
return aj
})();
window.Oid=(function(){var ak=function(al){this.rep=al
};
var aj=ak.prototype;
aj.asArray=function(){return this.rep
};
aj.asString=function(){var am="";
for(var al=0;
al<this.rep.length;
al++){am+=(this.rep[al].toString());
am+="."
}if(am.length>0&&am.charAt(am.length-1)=="."){am=am.slice(0,am.length-1)
}return am
};
ak.create=function(al){return new ak(al.split("."))
};
return ak
})();
var z=(function(){var aj=function(){};
aj.create=function(aq,am,ap){var ao=aq+":"+am;
var ak=[];
for(var an=0;
an<ao.length;
++an){ak.push(ao.charCodeAt(an))
}var al="Basic "+Base64.encode(ak);
return new ChallengeResponse(al,ap)
};
return aj
})();
function ah(){this.canHandle=function(aj){return false
};
this.handle=function(aj,ak){ak(null)
}
}window.PasswordAuthentication=(function(){function aj(al,ak){this.username=al;
this.password=ak
}aj.prototype.clear=function(){this.username=null;
this.password=null
};
return aj
})();
window.ChallengeRequest=(function(){var aj=function(ak,al){if(ak==null){throw new Error("location is not defined.")
}if(al==null){return
}var am="Application ";
if(al.indexOf(am)==0){al=al.substring(am.length)
}this.location=ak;
this.authenticationParameters=null;
var an=al.indexOf(" ");
if(an==-1){this.authenticationScheme=al
}else{this.authenticationScheme=al.substring(0,an);
if(al.length>an+1){this.authenticationParameters=al.substring(an+1)
}}};
return aj
})();
window.ChallengeResponse=(function(){var ak=function(al,am){this.credentials=al;
this.nextChallengeHandler=am
};
var aj=ak.prototype;
aj.clearCredentials=function(){if(this.credentials!=null){this.credentials=null
}};
return ak
})();
window.BasicChallengeHandler=(function(){var ak=function(){this.loginHandler=undefined;
this.loginHandlersByRealm={}
};
var aj=ak.prototype;
aj.setRealmLoginHandler=function(al,am){if(al==null){throw new ArgumentError("null realm")
}if(am==null){throw new ArgumentError("null loginHandler")
}this.loginHandlersByRealm[al]=am;
return this
};
aj.canHandle=function(al){return al!=null&&"Basic"==al.authenticationScheme
};
aj.handle=function(am,ap){if(am.location!=null){var an=this.loginHandler;
var al=ag.getRealm(am);
if(al!=null&&this.loginHandlersByRealm[al]!=null){an=this.loginHandlersByRealm[al]
}var ao=this;
if(an!=null){an(function(aq){if(aq!=null&&aq.username!=null){ap(z.create(aq.username,aq.password,ao))
}else{ap(null)
}});
return
}}ap(null)
};
aj.loginHandler=function(al){al(null)
};
return ak
})();
window.DispatchChallengeHandler=(function(){var at=function(){this.rootNode=new N();
var av="^(.*)://(.*)";
this.SCHEME_URI_PATTERN=new RegExp(av)
};
function al(aw,aA,av){var az=aq(aA);
var aB=aw;
for(var ay=0;
ay<az.length;
ay++){var ax=az[ay];
if(!aB.hasChild(ax.name,ax.kind)){return
}else{aB=aB.getChild(ax.name)
}}aB.removeValue(av)
}function aj(aw,aA,av){var az=aq(aA);
var aB=aw;
for(var ay=0;
ay<az.length;
ay++){var ax=az[ay];
if(!aB.hasChild(ax.name,ax.kind)){aB=aB.addChild(ax.name,ax.kind)
}else{aB=aB.getChild(ax.name)
}}aB.appendValues(av)
}function ar(ay,aw){var av=new Array();
if(aw!=null){var ax=am(ay,aw);
if(ax!=null){return ax.values
}}return av
}function ap(aA,aB){var av=null;
var aw=aB.location;
if(aw!=null){var az=am(aA,aw);
if(az!=null){var ay=az.getValues();
if(ay!=null){for(var aC=0;
aC<ay.length;
aC++){var ax=ay[aC];
if(ax.canHandle(aB)){av=ax;
break
}}}}}return av
}function am(aw,av){var ay=aq(av);
var ax=0;
return aw.findBestMatchingNode(ay,ax)
}function aq(aC){var aE=new Array();
if(aC==null||aC.length==0){return aE
}var aN=new RegExp("^(([^:/?#]+):(//))?([^/?#]*)?([^?#]*)(\\?([^#]*))?(#(.*))?");
var aw=aN.exec(aC);
if(aw==null){return aE
}var aF=aw[2]||"http";
var aB=aw[4];
var aJ=aw[5];
var ay=null;
var az=null;
var aD=null;
var ax=null;
if(aB!=null){var aH=aB;
var aA=aH.indexOf("@");
if(aA>=0){az=aH.substring(0,aA);
aH=aH.substring(aA+1);
var aM=az.indexOf(":");
if(aM>=0){aD=az.substring(0,aM);
ax=az.substring(aM+1)
}}var av=aH.indexOf(":");
if(av>=0){ay=aH.substring(av+1);
aH=aH.substring(0,av)
}}else{throw new ArgumentError("Hostname is required.")
}var aK=aH.split(/\./);
aK.reverse();
for(var aL=0;
aL<aK.length;
aL++){aE.push(new w(aK[aL],P.HOST))
}if(ay!=null){aE.push(new w(ay,P.PORT))
}else{if(ak(aF)>0){aE.push(new w(ak(aF).toString(),P.PORT))
}}if(az!=null){if(aD!=null){aE.push(new w(aD,P.USERINFO))
}if(ax!=null){aE.push(new w(ax,P.USERINFO))
}if(aD==null&&ax==null){aE.push(new w(az,P.USERINFO))
}}if(au(aJ)){if(aJ.charAt(0)=="/"){aJ=aJ.substring(1)
}if(au(aJ)){var aO=aJ.split("/");
for(var aI=0;
aI<aO.length;
aI++){var aG=aO[aI];
aE.push(new w(aG,P.PATH))
}}}return aE
}function ak(av){if(an[av.toLowerCase()]!=null){return an[av]
}else{return -1
}}function an(){http=80;
ws=80;
wss=443;
https=443
}function au(av){return av!=null&&av.length>0
}var ao=at.prototype;
ao.clear=function(){this.rootNode=new N()
};
ao.canHandle=function(av){return ap(this.rootNode,av)!=null
};
ao.handle=function(aw,ax){var av=ap(this.rootNode,aw);
if(av==null){return null
}return av.handle(aw,ax)
};
ao.register=function(aw,av){if(aw==null||aw.length==0){throw new Error("Must specify a location to handle challenges upon.")
}if(av==null){throw new Error("Must specify a handler to handle challenges.")
}aj(this.rootNode,aw,av);
return this
};
ao.unregister=function(aw,av){if(aw==null||aw.length==0){throw new Error("Must specify a location to un-register challenge handlers upon.")
}if(av==null){throw new Error("Must specify a handler to un-register.")
}al(this.rootNode,aw,av);
return this
};
return at
})();
window.NegotiableChallengeHandler=(function(){var ak=function(){this.candidateChallengeHandlers=new Array()
};
var al=function(am){var an=new Array();
for(var ao=0;
ao<am.length;
ao++){an.push(Oid.create(am[ao]).asArray())
}var ap=GssUtils.sizeOfSpnegoInitialContextTokenWithOids(null,an);
var aq=ByteBuffer.allocate(ap);
aq.skip(ap);
GssUtils.encodeSpnegoInitialContextTokenWithOids(null,an,aq);
return ByteArrayUtils.arrayToByteArray(Base64Util.encodeBuffer(aq))
};
var aj=ak.prototype;
aj.register=function(an){if(an==null){throw new Error("handler is null")
}for(var am=0;
am<this.candidateChallengeHandlers.length;
am++){if(an===this.candidateChallengeHandlers[am]){return this
}}this.candidateChallengeHandlers.push(an);
return this
};
aj.canHandle=function(am){return am!=null&&am.authenticationScheme=="Negotiate"&&am.authenticationParameters==null
};
aj.handle=function(aq,at){if(aq==null){throw Error(new ArgumentError("challengeRequest is null"))
}var av=new ab();
for(var ap=0;
ap<candidateChallengeHandlers.length;
ap++){var au=candidateChallengeHandlers[ap];
if(au.canHandle(aq)){try{var an=au.getSupportedOids();
for(var ao=0;
ao<an.length;
ao++){var am=new Oid(an[ao]).asString();
if(!av.containsKey(am)){av.put(am,au)
}}}catch(ar){}}}if(av.isEmpty()){at(null);
return
}};
return ak
})();
window.NegotiableChallengeHandler=(function(){var aj=function(){this.loginHandler=undefined
};
aj.prototype.getSupportedOids=function(){return new Array()
};
return aj
})();
window.NegotiableChallengeHandler=(function(){var aj=function(){this.loginHandler=undefined
};
aj.prototype.getSupportedOids=function(){return new Array()
};
return aj
})();
window.ChallengeHandlers=(function(){var aj=function(){};
aj._definedDefault=new ah();
aj.setDefault=function(ak){if(ak==null){throw new Error("challengeHandler not defined")
}aj._definedDefault=ak
};
aj.getDefault=function(){return aj._definedDefault
};
return aj
})();
var F={};
(function(){var am=l.getLogger("com.kaazing.gateway.client.html5.Windows1252");
var ao={8364:128,129:129,8218:130,402:131,8222:132,8230:133,8224:134,8225:135,710:136,8240:137,352:138,8249:139,338:140,141:141,381:142,143:143,144:144,8216:145,8217:146,8220:147,8221:148,8226:149,8211:150,8212:151,732:152,8482:153,353:154,8250:155,339:156,157:157,382:158,376:159};
var al={128:8364,129:129,130:8218,131:402,132:8222,133:8230,134:8224,135:8225,136:710,137:8240,138:352,139:8249,140:338,141:141,142:381,143:143,144:144,145:8216,146:8217,147:8220,148:8221,149:8226,150:8211,151:8212,152:732,153:8482,154:353,155:8250,156:339,157:157,158:382,159:376};
F.toCharCode=function(at){if(at<128||(at>159&&at<256)){return at
}else{var ar=al[at];
if(typeof(ar)=="undefined"){am.severe(this,"Windows1252.toCharCode: Error: Could not find "+at);
throw new Error("Windows1252.toCharCode could not find: "+at)
}return ar
}};
F.fromCharCode=function(at){if(at<256){return at
}else{var ar=ao[at];
if(typeof(ar)=="undefined"){am.severe(this,"Windows1252.fromCharCode: Error: Could not find "+at);
throw new Error("Windows1252.fromCharCode could not find: "+at)
}return ar
}};
var ap=String.fromCharCode(127);
var an=String.fromCharCode(0);
var aq="\n";
var ak=function(au){am.entering(this,"Windows1252.escapedToArray",au);
var ar=[];
for(var at=0;
at<au.length;
at++){var aw=F.fromCharCode(au.charCodeAt(at));
if(aw==127){at++;
if(at==au.length){ar.hasRemainder=true;
break
}var av=F.fromCharCode(au.charCodeAt(at));
switch(av){case 127:ar.push(127);
break;
case 48:ar.push(0);
break;
case 110:ar.push(10);
break;
case 114:ar.push(13);
break;
default:am.severe(this,"Windows1252.escapedToArray: Error: Escaping format error");
throw new Error("Escaping format error")
}}else{ar.push(aw)
}}return ar
};
var aj=function(at){am.entering(this,"Windows1252.toEscapedByteString",at);
var ar=[];
while(at.remaining()){var av=at.getUnsigned();
var au=String.fromCharCode(F.toCharCode(av));
switch(au){case ap:ar.push(ap);
ar.push(ap);
break;
case an:ar.push(ap);
ar.push("0");
break;
case aq:ar.push(ap);
ar.push("n");
break;
default:ar.push(au)
}}return ar.join("")
};
F.toArray=function(au,av){am.entering(this,"Windows1252.toArray",{s:au,escaped:av});
if(av){return ak(au)
}else{var ar=[];
for(var at=0;
at<au.length;
at++){ar.push(F.fromCharCode(au.charCodeAt(at)))
}return ar
}};
F.toByteString=function(at,au){am.entering(this,"Windows1252.toByteString",{buf:at,escaped:au});
if(au){return aj(at)
}else{var ar=[];
while(at.remaining()){var av=at.getUnsigned();
ar.push(String.fromCharCode(F.toCharCode(av)))
}return ar.join("")
}}
})();
var Y=(function(){var aj=function(ak,al){this.label=ak;
this.value=al
};
return aj
})();
var af=(function(){var ak=function(am){var an=new URI(am);
if(al(an.scheme)){this._uri=an
}else{throw new Error("HttpURI - invalid scheme: "+am)
}};
function al(am){return"http"==am||"https"==am
}var aj=ak.prototype;
aj.getURI=function(){return this._uri
};
aj.duplicate=function(am){try{return new ak(am)
}catch(an){throw an
}return null
};
aj.isSecure=function(){return("https"==this._uri.scheme)
};
aj.toString=function(){return this._uri.toString()
};
ak.replaceScheme=function(am,an){var ao=URI.replaceProtocol(am,an);
return new ak(ao)
};
return ak
})();
var ae=(function(){var ak=function(ap){var aq=new URI(ap);
if(ao(aq.scheme)){this._uri=aq;
if(aq.port==undefined){this._uri=new URI(ak.addDefaultPort(ap))
}}else{throw new Error("WSURI - invalid scheme: "+ap)
}};
function ao(ap){return"ws"==ap||"wss"==ap
}function an(ap){try{return new ak(ap)
}catch(aq){throw aq
}return null
}var aj=ak.prototype;
aj.getAuthority=function(){return this._uri.authority
};
aj.isSecure=function(){return"wss"==this._uri.scheme
};
aj.getHttpEquivalentScheme=function(){return this.isSecure()?"https":"http"
};
aj.toString=function(){return this._uri.toString()
};
var al=80;
var am=443;
ak.setDefaultPort=function(ap){if(ap.port==0){if(ap.scheme=="ws"){ap.port=al
}else{if(ap.scheme=="wss"){ap.port=am
}else{if(ap.scheme=="http"){ap.port=80
}else{if(ap.schemel=="https"){ap.port=443
}else{throw new Error("Unknown protocol: "+ap.scheme)
}}}}ap.authority=ap.host+":"+ap.port
}};
ak.addDefaultPort=function(ap){var aq=new URI(ap);
if(aq.port==undefined){ak.setDefaultPort(aq)
}return aq.toString()
};
ak.replaceScheme=function(ap,aq){var ar=URI.replaceProtocol(ap,aq);
return new ak(ar)
};
return ak
})();
var y=(function(){var ak={};
ak.ws="ws";
ak.wss="wss";
ak["javascript:wse"]="ws";
ak["javascript:wse+ssl"]="wss";
ak["javascript:ws"]="ws";
ak["javascript:wss"]="wss";
ak["flash:wsr"]="ws";
ak["flash:wsr+ssl"]="wss";
ak["flash:wse"]="ws";
ak["flash:wse+ssl"]="wss";
var al=function(aq){var ap=am(aq);
if(ao(ap)){this._uri=new URI(URI.replaceProtocol(aq,ak[ap]));
this._compositeScheme=ap;
this._location=aq
}else{throw new Error("WSCompositeURI - invalid composite scheme: "+am(aq))
}};
function am(ap){var aq=ap.indexOf("://");
if(aq>0){return ap.substr(0,aq)
}else{return""
}}function ao(ap){return ak[ap]!=null
}function an(ap){try{return new al(ap)
}catch(aq){throw aq
}return null
}var aj=al.prototype;
aj.isSecure=function(){var ap=this._uri.scheme;
return"wss"==ak[ap]
};
aj.getWSEquivalent=function(){try{var ap=ak[this._compositeScheme];
return ae.replaceScheme(this._location,ap)
}catch(aq){throw aq
}return null
};
aj.getPlatformPrefix=function(){if(this._compositeScheme.indexOf("javascript:")===0){return"javascript"
}else{if(this._compositeScheme.indexOf("flash:")===0){return"flash"
}else{return""
}}};
aj.toString=function(){return this._location
};
return al
})();
var V=(function(){var aj=function(){this._parent=null;
this._challengeResponse=new ChallengeResponse(null,null)
};
aj.prototype.toString=function(){return"[Channel]"
};
return aj
})();
var v=(function(){var ak=function(al,an,am){this._location=al;
this._protocol=an;
this._isBinary=am;
this._controlFrames={};
this._handshakePayload;
this._isEscape=false;
this._bufferedAmount=0
};
var aj=ak.prototype=new V();
aj.getBufferedAmount=function(){return this._bufferedAmount
};
aj.toString=function(){return"[WebSocketChannel "+_location+" "+_protocol!=null?_protocol:"- binary:"+isBinary+"]"
};
return ak
})();
var A=(function(){var ak=function(){this._nextHandler;
this._listener
};
var aj=ak.prototype;
aj.processConnect=function(am,al,an){this._nextHandler.processConnect(am,al,an)
};
aj.processAuthorize=function(am,al){this._nextHandler.processAuthorize(am,al)
};
aj.processTextMessage=function(al,am){this._nextHandler.processTextMessage(al,am)
};
aj.processBinaryMessage=function(am,al){this._nextHandler.processBinaryMessage(am,al)
};
aj.processClose=function(al){this._nextHandler.processClose(al)
};
aj.setListener=function(al){this._listener=al
};
aj.setNextHandler=function(al){this._nextHandler=al
};
return ak
})();
var L=(function(){var aj=function(){var ak="";
var al=""
};
aj.KAAZING_EXTENDED_HANDSHAKE="x-kaazing-handshake";
aj.KAAZING_SEC_EXTENSION_REVALIDATE="x-kaazing-http-revalidate";
aj.HEADER_SEC_EXTENSIONS="X-WebSocket-Extensions";
return aj
})();
var ad=(function(){var ak=function(al,an,am){this._location=al;
this._protocol=an;
this._isBinary=am;
this.requestHeaders=[];
this.responseHeaders={};
this.readyState=0;
this.authenticationReceived=false
};
var aj=ak.prototype=new v();
return ak
})();
var G=(function(){var ak=function(){};
var aj=ak.prototype;
aj.createChannel=function(al,ao,an){var am=new ad(al,ao,an);
if(ao){am.requestHeaders.push(new Y("X-WebSocket-Protocol",ao))
}am.requestHeaders.push(new Y(L.HEADER_SEC_EXTENSIONS,L.KAAZING_SEC_EXTENSION_REVALIDATE));
return am
};
return ak
})();
var B=(function(){var ak=function(){};
var aj=ak.prototype;
aj.createChannel=function(al,ao,an){var am=new ad(al,ao,an);
return am
};
return ak
})();
var M=(function(){var ak=function(al,an,am){this._location=al.getWSEquivalent();
this._protocol=an;
this._isBinary=am;
this._webSocket;
this._compositeScheme=al._compositeScheme;
this._connectionStrategies=[];
this._selectedChannel;
this.readyState=0;
this._closing=false;
this._compositeScheme=al._compositeScheme
};
var aj=ak.prototype=new v();
aj.getReadyState=function(){return this.readyState
};
aj.getWebSocket=function(){return this._webSocket
};
aj.getCompositeScheme=function(){return this._compositeScheme
};
aj.getNextStrategy=function(){if(this._connectionStrategies.length<=0){return null
}else{return this._connectionStrategies.shift()
}};
return ak
})();
var ac=(function(){var an="WebSocketControlFrameHandler";
var al=l.getLogger(an);
var am=function(){al.finest(an,"<init>")
};
var ak=function(ar,ap){var ao=0;
for(var aq=ap;
aq<ap+4;
aq++){ao=(ao<<8)+ar.get()
}return ao
};
var aj=am.prototype=new A();
aj.handleConnectionOpened=function(au,aw){al.finest(an,"handleConnectionOpened");
var av=au.responseHeaders;
if(av["X-WebSocket-Protocol"]!=null){au.protocol=av["X-WebSocket-Protocol"]
}if(av[L.HEADER_SEC_EXTENSIONS]!=null){var ar=av[L.HEADER_SEC_EXTENSIONS];
if(ar!=null&&ar.length>0){var aq=ar.split(",");
for(var ao=0;
ao<aq.length;
ao++){var ap=aq[ao].split(";");
if(ap.length>1){var at=ap[1].replace(/^\s+|\s+$/g,"");
au._controlFrames[parseInt(at,16)]=ap[0].replace(/^\s+|\s+$/g,"")
}}}}this._listener.connectionOpened(au,aw)
};
aj.handleMessageReceived=function(at,ar){al.finest(an,"handleMessageReceived",ar);
if(at._isEscape){at._isEscape=false;
this._listener.messageReceived(at,ar);
return
}if(ar==null||ar.limit<4){this._listener.messageReceived(at,ar);
return
}var ao=ar.position;
var au=ak(ar,0);
if(at._controlFrames[au]!=null){if(ar.limit==4){at._isEscape=true;
return
}else{if(L.KAAZING_SEC_EXTENSION_REVALIDATE==at._controlFrames[au]){var aq=ar.getString(Charset.UTF8).substr(1);
if(at._redirectUri!=null){if(typeof(at._redirectUri)=="string"){var ap=new URI(at._redirectUri);
aq=ap.scheme+"://"+ap.authority+aq
}else{aq=at._redirectUri.getHttpEquivalentScheme()+"://"+at._redirectUri.getAuthority()+aq
}}else{aq=at._location.getHttpEquivalentScheme()+"://"+at._location.getAuthority()+aq
}this._listener.authenticationRequested(at,aq,L.KAAZING_SEC_EXTENSION_REVALIDATE)
}}}else{ar.position=ao;
this._listener.messageReceived(at,ar)
}};
aj.setNextHandler=function(ap){this._nextHandler=ap;
var aq={};
var ao=this;
aq.connectionOpened=function(ar,at){ao.handleConnectionOpened(ar,at)
};
aq.messageReceived=function(at,ar){ao.handleMessageReceived(at,ar)
};
aq.connectionClosed=function(ar){ao._listener.connectionClosed(ar)
};
aq.connectionFailed=function(ar){ao._listener.connectionFailed(ar)
};
aq.authenticationRequested=function(au,ar,at){ao._listener.authenticationRequested(au,ar,at)
};
aq.redirected=function(at,ar){ao._listener.redirected(at,ar)
};
ap.setListener(aq)
};
aj.setListener=function(ao){this._listener=ao
};
return am
})();
var ai=(function(){var ak=l.getLogger("RevalidateHandler");
var al=function(am){ak.finest("ENTRY Revalidate.<init>");
this.channel=am
};
var aj=al.prototype;
aj.connect=function(am){ak.finest("ENTRY Revalidate.connect with {0}",am);
var ao=this;
var an=new XMLHttpRequest0();
an.open("GET",am+"&.kr="+Math.random(),true);
if(ao.channel._challengeResponse!=null&&ao.channel._challengeResponse.credentials!=null){an.setRequestHeader("Authorization",ao.channel._challengeResponse.credentials);
this.clearAuthenticationData(ao.channel)
}an.onreadystatechange=function(){switch(an.readyState){case 2:if(an.status==403){an.abort()
}break;
case 4:if(an.status==401){ao.handle401(ao.channel,am,an.getResponseHeader("WWW-Authenticate"));
return
}break
}};
an.send(null)
};
aj.clearAuthenticationData=function(am){if(am._challengeResponse!=null){am._challengeResponse.clearCredentials()
}};
aj.handle401=function(aq,am,ap){var ar=this;
var at=am;
if(at.indexOf("/;a/")>0){at=at.substring(0,at.indexOf("/;a/"))
}else{if(at.indexOf("/;ae/")>0){at=at.substring(0,at.indexOf("/;ae/"))
}else{if(at.indexOf("/;ar/")>0){at=at.substring(0,at.indexOf("/;ar/"))
}}}var ao=new ChallengeRequest(at,ap);
var an;
if(this.channel._challengeResponse.nextChallengeHandler!=null){an=this.channel._challengeResponse.nextChallengeHandler
}else{an=ChallengeHandlers.getDefault()
}if(an!=null&&an.canHandle(ao)){an.handle(ao,function(au){try{if(au!=null&&au.credentials!=null){ar.channel._challengeResponse=au;
ar.connect(am)
}}catch(av){}})
}};
return al
})();
var S=(function(){var am="WebSocketNativeDelegateHandler";
var al=l.getLogger(am);
var ak=function(){al.finest(am,"<init>")
};
var aj=ak.prototype=new A();
aj.processConnect=function(aq,ap,ar){al.finest(am,"connect",aq);
if(aq.readyState==2){throw new Error("WebSocket is already closed")
}if(aq._delegate==null){var ao=new a();
ao.parent=aq;
aq._delegate=ao;
an(ao,this)
}aq._delegate.connect(ap.toString(),ar)
};
aj.processTextMessage=function(ao,ap){al.finest(am,"connect",ao);
if(ao._delegate.readyState()==1){ao._delegate.send(ap)
}else{throw new Error("WebSocket is already closed")
}};
aj.processBinaryMessage=function(ap,ao){al.finest(am,"connect",ap);
if(ap._delegate.readyState()==1){ap._delegate.send(ao)
}else{throw new Error("WebSocket is already closed")
}};
aj.processClose=function(ao){al.finest(am,"close",ao);
try{ao._delegate.close()
}catch(ap){}};
var an=function(ao,aq){var ap={};
ap.connectionOpened=function(ar,at){aq._listener.connectionOpened(ar,at)
};
ap.messageReceived=function(at,ar){aq._listener.messageReceived(at,ar)
};
ap.connectionClosed=function(ar){aq._listener.connectionClosed(ar)
};
ap.connectionFailed=function(ar){aq._listener.connectionFailed(ar)
};
ap.authenticationRequested=function(au,ar,at){aq._listener.authenticationRequested(au,ar,at)
};
ap.redirected=function(at,ar){aq._listener.redirected(at,ar)
};
ao.setListener(ap)
};
return ak
})();
var D=(function(){var an="WebSocketNativeBalancingHandler";
var al=l.getLogger(an);
var am=function(){al.finest(an,"<init>")
};
var ak=function(ap,ao,aq){ao._redirecting=true;
ao._redirectUri=aq;
ap._nextHandler.processClose(ao)
};
var aj=am.prototype=new A();
aj.processConnect=function(ap,ao,aq){ap._balanced=false;
this._nextHandler.processConnect(ap,ao,aq)
};
aj.handleConnectionClosed=function(ao){if(ao._redirecting==true){ao._redirecting=false;
ao._redirected=true;
ao.handshakePayload.clear();
var ap=ao._protocol;
if(ap==null||ap.length==0){ap=L.KAAZING_EXTENDED_HANDSHAKE
}else{if(ap.indexOf(L.KAAZING_EXTENDED_HANDSHAKE)<0){ap+=","+L.KAAZING_EXTENDED_HANDSHAKE
}}this.processConnect(ao,ao._redirectUri,ap)
}else{this._listener.connectionClosed(ao)
}};
aj.handleMessageReceived=function(aq,ap){al.finest(an,"handleMessageReceived",ap);
if(aq._balanced||ap.remaining()<4){this._listener.messageReceived(aq,ap);
return
}var ao=ap.position;
var ar=ap.getBytes(3);
if(ar[0]==-17&&ar[1]==-125){var at=ap.getString(Charset.UTF8);
if(at.match("N$")){aq._balanced=true;
this._listener.connectionOpened(aq,"")
}else{if(at.indexOf("R")==0){var au=new ae(at.substring(1));
ak(this,aq,au)
}else{al.warning(an,"Invalidate balancing message: "+at)
}}return
}else{ap.position=ao;
this._listener.messageReceived(aq,ap)
}};
aj.setNextHandler=function(ap){this._nextHandler=ap;
var aq={};
var ao=this;
aq.connectionOpened=function(ar,at){ao._listener.connectionOpened(ar,at)
};
aq.messageReceived=function(at,ar){ao.handleMessageReceived(at,ar)
};
aq.connectionClosed=function(ar){ao.handleConnectionClosed(ar)
};
aq.connectionFailed=function(ar){ao._listener.connectionFailed(ar)
};
aq.authenticationRequested=function(au,ar,at){ao._listener.authenticationRequested(au,ar,at)
};
aq.redirected=function(at,ar){ao._listener.redirected(at,ar)
};
ap.setListener(aq)
};
aj.setListener=function(ao){this._listener=ao
};
return am
})();
var K=(function(){var aj="WebSocketNativeHandshakeHandler";
var at=l.getLogger(aj);
var an="Sec-WebSocket-Protocol";
var ao="Sec-WebSocket-Extensions";
var ap="Authorization";
var ar="WWW-Authenticate";
var ay="Set-Cookie";
var au="GET";
var az="HTTP/1.1";
var aC=":";
var aD=" ";
var aw="\r\n";
var ax=function(){at.finest(aj,"<init>")
};
var aA=function(aF,aH){at.finest(aj,"sendCookieRequest with {0}",aH);
var aE=new XMLHttpRequest0();
var aG=aF._location.getHttpEquivalentScheme()+"://"+aF._location.getAuthority()+(aF._location._uri.path||"");
aG=aG.replace(/[\/]?$/,"/;api/set-cookies");
aE.open("POST",aG,true);
aE.setRequestHeader("Content-Type","text/plain; charset=utf-8");
aE.onreadystatechange=function(){};
aE.send(aH)
};
var al=function(aJ,aH,aF){var aG=[];
var aE=[];
aG.push("WebSocket-Protocol");
aE.push("");
aG.push(an);
aE.push(aH._protocol);
aG.push(ao);
aE.push(L.KAAZING_SEC_EXTENSION_REVALIDATE);
aG.push(ap);
aE.push(aF);
var aI=am(aH._location,aG,aE);
aJ._nextHandler.processTextMessage(aH,aI)
};
var am=function(aF,aJ,aK){at.entering(aj,"encodeGetRequest");
var aM=[];
aM.push(au);
aM.push(aD);
var aL=[];
if(aF._uri.path!=undefined){aL.push(aF._uri.path)
}if(aF._uri.query!=undefined){aL.push("?");
aL.push(aF._uri.query)
}aM.push(aL.join(""));
aM.push(aD);
aM.push(az);
aM.push(aw);
for(var aG=0;
aG<aJ.length;
aG++){var aH=aJ[aG];
var aI=aK[aG];
if(aH!=null&&aI!=null){aM.push(aH);
aM.push(aC);
aM.push(aD);
aM.push(aI);
aM.push(aw)
}}aM.push(aw);
var aE=aM.join("");
return aE
};
var av=function(aE){var aF=aE.getString(Charset.UTF8);
return aF.split("\n")
};
var aq=function(aK,aI,aE){aI.handshakePayload.putBuffer(aE);
if(aE.capacity>0){return
}aI.handshakePayload.flip();
var aQ=av(aI.handshakePayload);
aI.handshakePayload.clear();
var aH="";
for(var aG=aQ.length-1;
aG>=0;
aG--){if(aQ[aG].indexOf("HTTP/1.1")==0){var aO=aQ[aG].split(" ");
aH=aO[1];
break
}}if("101"==aH){var aM="";
for(var aG in aQ){var aP=aQ[aG];
if(aP!=null&&aP.indexOf(ao)==0){aM=aP.substring(ao.length+2)
}else{if(aP!=null&&aP.indexOf(an)==0){aI.protocol=aP.substring(an.length+2)
}else{if(aP!=null&&aP.indexOf(ay)==0){aA(aI,aP.substring(ay.length+2))
}}}}if(aM.length>0){var aJ=aM.split(", ");
for(var aG in aJ){var aF=aJ[aG].split("; ");
if(aF.length>1){var aN=aF[1];
aI._controlFrames[parseInt(aN,16)]=aF[0]
}}}return
}else{if("401"==aH){aI.handshakestatus=2;
var aL="";
for(var aG in aQ){if(aQ[aG].indexOf(ar)==0){aL=aQ[aG].substring(ar.length+2);
break
}}aK._listener.authenticationRequested(aI,aI._location.toString(),aL)
}else{if(aI.handshakestatus<3){try{aI.handshakestatus=3;
aK._nextHandler.processClose(aI)
}finally{aK._listener.connectionFailed(aI)
}}}}};
var ak=function(aF,aE){if(aE.handshakestatus<3){try{aE.handshakestatus=3;
aF._nextHandler.processClose(aE)
}finally{aF._listener.connectionFailed(aE)
}}};
var aB=ax.prototype=new A();
aB.processConnect=function(aF,aE,aH){aF.handshakePayload=new ByteBuffer();
if(aH==null||aH.length==0){aH=L.KAAZING_EXTENDED_HANDSHAKE
}else{if(aH.indexOf(L.KAAZING_EXTENDED_HANDSHAKE)<0){aH+=","+L.KAAZING_EXTENDED_HANDSHAKE
}}this._nextHandler.processConnect(aF,aE,aH);
aF.handshakestatus=0;
var aG=this;
setTimeout(function(){if(aF.handshakestatus==0){ak(aG,aF)
}},5000)
};
aB.processAuthorize=function(aF,aE){al(this,aF,aE)
};
aB.handleConnectionOpened=function(aE,aG){at.finest(aj,"handleConnectionOpened");
if(L.KAAZING_EXTENDED_HANDSHAKE==aG){al(this,aE,null);
aE.handshakestatus=1;
var aF=this;
setTimeout(function(){if(aE.handshakestatus<2){ak(aF,aE)
}},5000)
}else{aE._balanced=true;
aE.handshakestatus=2;
this._listener.connectionOpened(aE,aE.protocol)
}};
aB.handleMessageReceived=function(aF,aE){at.finest(aj,"handleMessageReceived",aE);
if(aF.readyState==1){aF._isEscape=false;
this._listener.messageReceived(aF,aE)
}else{aq(this,aF,aE)
}};
aB.setNextHandler=function(aE){this._nextHandler=aE;
var aG=this;
var aF={};
aF.connectionOpened=function(aH,aI){aG.handleConnectionOpened(aH,aI)
};
aF.messageReceived=function(aI,aH){aG.handleMessageReceived(aI,aH)
};
aF.connectionClosed=function(aH){if(aH.handshakestatus<3){aH.handshakestatus=3;
aG._listener.connectionClosed(aH)
}};
aF.connectionFailed=function(aH){if(aH.handshakestatus<3){aH.handshakestatus=3;
aG._listener.connectionFailed(aH)
}};
aF.authenticationRequested=function(aJ,aH,aI){aG._listener.authenticationRequested(aJ,aH,aI)
};
aF.redirected=function(aI,aH){aG._listener.redirected(aI,aH)
};
aE.setListener(aF)
};
aB.setListener=function(aE){this._listener=aE
};
return ax
})();
var u=(function(){var al="WebSocketNativeAuthenticationHandler";
var ak=l.getLogger(al);
var am=function(){ak.finest(al,"<init>")
};
var aj=am.prototype=new A();
aj.handleClearAuthenticationData=function(an){if(an._challengeResponse!=null){an._challengeResponse.clearCredentials()
}};
aj.handleRemoveAuthenticationData=function(an){this.handleClearAuthenticationData(an);
an._challengeResponse=new ChallengeResponse(null,null)
};
aj.doError=function(an){this._nextHandler.processClose(an);
this.handleClearAuthenticationData(an);
this._listener.connectionFailed(an)
};
aj.handle401=function(au,an,at){var av=this;
var ar=au._location;
if(au.redirectUri!=null){ar=au._redirectUri
}if(L.KAAZING_SEC_EXTENSION_REVALIDATE==at){var aq=new ai(new ad(ar,au._protocol,au._isBinary));
aq.connect(an)
}else{var ap=new ChallengeRequest(ar.toString(),at);
var ao;
if(au._challengeResponse.nextChallengeHandler!=null){ao=au._challengeResponse.nextChallengeHandler
}else{ao=ChallengeHandlers.getDefault()
}if(ao!=null&&ao.canHandle(ap)){ao.handle(ap,function(aw){try{if(aw==null||aw.credentials==null){av.doError(au)
}else{au._challengeResponse=aw;
av._nextHandler.processAuthorize(au,aw.credentials)
}}catch(ax){av.doError(au)
}})
}else{this.doError(au)
}}};
aj.handleAuthenticate=function(ap,an,ao){ap.authenticationReceived=true;
this.handle401(ap,an,ao)
};
aj.setNextHandler=function(ao){this._nextHandler=ao;
var ap={};
var an=this;
ap.connectionOpened=function(aq,ar){an._listener.connectionOpened(aq,ar)
};
ap.messageReceived=function(ar,aq){an._listener.messageReceived(ar,aq)
};
ap.connectionClosed=function(aq){an._listener.connectionClosed(aq)
};
ap.connectionFailed=function(aq){an._listener.connectionFailed(aq)
};
ap.authenticationRequested=function(at,aq,ar){an.handleAuthenticate(at,aq,ar)
};
ap.redirected=function(ar,aq){an._listener.redirected(ar,aq)
};
ao.setListener(ap)
};
aj.setListener=function(an){this._listener=an
};
return am
})();
var E=(function(){var aj="WebSocketNativeHandler";
var ax=l.getLogger(aj);
var ak=function(){var az=new u();
return az
};
var an=function(){var az=new K();
return az
};
var al=function(){var az=new ac();
return az
};
var ay=function(){var az=new D();
return az
};
var ap=function(){var az=new S();
return az
};
var av=ak();
var aw=an();
var ar=al();
var am=ay();
var au=ap();
var ao=function(){ax.finest(aj,"<init>");
this.setNextHandler(av);
av.setNextHandler(aw);
aw.setNextHandler(ar);
ar.setNextHandler(am);
am.setNextHandler(au)
};
var aq=function(az,aA){ax.finest(aj,"<init>")
};
var at=ao.prototype=new A();
at.setNextHandler=function(az){this._nextHandler=az;
var aB=this;
var aA={};
aA.connectionOpened=function(aC,aD){aB._listener.connectionOpened(aC,aD)
};
aA.messageReceived=function(aD,aC){aB._listener.messageReceived(aD,aC)
};
aA.connectionClosed=function(aC){aB._listener.connectionClosed(aC)
};
aA.connectionFailed=function(aC){aB._listener.connectionFailed(aC)
};
aA.authenticationRequested=function(aE,aC,aD){aB._listener.authenticationRequested(aE,aC,aD)
};
aA.redirected=function(aD,aC){aB._listener.redirected(aD,aC)
};
az.setListener(aA)
};
at.setListener=function(az){this._listener=az
};
return ao
})();
var X=(function(){var am=l.getLogger("com.kaazing.gateway.client.html5.WebSocketEmulatedProxyDownstream");
var ay=512*1024;
var aq=1;
var aB=function(aF){am.entering(this,"WebSocketEmulatedProxyDownstream.<init>",aF);
this.retry=3000;
if(browser=="opera"||browser=="ie"){this.requiresEscaping=true
}var aI=new URI(aF);
var aG={http:80,https:443};
if(aI.port==undefined){aI.port=aG[aI.scheme];
aI.authority=aI.host+":"+aI.port
}this.origin=aI.scheme+"://"+aI.authority;
this.location=aF;
this.activeXhr=null;
this.reconnectTimer=null;
this.buf=new ByteBuffer();
var aH=this;
setTimeout(function(){ak(aH,true);
aH.activeXhr=aH.mostRecentXhr;
aE(aH,aH.mostRecentXhr)
},0);
am.exiting(this,"WebSocketEmulatedProxyDownstream.<init>")
};
var aw=aB.prototype;
var ap=0;
var an=255;
var aA=1;
var aD=128;
var at=127;
var au=3000;
aw.readyState=0;
function ak(aI,aF){am.entering(this,"WebSocketEmulatedProxyDownstream.connect");
if(aI.reconnectTimer!==null){aI.reconnectTimer=null
}var aH=new URI(aI.location);
var aG=[];
switch(browser){case"ie":aG.push(".kns=1");
break;
case"safari":aG.push(".kp=256");
break;
case"firefox":aG.push(".kp=1025");
break;
case"android":aG.push(".kp=4096");
aG.push(".kbp=4096");
break
}if(browser=="android"||browser.ios){aG.push(".kkt=20")
}aG.push(".kc=text/plain;charset=windows-1252");
aG.push(".kb=4096");
aG.push(".kid="+String(Math.random()).substring(2));
if(aG.length>0){if(aH.query===undefined){aH.query=aG.join("&")
}else{aH.query+="&"+aG.join("&")
}}var aJ=new XMLHttpRequest0();
aJ.id=aq++;
aJ.position=0;
aJ.opened=false;
aJ.reconnect=false;
aJ.requestClosing=false;
aJ.onprogress=function(){if(aJ==aI.activeXhr&&aI.readyState!=2){setTimeout(function(){aj(aI)
},0)
}};
aJ.onload=function(){if(aJ==aI.activeXhr&&aI.readyState!=2){aj(aI);
aJ.onerror=function(){};
aJ.ontimeout=function(){};
aJ.onreadystatechange=function(){};
if(!aJ.reconnect){ar(aI)
}else{if(aJ.requestClosing){ao(aI)
}else{if(aI.activeXhr==aI.mostRecentXhr){ak(aI);
aI.activeXhr=aI.mostRecentXhr;
aE(aI,aI.activeXhr)
}else{var aK=aI.mostRecentXhr;
aI.activeXhr=aK;
switch(aK.readyState){case 1:case 2:aE(aI,aK);
break;
case 3:aj(aI);
break;
case 4:aI.activeXhr.onload();
break;
default:}}}}}};
aJ.ontimeout=function(){am.entering(this,"WebSocketEmulatedProxyDownstream.connect.xhr.ontimeout");
ar(aI)
};
aJ.onerror=function(){am.entering(this,"WebSocketEmulatedProxyDownstream.connect.xhr.onerror");
ar(aI)
};
aJ.open("GET",aH.toString(),true);
aJ.send("");
aI.mostRecentXhr=aJ
}function aE(aF,aG){if(aF.location.indexOf("&.ki=p")==-1){setTimeout(function(){if(aG&&aG.readyState<3&&aF.readyState<2){aF.location+="&.ki=p";
ak(aF,false)
}},au)
}}aw.disconnect=function(){am.entering(this,"WebSocketEmulatedProxyDownstream.disconnect");
if(this.readyState!==2){ax(this)
}};
function ax(aF){am.entering(this,"WebSocketEmulatedProxyDownstream._disconnect");
if(aF.reconnectTimer!==null){clearTimeout(aF.reconnectTimer);
aF.reconnectTimer=null
}if(aF.mostRecentXhr!==null){aF.mostRecentXhr.onprogress=function(){};
aF.mostRecentXhr.onload=function(){};
aF.mostRecentXhr.onerror=function(){};
aF.mostRecentXhr.abort()
}if(aF.activeXhr!=aF.mostRecentXhr&&aF.activeXhr!==null){aF.activeXhr.onprogress=function(){};
aF.activeXhr.onload=function(){};
aF.activeXhr.onerror=function(){};
aF.activeXhr.abort()
}aF.lineQueue=[];
aF.lastEventId=null;
aF.location=null;
aF.readyState=2
}function aj(aO){var aT=aO.activeXhr;
var aN=aT.responseText;
if(aN.length>=ay){if(aO.activeXhr==aO.mostRecentXhr){ak(aO,false)
}}var aJ=aN.slice(aT.position);
aT.position=aN.length;
var aH=aO.buf;
var aU=F.toArray(aJ,aO.requiresEscaping);
if(aU.hasRemainder){aT.position--
}aH.position=aH.limit;
aH.putBytes(aU);
aH.position=0;
aH.mark();
parse:while(true){if(!aH.hasRemaining()){break
}var aP=aH.getUnsigned();
switch(aP&128){case ap:var aS=aH.indexOf(an);
if(aS==-1){break parse
}var aI=aH.array.slice(aH.position,aS);
var aK=new ByteBuffer(aI);
var aF=aS-aH.position;
aH.skip(aF+1);
aH.mark();
if(aP==aA){al(aO,aK)
}else{av(aO,aK)
}break;
case aD:var aG=0;
var aL=false;
while(aH.hasRemaining()){var aQ=aH.getUnsigned();
aG=aG<<7;
aG|=(aQ&127);
if((aQ&128)!=128){aL=true;
break
}}if(!aL){break parse
}if(aH.remaining()<aG){break parse
}var aM=aH.array.slice(aH.position,aH.position+aG);
var aR=new ByteBuffer(aM);
aH.skip(aG);
aH.mark();
az(aO,aR);
break;
default:throw new Error("Emulation protocol error. Unknown frame type: "+aP)
}}aH.reset();
aH.compact()
}function al(aG,aF){while(aF.remaining()){var aH=String.fromCharCode(aF.getUnsigned());
switch(aH){case"0":break;
case"1":aG.activeXhr.reconnect=true;
break;
case"2":aG.activeXhr.requestClosing=true;
break;
default:throw new Error("Protocol decode error. Unknown command: "+aH)
}}}function az(aH,aF){var aG=document.createEvent("Events");
aG.initEvent("message",true,true);
aG.lastEventId=aH.lastEventId;
aG.data=aF;
aG.decoder=n;
aG.origin=aH.origin;
if(aG.source!==null){aG.source=null
}if(typeof(aH.onmessage)==="function"){aH.onmessage(aG)
}}function av(aH,aF){var aG=document.createEvent("Events");
aG.initEvent("message",true,true);
aG.lastEventId=aH.lastEventId;
aG.text=aF;
aG.origin=aH.origin;
if(aG.source!==null){aG.source=null
}if(typeof(aH.onmessage)==="function"){aH.onmessage(aG)
}}function ao(aF){ar(aF)
}function ar(aF){if(aF.readyState!=2){aF.disconnect();
aC(aF)
}}function aC(aG){var aF=document.createEvent("Events");
aF.initEvent("error",true,true);
if(typeof(aG.onerror)==="function"){aG.onerror(aF)
}}return aB
})();
var R=(function(){var an=l.getLogger("WebSocketEmulatedProxy");
var az=function(){this.parent;
this._listener
};
var ax=az.prototype;
ax.connect=function(aD,aE){an.entering(this,"WebSocketEmulatedProxy.connect",{location:aD,subprotocol:aE});
this.URL=aD.replace("ws","http");
this.protocol=aE;
if(browser=="opera"||browser=="ie"){an.config(this,"WebSocketEmulatedProxy.<init>: browser is "+browser);
this.requiresEscaping=true
}this._sendQueue=[];
aj(this);
an.exiting(this,"WebSocketEmulatedProxy.<init>")
};
ax.readyState=0;
ax.bufferedAmount=0;
ax.URL="";
ax.onopen=function(){};
ax.onerror=function(){};
ax.onmessage=function(aD){};
ax.onclose=function(){};
var aC=128;
var au=0;
var ao=255;
var aB=1;
var al=[aB,48,49,ao];
var ay=[aB,48,50,ao];
ax.send=function(aE){an.entering(this,"WebSocketEmulatedProxy.send",{data:aE});
switch(this.readyState){case 0:an.severe(this,"WebSocketEmulatedProxy.send: Error: readyState is 0");
throw new Error("INVALID_STATE_ERR");
case 1:if(aE===null){an.severe(this,"WebSocketEmulatedProxy.send: Error: data is null");
throw new Error("data is null")
}var aD=new ByteBuffer();
if(typeof aE=="string"){an.finest(this,"WebSocketEmulatedProxy.send: Data is string");
aD.put(au);
aD.putString(aE,Charset.UTF8);
aD.put(ao)
}else{if(aE.constructor==ByteBuffer){an.finest(this,"WebSocketEmulatedProxy.send: Data is ByteBuffer");
aD.put(aC);
aA(aD,aE.remaining());
aD.putBuffer(aE)
}else{an.severe(this,"WebSocketEmulatedProxy.send: Error: Invalid type for send");
throw new Error("Invalid type for send")
}}aD.flip();
am(this,aD);
return true;
case 2:return false;
default:an.severe(this,"WebSocketEmulatedProxy.send: Error: invalid readyState");
throw new Error("INVALID_STATE_ERR")
}an.exiting(this,"WebSocketEmulatedProxy.send")
};
ax.close=function(){an.entering(this,"WebSocketEmulatedProxy.close");
switch(this.readyState){case 0:at(this);
break;
case 1:am(this,new ByteBuffer(ay));
at(this);
break
}};
ax.setListener=function(aD){this._listener=aD
};
function am(aE,aD){an.entering(this,"WebSocketEmulatedProxy.doSend",aD);
aE.bufferedAmount+=aD.remaining();
aE._sendQueue.push(aD);
if(!aE._writeSuspended){av(aE)
}}function av(aG){an.entering(this,"WebSocketEmulatedProxy.doFlush");
var aE=aG._sendQueue;
var aF=aE.length;
aG._writeSuspended=(aF>0);
if(aF>0){var aH=new XMLHttpRequest0();
aH.open("POST",aG._upstream+"&.kr="+Math.random(),true);
aH.onreadystatechange=function(){if(aH.readyState==4){an.finest(this,"WebSocketEmulatedProxy.doFlush: xhr.status="+aH.status);
switch(aH.status){case 200:setTimeout(function(){av(aG)
},0);
break;
default:at(aG);
break
}}};
var aD=new ByteBuffer();
while(aE.length){aD.putBuffer(aE.shift())
}aD.putBytes(al);
aD.flip();
if(browser=="firefox"){if(aH.sendAsBinary){an.finest(this,"WebSocketEmulatedProxy.doFlush: xhr.sendAsBinary");
aH.setRequestHeader("Content-Type","application/octet-stream");
aH.sendAsBinary(r(aD))
}else{aH.send(r(aD))
}}else{aH.setRequestHeader("Content-Type","text/plain; charset=utf-8");
aH.send(r(aD,aG.requiresEscaping))
}}aG.bufferedAmount=0
}var ap=function(aD){if(aD.challengeResponse==null){return
}aD.challengeResponse.clearCredentials()
};
var aj=function(aK){an.entering(this,"WebSocketEmulatedProxy.connect");
var aF=new URI(aK.URL);
aF.scheme=aF.scheme.replace("ws","http");
var aG=aK.requiresEscaping?"/;e/cte":"/;e/ct";
aF.path=aF.path.replace(/[\/]?$/,aG);
var aD=aF.toString();
var aL=aD.indexOf("?");
if(aL==-1){aD+="?"
}else{aD+="&"
}aD+=".kn="+String(Math.random()).substring(2);
an.finest(this,"WebSocketEmulatedProxy.connect: Connecting to "+aD);
var aJ=new XMLHttpRequest0();
var aH=false;
aJ.open("GET",aD,true);
aJ.setRequestHeader("X-WebSocket-Version","wseb-1.0");
for(var aI=0;
aI<aK.parent.requestHeaders.length;
aI++){var aE=aK.parent.requestHeaders[aI];
aJ.setRequestHeader(aE.label,aE.value)
}if(aK.challengeResponse!=null&&aK.challengeResponse.credentials!=null){aJ.setRequestHeader("Authorization",aK.challengeResponse.credentials);
ap(aK)
}aJ.onreadystatechange=function(){switch(aJ.readyState){case 2:if(aJ.status==403){aw(aK)
}else{timer=setTimeout(function(){if(!aH){aw(aK)
}},5000)
}break;
case 4:aH=true;
if(aJ.status==401){aK._listener.authenticationRequested(aK.parent,aJ._location,aJ.getResponseHeader("WWW-Authenticate"));
return
}if(aK.readyState<2){if(aJ.status==201){var aM=aJ.responseText.split("\n");
aK._upstream=aM[0];
var aN=aM[1];
aK._downstream=new X(aN);
var aO=aN.substring(0,aN.indexOf("/;e/"));
if(aO!=aK.parent._location.toString().replace("ws","http")){aK.parent._redirectUri=aO
}aq(aK,aK._downstream);
aK.parent.responseHeaders=aJ.getAllResponseHeaders();
ar(aK)
}else{aw(aK)
}}break
}};
aJ.send(null);
an.exiting(this,"WebSocketEmulatedProxy.connect")
};
var aA=function(aD,aE){an.entering(this,"WebSocketEmulatedProxy.encodeLength",{buf:aD,length:aE});
var aH=0;
var aF=0;
do{aF<<=8;
aF|=(aE&127);
aE>>=7;
aH++
}while(aE>0);
do{var aG=aF&255;
aF>>=8;
if(aH!=1){aG|=128
}aD.put(aG)
}while(--aH>0)
};
var ar=function(aD){an.entering(this,"WebSocketEmulatedProxy.doOpen");
aD.readyState=1;
aD._listener.connectionOpened(aD.parent,"")
};
function aw(aD){ap(aD);
if(aD.readyState<2){an.entering(this,"WebSocketEmulatedProxy.doError");
aD.readyState=2;
if(aD.onerror!=null){aD._listener.connectionFailed(aD.parent)
}}}var at=function(aD){an.entering(this,"WebSocketEmulatedProxy.doClose");
switch(aD.readyState){case 2:break;
case 0:case 1:aD.readyState=2;
aD._listener.connectionClosed(aD.parent);
break;
default:}};
var ak=function(aG,aF){an.finest("WebSocket.handleMessage: A WebSocket binary frame received on a WebSocket");
var aE;
if(aF.text){var aD=aF.text;
aE=ByteBuffer.allocate(aD.length);
aE.putString(aD,Charset.UTF8);
aE.position=0
}else{if(aF.data){aE=aF.data
}}aG._listener.messageReceived(aG.parent,aE)
};
var aq=function(aE,aD){an.entering(this,"WebSocketEmulatedProxy.bindHandlers");
aD.onmessage=function(aF){switch(aF.type){case"message":if(aE.readyState==1){ak(aE,aF)
}break
}};
aD.onerror=function(){try{aD.disconnect()
}finally{at(aE)
}}
};
return az
})();
var Z=(function(){var am="WebSocketEmulatedDelegateHandler";
var ak=l.getLogger(am);
var al=function(){ak.finest(am,"<init>")
};
var aj=al.prototype=new A();
aj.processConnect=function(aq,ap,ar){ak.finest(am,"connect",aq);
if(aq.readyState==2){throw new Error("WebSocket is already closed")
}var ao=new R();
ao.parent=aq;
aq._delegate=ao;
an(ao,this);
ao.connect(ap.toString(),ar)
};
aj.processTextMessage=function(ao,ap){ak.finest(am,"connect",ao);
if(ao.readyState==1){ao._delegate.send(ap)
}else{throw new Error("WebSocket is already closed")
}};
aj.processBinaryMessage=function(ap,ao){ak.finest(am,"connect",ap);
if(ap.readyState==1){ap._delegate.send(ao)
}else{throw new Error("WebSocket is already closed")
}};
aj.processClose=function(ao){try{ao._delegate.close()
}catch(ap){listener.connectionClosed(ao)
}};
var an=function(ao,aq){var ap={};
ap.connectionOpened=function(ar,at){aq._listener.connectionOpened(ar,at)
};
ap.messageReceived=function(at,ar){aq._listener.messageReceived(at,ar)
};
ap.connectionClosed=function(ar){aq._listener.connectionClosed(ar)
};
ap.connectionFailed=function(ar){aq._listener.connectionFailed(ar)
};
ap.authenticationRequested=function(au,ar,at){aq._listener.authenticationRequested(au,ar,at)
};
ap.redirected=function(at,ar){aq._listener.redirected(at,ar)
};
ao.setListener(ap)
};
return al
})();
var W=(function(){var am="WebSocketEmulatedAuthenticationHandler";
var ak=l.getLogger(am);
var al=function(){ak.finest(am,"<init>")
};
var aj=al.prototype=new A();
aj.handleClearAuthenticationData=function(an){if(an._challengeResponse!=null){an._challengeResponse.clearCredentials()
}};
aj.handleRemoveAuthenticationData=function(an){this.handleClearAuthenticationData(an);
an._challengeResponse=new ChallengeResponse(null,null)
};
aj.handle401=function(aq,aw,av){var at=this;
if(L.KAAZING_SEC_EXTENSION_REVALIDATE==av){var ar=new ai(aq);
ar.connect(aw)
}else{var au=aw;
if(au.indexOf("/;e/")>0){au=au.substring(0,au.indexOf("/;e/"))
}var ao=new ae(au.replace("http","ws"));
var ap=new ChallengeRequest(au,av);
var an;
if(aq._challengeResponse.nextChallengeHandler!=null){an=aq._challengeResponse.nextChallengeHandler
}else{an=ChallengeHandlers.getDefault()
}if(an!=null&&an.canHandle(ap)){an.handle(ap,function(ax){try{if(ax==null||ax.credentials==null){at.handleClearAuthenticationData(aq);
at._listener.connectionFailed(aq)
}else{aq._challengeResponse=ax;
at.processConnect(aq,ao,aq._protocol)
}}catch(ay){at.handleClearAuthenticationData(aq);
at._listener.connectionFailed(aq)
}})
}else{this.handleClearAuthenticationData(aq);
this._listener.connectionFailed(aq)
}}};
aj.processConnect=function(aq,an,ar){if(aq._challengeResponse!=null&&aq._challengeResponse.credentials!=null){var ao=aq._challengeResponse.credentials.toString();
var ap=new Y("Authorization",ao);
aq.requestHeaders.push(ap);
this.handleClearAuthenticationData(aq)
}this._nextHandler.processConnect(aq,an,ar)
};
aj.handleAuthenticate=function(ap,an,ao){ap.authenticationReceived=true;
this.handle401(ap,an,ao)
};
aj.setNextHandler=function(ao){this._nextHandler=ao;
var ap={};
var an=this;
ap.connectionOpened=function(aq,ar){an._listener.connectionOpened(aq,ar)
};
ap.messageReceived=function(ar,aq){an._listener.messageReceived(ar,aq)
};
ap.connectionClosed=function(aq){an._listener.connectionClosed(aq)
};
ap.connectionFailed=function(aq){an._listener.connectionFailed(aq)
};
ap.authenticationRequested=function(at,aq,ar){an.handleAuthenticate(at,aq,ar)
};
ap.redirected=function(ar,aq){an._listener.redirected(ar,aq)
};
ao.setListener(ap)
};
aj.setListener=function(an){this._listener=an
};
return al
})();
var J=(function(){var aj="WebSocketEmulatedHandler";
var au=l.getLogger(aj);
var ak=function(){var av=new W();
return av
};
var al=function(){var av=new ac();
return av
};
var an=function(){var av=new Z();
return av
};
var at=ak();
var ap=al();
var ar=an();
var am=function(){au.finest(aj,"<init>");
this.setNextHandler(at);
at.setNextHandler(ap);
ap.setNextHandler(ar)
};
var ao=function(av,aw){au.finest(aj,"<init>")
};
var aq=am.prototype=new A();
aq.setNextHandler=function(av){this._nextHandler=av;
var ax=this;
var aw={};
aw.connectionOpened=function(ay,az){ax._listener.connectionOpened(ay,az)
};
aw.messageReceived=function(az,ay){ax._listener.messageReceived(az,ay)
};
aw.connectionClosed=function(ay){ax._listener.connectionClosed(ay)
};
aw.connectionFailed=function(ay){ax._listener.connectionFailed(ay)
};
aw.authenticationRequested=function(aA,ay,az){ax._listener.authenticationRequested(aA,ay,az)
};
aw.redirected=function(az,ay){ax._listener.redirected(az,ay)
};
av.setListener(aw)
};
aq.setListener=function(av){this._listener=av
};
return am
})();
var H=(function(){var al="WebSocketFlashEmulatedDelegateHandler";
var ak=l.getLogger(al);
var an=function(){ak.finest(al,"<init>")
};
var aj=an.prototype=new A();
aj.processConnect=function(aq,ap,ar){ak.finest(al,"connect",aq);
if(aq.readyState==2){throw new Error("WebSocket is already closed")
}var ao=new f();
ao.parent=aq;
aq._delegate=ao;
am(ao,this);
ao.connect(ap.toString(),ar)
};
aj.processTextMessage=function(ao,ap){ak.finest(al,"connect",ao);
if(ao.readyState==1){ao._delegate.send(ap)
}else{throw new Error("WebSocket is already closed")
}};
aj.processBinaryMessage=function(ap,ao){ak.finest(al,"connect",ap);
if(ap.readyState==1){ap._delegate.send(ao)
}else{throw new Error("WebSocket is already closed")
}};
aj.processClose=function(ao){ak.finest(al,"close",ao);
if(ao.readyState==1){ao._delegate.close()
}else{throw new Error("WebSocket is already closed")
}};
var am=function(ao,aq){var ap={};
ap.connectionOpened=function(ar,at){aq._listener.connectionOpened(ar,at)
};
ap.messageReceived=function(at,ar){aq._listener.messageReceived(at,ar)
};
ap.connectionClosed=function(ar){aq._listener.connectionClosed(ar)
};
ap.connectionFailed=function(ar){aq._listener.connectionFailed(ar)
};
ap.authenticationRequested=function(au,ar,at){aq._listener.authenticationRequested(au,ar,at)
};
ap.redirected=function(at,ar){at._redirectUri=ar
};
ao.setListener(ap)
};
return an
})();
var T=(function(){var aj="WebSocketFlashEmulatedHandler";
var au=l.getLogger(aj);
var ak=function(){var av=new W();
return av
};
var al=function(){var av=new ac();
return av
};
var an=function(){var av=new H();
return av
};
var at=ak();
var ap=al();
var ar=an();
var am=function(){au.finest(aj,"<init>");
this.setNextHandler(at);
at.setNextHandler(ap);
ap.setNextHandler(ar)
};
var ao=function(av,aw){au.finest(aj,"<init>")
};
var aq=am.prototype=new A();
aq.setNextHandler=function(av){this._nextHandler=av;
var ax=this;
var aw={};
aw.connectionOpened=function(ay,az){ax._listener.connectionOpened(ay,az)
};
aw.messageReceived=function(az,ay){ax._listener.messageReceived(az,ay)
};
aw.connectionClosed=function(ay){ax._listener.connectionClosed(ay)
};
aw.connectionFailed=function(ay){ax._listener.connectionFailed(ay)
};
aw.authenticationRequested=function(aA,ay,az){ax._listener.authenticationRequested(aA,ay,az)
};
aw.redirected=function(az,ay){ax._listener.redirected(az,ay)
};
av.setListener(aw)
};
aq.setListener=function(av){this._listener=av
};
return am
})();
var Q=(function(){var am="WebSocketFlashRtmpDelegateHandler";
var ak=l.getLogger(am);
var ao;
var al=function(){ak.finest(am,"<init>");
ao=this
};
var aj=al.prototype=new A();
aj.processConnect=function(ar,aq,at){ak.finest(am,"connect",ar);
if(ar.readyState==2){throw new Error("WebSocket is already closed")
}var ap=new e();
ap.parent=ar;
ar._delegate=ap;
an(ap,this);
ap.connect(aq.toString(),at)
};
aj.processTextMessage=function(ap,aq){ak.finest(am,"connect",ap);
if(ap.readyState==1){ap._delegate.send(aq)
}else{throw new Error("WebSocket is already closed")
}};
aj.processBinaryMessage=function(aq,ap){ak.finest(am,"connect",aq);
if(aq.readyState==1){aq._delegate.send(ap)
}else{throw new Error("WebSocket is already closed")
}};
aj.processClose=function(ap){ak.finest(am,"close",ap);
if(ap.readyState==1){ap._delegate.close()
}else{throw new Error("WebSocket is already closed")
}};
var an=function(ap,ar){var aq={};
aq.connectionOpened=function(at,au){ar._listener.connectionOpened(at,au)
};
aq.messageReceived=function(au,at){ar._listener.messageReceived(au,at)
};
aq.connectionClosed=function(at){ar._listener.connectionClosed(at)
};
aq.connectionFailed=function(at){ar._listener.connectionFailed(at)
};
aq.authenticationRequested=function(av,at,au){ar._listener.authenticationRequested(av,at,au)
};
aq.redirected=function(au,at){au._redirectUri=at
};
ap.setListener(aq)
};
return al
})();
var U=(function(){var aj="WebSocketFlashRtmpHandler";
var au=l.getLogger(aj);
var ak=function(){var av=new W();
return av
};
var al=function(){var av=new ac();
return av
};
var am=function(){var av=new Q();
return av
};
var at=ak();
var ao=al();
var ar=am();
var ap=function(){au.finest(aj,"<init>");
this.setNextHandler(at);
at.setNextHandler(ao);
ao.setNextHandler(ar)
};
var an=function(av,aw){au.finest(aj,"<init>")
};
var aq=ap.prototype=new A();
aq.setNextHandler=function(av){this._nextHandler=av;
var ax=this;
var aw={};
aw.connectionOpened=function(ay,az){ax._listener.connectionOpened(ay,az)
};
aw.messageReceived=function(az,ay){ax._listener.messageReceived(az,ay)
};
aw.connectionClosed=function(ay){ax._listener.connectionClosed(ay)
};
aw.connectionFailed=function(ay){ax._listener.connectionFailed(ay)
};
aw.authenticationRequested=function(aA,ay,az){ax._listener.authenticationRequested(aA,ay,az)
};
aw.redirected=function(az,ay){ax._listener.redirected(az,ay)
};
av.setListener(aw)
};
aq.setListener=function(av){this._listener=av
};
return ap
})();
var aa=(function(){var am="WebSocketSelectedHandler";
var ak=l.getLogger(am);
var al=function(){ak.fine(am,"<init>")
};
var aj=al.prototype=new A();
aj.processConnect=function(ao,an,ap){ak.fine(am,"connect",ao);
if(ao.readyState==2){throw new Error("WebSocket is already closed")
}this._nextHandler.processConnect(ao,an,ap)
};
aj.handleConnectionOpened=function(ao,ap){ak.fine(am,"handleConnectionOpened");
var an=ao;
if(an.readyState==0){an.readyState=1;
this._listener.connectionOpened(ao,ap)
}};
aj.handleMessageReceived=function(ao,an){ak.fine(am,"handleMessageReceived",an);
if(ao.readyState!=1){return
}this._listener.messageReceived(ao,an)
};
aj.handleConnectionClosed=function(ao){ak.fine(am,"handleConnectionClosed");
var an=ao;
if(an.readyState!=2){an.readyState=2;
this._listener.connectionClosed(ao)
}};
aj.handleConnectionFailed=function(an){ak.fine(am,"connectionFailed");
if(an.readyState!=2){an.readyState=2;
this._listener.connectionFailed(an)
}};
aj.setNextHandler=function(an){this._nextHandler=an;
var ao={};
var ap=this;
ao.connectionOpened=function(aq,ar){ap.handleConnectionOpened(aq,ar)
};
ao.redirected=function(ar,aq){throw new Error("invalid event received")
};
ao.authenticationRequested=function(at,aq,ar){throw new Error("invalid event received")
};
ao.messageReceived=function(ar,aq){ap.handleMessageReceived(ar,aq)
};
ao.connectionClosed=function(aq){ap.handleConnectionClosed(aq)
};
ao.connectionFailed=function(aq){ap.handleConnectionFailed(aq)
};
an.setListener(ao)
};
aj.setListener=function(an){this._listener=an
};
return al
})();
var O=(function(){var aj=function(al,am,ak){this._nativeEquivalent=al;
this._handler=am;
this._channelFactory=ak
};
return aj
})();
var I=(function(){var ak="WebSocketCompositeHandler";
var aB=l.getLogger(ak);
var ay="javascript:ws";
var an="javascript:wss";
var ax="javascript:wse";
var aE="javascript:wse+ssl";
var az="flash:wse";
var ar="flash:wse+ssl";
var ap="flash:wsr";
var at="flash:wsr+ssl";
var aH={};
var am={};
var al=new B();
var aj=new G();
var au=function(){this._handlerListener=aq(this);
this._nativeHandler=aA(this);
this._emulatedHandler=aC(this);
this._emulatedFlashHandler=ao(this);
this._rtmpFlashHandler=aF(this);
aB.finest(ak,"<init>");
aw();
aH[ay]=new O("ws",this._nativeHandler,al);
aH[an]=new O("wss",this._nativeHandler,al);
aH[ax]=new O("ws",this._emulatedHandler,aj);
aH[aE]=new O("wss",this._emulatedHandler,aj);
aH[az]=new O("ws",this._emulatedFlashHandler,aj);
aH[ar]=new O("wss",this._emulatedFlashHandler,aj);
aH[ap]=new O("ws",this._rtmpFlashHandler,aj);
aH[at]=new O("wss",this._rtmpFlashHandler,aj)
};
function av(){if(browser!="ie"){return false
}var aI=navigator.appVersion;
return(aI.indexOf("MSIE 6.0")>=0||aI.indexOf("MSIE 7.0")>=0)
}function aw(){if(av()){am.ws=new Array(ay,az,ax);
am.wss=new Array(an,ar,aE)
}else{am.ws=new Array(ay,ax);
am.wss=new Array(an,aE)
}}function aq(aJ){var aI={};
aI.connectionOpened=function(aK,aL){aJ.handleConnectionOpened(aK,aL)
};
aI.messageReceived=function(aL,aK){aJ.handleMessageReceived(aL,aK)
};
aI.connectionClosed=function(aK){aJ.handleConnectionClosed(aK)
};
aI.connectionFailed=function(aK){aJ.handleConnectionFailed(aK)
};
aI.authenticationRequested=function(aM,aK,aL){};
aI.redirected=function(aL,aK){};
return aI
}function aA(aJ){var aI=new aa();
var aK=new E();
aI.setListener(aJ._handlerListener);
aI.setNextHandler(aK);
return aI
}function aC(aJ){var aI=new aa();
var aK=new J();
aI.setListener(aJ._handlerListener);
aI.setNextHandler(aK);
return aI
}function ao(aK){var aI=new aa();
var aJ=new T();
aI.setListener(aK._handlerListener);
aI.setNextHandler(aJ);
return aI
}function aF(aK){var aI=new aa();
var aJ=new U();
aI.setListener(aK._handlerListener);
aI.setNextHandler(aJ);
return aI
}var aD=function(aM,aL){var aP=aH[aL];
var aK=aP._channelFactory;
var aI=aM._location;
var aO=aM._protocol;
var aN=aM._isBinary;
var aJ=aK.createChannel(aI,aO,aN);
aM._selectedChannel=aJ;
aJ.parent=aM;
aJ._handler=aP._handler;
aJ._handler.processConnect(aM._selectedChannel,aI,aO)
};
var aG=au.prototype;
aG.fallbackNext=function(aJ){aB.finest(ak,"fallbackNext");
var aI=aJ.getNextStrategy();
if(aI==null){this.doClose(aJ)
}else{aD(aJ,aI)
}};
aG.doOpen=function(aI){if(aI.readyState==0){aI.readyState=1;
aI._webSocket.handleOpen()
}};
aG.doClose=function(aI){if(aI.readyState==0||aI.readyState==1){aI.readyState=2;
aI._webSocket.handleClose()
}};
aG.processConnect=function(aN,aJ,aP){aB.finest(ak,"connect",aN);
var aI=aN;
aB.finest("Current ready state = "+aI.readyState);
if(aI.readyState==1){aB.fine("Attempt to reconnect an existing open WebSocket to a different location");
throw new Error("Attempt to reconnect an existing open WebSocket to a different location")
}var aK=aI._compositeScheme;
if(aK!="ws"&&aK!="wss"){var aO=aH[aK];
if(aO==null){throw new Error("Invalid connection scheme: "+aK)
}aB.finest("Turning off fallback since the URL is prefixed with java:");
aI._connectionStrategies.push(aK)
}else{var aM=am[aK];
if(aM!=null){for(var aL=0;
aL<aM.length;
aL++){aI._connectionStrategies.push(aM[aL])
}}else{throw new Error("Invalid connection scheme: "+aK)
}}this.fallbackNext(aI)
};
aG.processTextMessage=function(aL,aK){aB.finest(ak,"send",aK);
var aJ=aL;
if(aJ.readyState!=1){aB.fine("Attempt to post message on unopened or closed web socket");
throw new Error("Attempt to post message on unopened or closed web socket")
}var aI=aJ._selectedChannel;
aI._handler.processTextMessage(aI,aK)
};
aG.processBinaryMessage=function(aL,aK){aB.finest(ak,"send",aK);
var aJ=aL;
if(aJ.readyState!=1){aB.fine("Attempt to post message on unopened or closed web socket");
throw new Error("Attempt to post message on unopened or closed web socket")
}var aI=aJ._selectedChannel;
aI._handler.processBinaryMessage(aI,aK)
};
aG.processClose=function(aK){aB.finest(ak,"close");
var aJ=aK;
if(aJ&&aJ.readyState==2){aB.fine("WebSocket already closed");
throw new Error("WebSocket already closed")
}if(aJ&&!aJ._closing){aJ._closing=true;
var aI=aJ._selectedChannel;
aI._handler.processClose(aI)
}};
aG.setListener=function(aI){this._listener=aI
};
aG.handleConnectionOpened=function(aJ,aK){var aI=aJ.parent;
this.doOpen(aI)
};
aG.handleMessageReceived=function(aK,aI){var aJ=aK.parent;
aJ._webSocket.handleMessage(aI)
};
aG.handleConnectionClosed=function(aJ){var aI=aJ.parent;
if(aI._closing){this.doClose(aI)
}else{if(aI.readyState==0&&!aJ.authenticationReceived){this.fallbackNext(aI)
}else{this.doClose(aI)
}}};
aG.handleConnectionFailed=function(aJ){var aI=aJ.parent;
if(aI._closing){this.doClose(aI)
}else{if(aI.readyState==0&&!aJ.authenticationReceived){this.fallbackNext(aI)
}else{this.doClose(aI)
}}};
return au
})();
(function(){var aj=new I();
window.WebSocket=(function(){var ak="WebSocket";
var ar=l.getLogger(ak);
var an=function(au,av){ar.entering(this,"WebSocket.<init>",{url:au,protocol:av});
this.readyState=0;
this.location=au;
this.protocol=av;
this._queue=[];
at(this,au,av)
};
var at=function(aw,au,ax){var av=new y(au);
aw._channel=new M(av,ax,false);
aw._channel._webSocket=aw;
aj.processConnect(aw._channel,av.getWSEquivalent(),ax)
};
function ao(aw,au){ar.entering(this,"WebSocket.doOpen");
if(aw.readyState<1){aw.readyState=1;
if(typeof(aw.onopen)!=="undefined"){if(!au){try{au=document.createEvent("Events");
au.initEvent("open",true,true)
}catch(ax){au={type:"open",bubbles:true,cancelable:true}
}}try{aw.onopen(au)
}catch(av){ar.severe(this,"WebSocket.onopen: Error thrown from application")
}}}}var am=an.prototype;
am.getURL=function(){return this.location
};
am.getProtocol=function(){return this._channel.protocol||""
};
am.getReadyState=function(){return this.readyState
};
am.send=function(au){switch(this.readyState){case 0:ar.error("WebSocket.send: Error: Attempt to send message on unopened or closed WebSocket");
throw new Error("Attempt to send message on unopened or closed WebSocket");
case 1:aj.processTextMessage(this._channel,au);
return true;
case 2:return false;
default:ar.error("WebSocket.send: Illegal state error");
throw new Error("Illegal state error")
}};
am.close=function(){aj.processClose(this._channel)
};
am.handleOpen=function(av){switch(this.readyState){case 0:ao(this,av);
break;
case 1:case 2:var au=(av?" from "+av.target:"");
ar.severe(this,"WebSocket.openHandler: Error: Invalid readyState for open event"+au);
throw new Error("Invalid readyState for open event"+au);
default:ar.severe(this,"WebSocket.openHandler: Error: Invalid readyState "+_readyState);
throw new Error("Socket has invalid readyState: "+_readyState)
}};
var al=function(aw,au){if(typeof(aw.onmessage)==="function"){var av;
try{av=document.createEvent("Events");
av.initEvent("message",true,true)
}catch(ax){av={type:"message",bubbles:true,cancelable:true}
}av.data=i(au,Charset.UTF8);
av.source=aw;
aw.onmessage(av)
}};
var aq=function(ax){var ay=new Date().getTime();
var av=ay+50;
while(ax._queue.length>0){if(new Date().getTime()>av){setTimeout(function(){aq(ax)
},0);
return
}var au=ax._queue.shift();
var aw=false;
try{al(ax,au);
aw=true
}finally{if(!aw){if(ax._queue.length==0){ax._delivering=false
}else{setTimeout(function(){aq(ax)
},0)
}}}}ax._delivering=false
};
am.handleMessage=function(au){switch(this.readyState){case 1:this._queue.push(au);
if(!this._delivering){this._delivering=true;
aq(this)
}break;
case 0:case 2:break;
default:throw new Error("Socket has invalid readyState: "+$this.readyState)
}};
var ap=function(av,au){ar.entering(av,"WebSocket.doClose");
if(av.readyState<2){av.readyState=2;
delete av._channel;
if(typeof(av.onclose)!=="undefined"){setTimeout(function(){if(!au){try{au=document.createEvent("Events");
au.initEvent("close",true,true)
}catch(ax){au={type:"close",bubbles:true,cancelable:true}
}}try{av.onclose(au)
}catch(aw){ar.severe(this,"WebSocket.onclose: Error thrown from application")
}},0)
}}};
am.handleClose=function(au){ap(this,au)
};
am.handleError=function(au){ap(this,au)
};
return an
})();
window.WebSocket.__impls__={};
window.WebSocket.__impls__["flash:wse"]=f
}());
(function(){window.ByteSocket=(function(){var ak="ByteSocket";
var ao=l.getLogger(ak);
var au=new I();
var ap=function(ay,az){ao.entering(this,"ByteSocket.<init>",{url:ay,subprotocol:az});
$this=this;
this.readyState=0;
this.location=ay;
this.protocol=az;
this._queue=[];
ax(this,ay,az)
};
var ax=function(aA,ay,aB){var az=new y(ay);
aA._channel=new M(az,aB,true);
aA._channel._webSocket=aA;
au.processConnect(aA._channel,az.getWSEquivalent(),aB)
};
var aq=ap.prototype;
aq.send=function(ay){ao.entering(this,"ByteSocket.send",ay);
if(ay.constructor!=window.ByteBuffer){throw new Error("ByteSocket.send must be called with a ByteBuffer argument")
}switch(this.readyState){case 0:ao.severe(this,"ByteSocket.send: Error: readyState is 0");
throw new Error("INVALID_STATE_ERR");
case 1:if(ay===null){ao.severe(this,"ByteSocket.send: Error: data is null");
throw new Error("data is null")
}au.processBinaryMessage(this._channel,ay);
am(this);
return true;
case 2:return false;
default:ao.severe(this,"ByteSocket.send: Error: Invalid readyState "+readyState);
throw new Error("INVALID_STATE_ERR")
}};
aq.handleOpen=function(az){switch(this.readyState){case 0:at(this,az);
break;
case 1:case 2:var ay=(az?" from "+az.target:"");
WSLOG.severe(this,"WebSocket.openHandler: Error: Invalid readyState for open event"+ay);
throw new Error("Invalid readyState for open event"+ay);
default:WSLOG.severe(this,"WebSocket.openHandler: Error: Invalid readyState "+_readyState);
throw new Error("Socket has invalid readyState: "+_readyState)
}};
var am=function(ay){};
aq.postMessage=aq.send;
aq.disconnect=aq.close;
aq.close=function(){ao.entering(this,"ByteSocket.close");
au.processClose(this._channel)
};
function at(aA,ay){ao.entering(aA,"ByteSocket.doOpen");
if(aA.readyState<1){aA.readyState=1;
if(typeof(aA.onopen)!=="undefined"){if(!ay){try{ay=document.createEvent("Events");
ay.initEvent("open",true,true)
}catch(aB){ay={type:"open",bubbles:true,cancelable:true}
}}try{aA.onopen(ay)
}catch(az){ao.severe(aA,"ByteSocket.doOpen: Error thrown from application")
}}}}function an(aA,az){ao.entering(aA,"ByteSocket.openHandler",az);
switch(aA.readyState){case 0:at(aA,az);
break;
case 1:case 2:var ay=(az?" from "+az.target:"");
ao.severe(aA,"ByteSocket.openHandler: Error: Invalid readyState for open event"+ay);
throw new Error("Invalid readyState for open event"+ay);
default:ao.severe(aA,"ByteSocket.openHandler: Error: Invalid readyState "+aA.readyState);
throw new Error("Socket has invalid readyState: "+aA.readyState)
}}function aj(aB,ay){if(typeof(aB.onmessage)==="function"){ao.entering(this,"ByteSocket.messageHandler");
var aA;
try{aA=document.createEvent("Events");
aA.initEvent("message",true,true)
}catch(aC){aA={type:"message",bubbles:true,cancelable:true}
}aA.data=ay;
aA.source=aB;
try{aB.onmessage(aA)
}catch(az){ao.severe(aB,"ByteSocket.doOpen: Error thrown from application message handler")
}}}function aw(aB){var aC=new Date().getTime();
var az=aC+50;
while(aB._queue.length>0){if(new Date().getTime()>az){setTimeout(function(){aw(aB)
},0);
return
}var ay=aB._queue.shift();
var aA=false;
try{aj(aB,ay);
aA=true
}finally{if(!aA){if(aB._queue.length==0){aB._delivering=false
}else{setTimeout(function(){aw(aB)
},0)
}}}}aB._delivering=false
}function ar(az,ay){switch(az.readyState){case 1:az._queue.push(ay);
if(!az._delivering){az._delivering=true;
setTimeout(function(){aw(az)
},0)
}break;
case 0:case 2:break;
default:throw new Error("Socket has invalid readyState: "+az.readyState)
}}function av(az,ay){ao.entering(az,"ByteSocket.doClose");
if(az.readyState<2){az.readyState=2;
if(typeof(az.onclose)!=="undefined"){setTimeout(function(){if(!ay){try{ay=document.createEvent("Events");
ay.initEvent("close",true,true)
}catch(aB){ay={type:"close",bubbles:true,cancelable:true}
}}try{az.onclose(ay)
}catch(aA){ao.severe(az,"ByteSocket.doClose: Error thrown from application")
}},0)
}}}function al(aA,az){ao.entering(aA,"ByteSocket.closeHandler",az);
switch(aA.readyState){case 0:unbindHandlers(aA);
fallbackNext(aA);
break;
case 1:av(aA,az);
break;
case 2:var ay=(az?" from "+az.target:"");
ao.severe(aA,"ByteSocket.closeHandler: Error: Invalid readyState for close event"+ay);
throw new Error("Invalid readyState for close event"+ay);
break;
default:ao.severe(aA,"ByteSocket.closeHandler: Error: Invalid readyState "+aA.readyState);
throw new Error("Socket has invalid readyState: "+aA.readyState)
}}aq.handleMessage=function(ay){switch(this.readyState){case 1:var az=this;
this._queue.push(ay);
if(!this._delivering){this._delivering=true;
setTimeout(function(){aw(az)
},0)
}break;
case 0:case 2:break;
default:throw new Error("Socket has invalid readyState: "+az.readyState)
}};
aq.handleClose=function(ay){av(this,ay)
};
aq.handleError=function(ay){av(this,ay)
};
return ap
})()
}());
window.___Loader=new x(q)
})()
})();
function StompConnectionFactory(a){var b=this;
this.createConnection=function(){var c=null;
var i=arguments.length;
var h=this;
var k;
var e=null;
var j=null;
var f="";
var l=false;
var d={};
if(i==1){k=arguments[0];
e=null;
j=null;
f=null
}else{if(i==3){e=arguments[0];
j=arguments[1];
k=arguments[2];
f=null
}else{if(i==4){e=arguments[0];
j=arguments[1];
f=arguments[2];
k=arguments[3]
}else{throw new Error("Wrong number of arguments to StompConnectionFactory.createConnection()")
}}}function g(n){if(typeof StompConnectionFactory.init=="function"){if(!l){l=true;
StompConnectionFactory.init(h,a)
}var m=StompConnectionFactory.createConnection(h,e,j,f,function(){if(m.value!==undefined){d.value=m.value
}else{if(m.exception!==undefined){d.exception=m.exception
}}d.getValue=function(){return m.getValue()
};
n()
})
}else{setTimeout(function(){g(n)
},100)
}}g(k);
return d
}
}function StompJms(){var I="",Nb="\n--><\/script>",hb='" for "gwt:onLoadErrorFn"',fb='" for "gwt:onPropertyErrorFn"',Cb='"<script src=\\"',S='"><\/script>',U="#",Mb=");",Db='.cache.js\\"></scr" + "ipt>"',W="/",vb="0D15A0D6281FBDAF8BD0307657B51249",wb="1AE9D7DF30E09DF8EFE7490FDF1BE1A9",xb="3506C007EFDAADBD5E210E851F310519",yb="808A0ED4317BB82E67E226D831ED1E64",zb="81951DAE9DA22C9E38900F0208A642D2",R='<script id="',Eb="<script><!--\n",cb="=",V="?",eb='Bad handler "',sb="Cross-site hosted mode not yet implemented. See issue ",Ab="DOMContentLoaded",T="SCRIPT",J="StompJms",Q="__gwt_marker_StompJms",X="base",M="begin",L="bootstrap",Z="clear.cache.gif",bb="content",Lb="document.write(",P="end",Hb='evtGroup: "loadExternalRefs", millis:(new Date()).getTime(),',Jb='evtGroup: "moduleStartup", millis:(new Date()).getTime(),',pb="gecko",qb="gecko1_8",N="gwt.hybrid",gb="gwt:onLoadErrorFn",db="gwt:onPropertyErrorFn",ab="gwt:property",tb="http://code.google.com/p/google-web-toolkit/issues/detail?id=2079",ob="ie6",nb="ie8",Y="img",Bb="loadExternalRefs",$="meta",Gb='moduleName:"StompJms", sessionId:$sessionId, subSystem:"startup",',O="moduleStartup",mb="msie",_="name",jb="opera",lb="safari",ub="selectingPermutation",K="startup",Ib='type: "end"});',Kb='type: "moduleRequested"});',rb="unknown",ib="user.agent",kb="webkit",Fb="window.__gwtStatsEvent && window.__gwtStatsEvent({";
var k=window,l=document,m=k.__gwtStatsEvent?function(a){return k.__gwtStatsEvent(a)
}:null,n,o,p=I,q={},r=[],s=[],t=[],u,v;
m&&m({moduleName:J,sessionId:$sessionId,subSystem:K,evtGroup:L,millis:(new Date).getTime(),type:M});
if(!k.__gwt_stylesLoaded){k.__gwt_stylesLoaded={}
}if(!k.__gwt_scriptsLoaded){k.__gwt_scriptsLoaded={}
}function w(){try{return k.external&&(k.external.gwtOnLoad&&k.location.search.indexOf(N)==-1)
}catch(a){return false
}}function x(){if(n&&o){n(u,J,p);
m&&m({moduleName:J,sessionId:$sessionId,subSystem:K,evtGroup:O,millis:(new Date).getTime(),type:P})
}}function y(){var e,f=Q,g;
l.write(R+f+S);
g=l.getElementById(f);
e=g&&g.previousSibling;
while(e&&e.tagName!=T){e=e.previousSibling
}function h(a){var b=a.lastIndexOf(U);
if(b==-1){b=a.length
}var c=a.indexOf(V);
if(c==-1){c=a.length
}var d=a.lastIndexOf(W,Math.min(c,b));
return d>=0?a.substring(0,d+1):I
}if(e&&e.src){p=h(e.src)
}if(p==I){var i=l.getElementsByTagName(X);
if(i.length>0){p=i[i.length-1].href
}else{p=h(l.location.href)
}}else{if(p.match(/^\w+:\/\//)){}else{var j=l.createElement(Y);
j.src=p+Z;
p=h(j.src)
}}if(g){g.parentNode.removeChild(g)
}}function z(){var b=document.getElementsByTagName($);
for(var c=0,d=b.length;
c<d;
++c){var e=b[c],f=e.getAttribute(_),g;
if(f){if(f==ab){g=e.getAttribute(bb);
if(g){var h,i=g.indexOf(cb);
if(i>=0){f=g.substring(0,i);
h=g.substring(i+1)
}else{f=g;
h=I
}q[f]=h
}}else{if(f==db){g=e.getAttribute(bb);
if(g){try{v=eval(g)
}catch(a){alert(eb+g+fb)
}}}else{if(f==gb){g=e.getAttribute(bb);
if(g){try{u=eval(g)
}catch(a){alert(eb+g+hb)
}}}}}}}}function C(a,b){var c=t;
for(var d=0,e=a.length-1;
d<e;
++d){c=c[a[d]]||(c[a[d]]=[])
}c[a[e]]=b
}function D(a){var b=s[a](),c=r[a];
if(b in c){return b
}var d=[];
for(var e in c){d[c[e]]=e
}if(v){v(a,d,b)
}throw null
}s[ib]=function(){var b=navigator.userAgent.toLowerCase();
var c=function(a){return parseInt(a[1])*1000+parseInt(a[2])
};
if(b.indexOf(jb)!=-1){return jb
}else{if(b.indexOf(kb)!=-1){return lb
}else{if(b.indexOf(mb)!=-1){if(document.documentMode>=8){return nb
}else{var d=/msie ([0-9]+)\.([0-9]+)/.exec(b);
if(d&&d.length==3){var e=c(d);
if(e>=6000){return ob
}}}}else{if(b.indexOf(pb)!=-1){var d=/rv:([0-9]+)\.([0-9]+)/.exec(b);
if(d&&d.length==3){if(c(d)>=1008){return qb
}}return pb
}}}}return rb
};
r[ib]={gecko:0,gecko1_8:1,ie6:2,ie8:3,opera:4,safari:5};
StompJms.onScriptLoad=function(a){StompJms=null;
n=a;
x()
};
if(w()){alert(sb+tb);
return
}y();
z();
m&&m({moduleName:J,sessionId:$sessionId,subSystem:K,evtGroup:L,millis:(new Date).getTime(),type:ub});
var E;
try{C([lb],vb);
C([ob],wb);
C([jb],xb);
C([pb],yb);
C([qb],yb);
C([nb],zb);
E=t[D(ib)]
}catch(a){return
}var F;
function G(){if(!o){o=true;
x();
if(l.removeEventListener){l.removeEventListener(Ab,G,false)
}if(F){clearInterval(F)
}}}if(l.addEventListener){l.addEventListener(Ab,function(){G()
},false)
}var F=setInterval(function(){if(/loaded|complete/.test(l.readyState)){G()
}},50);
m&&m({moduleName:J,sessionId:$sessionId,subSystem:K,evtGroup:L,millis:(new Date).getTime(),type:P});
m&&m({moduleName:J,sessionId:$sessionId,subSystem:K,evtGroup:Bb,millis:(new Date).getTime(),type:M});
var H=Cb+p+E+Db;
l.write(Eb+Fb+Gb+Hb+Ib+Fb+Gb+Jb+Kb+Lb+H+Mb+Nb)
}StompJms();