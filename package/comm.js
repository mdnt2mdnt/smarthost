function setKeyValue( key, value )
{

}

function delKey( key )
{

}

function strToMap(str, sp1, sp2)
{
	var arr = str.split(sp1||'&'),
		obj = {};
	for(var i=0,il=arr.length;i<il;i++){
		var tmp = arr[i].split(sp2||'=');
		if(tmp.length==2&&tmp[0].length){
			obj[tmp[0].replace(/[^a-z0-9]+/gi,'')] = tmp[1];
		}
	}
	return obj;
}
(function(){
	window.gQuery = strToMap(location.search.substr(1));
	window.gHash  = strToMap(location.hash.substr(1));
	window.gUA = navigator.userAgent;
    if (/MicroMessenger/i.test(gUA)){
        document.addEventListener('WeixinJSBridgeReady', function onBridgeReady() {
            WeixinJSBridge.on('menu:share:appmessage', function (argv) {return;});
            WeixinJSBridge.on('menu:share:timeline', function (argv) {return;});
            WeixinJSBridge.on('menu:share:weibo', function (argv) {return;});
			WeixinJSBridge.invoke('hideOptionMenu');
			WeixinJSBridge.invoke('hideToolbar');
		});
	}
})();
