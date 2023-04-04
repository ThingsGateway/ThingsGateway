import Link from "@docusaurus/Link";
import { useColorMode } from "@docusaurus/theme-common";
import useBaseUrl from "@docusaurus/useBaseUrl";
import useDocusaurusContext from "@docusaurus/useDocusaurusContext";
import Layout from "@theme/Layout";
import components from "@theme/MDXComponents";
import React from "react";
import AndroidIcon from "./android.svg";
import DockerIcon from "./docker.svg";
import "./index.css";
import "./index.own.css";
import KubernetesIcon from "./kubernetes.svg";
import LinuxIcon from "./linux.svg";
import MacOSIcon from "./macos.svg";
import WindowIcon from "./windows.svg";

function Home() {
  const context = useDocusaurusContext();
  const { siteConfig = {} } = context;

  React.useEffect(() => {}, []);

  return (
    <Layout
      title={`ThingsGateway ${siteConfig.title}`}
      description="ThingsGateway物联网关"
    >
      <Banner />
      <Gitee />
    </Layout>
  );
}

function Banner() {
  return (
    <div className="ThingsGateway-banner">
      <div className="ThingsGateway-banner-container">
        <div className="ThingsGateway-banner-item">
          <div className="ThingsGateway-banner-project">
            ThingsGateway{" "}
            <span
              style={{ fontSize: 14, fontWeight: "normal", color: "#8759ff" }}
            ></span>
          </div>

          <div className="ThingsGateway-banner-description">
            基于NetCore的跨平台物联网关。
          </div>
          <ul className="ThingsGateway-banner-spec">
            <li> Apache-2.0 宽松开源协议，商业免费授权</li>
            <li>支持 .NET/6/7+ 平台</li>
          </ul>
          <div className="ThingsGateway-support-platform">受支持平台：</div>
          <div className="ThingsGateway-support-icons">
            <span>
              <WindowIcon height="39" width="39" />
            </span>
            <span>
              <LinuxIcon height="39" width="39" />
            </span>
            <span>
              <AndroidIcon height="39" width="39" />
            </span>
            <span>
              <MacOSIcon height="39" width="39" />
            </span>
            <span>
              <DockerIcon height="39" width="39" />
            </span>
            <span>
              <KubernetesIcon height="39" width="39" />
            </span>
          </div>
          <div className="ThingsGateway-get-start-btn">
            <Link className="ThingsGateway-get-start" to={useBaseUrl("docs")}>
              入门指南
              <span className="ThingsGateway-version">^1.3.0</span>
            </Link>
          </div>
        </div>

      </div>
    </div>
  );
}

function Gitee() {
  const { colorMode, setLightTheme, setDarkTheme } = useColorMode();
  const isDarkTheme = colorMode === "dark";

  return (
    <div className="ThingsGateway-content">
      <p className={"ThingsGateway-small-title" + (isDarkTheme ? " dark" : "")}>
        开源免费/商业免费授权
      </p>
      <h1 className={"ThingsGateway-big-title" + (isDarkTheme ? " dark" : "")}>
        ⭐️ Apache-2.0 开源协议，代码在 Gitee 平台托管 ⭐️
      </h1>

    </div>
  );
}

function CodeSection(props) {
  let { language, replace, section, source } = props;

  source = source.replace(/\/\/ <.*?\n/g, "");

  if (replace) {
    for (const [pattern, value] of Object.entries(replace)) {
      source = source.replace(new RegExp(pattern, "gs"), value);
    }
  }

  source = source.trim();
  if (!source.includes("\n")) {
    source += "\n";
  }

  return (
    <components.pre>
      <components.code
        children={source}
        className={`language-${language}`}
        mdxType="code"
        originalType="code"
        parentName="pre"
      />
    </components.pre>
  );
}

function SystemWindow(systemWindowProps) {
  const { children, className, ...props } = systemWindowProps;
  return (
    <div
      {...props}
      className={"system-window blue-accent preview-border " + className}
    >
      <div className="system-top-bar">
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#8759ff" }}
        />
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#3fc4fe" }}
        />
        <span
          className="system-top-bar-circle"
          style={{ backgroundColor: "#42ffac" }}
        />
      </div>
      {children}
    </div>
  );
}

export default Home;
