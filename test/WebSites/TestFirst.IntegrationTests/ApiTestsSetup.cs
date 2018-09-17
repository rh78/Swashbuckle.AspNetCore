using System.Collections.Generic;
using System.IO;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.ApiTesting;
using Xunit;

namespace TestFirst.IntegrationTests
{
    [CollectionDefinition("ApiTests")]
    public class ApiTestsCollection : ICollectionFixture<ApiTestRunner>
    {}

    public class ApiTestRunner : ApiTestRunnerBase
    {
        public ApiTestRunner()
        {
            Configure(c =>
            {
                c.OpenApiDocuments.Add("v1", new OpenApiDocument
                {
                    Info = new OpenApiInfo
                    {
                        Version = "V1",
                        Title = "V1 API",
                    },
                    Paths = new OpenApiPaths(),
                    Components = new OpenApiComponents
                    {
                        Schemas = new Dictionary<string, OpenApiSchema>
                        {
                            [ "product" ] = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    [ "id" ] = new OpenApiSchema { Type = "number" },
                                    [ "name" ] = new OpenApiSchema { Type = "string" }
                                },
                                Required = new SortedSet<string> { "name" }
                            }
                        }
                    }
                });

                c.GenerateOpenApiFiles = true;
                c.FileOutputRoot = Path.Combine(System.AppContext.BaseDirectory, "..", "..", "..", "..", "TestFirst", "wwwroot", "api-docs");
            });
        }
    }
}