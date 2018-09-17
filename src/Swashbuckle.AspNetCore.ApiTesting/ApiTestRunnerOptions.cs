using System.Collections.Generic;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class ApiTestRunnerOptions
    {
        public ApiTestRunnerOptions()
        {
            OpenApiDocuments = new Dictionary<string, OpenApiDocument>();
            ContentValidators = new List<IContentValidator> { new JsonContentValidator() };
            GenerateOpenApiFiles = false;
            FileOutputRoot = null;
        }

        public Dictionary<string, OpenApiDocument> OpenApiDocuments { get; }

        public List<IContentValidator> ContentValidators { get; }

        public bool GenerateOpenApiFiles { get; set; }

        public string FileOutputRoot { get; set; }
    }
}