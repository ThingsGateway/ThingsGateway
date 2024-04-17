
module.exports = {
    title: "thingsgateway",
    tagline: "物联网",
    url: "https://diego2098.gitee.io",
    baseUrl: "/thingsgateway-docs/",
  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "throw",
  favicon: "img/favicon.ico",
  projectName: "ThingsGateway",
  scripts: [],
  themeConfig: {
    zoom: {
      selector:
        ".markdown :not(em) > img,.markdown > img, article img[loading]",
      background: {
        light: "rgb(255, 255, 255)",
        dark: "rgb(50, 50, 50)",
      },
      // options you can specify via https://github.com/francoischalifour/medium-zoom#usage
      config: {},
    },
    docs: {
      sidebar: {
        hideable: true,
        autoCollapseCategories: true,
      },
    },
    prism: {
      additionalLanguages: ["powershell", "csharp", "sql"],
    },
    navbar: {
        title: "ThingsGateway",
        logo: {
            alt: "ThingsGateway Logo",
            src: "img/ThingsGatewayLogo.png",
      },
      hideOnScroll: true,
      items: [
        {
          label: "更新日志",
          position: "left",
          to: "docs/upgrade"
        },
        {
          label: "演示",
          position: "right",
          href: "http://120.24.62.140:5000",
        },
        {
          label: "源码",
          position: "right",
          items: [
            {
              label: "Gitee（主库）",
              href: "https://gitee.com/diego2098/ThingsGateway",
            },
            {
              label: "GitHub",
              href: "https://github.com/kimdiego2098/ThingsGateway",
            },
            {
              label: "Nuget",
              href: "https://www.nuget.org/profiles/kimdiego",
            },
          ],
        },
        // {
        //   label: "博客",
        //   position: "right",
        //   href: "https://www.cnblogs.com/ThingsGateway/collections/1104",
        // },
        {
          label: "视频",
          position: "right",
          href: "https://space.bilibili.com/88105259/channel/series",
        },
        {
          label: "社区",
          position: "right",
          href: "https://gitee.com/dotnetchina",
        },
      ],
    },
    footer: {
      style: "dark",
      links: [
        {
          title: "文档",
          items: [
            {
              label: "入门",
              to: "docs",
            },
            {
              label: "手册",
              to: "docs",
            },
          ],
        },
        {
          title: "社区",
          items: [
            {
              label: "讨论",
              href: "https://gitee.com/diego2098/ThingsGateway/issues",
            },
            {
              label: "看板",
              href: "https://gitee.com/diego2098/ThingsGateway/board",
            },
          ],
        },
        {
          title: "更多",
          items: [
            {
              label: "仓库",
              href: "https://gitee.com/diego2098/ThingsGateway",
            },
          ],
        },
      ],
      copyright: `Copyright © 2020-${new Date().getFullYear()} Diego.`,
    },
  },
  presets: [
    [
      "@docusaurus/preset-classic",
      {
        docs: {
          sidebarPath: require.resolve("./sidebars.js"),
          editUrl:
            "https://gitee.com/diego2098/ThingsGateway/tree/master/doc/",
          showLastUpdateTime: true,
          showLastUpdateAuthor: true,
          sidebarCollapsible: true,
          sidebarCollapsed: true,
          
          // sidebarCollapsible: true,
        },
        // blog: {
        //   showReadingTime: true,
        //   editUrl:
        //     "https://gitee.com/diego2098/ThingsGateway/tree/master/doc/",
        // },
        theme: {
          customCss: require.resolve("./src/css/custom.css"),
        },
      },
    ],
  ],
  plugins: [require.resolve("docusaurus-plugin-image-zoom")],
  themes: [
    [
      "@easyops-cn/docusaurus-search-local",
      {
        hashed: true,
        language: ["zh","en"],
        highlightSearchTermsOnTargetPage: true,
        explicitSearchResultPath: true,
      },
    ],
  ],
};
