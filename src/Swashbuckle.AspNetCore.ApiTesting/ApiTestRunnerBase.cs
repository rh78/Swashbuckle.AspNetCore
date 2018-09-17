using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public abstract class ApiTestRunnerBase : IDisposable
    {
        private readonly ApiTestRunnerOptions _options;
        private readonly RequestValidator _requestValidator;
        private readonly ResponseValidator _responseValidator;

        protected ApiTestRunnerBase()
        {
            _options = new ApiTestRunnerOptions();
            _requestValidator = new RequestValidator(_options.ContentValidators);
            _responseValidator = new ResponseValidator(_options.ContentValidators);
        }

        public void Configure(Action<ApiTestRunnerOptions> setupAction)
        {
            setupAction(_options);
        }

        public void SetPathSpec(string apiGroupName, string pathTemplate, OpenApiPathItem pathSpec)
        {
            var openApiDocument = GetOpenApiDocumentBy(apiGroupName);
            openApiDocument.Paths[pathTemplate] = pathSpec;
        }

        public async Task TestAsync(
            string apiGroupName,
            string expectedStatusCode,
            HttpRequestMessage request,
            HttpClient httpClient)
        {
            var openApiDocument = GetOpenApiDocumentBy(apiGroupName);

            if (!openApiDocument.ContainsSpecFor(request, expectedStatusCode, out string pathTemplate, out OperationType operationType))
            {
                throw new InvalidOperationException(
                    $"Spec. not provided for URI '{request.RequestUri}', method '{request.Method}' and status code '{expectedStatusCode}'");
            }

            if (expectedStatusCode.StartsWith("2"))
                _requestValidator.Validate(pathTemplate, operationType, openApiDocument, request);

            var response = await httpClient.SendAsync(request);

            _responseValidator.Validate(pathTemplate, operationType, expectedStatusCode, openApiDocument, response);
        }

        public void Dispose()
        {
            if (!_options.GenerateOpenApiFiles) return;

            if (_options.FileOutputRoot == null)
                throw new Exception("GenerateOpenApiFiles set but FileOutputRoot is null");

            foreach (var entry in _options.OpenApiDocuments)
            {
                var outputDir = Path.Combine(_options.FileOutputRoot, entry.Key);
                Directory.CreateDirectory(outputDir);

                using (var streamWriter = new StreamWriter(Path.Combine(outputDir, "openapi.json")))
                {
                    var openApiJsonWriter = new OpenApiJsonWriter(streamWriter);
                    entry.Value.SerializeAsV3(openApiJsonWriter);
                }
            }
        }
        private OpenApiDocument GetOpenApiDocumentBy(string apiGroupName)
        {
            if (!_options.OpenApiDocuments.TryGetValue(apiGroupName, out OpenApiDocument openApiDocument))
                throw new InvalidOperationException($"No Open API document configured for group name '{apiGroupName}'");

            return openApiDocument;
        }
    }
}