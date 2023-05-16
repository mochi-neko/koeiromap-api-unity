#nullable enable
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using Cysharp.Threading.Tasks;
using Mochineko.Relent.Extensions.NewtonsoftJson;
using Mochineko.Relent.Result;
using Mochineko.Relent.UncertainResult;
using Newtonsoft.Json;

namespace Mochineko.KoeiromapAPI
{
    public static class SpeechSynthesisAPI
    {
        private const string EndPoint = "https://api.rinna.co.jp/models/cttse/koeiro";
        private const string ResponsePrefix = "data:audio/x-wav;base64,";

        public static async UniTask<IUncertainResult<SpeechSynthesisResult>> SynthesizeSpeechAsync(
            HttpClient httpClient,
            string text,
            CancellationToken cancellationToken,
            float? speakerX = null,
            float? speakerY = null,
            Style? style = null,
            ulong? seed = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return UncertainResults.FailWithTrace<SpeechSynthesisResult>(
                    "Failed because text is null or empty.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return UncertainResults.RetryWithTrace<SpeechSynthesisResult>(
                    "Retryable because cancellation has been already requested.");
            }

            // Create request body
            var requestBody = new RequestBody(
                text,
                speakerX,
                speakerY,
                style,
                seed);

            // Serialize request body
            string requestBodyJson;
            var serializationResult = RelentJsonSerializer.Serialize(
                requestBody,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
            if (serializationResult is ISuccessResult<string> serializationSuccess)
            {
                requestBodyJson = serializationSuccess.Result;
            }
            else if (serializationResult is IFailureResult<string> serializationFailure)
            {
                return UncertainResults.FailWithTrace<SpeechSynthesisResult>(
                    $"Failed because -> {serializationFailure}.");
            }
            else
            {
                throw new ResultPatternMatchException(nameof(serializationResult));
            }

            // Build request message
            var requestMessage = new HttpRequestMessage(
                HttpMethod.Post,
                EndPoint);

            // Request contents
            var requestContent = new StringContent(
                content: requestBodyJson,
                encoding: System.Text.Encoding.UTF8,
                mediaType: "application/json");
            requestMessage.Content = requestContent;

            HttpResponseMessage responseMessage;
            var apiResult = await UncertainTryFactory
                .TryAsync<HttpResponseMessage>(async innerCancellationToken
                    => await httpClient.SendAsync(requestMessage, innerCancellationToken))
                .CatchAsRetryable<HttpResponseMessage, HttpRequestException>(exception
                    => $"Retryable because -> {exception}.")
                .CatchAsRetryable<HttpResponseMessage, OperationCanceledException>(exception
                    => $"Retryable because -> {exception}.")
                .CatchAsFailure<HttpResponseMessage, Exception>(exception
                    => $"Failure because -> {exception}.")
                .ExecuteAsync(cancellationToken);
            switch (apiResult)
            {
                case IUncertainSuccessResult<HttpResponseMessage> apiSuccess:
                    responseMessage = apiSuccess.Result;
                    break;

                case IUncertainRetryableResult<HttpResponseMessage> apiRetryable:
                    return UncertainResults.RetryWithTrace<SpeechSynthesisResult>(
                        $"Retryable the API because -> {apiRetryable.Message}.");

                case IUncertainFailureResult<HttpResponseMessage> apiFailure:
                    return UncertainResults.FailWithTrace<SpeechSynthesisResult>(
                        $"Failed the API because -> {apiFailure.Message}.");

                default:
                    throw new ResultPatternMatchException(nameof(apiResult));
            }

            var responseString = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseString))
            {
                return UncertainResults.FailWithTrace<SpeechSynthesisResult>(
                    $"Failed because {nameof(responseString)} was null or empty.");
            }

            // Succeeded
            if (responseMessage.IsSuccessStatusCode)
            {
                // Deserialize response body
                var deserializationResult = RelentJsonSerializer.Deserialize<ResponseBody>(responseString);
                switch (deserializationResult)
                {
                    case ISuccessResult<ResponseBody> deserializationSuccess:
                    {
                        var responseBody = deserializationSuccess.Result;
                        if (!responseBody.Audio.StartsWith(ResponsePrefix))
                        {
                            return UncertainResults.FailWithTrace<SpeechSynthesisResult>(
                                $"Failed because response text prefix was not start with \"{ResponsePrefix}\" -> {responseBody.Audio}.");
                        }

                        // Base64 decode
                        var base64EncodedAudio = responseBody.Audio
                            .Remove(0, ResponsePrefix.Length);

                        var base64DecodedAudio = Convert.FromBase64String(base64EncodedAudio);

                        return UncertainResults.Succeed(
                            new SpeechSynthesisResult(
                                new MemoryStream(base64DecodedAudio),
                                responseBody.Phonemes)
                        );
                    }

                    case IFailureResult<ResponseBody> deserializationFailure:
                        return UncertainResults.FailWithTrace<SpeechSynthesisResult>(
                            $"Failed because -> {deserializationFailure.Message}.");

                    default:
                        throw new ResultPatternMatchException(nameof(deserializationResult));
                }
            }
            // Retryable
            else if (responseMessage.StatusCode is HttpStatusCode.TooManyRequests
                     || (int)responseMessage.StatusCode is >= 500 and <= 599)
            {
                return UncertainResults.RetryWithTrace<SpeechSynthesisResult>(
                    $"Retryable because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode} with response -> {responseString}.");
            }
            // Response error
            else
            {
                return UncertainResults.FailWithTrace<SpeechSynthesisResult>(
                    $"Failed because the API returned status code:({(int)responseMessage.StatusCode}){responseMessage.StatusCode} with response -> {responseString}."
                );
            }
        }
    }
}