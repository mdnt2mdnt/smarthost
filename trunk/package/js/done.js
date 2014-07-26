function loadConfig(){
	$.ajax({
		url:'Configs/'+gQuery.oid+'.txt?'+(''+Math.random()).substr(-5),
		dataType:'text',
		method:'GET',
		success: function(xhr){
			restoreHost(xhr.responseText);
		}
	});
}
function restoreHost(postStr){
	console.log(postStr);
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