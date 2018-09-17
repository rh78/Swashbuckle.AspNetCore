using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.OpenApi.Models;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public class RequestValidator
    {
        private readonly IEnumerable<IContentValidator> _contentValidators;

        public RequestValidator(IEnumerable<IContentValidator> contentValidators)
        {
            _contentValidators = contentValidators;
        }

        public void Validate(
            string pathTemplate,
            OperationType operationType,
            OpenApiDocument openApiDocument,
            HttpRequestMessage request)
        {
            var operationSpec = openApiDocument.GetOperationByPathAndType(pathTemplate, operationType, out OpenApiPathItem pathSpec);
            var parameterSpecs = ExpandParameterSpecs(pathSpec, operationSpec, openApiDocument);

            // Convert to absolute Uri as a workaround to limitation with Uri class - i.e. most of it's methods are not supported for relative Uri's.
            var requestUri = new Uri(new Uri("http://tempuri.org"), request.RequestUri);

            if (!TryParsePathValues(pathTemplate, requestUri, out NameValueCollection pathValues))
                throw new RequestDoesNotMatchSpecException($"Request URI '{requestUri.AbsolutePath}' does not match specified template '{pathTemplate}'");

            if (request.Method != new HttpMethod(operationType.ToString()))
                throw new RequestDoesNotMatchSpecException($"Request method '{request.Method}' does not match specified operation type '{operationType}'");

            ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Path), openApiDocument, pathValues);
            ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Query), openApiDocument, requestUri.ParseQueryString());
            ValidateParameters(parameterSpecs.Where(p => p.In == ParameterLocation.Header), openApiDocument, request.Headers.ToNameValueCollection());

            if (operationSpec.RequestBody != null)
                ValidateContent(operationSpec.RequestBody, openApiDocument, request.Content);
        }

        private bool TryParsePathValues(string pathTemplate, Uri requestUri, out NameValueCollection pathValues)
        {
            var templateMatcher = new TemplateMatcher(TemplateParser.Parse(pathTemplate), null);

            var routeValues = new RouteValueDictionary();
            var success = templateMatcher.TryMatch(new PathString(requestUri.AbsolutePath), routeValues);

            pathValues = new NameValueCollection();
            foreach (var entry in routeValues)
            {
                pathValues.Add(entry.Key, entry.Value.ToString());
            }
            return success;
        }

        private static IEnumerable<OpenApiParameter> ExpandParameterSpecs(
            OpenApiPathItem pathSpec,
            OpenApiOperation operationSpec,
            OpenApiDocument openApiDocument)
        {
            var securityParameterSpecs = DeriveSecurityParameterSpecs(operationSpec, openApiDocument);

            return securityParameterSpecs
                .Concat(pathSpec.Parameters)
                .Concat(operationSpec.Parameters)
                .Select(p =>
                {
                    return (p.Reference != null)
                        ? (OpenApiParameter)openApiDocument.ResolveReference(p.Reference)
                        : p;
                });
        }

        private static IEnumerable<OpenApiParameter> DeriveSecurityParameterSpecs(
            OpenApiOperation operationSpec,
            OpenApiDocument openApiDocument)
        {
            // TODO
            return new OpenApiParameter[] { };
        }

        private void ValidateParameters(
            IEnumerable<OpenApiParameter> parameterSpecs,
            OpenApiDocument openApiDocument,
            NameValueCollection parameterValues)
        {
            foreach (var parameterSpec in parameterSpecs)
            {
                var value = parameterValues[parameterSpec.Name];

                if ((parameterSpec.In == ParameterLocation.Path || parameterSpec.Required) && value == null)
                    throw new RequestDoesNotMatchSpecException($"Required parameter '{parameterSpec.Name}' is not present");

                if (value == null || parameterSpec.Schema == null) continue;

                var schema = (parameterSpec.Schema.Reference != null)
                    ? (OpenApiSchema)openApiDocument.ResolveReference(parameterSpec.Schema.Reference)
                    : parameterSpec.Schema;

                if (!schema.TryParse(value, out object typedValue))
                    throw new RequestDoesNotMatchSpecException($"Parameter '{parameterSpec.Name}' is not of type '{parameterSpec.Schema.TypeIdentifier()}'");
            }
        }

        private void ValidateContent(OpenApiRequestBody requestBodySpec, OpenApiDocument openApiDocument, HttpContent content)
        {
            requestBodySpec = (requestBodySpec.Reference != null)
                ? (OpenApiRequestBody)openApiDocument.ResolveReference(requestBodySpec.Reference)
                : requestBodySpec;

            if (requestBodySpec.Required && content == null)
                throw new RequestDoesNotMatchSpecException("Required content is not present");

            if (content == null) return;

            if (!requestBodySpec.Content.TryGetValue(content.Headers.ContentType.MediaType, out OpenApiMediaType mediaTypeSpec))
                throw new RequestDoesNotMatchSpecException($"Content media type '{content.Headers.ContentType.MediaType}' is not specified");

            try
            {
                foreach (var contentValidator in _contentValidators)
                {
                    if (contentValidator.CanValidate(content.Headers.ContentType.MediaType))
                        contentValidator.Validate(mediaTypeSpec, openApiDocument, content);
                }
            }
            catch (ContentDoesNotMatchSpecException contentException)
            {
                throw new RequestDoesNotMatchSpecException($"Content does not match spec. {contentException.Message}");
            }
        }
    }

    public class RequestDoesNotMatchSpecException : Exception
    {
        public RequestDoesNotMatchSpecException(string message)
            : base(message)
        { }
    }
}