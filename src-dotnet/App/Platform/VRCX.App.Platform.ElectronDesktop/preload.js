const { contextBridge, ipcRenderer } = require('electron');

const webviewMessenger = new (class extends EventTarget {
    postMessage(message) {
        ipcRenderer.send('webview-message', message);
    }
})();

ipcRenderer.on('webview-message', (event, message) => {
    console.log('Received message from main process:', message);
    webviewMessenger.dispatchEvent(
        new CustomEvent('message', { detail: message })
    );
});

contextBridge.exposeInMainWorld('__webview_interop__', {
    addEventListener: (event, callback) => {
        webviewMessenger.addEventListener(event, (arg) =>
            callback({
                detail: arg.detail
            })
        );
    },
    postMessage: (message) => {
        webviewMessenger.postMessage(message);
    }
});
