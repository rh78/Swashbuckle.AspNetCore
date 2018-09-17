using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Xunit
{
    [Collection("ApiTests")]
    public class ApiTestFixture<TEntryPoint> :
        IClassFixture<WebApplicationFactory<TEntryPoint>> where TEntryPoint : class
    {
        private readonly ApiTestRunnerBase _apiTestRunner;
        private readonly WebApplicationFactory<TEntryPoint> _webAppFactory;

        public ApiTestFixture(
            ApiTestRunnerBase apiTestRunner,
            WebApplicationFactory<TEntryPoint> webAppFactory)
        {
            _apiTestRunner = apiTestRunner;
            _webAppFactory = webAppFactory;
        }

        public void Describe(string apiGroupName, string pathTemplate, OpenApiPathItem pathSpec)
        {
            _apiTestRunner.SetPathSpec(apiGroupName, pathTemplate, pathSpec);
        }

        public async Task TestAsync(string apiGroupName, string expectedStatusCode, HttpRequestMessage request)
        {
            await _apiTestRunner.TestAsync(
                apiGroupName,
                expectedStatusCode,
                request,
                _webAppFactory.CreateClient());
        }
    }
}
//
//        public void Describe(
//            string documentName,
//            string pathTemplate,
//            OperationType operationType,
//            OpenApiOperation operationSpec)
//        {
//            _documentName = documentName;
//            _pathTemplate = pathTemplate;
//            _operationType = operationType;
//
//            _apiTestRunner.AddOperation(documentName, pathTemplate, operationType, operationSpec);
//        }
//
//        public async Task TestAsync(HttpRequestMessage requestMessage)
//        {
//            await _apiTestRunner.TestAsync(
//                requestMessage,
//                _webAppFactory.CreateClient(), 
//                AssertResponseMatchesSpec);
//        }
//
//        public async Task TestAsync(string expectedStatusCode, Dictionary<string, object> requestParameters)
//            => await TestAsync(expectedStatusCode, requestParameters, null);
//
//        public async Task TestAsync(string expectedStatusCode, object requestBody)
//            => await TestAsync(expectedStatusCode, null, requestBody);
//
//        private void AssertResponseMatchesSpec(
//            OpenApiDocument openApiDocument,
//            string expectedStatusCode,
//            OpenApiResponse responseSpec,
//            HttpResponseMessage response)
//        {
//            Assert.Equal(expectedStatusCode, ((int)response.StatusCode).ToString());
//
//            AssertResponseHeaders(openApiDocument, responseSpec, response);
//
//            AssertResponseContent(openApiDocument, responseSpec, response);
//        }
//
//        private void AssertResponseHeaders(
//            OpenApiDocument openApiDocument,
//            OpenApiResponse responseSpec,
//            HttpResponseMessage response)
//        {
//            var responseHeaders = response.Headers.Select(entry => entry.Key);
//
//            foreach (var entry in responseSpec.Headers.Where(h => h.Value.Required))
//            {
//                Assert.Contains(entry.Key, responseHeaders);
//            }
//        }
//
//        private void AssertResponseContent(
//            OpenApiDocument openApiDocument,
//            OpenApiResponse responseSpec,
//            HttpResponseMessage response)
//        {
//            if (!responseSpec.Content.Any()) return;
//
//            Assert.NotNull(response.Content);
//            Assert.Contains(response.Content.Headers.ContentType.MediaType, responseSpec.Content.Keys);
//            
//            var mediaType = response.Content.Headers.ContentType.MediaType;
//            if (!mediaType.Contains("json", StringComparison.InvariantCultureIgnoreCase)) return;
//
//            var validator = new JsonValidator(openApiDocument);
//            var isValid = validator.Validate(
//                responseSpec.Content[mediaType].Schema,
//                JToken.Parse(response.Content.ReadAsStringAsync().Result),
//                out IEnumerable<string> errorMessages);
//
//            Assert.True(isValid, string.Join(", ", errorMessages));
//        }
//    }
//}