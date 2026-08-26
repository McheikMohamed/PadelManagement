using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Padel.Application.Exceptions;

public class RegleMetierException : Exception
{
    public string Code { get; }

    public RegleMetierException(string code, string message) : base(message)
    {
        Code = code;
    }
}