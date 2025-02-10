var s = {
    canvases: {},
    tracked: {},
    getBoundingClientRect: el => {
        return el.getBoundingClientRect();
    },
    mo: new MutationObserver(() => {
        for (id in s.canvases) {
            const canvas = s.canvases[id];
            const lastBounds = canvas.lastBounds;
            const bounds = canvas.elem.getBoundingClientRect();
            if (lastBounds.left !== bounds.left || lastBounds.top !== bounds.top || lastBounds.width !== bounds.width ||
                lastBounds.height !== bounds.height) {
                canvas.lastBounds = bounds;
                canvas.ref.invokeMethodAsync('OnResize', bounds);
            }
        }
    }),
    ro: new ResizeObserver(entries => {
        for (const entry of entries) {
            let id = Array.from(entry.target.attributes).find(e => e.name.startsWith('_bl')).name.substring(4);
            let element = s.tracked[id];
            if (element) {
                element.ref.invokeMethodAsync('OnResize', entry.target.getBoundingClientRect());
            }
        }
    }),
    observe: (element, ref, id) => {
        if (!element) return;
        s.ro.observe(element);
        s.tracked[id] = {
            ref: ref
        };
        if (element.classList.contains("diagram-canvas")) {
            s.canvases[id] = {
                elem: element,
                ref: ref,
                lastBounds: element.getBoundingClientRect()
            };
        }
    },
    unobserve: (element, id) => {
        if (element) {
            s.ro.unobserve(element);
        }
        delete s.tracked[id];
        delete s.canvases[id];
    }
};
window.BlazorDiagrams = s;
window.addEventListener('scroll', () => {
    for (id in s.canvases) {
        const canvas = s.canvases[id];
        canvas.lastBounds = canvas.elem.getBoundingClientRect();
        canvas.ref.invokeMethodAsync('OnResize', canvas.lastBounds);
    }
});
s.mo.observe(document.body, { childList: true, subtree: true });
