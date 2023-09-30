export function prismHighlightLines (pre) {
    if (!pre) return;
    try{
        setTimeout(() => {
            Prism.plugins.lineHighlight.highlightLines(pre)();
        }, 300) // in code-group-item, need to wait for 0.3s transition animation
    } catch (err) {
        console.error(err);
    }
}