using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Kanban.Tests.Integration.TestHelpers
{
    public static class SerializationExtensions
    {
        public static StringContent AsJsonStringContent(this object obj)
        {
            return new StringContent(JsonConvert.SerializeObject(obj), Encoding.Default, "application/json");
        }
    }
}