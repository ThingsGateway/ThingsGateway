module.exports = {
  docs: [
    {
      type: "doc",
      id: "upgrade",
      label: "更新日志"
    },
    {
      type: "doc",
      id: "1",
      label: "版权说明"
    },
    {
      type: "doc",
      id: "2",
      label: "产品介绍"
    },

    {
      type: "doc",
      id: "3",
      label: "入门指南"
    },
    {
      type: "doc",
      id: "100",
      label: "驱动调试"
    },
    
    {
      type: "doc",
      id: "40001",
      label: "软件配置"
    },
    {
      type: "category",
      label: "基础手册",
      items: [
        {
          type: "category",
          label: "网关配置",
          items: [

            {
              type: "doc",
              id: "101",
              label: "插件管理"
            },
            {
              type: "doc",
              id: "102",
              label: "通道管理"
            },
            {
              type: "doc",
              id: "103",
              label: "设备"
            },
            {
              type: "doc",
              id: "104",
              label: "变量"
            },
          ]
        },
        {
          type: "doc",
          id: "105",
          label: "网关状态查看"
        },
        {
          type: "doc",
          id: "106",
          label: "网关日志"
        },
      ]
    },

    {
      type: "category",
      label: "采集插件手册",
      items: [
        {
          type: "doc",
          id: "200",
          label: "ModbusMaster"
        },

        {
          type: "doc",
          id: "202",
          label: "Dlt645Master"
        },

        {
          type: "doc",
          id: "203",
          label: "SiemensS7Master"
        },

        {
          type: "doc",
          id: "204",
          label: "OpcDaMaster"
        },

        {
          type: "doc",
          id: "205",
          label: "OpcUaMaster"
        },

        {
          type: "doc",
          id: "206",
          label: "VariableExpression"
        },
        {
          type: "doc",
          id: "207",
          label: "MqttCollect"
        }

      ]
    },
    {
      type: "category",
      label: "业务插件手册",
      items: [
        {
          type: "doc",
          id: "201",
          label: "ModbusSlave"
        },
        {
          type: "doc",
          id: "301",
          label: "MqttClient"
        },
        {
          type: "doc",
          id: "302",
          label: "MqttServer"
        },
        {
          type: "doc",
          id: "303",
          label: "RabbitMQProducer"
        },
        {
          type: "doc",
          id: "304",
          label: "KafkaProducer"
        },
        {
          type: "doc",
          id: "305",
          label: "TDengineDBProducer"
        },
        {
          type: "doc",
          id: "306",
          label: "QuestDBProducer"
        },
        {
          type: "doc",
          id: "307",
          label: "SqlDBProducer"
        },

        {
          type: "doc",
          id: "308",
          label: "SqlHisAlarm"
        },
        {
          type: "doc",
          id: "309",
          label: "OpcUaServer"
        },
      ]
    },
    {
      type: "category",
      label: "Pro插件手册",
      items: [
        {
          type: "doc",
          id: "10001",
          label: "ABCipMatser"
        },
      ]
    },
    {
      type: "category",
      label: "部署",
      items: [
        {
          type: "doc",
          id: "400",
          label: "编译发布"
        },
        {
          type: "doc",
          id: "401",
          label: "windows服务部署"
        },
        {
          type: "doc",
          id: "402",
          label: "iis部署"
        },
        {
          type: "doc",
          id: "403",
          label: "docker部署"
        },
        {
          type: "doc",
          id: "404",
          label: "linux服务部署"
        },
        {
          type: "doc",
          id: "405",
          label: "性能优化"
        }
      ]
    },
    {
      type: "category",
      label: "常见问题",
      items: [
        {
          type: "doc",
          id: "501",
          label: "常见问题"
        },
        {
          type: "doc",
          id: "502",
          label: "脚本常见问题"
        },
      ]
    },
    {
      type: "category",
      label: "插件开发",
      items: [
        {
          type: "doc",
          id: "601",
          label: "采集插件"
        },
        {
          type: "doc",
          id: "602",
          label: "业务插件"
        },
      ]
    },
    {
      type: "category",
      label: "nuget包文档",
      items: [
        {
          type: "doc",
          id: "20001",
          label: "Modbus"
        },
        {
          type: "doc",
          id: "20002",
          label: "SiemensS7"
        },
        {
          type: "doc",
          id: "20003",
          label: "OpcDa"
        },
        {
          type: "doc",
          id: "20004",
          label: "OpcUa"
        },
        {
          type: "doc",
          id: "20005",
          label: "Dlt645"
        },
      ]
    },

    {
      type: "category",
      label: "协议开发教程",
      items: [
        {
          type: "doc",
          label: "基础知识",
          id: "30001",

        },
        {
          type: "doc",
          label: "Modbus类库",
          id: "30002",

        }
      ]
    },

    {
      type: "category",
      label: "技术支持/合作",
      items: [
        {
          type: "doc",
          id: "1002",
          label: "联系我们"
        },
        {
          type: "doc",
          id: "1000",
          label: "赞助项目"
        },
        {
          type: "doc",
          id: "1001",
          label: "Pro版"
        },
      ]
    },

  ]
};

