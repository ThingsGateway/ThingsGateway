let handlers = {};

export function init(id, invoke, options) {
    //function getCellByClass(row, className) {
    //    // 直接用 querySelector 精确查找
    //    return row.querySelector(`td.${className}`);
    //}
    var variableHandler = setInterval(async () => {
        var admintable = document.getElementById(id);
        if (!admintable) {
            clearInterval(variableHandler);
            return;
        }

        var tables = admintable.getElementsByTagName('table');

        if (!tables || tables.length === 0) {
            return;
        }

        var table = tables[tables.length - 1];

        if (!table) {
            clearInterval(variableHandler);
            return;
        }

        var { method } = options;

        if (!invoke) return;
        var valss = await invoke.invokeMethodAsync(method);
        if (valss == null) return;
        for (let rowIndex = 0; rowIndex < valss.length; rowIndex++) {

            const vals = valss[rowIndex];
            if (vals == null) continue;


            var row = table.rows[rowIndex];
            if (!row) continue;


            for (let i = 0; i < vals.length; i++) {

                const cellValue = vals[i];
                if (cellValue == null) continue;

                //var cell = getCellByClass(row, cellName)
                var cell = row.cells[i + 2]

                if (!cell) continue;

                // 查找 tooltip span
                var cellDiv = cell.querySelector('.table-cell');
                if (cellDiv) {
                    var tooltipSpan = cell.querySelector('.bb-tooltip');
                    if (tooltipSpan) {

                        if (tooltipSpan.innerText != cellValue) {

                            tooltipSpan.innerText = cellValue ?? '';      // 更新显示文字
                            tooltipSpan.setAttribute('data-bs-original-title', cellValue ?? '');  // 同步 tooltip 提示

                        }
                        continue;


                    }
                    else {
                        if (cellDiv.innerText != cellValue) {

                            cellDiv.innerText = cellValue ?? '';
                        }
                    }
                    if (cellValue == "Online") {
                        cellDiv.classList.remove('red--text');
                        cellDiv.classList.add('green--text');
                    }
                    else if (cellValue == "Offline") {
                        cellDiv.classList.remove('green--text');
                        cellDiv.classList.add('red--text');
                    }
                    else {
                        cellDiv.classList.remove('red--text');
                        cellDiv.classList.remove('green--text');

                    }
                }

                //// 查找 switch
                //var switchDiv = cell.querySelector('.switch');
                //if (switchDiv) {
                //    if (cellValue === true || cellValue === "on" || cellValue === "True" || cellValue === "true") {
                //        switchDiv.classList.add('is-checked');
                //        switchDiv.classList.add('enable');
                //        switchDiv.classList.remove('is-unchecked');
                //        switchDiv.classList.remove('disabled');

                //        switchDiv.querySelectorAll('span')[0].classList.add('border-success');
                //        switchDiv.querySelectorAll('span')[0].classList.add('bg-success');

                //    } else {
                //        switchDiv.classList.remove('is-checked');
                //        switchDiv.classList.remove('enable');
                //        switchDiv.classList.add('is-unchecked');
                //        switchDiv.classList.add('disabled');

                //        switchDiv.querySelectorAll('span')[0].classList.remove('border-success');
                //        switchDiv.querySelectorAll('span')[0].classList.remove('bg-success');
                //    }
                //    continue;
                //}
                //// 默认情况（普通单元格）
                //cell.innerText = cellValue;


            }

        }

    }
        , 500) //1000ms刷新一次

    handlers[id] = { variableHandler, invoke };

}
export function dispose(id) {
    const handler = handlers[id];
    if (handler) {
        clearInterval(handler.timer);
        handler.invoke = null;
        delete handlers[id];
    }
}
