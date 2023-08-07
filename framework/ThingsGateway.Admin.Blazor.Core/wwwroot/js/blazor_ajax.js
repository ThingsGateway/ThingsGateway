export function blazor_ajax(url, method, data) {
    data = JSON.stringify(data);
    var res = null;
    $.ajax({
        url: url,
        data: data,
        method: method,
        contentType: 'application/json',
        dataType: 'json',
        async: false,
        success: function (result) {
            res = result;
        },
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            return null;
        }
    });
    if (res == null) {
        return null;
    }
    return JSON.stringify(res);
};

export function blazor_ajax_goto(url) {
    window.location.href = url;
}

export function blazor_downloadFile(url, fileName, dtoObject) {

    const params = new URLSearchParams();

    for (const key in dtoObject) {
        if (dtoObject[key] !== null) {
            params.append(key, dtoObject[key]);
        }
    }
    const fullUrl = `${url}?${params.toString()}`;
    fetch(fullUrl)
        .then(response => {
            const dispositionHeader = response.headers.get('content-disposition');
            let resolvedFileName = fileName;

            if (dispositionHeader) {
                const match = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/.exec(dispositionHeader);
                const serverFileName = match && match[1] ? match[1].replace(/['"]/g, '') : null;
                if (serverFileName) {
                    resolvedFileName = serverFileName;
                }
            }

            return response.blob().then(blob => {
                const fileUrl = window.URL.createObjectURL(blob);
                const anchorElement = document.createElement('a');
                anchorElement.href = fileUrl;
                anchorElement.download = resolvedFileName;
                anchorElement.style.display = 'none';
                document.body.appendChild(anchorElement);
                anchorElement.click();
                document.body.removeChild(anchorElement);
                window.URL.revokeObjectURL(fileUrl);
            });
        })
        .catch(error => {
            console.error('DownFile Error:', error);
        });



}