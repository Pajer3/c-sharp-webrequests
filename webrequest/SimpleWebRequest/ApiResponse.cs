using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleWebRequest
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }
    }
} 