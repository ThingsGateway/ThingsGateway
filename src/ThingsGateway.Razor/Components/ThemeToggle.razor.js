import { getPreferredTheme, setTheme } from "/_content/BootstrapBlazor/modules/theme.js?v=$version"
import EventHandler from "/_content/BootstrapBlazor/modules/event-handler.js?v=$version"

export function init() {
    const themeElements = document.querySelectorAll('.icon-theme');
    if (themeElements) {
        themeElements.forEach(el => {
            EventHandler.on(el, 'click', e => {
                let theme = getPreferredTheme();
                if (theme === 'dark') {
                    theme = 'light';
                }
                else {
                    theme = 'dark';
                }
                setTheme(theme);
            });
        });
    }
}

export function dispose() {
}