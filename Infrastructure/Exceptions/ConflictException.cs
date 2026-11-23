using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Exceptions
{
    public class ConflictException : Exception
    {
        public ConflictException() : base("Resource conflict detected.")
        {
        }

        public ConflictException(string message) : base(message)
        {
        }
    }
}
