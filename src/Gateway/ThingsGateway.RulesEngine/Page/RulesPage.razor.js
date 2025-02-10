export function initJS(id, src) {
    return new Promise((resolve, reject) => {
        // 检查是否已经存在同 ID 的脚本
        if (document.getElementById(id)) {
            console.warn(`Script with ID "${id}" already loaded.`);
            return resolve();
        }

        // 创建新的脚本元素
        const scriptElement = document.createElement("script");
        scriptElement.id = id;
        scriptElement.src = src;
        scriptElement.type = "text/javascript";
        scriptElement.async = true;

        // 绑定事件监听
        scriptElement.onload = () => {
            console.log(`Script with ID "${id}" loaded.`);
            resolve();
        };

        scriptElement.onerror = (err) => {
            console.error(`Failed to load script with ID "${id}": ${src}`);
            reject(err);
        };

        // 添加到文档的 head 中
        document.body.appendChild(scriptElement);
    });
}

export function disposeJS(id) {
    // 查找指定 ID 的脚本元素
    const scriptElement = document.getElementById(id);
    if (scriptElement) {
        document.body.removeChild(scriptElement);
        console.log(`Script with ID "${id}" removed.`);
    } else {
        console.warn(`No script found with ID "${id}" to dispose.`);
    }
}


export function initCSS(id, href) {
    return new Promise((resolve, reject) => {
        // 检查是否已经存在同 ID 的样式
        if (document.getElementById(id)) {
            console.warn(`CSS with ID "${id}" already loaded.`);
            return resolve();
        }

        // 创建新的 link 元素
        const linkElement = document.createElement("link");
        linkElement.id = id;
        linkElement.href = href;
        linkElement.rel = "stylesheet";
        linkElement.type = "text/css";

        // 绑定事件监听
        linkElement.onload = () => {
            console.log(`CSS with ID "${id}" loaded.`);
            resolve();
        };

        linkElement.onerror = (err) => {
            console.error(`Failed to load CSS with ID "${id}": ${href}`);
            reject(err);
        };

        // 添加到文档的 head 中
        document.head.appendChild(linkElement);
    });
}

export function disposeCSS(id) {
    // 查找指定 ID 的样式元素
    const linkElement = document.getElementById(id);
    if (linkElement) {
        document.head.removeChild(linkElement);
        console.log(`CSS with ID "${id}" removed.`);
    } else {
        console.warn(`No CSS found with ID "${id}" to dispose.`);
    }
}
