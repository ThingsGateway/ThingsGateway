import React from 'react';
import ComponentCreator from '@docusaurus/ComponentCreator';

export default [
  {
    path: '/thingsgateway-docs/search',
    component: ComponentCreator('/thingsgateway-docs/search', '479'),
    exact: true
  },
  {
    path: '/thingsgateway-docs/docs',
    component: ComponentCreator('/thingsgateway-docs/docs', 'b45'),
    routes: [
      {
        path: '/thingsgateway-docs/docs/',
        component: ComponentCreator('/thingsgateway-docs/docs/', 'ef3'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/backendlog',
        component: ComponentCreator('/thingsgateway-docs/docs/backendlog', 'ecd'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/collectdevice',
        component: ComponentCreator('/thingsgateway-docs/docs/collectdevice', 'e6c'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/devicevariable',
        component: ComponentCreator('/thingsgateway-docs/docs/devicevariable', '3ac'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/donate',
        component: ComponentCreator('/thingsgateway-docs/docs/donate', 'f6f'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/enterprise',
        component: ComponentCreator('/thingsgateway-docs/docs/enterprise', '71c'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/foundation/modbus',
        component: ComponentCreator('/thingsgateway-docs/docs/foundation/modbus', 'cb4'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/foundation/opcda',
        component: ComponentCreator('/thingsgateway-docs/docs/foundation/opcda', '2c4'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/foundation/opcua',
        component: ComponentCreator('/thingsgateway-docs/docs/foundation/opcua', 'a04'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/foundation/s7',
        component: ComponentCreator('/thingsgateway-docs/docs/foundation/s7', '98e'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/hardwareinfo',
        component: ComponentCreator('/thingsgateway-docs/docs/hardwareinfo', '108'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/hisalarm',
        component: ComponentCreator('/thingsgateway-docs/docs/hisalarm', '4f4'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/hisdata',
        component: ComponentCreator('/thingsgateway-docs/docs/hisdata', '327'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/linuxrelease',
        component: ComponentCreator('/thingsgateway-docs/docs/linuxrelease', 'b31'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/memoryvariable',
        component: ComponentCreator('/thingsgateway-docs/docs/memoryvariable', 'a47'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/otherconfig',
        component: ComponentCreator('/thingsgateway-docs/docs/otherconfig', '818'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/otherpage',
        component: ComponentCreator('/thingsgateway-docs/docs/otherpage', '98d'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginabcip',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginabcip', '913'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginconfig',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginconfig', '5cf'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/plugindebug',
        component: ComponentCreator('/thingsgateway-docs/docs/plugindebug', 'e87'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/plugindlt6452007',
        component: ComponentCreator('/thingsgateway-docs/docs/plugindlt6452007', 'a37'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginkafka',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginkafka', '6c4'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginmodbus',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginmodbus', '939'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginmodbusserver',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginmodbusserver', 'f24'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginmqttclient',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginmqttclient', '8e4'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginmqttserver',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginmqttserver', 'c02'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginopcdaclient',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginopcdaclient', '42a'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginopcuaclient',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginopcuaclient', '37e'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginopcuaserver',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginopcuaserver', '3c2'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginrabbitmqclient',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginrabbitmqclient', 'a46'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginsiemens',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginsiemens', 'ff9'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/pluginvigorvs',
        component: ComponentCreator('/thingsgateway-docs/docs/pluginvigorvs', '725'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/realalarm',
        component: ComponentCreator('/thingsgateway-docs/docs/realalarm', '408'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/realdata',
        component: ComponentCreator('/thingsgateway-docs/docs/realdata', 'ff7'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/release',
        component: ComponentCreator('/thingsgateway-docs/docs/release', '1df'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/rpclog',
        component: ComponentCreator('/thingsgateway-docs/docs/rpclog', '88d'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/runtimestate',
        component: ComponentCreator('/thingsgateway-docs/docs/runtimestate', '907'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/startguide',
        component: ComponentCreator('/thingsgateway-docs/docs/startguide', '764'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/upgrade',
        component: ComponentCreator('/thingsgateway-docs/docs/upgrade', '719'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/uploaddevice',
        component: ComponentCreator('/thingsgateway-docs/docs/uploaddevice', '067'),
        exact: true,
        sidebar: "docs"
      },
      {
        path: '/thingsgateway-docs/docs/windowsrelease',
        component: ComponentCreator('/thingsgateway-docs/docs/windowsrelease', '22f'),
        exact: true,
        sidebar: "docs"
      }
    ]
  },
  {
    path: '/thingsgateway-docs/',
    component: ComponentCreator('/thingsgateway-docs/', 'a9f'),
    exact: true
  },
  {
    path: '*',
    component: ComponentCreator('*'),
  },
];
