using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Exceptions
{
    public class UnAuthException : Exception
    {
        public UnAuthException() : base("Un authorization")
        {
            
        }

        public UnAuthException(string message) : base(message)
        {
            
        }
    }
}
