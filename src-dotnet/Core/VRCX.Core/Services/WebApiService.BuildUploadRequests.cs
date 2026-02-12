using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SixLabors.ImageSharp;
using VRCX.Core.Models.WebApi;
using VRCX.Core.Utils;

namespace VRCX.Core.Services;

public sealed partial class WebApiService
{
    private static HttpRequestMessage BuildLegacyImageUploadRequest(WebApiUploadImageLegacyRequest requestPayload)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, requestPayload.Url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        if (requestPayload.PostData is { } postData)
        {
            content.Add(new StringContent(postData), "data");
        }

        var imageData = requestPayload.ImageData;
        var fileToUpload = ImageUtils.ResizeImageToFitLimits(Convert.FromBase64String(imageData), false);

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "image", "image.png");

        request.Content = content;
        return request;
    }

    private static HttpRequestMessage BuildUploadFilePutRequest(WebApiUploadFilePutRequest requestPayload)
    {
        var request = new HttpRequestMessage(HttpMethod.Put, requestPayload.Url);

        var fileData = requestPayload.FileData;
        var sentData = Convert.FromBase64CharArray(fileData.ToCharArray(), 0, fileData.Length);

        var content = new ByteArrayContent(sentData);
        content.Headers.ContentType = new MediaTypeHeaderValue(requestPayload.FileMime);

        if (requestPayload.FileMd5 is { } fileMd5)
            content.Headers.ContentMD5 = Convert.FromBase64String(fileMd5);

        request.Content = content;
        return request;
    }

    private static HttpRequestMessage BuildImageUploadRequest(WebApiUploadImageRequest uploadImageRequest)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, uploadImageRequest.Url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        if (uploadImageRequest.PostData is { } postData)
        {
            var jsonPostData = JsonSerializer.Deserialize<Dictionary<string, string>>(postData);
            if (jsonPostData != null)
            {
                foreach (var data in jsonPostData)
                {
                    content.Add(new StringContent(data.Value), data.Key);
                }
            }
        }

        var imageData = uploadImageRequest.ImageData;
        var matchingDimensions = uploadImageRequest.MatchingDimensions;
        var fileToUpload = ImageUtils.ResizeImageToFitLimits(Convert.FromBase64String(imageData), matchingDimensions);

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(imageContent, "file", "blob");

        request.Content = content;
        return request;
    }

    private async ValueTask<HttpRequestMessage> BuildPrintImageUploadRequestAsync(
        WebApiUploadImagePrintRequest uploadImagePrintRequest)
    {
        var fileToUpload = await ProcessPrintImageDataAsync(
            uploadImagePrintRequest.ImageData,
            uploadImagePrintRequest.CropWhiteBorder
        );

        var request = new HttpRequestMessage(HttpMethod.Post, uploadImagePrintRequest.Url);
        var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
        var content = new MultipartFormDataContent(boundary);

        var imageContent = new ByteArrayContent(fileToUpload);
        imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        imageContent.Headers.ContentLength = fileToUpload.Length;
        content.Add(imageContent, "image", "image");

        if (uploadImagePrintRequest.PostData is { } postData)
        {
            var jsonPostData = JsonSerializer.Deserialize<Dictionary<string, string>>(postData);
            if (jsonPostData != null)
            {
                foreach (var (key, value) in jsonPostData)
                {
                    var stringContent = new StringContent(value, Encoding.UTF8, "text/plain");
                    content.Add(stringContent, key);
                }
            }
        }

        request.Content = content;
        return request;
    }

    private async ValueTask<byte[]> ProcessPrintImageDataAsync(string imageAsBase64, bool cropWhiteBoard)
    {
        var imageBytes = Convert.FromBase64String(imageAsBase64);

        if (cropWhiteBoard)
        {
            using var imageStream = new MemoryStream(imageBytes);
            using var print = await Image.LoadAsync(imageStream);
            if (ImageUtils.CropPrint(print))
            {
                using var cropResultStream = new MemoryStream();
                await print.SaveAsPngAsync(cropResultStream);

                imageBytes = cropResultStream.ToArray();
            }
        }

        return ImageUtils.ResizePrintImage(imageBytes);
    }
}