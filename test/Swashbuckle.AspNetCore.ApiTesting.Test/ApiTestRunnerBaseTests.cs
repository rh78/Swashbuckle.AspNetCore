using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Xunit;

namespace Swashbuckle.AspNetCore.ApiTesting.Test
{
    public class ApiTestRunnerBaseTests
    {
        [Fact]
        public async Task TestAsync_ThrowsException_IfUnknownApiGroupNameIsProvided()
        {
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Subject().TestAsync(
                "v1",
                "200",
                new HttpRequestMessage(),
                CreateHttpClient()));

            Assert.Equal("No OpenAPI document configured for group name 'v1'", exception.Message);
        }

        [Theory]
        [InlineData("/api/foo", "GET", "200", "Spec. not provided for URI '/api/foo', method 'GET' and status code '200'")]
        [InlineData("/api/products", "POST", "200", "Spec. not provided for URI '/api/products', method 'POST' and status code '200'")]
        [InlineData("/api/products", "GET", "400", "Spec. not provided for URI '/api/products', method 'GET' and status code '400'")]
        [InlineData("/api/products", "GET", "200", null)]
        public async Task TestAsync_ThrowsException_IfSpecNotProvidedForRequestAndExpectedStatusCode(
            string requestUri,
            string requestMethod,
            string statusCode,
            string expectedExceptionMessage)
        {
            var subject = new FakeApiTestRunner();
            subject.Configure(c =>
            {
                c.OpenApiDocuments.Add("v1", new OpenApiDocument
                {
                    Paths = new OpenApiPaths
                    {
                        ["/api/products"] = new OpenApiPathItem
                        {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                [OperationType.Get] = new OpenApiOperation
                                {
                                    Responses = new OpenApiResponses
                                    {
                                        [ "200" ] = new OpenApiResponse() 
                                    }
                                }
                            }
                        }
                    }
                });
            });

            var exception = await Record.ExceptionAsync(() => subject.TestAsync(
                "v1",
                statusCode,
                new HttpRequestMessage
                {
                    RequestUri = new Uri(requestUri, UriKind.Relative),
                    Method = new HttpMethod(requestMethod)
                },
                CreateHttpClient()));
 
            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData("/api/products", "200", "Required parameter 'param' is not present")]
        [InlineData("/api/products?param=foo", "200", null)]
        public async Task TestAsync_ThrowsException_IfExpectedStatusCodeIs2xxAndRequestDoesNotMatchSpec(
            string requestUri,
            string statusCode,
            string expectedExceptionMessage)
        {
            var subject = new FakeApiTestRunner();
            subject.Configure(c =>
            {
                c.OpenApiDocuments.Add("v1", new OpenApiDocument
                {
                    Paths = new OpenApiPaths
                    {
                        ["/api/products"] = new OpenApiPathItem
                        {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                [OperationType.Get] = new OpenApiOperation
                                {
                                    Parameters = new List<OpenApiParameter>
                                    {
                                        new OpenApiParameter
                                        {
                                            Name = "param",
                                            Required = true,
                                            In = ParameterLocation.Query
                                        }
                                    },
                                    Responses = new OpenApiResponses
                                    {
                                        [ "200" ] = new OpenApiResponse() 
                                    }
                                }
                            }
                        }
                    }
                });
            });

            var exception = await Record.ExceptionAsync(() => subject.TestAsync(
                "v1",
                statusCode,
                new HttpRequestMessage
                {
                    RequestUri = new Uri(requestUri, UriKind.Relative),
                    Method = HttpMethod.Get
                },
                CreateHttpClient()));
 
            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        [Theory]
        [InlineData("/api/products", "400", "Status code '200' does not match expected value '400'")]
        [InlineData("/api/products?param=foo", "200", null)]
        public async Task TestAsync_ThrowsException_IfResponseDoesNotMatchSpec(
            string requestUri,
            string statusCode,
            string expectedExceptionMessage)
        {
            var subject = new FakeApiTestRunner();
            subject.Configure(c =>
            {
                c.OpenApiDocuments.Add("v1", new OpenApiDocument
                {
                    Paths = new OpenApiPaths
                    {
                        ["/api/products"] = new OpenApiPathItem
                        {
                            Operations = new Dictionary<OperationType, OpenApiOperation>
                            {
                                [OperationType.Get] = new OpenApiOperation
                                {
                                    Responses = new OpenApiResponses
                                    {
                                        [ "400" ] = new OpenApiResponse(),
                                        [ "200" ] = new OpenApiResponse() 
                                    }
                                }
                            }
                        }
                    }
                });
            });

            var exception = await Record.ExceptionAsync(() => subject.TestAsync(
                "v1",
                statusCode,
                new HttpRequestMessage
                {
                    RequestUri = new Uri(requestUri, UriKind.Relative),
                    Method = HttpMethod.Get
                },
                CreateHttpClient()));
 
            Assert.Equal(expectedExceptionMessage, exception?.Message);
        }

        private FakeApiTestRunner Subject()
        {
            return new FakeApiTestRunner();
        }

        private HttpClient CreateHttpClient()
        {
            var client = new HttpClient(new FakeHttpMessageHandler());
            client.BaseAddress = new Uri("http://tempuri.org");
            return client;
        }
    }

    internal class FakeApiTestRunner : ApiTestRunnerBase
    {
        public FakeApiTestRunner()
        {
        }
    }

    internal class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage());
        }
    }
}
