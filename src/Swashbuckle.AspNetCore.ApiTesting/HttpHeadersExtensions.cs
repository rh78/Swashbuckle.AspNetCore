using System;
using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace Swashbuckle.AspNetCore.ApiTesting
{
    public static class HttpHeadersExtensions
    {
        public static NameValueCollection ToNameValueCollection(this HttpHeaders httpHeaders)
        {
            var nameValueCollection = new NameValueCollection();  
            foreach (var entry in httpHeaders)
            {
                nameValueCollection.Add(entry.Key, string.Join(',', entry.Value));
            }
            return nameValueCollection;
        }
    }
}
