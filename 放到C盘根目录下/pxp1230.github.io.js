d = document;
//判断是否是Android设备
var ua = navigator.userAgent.toLowerCase();
var isAndroid="file:///C:/";
if (/android|linux/.test(ua))
{isAndroid="file:///storage/emulated/0/";}
//判断是否是index.html
var isIndex=false;
var str=window.location.pathname;
a=str.substr(str.lastIndexOf("/")+1);
a=a.substr(0,a.lastIndexOf(".")).toLowerCase();
if(a==""||a=="index"){isIndex=true;}
if(!isIndex){
hljs.initHighlightingOnLoad();
//监听键盘事件
d.onkeydown=function(e){if(e.keyCode==88||e.keyCode==27||e.keyCode==46){window.open('','_self');window.close()}else if(e.keyCode==8)window.open('index.html','_self');else if(e.keyCode==84)$("#toc").toggle()}
//监听鼠标事件
d.onclick=function(e){if(e.srcElement.id!='tocbutton')$("#toc").hide()}
}else{
function FindFirstChar(str,ch){for(var i=0;i<str.length;i++){var testChar=str.charAt(i).toUpperCase();var testCharCode=testChar.charCodeAt(0);if((testCharCode<=57&&testCharCode>=48)||(testCharCode<=90&&testCharCode>=65)){if(testChar==ch)return true;break}}return false}
d.onkeydown=function(e){if(e.keyCode==37)window.open('../index.html','_self');else if(e.keyCode==27||e.keyCode==46){window.open('','_self');window.close()}else if((e.keyCode<=57&&e.keyCode>=48)||(e.keyCode<=90&&e.keyCode>=65)){var keychar=String.fromCharCode(e.keyCode);var links=d.links;for(var i=0;i<links.length;i++){if(!links[i].onclick&&FindFirstChar(links[i].innerText,keychar)){window.open(links[i].href,links[i].target=='_blank'?'_blank':'_self');break}}}}
}
c = d.createElement('style');
c.type = "text/css";
var css = "#closebutton,#backbutton,#tocbutton{z-index:99;width:40px;height:40px;border-radius:20px;background:rgba(255,255,255,0.02);right:10px;position:fixed;margin:0;}#closebutton{bottom:10px;}#backbutton{bottom:60px;}#tocbutton{z-index:97;top:10px;}#toc .active>a{background:#500064 !important;}#toc li>a{display:block;}#toc li>a:hover{background:#3a0048 !important;}.tocify{width:250px !important;right:10px !important;top:10px !important;z-index:98;background:#2a0034;border-color:#C0F !important;margin:0;}";
if(c.styleSheet){
	c.styleSheet.cssText = css;
}else{
	c.appendChild(d.createTextNode(css));
}
//加载js文件或css文件
function loadjscssfile(filename,filetype){
	if(filetype == "js"){
		var fileref = document.createElement('script');
		fileref.setAttribute("type","text/javascript");
		fileref.setAttribute("src",filename);
	}else if(filetype == "css"){
		var fileref = document.createElement('link');
		fileref.setAttribute("rel","stylesheet");
		fileref.setAttribute("type","text/css");
		fileref.setAttribute("href",filename);
	}
	if(typeof fileref != "undefined"){
		document.head.appendChild(fileref);
	}
}
window.onload = function() {
	d.head.appendChild(c);
	var closebutton = d.createElement("div");
	closebutton.id="closebutton";
	closebutton.onclick=function(){window.open('','_self');window.close()}
	d.body.appendChild(closebutton);
	if(!isIndex){
		//JSLoader
		var JSLoader=function(){var scripts={};function getScript(url){var script=scripts[url];if(!script){script={loaded:false,funs:[]};scripts[url]=script;add(script,url)}return script}function run(script){var funs=script.funs,len=funs.length,i=0;for(;i<len;i++){var fun=funs.pop();fun()}}function add(script,url){var scriptdom=document.createElement('script');scriptdom.type='text/javascript';scriptdom.loaded=false;scriptdom.src=url;scriptdom.onload=function(){scriptdom.loaded=true;run(script);scriptdom.onload=scriptdom.onreadystatechange=null};scriptdom.onreadystatechange=function(){if((scriptdom.readyState==='loaded'||scriptdom.readyState==='complete')&&!scriptdom.loaded){run(script);scriptdom.onload=scriptdom.onreadystatechange=null}};document.getElementsByTagName('head')[0].appendChild(scriptdom)}return{load:function(url){var arg=arguments,len=arg.length,i=1,script=getScript(url),loaded=script.loaded;for(;i<len;i++){var fun=arg[i];if(typeof fun==='function'){if(loaded){fun()}else{script.funs.push(fun)}}}}}}();
	
		//https://highlightjs.org/，使用过的样式：pojoaque.css
		loadjscssfile(isAndroid+"highlight/styles/railscasts.css","css");
		//创建TOC相关元素
		
		JSLoader.load(isAndroid+"jquery.tocify.js-master/jquery.tocify.js-master/libs/jquery/jquery-1.8.3.min.js","js",function(){
			var toc = d.createElement("div");
			toc.id="toc";
			d.body.appendChild(toc);
			var backbutton = d.createElement("div");
			backbutton.id="backbutton";
			backbutton.onclick=function(){window.open('index.html','_self')}
			d.body.appendChild(backbutton);
			var tocbutton = d.createElement("div");
			tocbutton.id="tocbutton";
			tocbutton.onclick=function(){$("#toc").toggle()}
			d.body.appendChild(tocbutton);
			JSLoader.load(isAndroid+"jquery.tocify.js-master/jquery.tocify.js-master/src/javascripts/jquery.tocify.js",function(){var toc = $("#toc").tocify();$("#toc").hide();});
		});
		loadjscssfile(isAndroid+"jquery.tocify.js-master/jquery.tocify.js-master/libs/jqueryui/jquery-ui-1.9.1.custom.min.js","js");
		loadjscssfile(isAndroid+"jquery.tocify.js-master/jquery.tocify.js-master/src/stylesheets/jquery.tocify.css","css");
	}
}