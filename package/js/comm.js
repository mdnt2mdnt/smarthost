function setKey( key, value )
{
	try{localStorage.setItem(key,value);}catch(e){}
}
function getKey(key){
	try{
		return localStorage.getItem(key);
	}catch(e){
		return null;
	}
}

function delKey( key )
{
	try{
		localStorage.removeItem(key);
	}catch(e){}
}
function clearStore()
{
	try{
		localStorage.clear();
	}catch(e){}
}
function restoreRemoteConfig(restoreHost){
	var model = getKey('proxyModel');
	if(model=='local'||model=='remote'||model==''||model==null){
		var host=getKey('remoteHost')||'',
			port=getKey('remotePort')||'';
		restoreHost('proxyModel='+(model||'')+'&remoteHost='+host+'&remotePort='+port);
	}
}
function strToMap(str, sp1, sp2)
{
	var arr = str.split(sp1||'&'),
		obj = {};
	for(var i=0,il=arr.length;i<il;i++){
		var tmp = arr[i].split(sp2||'=');
		if(tmp.length==2&&tmp[0].length){
			try{ tmp[1] = decodeURIComponent(tmp[1]); }catch(e){}
			obj[tmp[0].replace(/\s+/gi,'')] = tmp[1];
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
