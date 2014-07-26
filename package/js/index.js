function loadConfig(){
	$.ajax({
		url:'Configs/'+gQuery.oid+'.txt?'+(''+Math.random()).substr(-5),
		dataType:'text',
		method:'GET',
		success: function(xhr){
			restoreHost(xhr);
		}
	});
}
function restoreHost(postStr){
	var obj = strToMap(postStr);
	if(obj.proxyModel == 'remote'){
		$('#remoteHost').prop('name','remoteHost').val(obj.remoteHost);
		$('#remotePort').prop('name','remotePort').val(obj.remotePort);
	}else{
	}
	if(obj.proxyModel == 'local' || obj.proxyModel == 'remote'){
		modelSEL.val(obj.proxyModel);
		SelectModel();
	}
}
function CloneHost(){
	var div = cloneDOM.cloneNode(true);
	cloneBTN.parentNode.insertBefore(div,cloneBTN);
	$('input',div).removeClass('invalid').val('')[0].focus();
	return false;
}
function checkForm(){
	var valid = false;
	if(modelSEL && modelSEL.val().trim() == 'local'){
		return checkHosts();
	}else{
		return checkRemote();
	}
}
function UpdateFormFields(){
	var list = $('div.info');
	for(var i=0,il=list.length;i<il;i++){
		var $hide = $('input[type="hidden"]',list[i]),
			$name = $('input[key="key"]',list[i]),
			$val  = $('input[key="val"]',list[i]);
		if($name.val().length>0 && $val.val().length>0){
			$hide.prop('name',$name.val().replace(/(^\s+|\s+$)/gi,''))
			.val($val.val().replace(/(^s+|\s+$)/gi,''));
		}
	}
}
function checkHosts(){
	UpdateFormFields();
	var hosts = $('div.info input[type="hidden"]'),validNum=0;
	for(var i=0,il=hosts.length;i<il;i++){
		var host = hosts[i];
		if(host.value.length>0 && !/^[0-i]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$/.test(host.value)){
			$('input[key="val"]',host.parentNode).addClass('invalid')[0].focus();
			return false;
		}
		if(host.name.length<3 || !/^[\w+\.]+\w+$/i.test(host.name)) {
			$('input[key="key"]',host.parentNode).addClass('invalid')[0].focus();
			return false;
		}
		$('input[key]',host.parentNode).removeClass('invalid');
		validNum++;
	}
	if(validNum>0){
		checkRemote();
		return true;
	}else{
		return false;
	}
}
function checkRemote(){
	var $host = $('#remoteHost'),host = $host.val(), $port = $('#remotePort'),port=$port.val(),valid = true;
	if(!/^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$/.test(host)){
		$host.addClass('invalid');
		valid = false;
	}else{
		$host.removeClass('invalid');
	}
	if(!/^[1-9][0-9]+$/.test(port)){
		$port.addClass('invalid');
		valid = false;
	}else{
		$port.removeClass('invalid');
	}
	if(valid){
		$host.prop('name','remoteHost');
		$port.prop('name','remotePort');
		UpdateFormFields();
		return true;
	}else{
		return false;
	}
}
function SelectModel(){
	var val = modelSEL.val().trim();
	$('#'+val+'Content').show();
	$('#'+(val=='local'?'remote':'local')+'Content').hide();
}
function initEvents(){
	window.cloneDOM = $('div.info')[0];
	window.cloneBTN = $('#addBtnOne')[0];
	window.dataHOLD = $('span[holder')[0];
	window.modelSEL = $('#modelSelect');
	cloneBTN.onclick=CloneHost;
	modelSEL.change(SelectModel);
}
$(document).ready(function(){
	if(gQuery.oid){
		gQuery.oid = gQuery.oid.replace(/[^a-z0-9]+/gi,'');
		$('#oidInput').prop('name','oid').val(gQuery.oid);
		loadConfig();
	}
	initEvents();
});