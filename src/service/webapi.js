// requires binding of WebApi

class WebApiService {
    clearCookies() {
        return WebApi.ClearCookies();
    }

    getCookies() {
        return WebApi.GetCookies();
    }

    setCookies(cookie) {
        return WebApi.SetCookies(cookie);
    }

    /**
     * @param {any} options
     * @returns {Promise<{status: number, data?: string}>}
     */
    async execute(options) {
        if (!options) {
            throw new Error('options is required');
        }

        const requestJson = JSON.stringify(options);
        var json = await WebApi.ExecuteJson(requestJson);
        var data = JSON.parse(json);
        if (data.status === -1) {
            throw new Error(data.bodyOrErrorMessage);
        }
        return {
            status: data.status,
            data: data.bodyOrErrorMessage
        };
    }
}

var self = new WebApiService();
window.webApiService = self;

export { self as default, WebApiService };
