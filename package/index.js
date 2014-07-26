function loadConfig(){
	$.ajax({
		url:'Configs/'+gQuery.oid+'.txt?'+(''+Math.random()).substr(2,5),
		dataType:'text',
		method:'GET'
	}).done(function(res){
		console.log(res);
	});
}
function CloneHost(){
	var div = cloneDOM.cloneNode(true);
	cloneBTN.parentNode.insertBefore(div,cloneBTN);
	$('input',div).removeClass('invalid').val('')[0].focus();
	return false;
}
function checkForm(){
	var valid = false;
	if(modelSEL.val().trim() == 'local'){
		valid = checkHosts();
	}else{
		valid = checkRemote();
	}
	return valid && updateUserConfig();
}
function UpdateFormFields(){
	var list = $('div.info');
	for(var i=0,il=list.length;i<il;i++){
		var $hide = $('input[type="hidden"]',list[i]),
			$name = $('input[key="key"]',list[i]),
			$val  = $('input[key="val"]',list[i]);
		if($name.val().length>0 && $val.val().length>0){
			$hide.attr('name',$name.val().replace(/(^\s+|\s+$)/gi,''))
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
	var $host = $('#remoteHost'),host = $host.val(), $port = $('#remotePort'),port=$port.val();
	if(!/^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$/.test(host)){
		$host.addClass('invalid');
		return false;
	}
	if(!/^[1-9][0-9]+$/.test(port)){
		$port.addClass('invalid');
		return false;
	}
	$host.attr('name','remoteHost').removeClass('invalid');
	$port.attr('name','remotePort').removeClass('invalid');
	UpdateFormFields();
	return true;
}
function updateUserConfig(){
	if(gQuery.oid){
		$('#oidInput').prop('name','oid').val(gQuery.oid);
	}
	return true;
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