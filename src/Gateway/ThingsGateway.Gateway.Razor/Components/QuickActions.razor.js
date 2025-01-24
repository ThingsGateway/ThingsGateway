export function toggle(id) {
    const el = document.getElementById(id)
    if (el === null) {
        return
    }
    const themeList = el.querySelector('.quickactions-list')
    //切换高度
    themeList.classList.toggle('is-open')
}
export function getAutoRestartThread() {
    return JSON.parse(localStorage.getItem('autoRestartThread'))??true;
}
export function getShowType() {
    return JSON.parse(localStorage.getItem('showType'))??0;
}
export function saveShowType(showType) {
    if (localStorage) {
        localStorage.setItem('showType', JSON.stringify(showType));
    }
}
export function saveAutoRestartThread(autoRestartThread) {
    if (localStorage) {
        localStorage.setItem('autoRestartThread', JSON.stringify(autoRestartThread));
    }
}