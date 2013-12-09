var shareData = {
	img_url : 'https://code.google.com/p/smarthost/logo',
	img_width : '640',
	img_height : '640',
	link : 'http://smarthost.sinaapp.com/help.php?id=1',
	desc : '手机端开发利器',
	title : 'SmartHost',
	content : '#SmartHost#手机端开发利器',
	url : 'http://smarthost.sinaapp.com/help.php?id=1'
};
function shareFriend() {
	WeixinJSBridge.invoke("sendAppMessage", {
		appid : shareData.appid,
		img_url : shareData.img_url,
		img_width : shareData.img_width,
		img_height : shareData.img_height,
		link : shareData.link,
		desc : shareData.desc,
		title : shareData.title
	}, function (a) {});
}
function shareTimeline() {
	var title = shareData.title;
	if (title.indexOf(shareData.desc) == -1) {
		title += ":" + shareData.desc;
	}
	WeixinJSBridge.invoke("shareTimeline", {
		img_url : shareData.img_url,
		img_width : shareData.img_width,
		img_height : shareData.img_height,
		link : shareData.link,
		desc : shareData.desc,
		title : title
	}, function (a) {});
}
function shareWeibo() {
	WeixinJSBridge.invoke("shareWeibo", {
		content : shareData.content,
		url : shareData.url || ' '
	}, function (a) {});
}
(function () {
    if (/MicroMessenger/i.test(navigator.userAgent)) {
        document.addEventListener('WeixinJSBridgeReady', function onBridgeReady() {
            WeixinJSBridge.on('menu:share:appmessage', function (argv) {
                shareFriend();
            });
            WeixinJSBridge.on('menu:share:timeline', function (argv) {
                shareTimeline();
            });
            WeixinJSBridge.on('menu:share:weibo', function (argv) {
                shareWeibo();
            });
        }, false);
    }
})();