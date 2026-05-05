using System;
using System.Collections.Generic;
using System.Text;

namespace PRESAM.Domain.Entities
{
    public class Result
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; } = string.Empty;

        public static Result Success(string message = "Operation successful")
        {
            return new Result { Succeeded = true, Message = message };
        }

        public static Result Failure(string message)
        {
            return new Result { Succeeded = false, Message = message };
        }
    }
}
