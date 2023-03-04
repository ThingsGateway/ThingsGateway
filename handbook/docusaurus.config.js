module.exports = {
  title: "ThingsGateway",
  tagline: "ThingsGateway物联网关",
  url: "https://diego2098.gitee.io",
  baseUrl: "/thingsgateway",
  onBrokenLinks: "throw",
  onBrokenMarkdownLinks: "warn",
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
        src: "img/thingsgatewaylogo.png",
      },
      hideOnScroll: true,
      items: [
        {
          to: "docs",
          activeBasePath: "docs",
          label: "文档",
          position: "left",
        },
        {
          to: "/docs/upgrade",
          activeBasePath: "docs",
          label: "更新日志",
          position: "left",
        },
        {
          label: "源码",
          position: "right",
          href: "https://gitee.com/diego2098/ThingsGateway",

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
          editUrl: "https://gitee.com/diego2098/ThingsGateway/tree/master/handbook/",
          showLastUpdateTime: true,
          showLastUpdateAuthor: true,
          sidebarCollapsible: true,
          sidebarCollapsed: true,
          // sidebarCollapsible: true,
        },
        blog: {
          showReadingTime: true,
          editUrl: "https://gitee.com/diego2098/ThingsGateway/tree/master/handbook/",
        },
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
        language: ["en", "zh"],
        highlightSearchTermsOnTargetPage: true,
        explicitSearchResultPath: true,
      },
    ],
  ],
};
