using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Polly;
using Polly.Retry;

namespace CosmosPeddler;

public class SpaceTradersHandler : DelegatingHandler
{
    private readonly ConcurrentDictionary<string, Task<HttpResponseMessage>> requests = new();
    private static readonly RetryPolicy retryHttpClonePolicy = Policy.Handle<Exception>().Retry(5, (e, i) => Godot.GD.Print($"Attempt {i} at cloning HTTP response failed"));

    public SpaceTradersHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri != null && requests.TryGetValue(request.RequestUri.ToString(), out var cachedResponse))
        {
            if (cachedResponse.IsCompleted)
            {
                requests.TryRemove(new KeyValuePair<string, Task<HttpResponseMessage>>(request.RequestUri.ToString(), cachedResponse));
            }
            else
            {
                Godot.GD.Print($"Returned cached response for {request.RequestUri}");
                return CloneHttpResponseMessage(await cachedResponse);
            }
        }

        var response = base.SendAsync(request, cancellationToken).ContinueWith(t =>
        {
            if (request.RequestUri != null && requests.ContainsKey(request.RequestUri.ToString()))
            {
                requests.TryRemove(new KeyValuePair<string, Task<HttpResponseMessage>>(request.RequestUri.ToString(), t));
            }

            return t.Result;
        });

        if (request.RequestUri != null && !requests.ContainsKey(request.RequestUri.ToString())) requests.TryAdd(request.RequestUri.ToString(), response);

        return CloneHttpResponseMessage(await response);
    }

    private static MemoryStream HttpResponseMessageToStream(HttpResponseMessage response)
    {
        var stream = new MemoryStream();
        response.Content.CopyTo(stream, null, new CancellationToken());
        stream.Position = 0;
        return stream;
    }

    private static HttpResponseMessage CloneHttpResponseMessage(HttpResponseMessage res)
    {
        return retryHttpClonePolicy.Execute(() =>
        {
            var clone = new HttpResponseMessage(res.StatusCode);

            // Copy the request's content (via a MemoryStream) into the cloned object
            if (res.Content != null)
            {
                lock(res)
                {
                    var ms = HttpResponseMessageToStream(res);

                    var ms2 = new MemoryStream();
                    ms.CopyTo(ms2);
                    ms.Position = 0;
                    ms2.Position = 0;

                    res.Content = new StreamContent(ms);
                    clone.Content = new StreamContent(ms2);
                }

                // Copy the content headers
                foreach (var h in res.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }

            clone.Version = res.Version;

            foreach (var header in res.Headers)
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

            foreach (var header in res.TrailingHeaders)
                clone.TrailingHeaders.TryAddWithoutValidation(header.Key, header.Value);

            clone.RequestMessage = res.RequestMessage;

            return clone;
        });
    }
}