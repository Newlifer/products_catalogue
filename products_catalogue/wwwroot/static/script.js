function getCode() {
    var url = new URL(window.location)
    return url.searchParams.get('code');
}

function getName() {
    var url = new URL(window.location)
    return url.searchParams.get('name');
}

const copyToClipboard = str => {
    if (navigator && navigator.clipboard && navigator.clipboard.writeText)
      return navigator.clipboard.writeText(str);
    return Promise.reject('The Clipboard API is not available.');
};

function copyDecimalToClipboard(row) {
    let productCode = `${row.cells[0].innerHTML}.${row.cells[1].innerHTML}.${row.cells[2].innerHTML}`;
    copyToClipboard(productCode)
}

function setSearchParam() {
    document.getElementById('code').value = getCode();
    document.getElementById('name').value = getName();
}

window.onload = setSearchParam;