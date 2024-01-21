export function switchTheme(dotNetHelper, dark, x, y) {
    document.documentElement.style.setProperty('--x', x + 'px')
    document.documentElement.style.setProperty('--y', y + 'px')
    document.startViewTransition(() => {
        dotNetHelper.invokeMethodAsync('ToggleTheme', dark);
    });
}

export function isDarkPreferColor() {
    return window.matchMedia('(prefers-color-scheme: dark)').matches
}

export function getTimezoneOffset() {
    const offset = new Date().getTimezoneOffset();
    return offset;
}

export function prismHighlightLines(pre) {
    if (!pre) return;
    try {
        setTimeout(() => {
            Prism.plugins.lineHighlight.highlightLines(pre)();
        }, 300) // in code-group-item, need to wait for 0.3s transition animation
    } catch (err) {
        console.error(err);
    }
}