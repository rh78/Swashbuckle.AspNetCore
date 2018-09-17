using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Swashbuckle.AspNetCore.ApiTesting.Xunit;
using Xunit;

namespace TestFirst.IntegrationTests
{
    public class ProductsApiTests : ApiTestFixture<TestFirst.Startup>
    {
        public ProductsApiTests(
            ApiTestRunner apiTestRunner,
            WebApplicationFactory<TestFirst.Startup> webApplicationFactory)
            : base(apiTestRunner, webApplicationFactory)
        {
            Describe("v1", "/api/products", new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [ OperationType.Post ] = new OpenApiOperation
                    {
                        Description = "Creates a new product",
                        RequestBody = new OpenApiRequestBody
                        {
                            Required = true,
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                [ "application/json" ] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "product" }
                                    }
                                }
                            }
                        },
                        Responses = new OpenApiResponses()
                        {
                            [ "400" ] = new OpenApiResponse
                            {
                                Description = "Invalid content"
                            },
                            [ "201" ] = new OpenApiResponse
                            {
                                Description = "Product created",
                                Headers = new Dictionary<string, OpenApiHeader>
                                {
                                    [ "Location" ] = new OpenApiHeader
                                    {
                                        Required = true
                                    }
                                }
                            }
                        }
                    },
                    [ OperationType.Get ] = new OpenApiOperation
                    {
                        Description = "Retrieves products",
                        Parameters = new List<OpenApiParameter>
                        {
                            new OpenApiParameter
                            {
                                Name = "pageNo",
                                In = ParameterLocation.Query,
                                Required = true,
                                Schema = new OpenApiSchema { Type = "number" }
                            },
                            new OpenApiParameter
                            {
                                Name = "pageSize",
                                In = ParameterLocation.Query,
                                Schema = new OpenApiSchema { Type = "number" }
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            [ "400" ] = new OpenApiResponse
                            {
                                Description = "Invalid parameters"
                            },
                            [ "200" ] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    [ "application/json" ] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Type = "array",
                                            Items = new OpenApiSchema
                                            {
                                                Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "product" }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });

            Describe("v1", "/api/products/{id}", new OpenApiPathItem
            {
                Parameters = new List<OpenApiParameter>
                {
                    new OpenApiParameter
                    {
                        Name = "id",
                        In = ParameterLocation.Path
                    }
                },
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [ OperationType.Get ] = new OpenApiOperation
                    {
                        Description = "Retrieves a specific product",
                        Responses = new OpenApiResponses
                        {
                            [ "404" ] = new OpenApiResponse
                            {
                                Description = "Product not found"
                            },
                            [ "200" ] = new OpenApiResponse
                            {
                                Content = new Dictionary<string, OpenApiMediaType>
                                {
                                    [ "application/json" ] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema
                                        {
                                            Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "product" }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    [ OperationType.Put ] = new OpenApiOperation
                    {
                        Description = "Updates a specific product",
                        RequestBody = new OpenApiRequestBody
                        {
                            Required = true,
                            Content = new Dictionary<string, OpenApiMediaType>
                            {
                                [ "application/json" ] = new OpenApiMediaType
                                {
                                    Schema = new OpenApiSchema
                                    {
                                        Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "product" }
                                    }
                                }
                            }
                        },
                        Responses = new OpenApiResponses
                        {
                            [ "404" ] = new OpenApiResponse
                            {
                                Description = "Product not found"
                            },
                            [ "400" ] = new OpenApiResponse
                            {
                                Description = "Invalid content"
                            },
                            [ "204" ] = new OpenApiResponse
                            {
                                Description = "Product updated"
                            }
                        }
                    },
                    [ OperationType.Delete ] = new OpenApiOperation
                    {
                        Description = "Deletes a specific product",
                        Responses = new OpenApiResponses
                        {
                            [ "404" ] = new OpenApiResponse
                            {
                                Description = "Product not found"
                            },
                            [ "204" ] = new OpenApiResponse
                            {
                                Description = "Product deleted"
                            }
                        }
                    }
                }
            });
        }

        [Fact]
        public async Task PostToProducts_Returns400_IfContentIsInvalid()
        {
            await TestAsync(
                "v1",
                "400",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = new StringContent(
                        JsonConvert.SerializeObject( new { } ),
                        Encoding.UTF8,
                        "application/json")
                }
            );
        }

        [Fact]
        public async Task PostToProducts_Returns201_IfContentIsValid()
        {
            await TestAsync(
                "v1",
                "201",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = new StringContent(
                        JsonConvert.SerializeObject( new { name = "foo" } ),
                        Encoding.UTF8,
                        "application/json")
                }
            );
        }

        [Fact]
        public async Task GetProducts_Returns400_IfRequiredParametersAreNotProvided()
        {
            await TestAsync(
                "v1",
                "400",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products", UriKind.Relative),
                    Method = HttpMethod.Get
                }
            );
        }

        [Fact]
        public async Task GetProducts_Returns200_IfRequiredParametersAreProvided()
        {
            await TestAsync(
                "v1",
                "200",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products?pageNo=1", UriKind.Relative),
                    Method = HttpMethod.Get
                }
            );
        }

        [Fact]
        public async Task GetProduct_Returns404_IfUnknownIdIsProvided()
        {
            await TestAsync(
                "v1",
                "404",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products/0", UriKind.Relative),
                    Method = HttpMethod.Get
                }
            );
        }

        [Fact]
        public async Task GetProduct_Returns200_IfKnownIdIsProvided()
        {
            await TestAsync(
                "v1",
                "200",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products/1", UriKind.Relative),
                    Method = HttpMethod.Get
                }
            );
        }

        [Fact]
        public async Task PutToProduct_Returns404_IfUnknownIdIsProvided()
        {
            await TestAsync(
                "v1",
                "404",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products/0", UriKind.Relative),
                    Method = HttpMethod.Put,
                    Content = new StringContent("", Encoding.UTF8, "application/json")
                }
            );
        }

        [Fact]
        public async Task PutToProduct_Returns400_IfContentIsInvalid()
        {
            await TestAsync(
                "v1",
                "400",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products/1", UriKind.Relative),
                    Method = HttpMethod.Put,
                    Content = new StringContent(
                        JsonConvert.SerializeObject( new { } ),
                        Encoding.UTF8,
                        "application/json")
                }
            );
        }

        [Fact]
        public async Task PutToProduct_Returns204_IfKnownIdIsProvidedAndContentIsValid()
        {
            await TestAsync(
                "v1",
                "204",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products/1", UriKind.Relative),
                    Method = HttpMethod.Put,
                    Content = new StringContent(
                        JsonConvert.SerializeObject( new { name = "foo" } ),
                        Encoding.UTF8,
                        "application/json")
                }
            );
        }

        [Fact]
        public async Task DeleteProduct_Returns404_IfUnknownIdIsProvided()
        {
            await TestAsync(
                "v1",
                "404",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products/0", UriKind.Relative),
                    Method = HttpMethod.Delete
                }
            );
        }

        [Fact]
        public async Task DeleteProduct_Returns204_IfKnownIdIsProvided()
        {
            await TestAsync(
                "v1",
                "204",
                new HttpRequestMessage
                {
                    RequestUri = new Uri("/api/products/1", UriKind.Relative),
                    Method = HttpMethod.Delete
                }
            );
        }
    }
}