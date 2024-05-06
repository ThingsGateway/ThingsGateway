import { getPreferredTheme, setTheme } from "/_content/BootstrapBlazor/modules/utility.js"
import EventHandler from "/_content/BootstrapBlazor/modules/event-handler.js"

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
