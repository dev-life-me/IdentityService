using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException() : base("Resource not found.")
        {
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }
}
