class CoreIpcApi {
    constructor() {
        window.jsonIpcApi = new (class {
            lastRequestId = 0;
            requestestMap = new Map();

            constructor() {
                if (!window.__webview_interop__)
                    window.__webview_interop__ = window.chrome.webview;

                window.__webview_interop__.addEventListener(
                    'message',
                    (arg) => {
                        try {
                            // CefGlue: CustomEvent, arg.data for WebView2
                            const payload = JSON.parse(arg.data ?? arg.detail);

                            const promiseActions = this.requestestMap.get(
                                payload.data.requestId
                            );

                            promiseActions.resolve(payload.data.resultJson);
                        } catch (e) {
                            console.error(
                                'Failed to handle message from .NET:',
                                e
                            );

                            promiseActions.reject(e);
                        }
                    }
                );
            }

            async InvokeJsonIpcMethod(className, methodName, argsJson) {
                const requestId = (this.lastRequestId++).toString();

                window.__webview_interop__.postMessage(
                    JSON.stringify({
                        type: 'InvokeJsonIpcMethod',
                        data: {
                            className,
                            methodName,
                            argsJson,
                            requestId
                        }
                    })
                );

                const promise = new Promise((resolve, reject) => {
                    this.requestestMap.set(requestId, {
                        resolve,
                        reject
                    });
                });

                return await promise;
            }
        })();

        return new Proxy(this, {
            get(target, prop) {
                if (!CORE) {
                    return undefined;
                }
                // If the property is not a method of InteropApi,
                // treat it as a .NET class name
                if (typeof prop === 'string' && !target[prop]) {
                    return new Proxy(
                        {},
                        {
                            get(_, methodName) {
                                // Return a method that calls the .NET method dynamically
                                return async (...args) => {
                                    return await target.callMethod(
                                        prop,
                                        methodName,
                                        ...args
                                    );
                                };
                            }
                        }
                    );
                }
                return target[prop];
            }
        });
    }

    async callMethod(className, methodName, ...args) {
        const response = await window.jsonIpcApi.InvokeJsonIpcMethod(
            className,
            methodName,
            JSON.stringify(args)
        );

        return JSON.parse(response);
    }
}

export default new CoreIpcApi();
