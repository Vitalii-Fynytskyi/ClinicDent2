using System;
using System.Collections.Generic;
using System.Text;

namespace ClinicDent2.Exceptions
{
    public class ConflictException : Exception
    {
        public object Param;
        public ConflictException() { }
        public ConflictException(string message) : base(message) { }
        public ConflictException(string message, object param) : base(message)
        {
            Param = param;
        }
    }
}
