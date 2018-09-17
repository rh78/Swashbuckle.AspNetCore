using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class OpenApiDocumentExtensions
    {
        internal static bool ContainsSpecFor(
            this OpenApiDocument openApiDocument,
            HttpRequestMessage request,
            string statusCode,
            out string pathTemplate,
            out OperationType operationType)
        {
            pathTemplate = null;
            operationType = OperationType.Get;

            var pathEntry = openApiDocument.Paths
                .FirstOrDefault(entry =>
                {
                    var templateMatcher = new TemplateMatcher(TemplateParser.Parse(entry.Key), null);

                    var pathSansQueryString = request.RequestUri.ToString().Split('?').First();
                    return templateMatcher.TryMatch(new PathString(pathSansQueryString),
                        new RouteValueDictionary());
                });

            if (pathEntry.Value == null) return false;

            var operationEntry = pathEntry.Value.Operations
                .FirstOrDefault(entry =>
                    string.Equals(entry.Key.ToString(), request.Method.ToString(), StringComparison.InvariantCultureIgnoreCase));

            if (operationEntry.Value == null || !operationEntry.Value.Responses.ContainsKey(statusCode)) return false;

            pathTemplate = pathEntry.Key;
            operationType = operationEntry.Key;
            return true;
        }

        internal static OpenApiOperation GetOperationByPathAndType(
            this OpenApiDocument openApiDocument,
            string pathTemplate,
            OperationType operationType,
            out OpenApiPathItem pathSpec)
        {
            if (openApiDocument.Paths.TryGetValue(pathTemplate, out pathSpec))
            {
                if (pathSpec.Operations.ContainsKey(operationType))
                    return pathSpec.Operations[operationType];
            }

            throw new InvalidOperationException("TODO: Operation not found");
        }
    }
}
