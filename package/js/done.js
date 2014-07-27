function loadConfig(){
	$.ajax({
		url:'Configs/'+gQuery.oid+'.txt?'+(''+Math.random()).substr(-5),
		dataType:'text',
		method:'GET',
		success: function(res){
			restoreHost(res)
		}
	});
}
function restoreHost(postStr){
	var obj = strToMap(postStr);
}

function backToConfig(){
}

function initEvents(){
	window.backBTN = $('button.blue')[0];
	backBTN.onclick = backToConfig;
}
$(document).ready(function(){
	if(gQuery.oid){
		gQuery.oid = gQuery.oid.replace(/[^a-z0-9]+/gi,'');
		$('#oidInput').prop('name','oid').val(gQuery.oid);
		loadConfig();
	}
	initEvents();
});