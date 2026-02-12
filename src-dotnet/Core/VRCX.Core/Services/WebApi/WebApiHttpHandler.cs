namespace VRCX.Core.Services.WebApi;

internal sealed class WebApiHttpHandler(AfterResponseDelegate afterResponse) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var response = await base.SendAsync(request, cancellationToken);
        await afterResponse(response);
        return response;
    }
}

internal delegate Task AfterResponseDelegate(HttpResponseMessage response);