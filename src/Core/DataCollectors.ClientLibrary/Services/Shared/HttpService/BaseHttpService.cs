using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DataCollectors.ClientLibrary.Contracts.Responses;
using DataCollectors.ClientLibrary.Options;
using DataCollectors.ClientLibrary.Services.External.Configuration;
using DataCollectors.ClientLibrary.Services.Serialization;

namespace DataCollectors.ClientLibrary.Services.Shared.HttpService;

[ExcludeFromCodeCoverage]
public abstract class BaseHttpService
{
    private readonly IHttpClientFactory _httpClientFactory;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: true) },
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    protected BaseHttpService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    protected HttpClient CreateHttpClient(string name, WebApi webApi)
    {
        var client = _httpClientFactory.CreateClient(name);

        client.BaseAddress = new Uri(webApi.BaseUrl);

        if (webApi.Headers != null)
        {
            foreach (var header in webApi.Headers)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        }

        return client;
    }

    protected async Task<ServiceResponse<T>> Get<T>(HttpClient httpClient, string uri, CancellationToken cancellationToken = default)
    {
        var result = await SendRequest<T>(httpClient, HttpMethod.Get, uri, null, cancellationToken);
        return result;
    }

    protected async Task<ServiceResponse<T>> Post<T>(HttpClient httpClient, string uri, object? query = null, CancellationToken cancellationToken = default)
    {
        var result = await SendRequest<T>(httpClient, HttpMethod.Post, uri, query, cancellationToken);
        return result;
    }

    protected async Task<ServiceResponse<T>> PostExternal<T>(HttpClient httpClient, string uri, object? query = null, CancellationToken cancellationToken = default)
    {
        var result = await SendRequestExternal<T>(httpClient, HttpMethod.Post, uri, query, cancellationToken);
        return result;
    }

    private async Task<ServiceResponse<T>> SendRequest<T>(HttpClient httpClient, HttpMethod httpMethod, string uri, object? query = null, CancellationToken cancellationToken = default)
    {
        using var httpRequestMessage = CreateHttpRequestMessage(httpMethod, uri, query);

        ServiceResponse<T> result;

        try
        {
            using var response = await httpClient
                .SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            var responseString = await response
                .Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            result = response.IsSuccessStatusCode
                ? JsonSerializer.Deserialize<ServiceResponse<T>>(responseString, _jsonSerializerOptions)!
                : await BuildErrorResponse<T>(response, cancellationToken);
        }
        catch (Exception ex)
        {
            result = BuildErrorResponse<T>(ex);
        }

        return result;
    }

    private async Task<ServiceResponse<T>> SendRequestExternal<T>(HttpClient httpClient, HttpMethod httpMethod, string uri, object? query = null, CancellationToken cancellationToken = default)
    {
        using var httpRequestMessage = CreateHttpRequestMessage(httpMethod, uri, query);

        var result = new ServiceResponse<T>();

        try
        {
            using var response = await httpClient
                .SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            var responseString = await response
                .Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (response.IsSuccessStatusCode)
            {
                T responseContent;

                if (typeof(T) == typeof(string))
                {
                    responseContent = (T)(object)responseString;
                }
                else
                {
                    responseContent = JsonSerializer.Deserialize<T>(responseString, _jsonSerializerOptions)!;
                }

                result = new ServiceResponse<T>
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Result = new List<T>
                    {
                        responseContent
                    }
                };
            }
            else
            {
                result = await BuildErrorResponse<T>(response, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            result = BuildErrorResponse<T>(ex);
        }

        return result;
    }

    private async Task<ServiceResponse<T>> BuildErrorResponse<T>(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
    {
        ServiceResponse<T> result = new();

        var responseString = await responseMessage
            .Content
            .ReadAsStringAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(responseString))
        {
            result = new ServiceResponse<T>
            {
                StatusCode = (int)responseMessage.StatusCode,
                Errors = new List<string> { $"{responseMessage.StatusCode} : {responseMessage.ReasonPhrase} ({responseMessage.RequestMessage?.RequestUri})" }
            };

            return result;
        }

        if (responseMessage.StatusCode == HttpStatusCode.UnsupportedMediaType ||
            (responseMessage.StatusCode == HttpStatusCode.NotFound
            && responseString.StartsWith("<!DOCTYPE HTML")))
        {
            result = new ServiceResponse<T>
            {
                StatusCode = (int)responseMessage.StatusCode,
                Errors = new List<string> { $"{responseMessage.ReasonPhrase} - {responseMessage.RequestMessage?.RequestUri}" }
            };

            return result;
        }

        if ((responseMessage.StatusCode == HttpStatusCode.InternalServerError ||
            responseMessage.StatusCode == HttpStatusCode.MethodNotAllowed)
            && !string.IsNullOrWhiteSpace(responseString))
        {
            result = new ServiceResponse<T>
            {
                StatusCode = (int)responseMessage.StatusCode,
                Errors = new List<string> { responseString }
            };

            return result;
        }

        try
        {
            var errorServiceResponse = JsonSerializer.Deserialize<ServiceResponse<string>>(responseString, _jsonSerializerOptions);

            if (errorServiceResponse != null)
            {
                result = new ServiceResponse<T>
                {
                    StatusCode = errorServiceResponse.StatusCode,
                    Errors = errorServiceResponse.Errors
                };
            }
        }
        catch (Exception ex)
        {
            var jex = new Exception($"Error deserialising response: {ex.GetBaseException().Message}", ex);

            jex.Data.Add("Response Json:", responseString);
            jex.Data.Add("Original Status Code:", responseMessage.StatusCode.ToString());
            jex.Data.Add("Original Reason Phrase:", responseMessage.ReasonPhrase);
            jex.Data.Add("Original Request Uri:", responseMessage.RequestMessage?.RequestUri);
            throw jex;
        }

        return result;
    }

    private static ServiceResponse<T> BuildErrorResponse<T>(Exception ex)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var error = ex.Message;

        if (ex.Message.Contains("No connection could be made") ||
            ex.Message.Contains("No such host is known"))
        {
            statusCode = HttpStatusCode.ServiceUnavailable;
        }

        var result = new ServiceResponse<T>
        {
            StatusCode = (int)statusCode,
            Errors = new List<string> { error }
        };

        return result;
    }

    private static HttpRequestMessage CreateHttpRequestMessage(HttpMethod httpMethod, string uri, object? query = null)
    {
        var httpRequestMessage = new HttpRequestMessage(httpMethod, uri);

        if (query != null)
        {
            httpRequestMessage.Content = BuildContent(query);
        }

        httpRequestMessage.Headers.Add("Accept", MediaTypeNames.Application.Json);

        return httpRequestMessage;
    }

    private static StringContent BuildContent(object query)
    {
        dynamic stringContent = JsonSerializer.Serialize(query, CustomJsonSerializerOptions.Standard);
        var content = new StringContent(stringContent);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);

        return content;
    }
}