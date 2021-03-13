﻿using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TaskCombinatorsExercises.Core
{
    public static class HttpClientExtensions
    {
        /*
         Write cancellable async method with timeout handling, that concurrently tries to get data from
         provided urls (first wins and its response is returned, rest is __cancelled__).
         
         Tips:
         * consider using HttpClient.GetAsync (as it is cancellable)
         * consider using Task.WhenAny
         * you may use urls like for testing https://postman-echo.com/delay/3
         * you should have problem with tasks cancellation -
            - how to merge tokens of operations (timeouts) with the provided token? 
            - Tip: you can link tokens with the help of CancellationTokenSource.CreateLinkedTokenSource(token)
         */
        public static async Task<string> ConcurrentDownloadAsync(this HttpClient httpClient,
            string[] urls, int millisecondsTimeout, CancellationToken token)
        {
            using (var cts = new CancellationTokenSource())
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(token, cts.Token))
            {
                var delayTask = Task.Delay(millisecondsTimeout, token);
                var requests = Task.WhenAny(urls.Select(x => GetHttpResponseAsync(httpClient, x, linkedCts.Token)));
                var first = await Task.WhenAny(delayTask, requests);

                if (first == delayTask)
                {
                    cts.Cancel();
                    throw new TaskCanceledException();
                }

                var firstRequest = await requests;
                string response = await firstRequest;
                cts.Cancel();
                return response;
            }
        }

        private static async Task<string> GetHttpResponseAsync(HttpClient httpClient, string url, CancellationToken token)
        {
            var response = await httpClient.GetAsync(url, token);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
