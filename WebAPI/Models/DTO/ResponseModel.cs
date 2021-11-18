using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Enums;

namespace WebAPI.Models.DTO
{
    public class ResponseModel
    {

        public ResponseModel(ResponseCode responseCode, string responseMessage,object dataSet)
        {
            ResponseCode = responseCode;
            ResponseMessage = responseMessage;
            DataSet = dataSet;
        }
        public ResponseCode ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public object DataSet { get; set; }
    }
}
