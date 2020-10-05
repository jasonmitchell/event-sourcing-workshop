using System;

namespace Kanban.Domain
{
    internal static class Ensure
    {
        public static void NotNullOrEmpty(string paramName, string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
            
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value was not provided.", paramName);
            }
        }
    }
}