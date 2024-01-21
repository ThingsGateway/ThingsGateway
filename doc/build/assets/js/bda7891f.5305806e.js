"use strict";(self.webpackChunkthingsgateway=self.webpackChunkthingsgateway||[]).push([[6017],{3569:(e,t,n)=>{n.r(t),n.d(t,{assets:()=>m,contentTitle:()=>i,default:()=>o,frontMatter:()=>r,metadata:()=>u,toc:()=>d});var a=n(7462),l=(n(7294),n(3905));n(4996),n(510),n(2969);const r={id:301,title:"MqttClient"},i=void 0,u={unversionedId:"301",id:"301",title:"MqttClient",description:"\u901a\u8fc7\u81ea\u5b9a\u4e49\u811a\u672c\uff0c\u53ef\u5feb\u901f\u9002\u914d\u4e1a\u52a1\u6a21\u578b\uff0c\u6bd4\u5982\u5404\u5927\u4e91\u5e73\u53f0\u7684Iot\u7269\u6a21\u578b",source:"@site/docs/301.mdx",sourceDirName:".",slug:"/301",permalink:"/thingsgateway-docs/docs/301",draft:!1,editUrl:"https://gitee.com/diego2098/ThingsGateway/tree/master/doc/docs/301.mdx",tags:[],version:"current",lastUpdatedBy:"Kimdiego2098",lastUpdatedAt:1705822711,formattedLastUpdatedAt:"Jan 21, 2024",frontMatter:{id:"301",title:"MqttClient"},sidebar:"docs",previous:{title:"ModbusSlave",permalink:"/thingsgateway-docs/docs/201"},next:{title:"MqttServer",permalink:"/thingsgateway-docs/docs/302"}},m={},d=[{value:"\u4e00\u3001\u8bf4\u660e",id:"\u4e00\u8bf4\u660e",level:2},{value:"\u4e8c\u3001\u63d2\u4ef6\u5c5e\u6027\u914d\u7f6e\u9879",id:"\u4e8c\u63d2\u4ef6\u5c5e\u6027\u914d\u7f6e\u9879",level:2},{value:"\u811a\u672c\u63a5\u53e3",id:"\u811a\u672c\u63a5\u53e3",level:3},{value:"DeviceData",id:"devicedata",level:3},{value:"VariableData",id:"variabledata",level:3},{value:"AlarmVariable",id:"alarmvariable",level:3},{value:"\u4e09\u3001\u53d8\u91cf\u4e1a\u52a1\u5c5e\u6027",id:"\u4e09\u53d8\u91cf\u4e1a\u52a1\u5c5e\u6027",level:2},{value:"\u5141\u8bb8\u5199\u5165",id:"\u5141\u8bb8\u5199\u5165",level:3}],p={toc:d},g="wrapper";function o(e){let{components:t,...r}=e;return(0,l.kt)(g,(0,a.Z)({},p,r,{components:t,mdxType:"MDXLayout"}),(0,l.kt)("admonition",{type:"tip"},(0,l.kt)("mdxAdmonitionTitle",{parentName:"admonition"},(0,l.kt)("inlineCode",{parentName:"mdxAdmonitionTitle"},"\u63d0\u793a")),(0,l.kt)("p",{parentName:"admonition"},"\u901a\u8fc7\u81ea\u5b9a\u4e49\u811a\u672c\uff0c\u53ef\u5feb\u901f\u9002\u914d\u4e1a\u52a1\u6a21\u578b\uff0c\u6bd4\u5982\u5404\u5927\u4e91\u5e73\u53f0\u7684Iot\u7269\u6a21\u578b"),(0,l.kt)("p",{parentName:"admonition"},"\u811a\u672c\u7684\u793a\u4f8b\u8bf7\u67e5\u770b",(0,l.kt)("strong",{parentName:"p"},"\u5e38\u89c1\u95ee\u9898"))),(0,l.kt)("h2",{id:"\u4e00\u8bf4\u660e"},"\u4e00\u3001\u8bf4\u660e"),(0,l.kt)("p",null,"MqttClient\u901a\u8fc7Tcp/WebSocket\u7684\u65b9\u5f0f\uff0c\u53d1\u5e03\u5185\u5bb9\u5230Broker(Server)\uff0c\u53ef\u4ee5\u5b9a\u65f6/\u53d8\u5316\u53d1\u5e03\u6570\u636e"),(0,l.kt)("p",null,"\u901a\u9053\u53ea\u652f\u6301 Other "),(0,l.kt)("h2",{id:"\u4e8c\u63d2\u4ef6\u5c5e\u6027\u914d\u7f6e\u9879"},"\u4e8c\u3001\u63d2\u4ef6\u5c5e\u6027\u914d\u7f6e\u9879"),(0,l.kt)("img",{src:n(5496).Z}),(0,l.kt)("table",null,(0,l.kt)("thead",{parentName:"table"},(0,l.kt)("tr",{parentName:"thead"},(0,l.kt)("th",{parentName:"tr",align:null},"\u5c5e\u6027"),(0,l.kt)("th",{parentName:"tr",align:null},"\u8bf4\u660e"),(0,l.kt)("th",{parentName:"tr",align:null},"\u5907\u6ce8"))),(0,l.kt)("tbody",{parentName:"table"},(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"IP"),(0,l.kt)("td",{parentName:"tr",align:null},"ServerIP,\u4e3a\u7a7a\u65f6\u6307\u4efb\u610fIP"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u7aef\u53e3"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8fde\u63a5\u7aef\u53e3"),(0,l.kt)("td",{parentName:"tr",align:null},"1883")),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426WebSocket\u8fde\u63a5"),(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426WebSocket\u8fde\u63a5"),(0,l.kt)("td",{parentName:"tr",align:null},"False")),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"WebSocketUrl"),(0,l.kt)("td",{parentName:"tr",align:null},"WebSocketUrl"),(0,l.kt)("td",{parentName:"tr",align:null},"ws://127.0.0.1:8083/mqtt")),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u8d26\u53f7"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8d26\u53f7"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u5bc6\u7801"),(0,l.kt)("td",{parentName:"tr",align:null},"\u5bc6\u7801"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u8fde\u63a5Id"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8fde\u63a5Id"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u8fde\u63a5\u8d85\u65f6\u65f6\u95f4"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8fde\u63a5\u8d85\u65f6\u65f6\u95f4"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u5141\u8bb8Rpc\u5199\u5165"),(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426\u5141\u8bb8\u5199\u5165\u53d8\u91cf"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"Rpc\u5199\u5165Topic"),(0,l.kt)("td",{parentName:"tr",align:null},"\u5199\u5165\u53d8\u91cf\u7684\u4e3b\u9898"),(0,l.kt)("td",{parentName:"tr",align:null},"\u5b9e\u9645\u7684\u5199\u5165\u4e3b\u9898\u4e3a\u56fa\u5b9a\u901a\u914d {ThingsGateway/+/","[\u586b\u5165\u503c]","} ,\u5176\u4e2dRpcWrite\u4e3a\u8be5\u5c5e\u6027\u586b\u5165\u5185\u5bb9\uff0c+\u901a\u914d\u7b26\u662f\u4e0d\u56fa\u5b9aGUID\u503c\uff0c\u6bcf\u6b21\u6267\u884c\u5199\u5165\u65f6\u4f1a\u5728\u4e0d\u540c\u7684\u4e3b\u9898\u4e2d\u8fd4\u56de\uff1b\u8fd4\u56de\u7ed3\u679c\u4e3b\u9898\u4f1a\u5728\u4e3b\u9898\u540e\u6dfb\u52a0Response , \u4e5f\u5c31\u662f{ThingsGateway/+/","[\u586b\u5165\u503c]","/Response}")),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u6570\u636e\u8bf7\u6c42Topic"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8be5\u4e3b\u9898\u63a5\u53d7\u5230\u4efb\u4f55\u6d88\u606f\u90fd\u4f1a\u53d1\u5e03\u5168\u90e8\u4fe1\u606f\u5230\u5bf9\u5e94\u7684\u53d8\u91cf/\u8bbe\u5907/\u62a5\u8b66\u4e3b\u9898\u4e2d"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u8bbe\u5907\u662f\u5426\u5217\u8868"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8bbe\u5907\u662f\u5426\u5217\u8868\u4e0a\u4f20\uff0cfalse\u65f6\u6bcf\u4e2a\u8bbe\u5907\u5b9e\u4f53\u90fd\u4f1a\u5355\u72ec\u53d1\u5e03\uff0c\u6ce8\u610f\u6027\u80fd\u9700\u6c42\uff0c\u9ed8\u8ba4\u4e3atrue"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u53d8\u91cf\u662f\u5426\u5217\u8868"),(0,l.kt)("td",{parentName:"tr",align:null},"\u53d8\u91cf\u662f\u5426\u5217\u8868\u4e0a\u4f20\uff0cfalse\u65f6\u6bcf\u4e2a\u53d8\u91cf\u5b9e\u4f53\u90fd\u4f1a\u5355\u72ec\u53d1\u5e03\uff0c\u6ce8\u610f\u6027\u80fd\u9700\u6c42\uff0c\u9ed8\u8ba4\u4e3atrue"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u62a5\u8b66\u662f\u5426\u5217\u8868"),(0,l.kt)("td",{parentName:"tr",align:null},"\u62a5\u8b66\u662f\u5426\u5217\u8868\u4e0a\u4f20\uff0cfalse\u65f6\u6bcf\u4e2a\u62a5\u8b66\u5b9e\u4f53\u90fd\u4f1a\u5355\u72ec\u53d1\u5e03\uff0c\u6ce8\u610f\u6027\u80fd\u9700\u6c42\uff0c\u9ed8\u8ba4\u4e3atrue"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u8bbe\u5907Topic"),(0,l.kt)("td",{parentName:"tr",align:null},"\u8bbe\u5907\u5b9e\u4f53\u7684\u53d1\u5e03\u4e3b\u9898 \uff0c\u4f7f\u7528${key}\u4f5c\u4e3a\u5339\u914d\u9879\uff0ckey\u5fc5\u987b\u662f\u4e0a\u4f20\u5b9e\u4f53\u4e2d\u7684\u5c5e\u6027"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u53d8\u91cfTopic"),(0,l.kt)("td",{parentName:"tr",align:null},"\u53d8\u91cf\u5b9e\u4f53\u7684\u53d1\u5e03\u4e3b\u9898 \uff0c\u4f7f\u7528${key}\u4f5c\u4e3a\u5339\u914d\u9879\uff0ckey\u5fc5\u987b\u662f\u4e0a\u4f20\u5b9e\u4f53\u4e2d\u7684\u5c5e\u6027"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u62a5\u8b66Topic"),(0,l.kt)("td",{parentName:"tr",align:null},"\u62a5\u8b66\u5b9e\u4f53\u7684\u53d1\u5e03\u4e3b\u9898 \uff0c\u4f7f\u7528${key}\u4f5c\u4e3a\u5339\u914d\u9879\uff0ckey\u5fc5\u987b\u662f\u4e0a\u4f20\u5b9e\u4f53\u4e2d\u7684\u5c5e\u6027"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u8bbe\u5907\u5b9e\u4f53\u811a\u672c"),(0,l.kt)("td",{parentName:"tr",align:null},"\u811a\u672c\u8fd4\u56de\u65b0\u7684\u5b9e\u4f53\u5217\u8868\uff0c\u52a8\u6001\u7c7b\u4e2d\u9700\u7ee7\u627f",(0,l.kt)("strong",{parentName:"td"},"IDynamicModel"),"\uff0c\u4f20\u5165\u5217\u8868\u4e3a",(0,l.kt)("strong",{parentName:"td"},"DeviceData"),",\u67e5\u770b\u4ee5\u4e0b\u5177\u4f53\u5c5e\u6027"),(0,l.kt)("td",{parentName:"tr",align:null},"\u7f16\u8f91\u9875\u9762\u4e2d\uff0c\u53ef\u901a\u8fc7\u68c0\u67e5\u6309\u94ae\u9a8c\u8bc1\u811a\u672c\uff0c\u811a\u672c\u793a\u4f8b\u8bf7\u67e5\u770b",(0,l.kt)("strong",{parentName:"td"},"\u5e38\u89c1\u95ee\u9898"))),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u53d8\u91cf\u5b9e\u4f53\u811a\u672c"),(0,l.kt)("td",{parentName:"tr",align:null},"\u811a\u672c\u8fd4\u56de\u65b0\u7684\u5b9e\u4f53\u5217\u8868\uff0c\u52a8\u6001\u7c7b\u4e2d\u9700\u7ee7\u627f",(0,l.kt)("strong",{parentName:"td"},"IDynamicModel"),"\uff0c\u4f20\u5165\u5217\u8868\u4e3a",(0,l.kt)("strong",{parentName:"td"},"VariableData"),",\u67e5\u770b\u4ee5\u4e0b\u5177\u4f53\u5c5e\u6027"),(0,l.kt)("td",{parentName:"tr",align:null},"\u7f16\u8f91\u9875\u9762\u4e2d\uff0c\u53ef\u901a\u8fc7\u68c0\u67e5\u6309\u94ae\u9a8c\u8bc1\u811a\u672c\uff0c\u811a\u672c\u793a\u4f8b\u8bf7\u67e5\u770b",(0,l.kt)("strong",{parentName:"td"},"\u5e38\u89c1\u95ee\u9898"))),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u62a5\u8b66\u5b9e\u4f53\u811a\u672c"),(0,l.kt)("td",{parentName:"tr",align:null},"\u811a\u672c\u8fd4\u56de\u65b0\u7684\u5b9e\u4f53\u5217\u8868\uff0c\u52a8\u6001\u7c7b\u4e2d\u9700\u7ee7\u627f",(0,l.kt)("strong",{parentName:"td"},"IDynamicModel"),"\uff0c\u4f20\u5165\u5217\u8868\u4e3a",(0,l.kt)("strong",{parentName:"td"},"AlarmVariable"),",\u67e5\u770b\u4ee5\u4e0b\u5177\u4f53\u5c5e\u6027"),(0,l.kt)("td",{parentName:"tr",align:null},"\u7f16\u8f91\u9875\u9762\u4e2d\uff0c\u53ef\u901a\u8fc7\u68c0\u67e5\u6309\u94ae\u9a8c\u8bc1\u811a\u672c\uff0c\u811a\u672c\u793a\u4f8b\u8bf7\u67e5\u770b",(0,l.kt)("strong",{parentName:"td"},"\u5e38\u89c1\u95ee\u9898"))),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426\u9009\u62e9\u5168\u90e8\u53d8\u91cf"),(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426\u9009\u62e9\u5168\u90e8\u53d8\u91cf\uff0ctrue\u65f6\u4e0d\u9700\u8981\u5355\u4e2a\u53d8\u91cf\u6dfb\u52a0\u4e1a\u52a1\u5c5e\u6027"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426\u95f4\u9694\u6267\u884c"),(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426\u9009\u62e9\u5168\u90e8\u53d8\u91cf\uff0ctrue\u95f4\u9694\u4e0a\u4f20\uff0cFalse\u53d8\u5316\u68c0\u6d4b\u4e0a\u4f20"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u95f4\u9694\u6267\u884c\u65f6\u95f4"),(0,l.kt)("td",{parentName:"tr",align:null},"\u95f4\u9694\u6267\u884c\u65f6\u95f4"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u542f\u7528\u7f13\u5b58"),(0,l.kt)("td",{parentName:"tr",align:null},"\u662f\u5426\u542f\u7528\u7f13\u5b58"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u4e0a\u4f20\u5217\u8868\u6700\u5927\u6570\u91cf"),(0,l.kt)("td",{parentName:"tr",align:null},"\u6bcf\u4e00\u6b21\u4e0a\u4f20\u7684\u5217\u8868\u6700\u5927\u6570\u91cf"),(0,l.kt)("td",{parentName:"tr",align:null})),(0,l.kt)("tr",{parentName:"tbody"},(0,l.kt)("td",{parentName:"tr",align:null},"\u5185\u5b58\u961f\u5217\u6700\u5927\u6570\u91cf"),(0,l.kt)("td",{parentName:"tr",align:null},"\u5185\u5b58\u961f\u5217\u7684\u6700\u5927\u6570\u91cf\uff0c\u8d85\u51fa\u6216\u5931\u8d25\u65f6\u8f6c\u5165\u6587\u4ef6\u7f13\u5b58\uff0c\u6839\u636e\u6570\u636e\u91cf\u8bbe\u5b9a\u9002\u5f53\u503c"),(0,l.kt)("td",{parentName:"tr",align:null})))),(0,l.kt)("h3",{id:"\u811a\u672c\u63a5\u53e3"},"\u811a\u672c\u63a5\u53e3"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},"\npublic interface IDynamicModel\n{\n    IEnumerable<dynamic> GetList(IEnumerable<dynamic> datas);\n}\n\n\n")),(0,l.kt)("h3",{id:"devicedata"},"DeviceData"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},'\n/// <summary>\n/// \u8bbe\u5907\u4e1a\u52a1\u53d8\u5316\u6570\u636e\n/// </summary>\npublic class DeviceData\n{\n    /// <inheritdoc cref="PrimaryIdEntity.Id"/>\n    public long Id { get; set; }\n\n    /// <inheritdoc cref="Device.Name"/>\n    public string Name { get; set; }\n\n    /// <inheritdoc cref="DeviceRunTime.ActiveTime"/>\n    public DateTime ActiveTime { get; set; }\n\n    /// <inheritdoc cref="DeviceRunTime.DeviceStatus"/>\n    public DeviceStatusEnum DeviceStatus { get; set; }\n\n    /// <inheritdoc cref="DeviceRunTime.LastErrorMessage"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string LastErrorMessage { get; set; }\n\n    /// <inheritdoc cref="Device.Remark1"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark1 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark2"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark2 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark3"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark3 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark4"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark4 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark5"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark5 { get; set; }\n}\n\n')),(0,l.kt)("h3",{id:"variabledata"},"VariableData"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},'\n/// <summary>\n/// \u53d8\u91cf\u4e1a\u52a1\u53d8\u5316\u6570\u636e\n/// </summary>\npublic class VariableData\n{\n    /// <inheritdoc cref="PrimaryIdEntity.Id"/>\n    public long Id { get; set; }\n\n    /// <inheritdoc cref="Variable.Name"/>\n    public string Name { get; set; }\n\n    /// <inheritdoc cref="VariableRunTime.DeviceName"/>\n    public string DeviceName { get; set; }\n\n    /// <inheritdoc cref="VariableRunTime.Value"/>\n    public object Value { get; set; }\n\n    /// <inheritdoc cref="VariableRunTime.ChangeTime"/>\n    public DateTime ChangeTime { get; set; }\n\n    /// <inheritdoc cref="VariableRunTime.CollectTime"/>\n    public DateTime CollectTime { get; set; }\n\n    /// <inheritdoc cref="VariableRunTime.IsOnline"/>\n    public bool IsOnline { get; set; }\n\n    /// <inheritdoc cref="VariableRunTime.LastErrorMessage"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string? LastErrorMessage { get; set; }\n\n    /// <inheritdoc cref="Device.Remark1"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark1 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark2"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark2 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark3"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark3 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark4"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark4 { get; set; }\n\n    /// <inheritdoc cref="Device.Remark5"/>\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark5 { get; set; }\n}\n\n')),(0,l.kt)("h3",{id:"alarmvariable"},"AlarmVariable"),(0,l.kt)("pre",null,(0,l.kt)("code",{parentName:"pre"},'\n/// <summary>\n/// \u62a5\u8b66\u53d8\u91cf\n/// </summary>\npublic class AlarmVariable\n{\n    /// <inheritdoc  cref="Variable.Name"/>\n    [SugarColumn(ColumnDescription = "\u53d8\u91cf\u540d\u79f0", IsNullable = false)]\n    public string Name { get; set; }\n\n    /// <inheritdoc  cref="Variable.Description"/>\n    [SugarColumn(ColumnDescription = "\u63cf\u8ff0", IsNullable = true)]\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string? Description { get; set; }\n\n    /// <inheritdoc  cref="VariableRunTime.DeviceName"/>\n    [SugarColumn(ColumnDescription = "\u8bbe\u5907\u540d\u79f0", IsNullable = true)]\n    public string DeviceName { get; set; }\n\n    /// <inheritdoc  cref="Variable.RegisterAddress"/>\n    [SugarColumn(ColumnDescription = "\u53d8\u91cf\u5730\u5740")]\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string RegisterAddress { get; set; }\n\n    /// <inheritdoc  cref="Variable.DataType"/>\n    [SugarColumn(ColumnDescription = "\u6570\u636e\u7c7b\u578b", ColumnDataType = "varchar(100)")]\n    public DataTypeEnum DataType { get; set; }\n\n    /// <inheritdoc  cref="VariableRunTime.AlarmCode"/>\n    [SugarColumn(ColumnDescription = "\u62a5\u8b66\u503c", IsNullable = false)]\n    public string AlarmCode { get; set; }\n\n    /// <inheritdoc  cref="VariableRunTime.AlarmLimit"/>\n    [SugarColumn(ColumnDescription = "\u62a5\u8b66\u9650\u503c", IsNullable = false)]\n    public string AlarmLimit { get; set; }\n\n    /// <inheritdoc  cref="VariableRunTime.AlarmText"/>\n    [SugarColumn(ColumnDescription = "\u62a5\u8b66\u6587\u672c", IsNullable = true)]\n    public string? AlarmText { get; set; }\n\n    /// <inheritdoc  cref="VariableRunTime.AlarmTime"/>\n    [SugarColumn(ColumnDescription = "\u62a5\u8b66\u65f6\u95f4", IsNullable = false)]\n    public DateTime AlarmTime { get; set; }\n\n    /// <inheritdoc  cref="VariableRunTime.EventTime"/>\n    public DateTime EventTime { get; set; }\n\n    /// <summary>\n    /// \u62a5\u8b66\u7c7b\u578b\n    /// </summary>\n    public AlarmTypeEnum? AlarmType { get; set; }\n\n    /// <summary>\n    /// \u4e8b\u4ef6\u7c7b\u578b\n    /// </summary>\n    public EventTypeEnum EventType { get; set; }\n\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark1 { get; set; }\n\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark2 { get; set; }\n\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark3 { get; set; }\n\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark4 { get; set; }\n\n    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]\n    public string Remark5 { get; set; }\n}\n\n')),(0,l.kt)("h2",{id:"\u4e09\u53d8\u91cf\u4e1a\u52a1\u5c5e\u6027"},"\u4e09\u3001\u53d8\u91cf\u4e1a\u52a1\u5c5e\u6027"),(0,l.kt)("h3",{id:"\u5141\u8bb8\u5199\u5165"},"\u5141\u8bb8\u5199\u5165"),(0,l.kt)("p",null,"\u5355\u72ec\u914d\u7f6e\u53d8\u91cf\u662f\u5426\u5141\u8bb8\u5199\u5165"))}o.isMDXComponent=!0},5496:(e,t,n)=>{n.d(t,{Z:()=>a});const a=n.p+"assets/images/MqttClient-8fa7dc0592dd20c39ac0db9d17799f96.png"}}]);