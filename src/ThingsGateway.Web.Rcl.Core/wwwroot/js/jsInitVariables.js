export function getTimezoneOffset() {
    const offset = new Date().getTimezoneOffset();
    return offset;
}