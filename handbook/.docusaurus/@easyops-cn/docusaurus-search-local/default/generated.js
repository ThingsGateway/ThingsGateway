import lunr from "E:\\Tg\\ThingsGateway\\ThingsGateway-DEV\\handbook\\node_modules\\lunr\\lunr.js";
require("E:\\Tg\\ThingsGateway\\ThingsGateway-DEV\\handbook\\node_modules\\lunr-languages\\lunr.stemmer.support.js")(lunr);
require("@easyops-cn/docusaurus-search-local/dist/client/shared/lunrLanguageZh").lunrLanguageZh(lunr);
require("E:\\Tg\\ThingsGateway\\ThingsGateway-DEV\\handbook\\node_modules\\lunr-languages\\lunr.multi.js")(lunr);
export const language = ["en","zh"];
export const removeDefaultStopWordFilter = false;
export const removeDefaultStemmer = false;
export { default as Mark } from "E:\\Tg\\ThingsGateway\\ThingsGateway-DEV\\handbook\\node_modules\\mark.js\\dist\\mark.js"
export const searchIndexUrl = "search-index{dir}.json?_=2660899d";
export const searchResultLimits = 8;
export const searchResultContextMaxLength = 50;
export const explicitSearchResultPath = true;
export const searchBarShortcut = true;
export const searchBarShortcutHint = true;
export const searchBarPosition = "right";
export const docsPluginIdForPreferredVersion = undefined;
export const indexDocs = true;
export const searchContextByPaths = null;
export const hideSearchBarWithNoSearchContext = false;