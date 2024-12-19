export function toggle(id) {
    const el = document.getElementById(id)
    if (el === null) {
        return
    }
    const themeList = el.querySelector('.quickactions-list')
    //切换高度
    themeList.classList.toggle('is-open')
}