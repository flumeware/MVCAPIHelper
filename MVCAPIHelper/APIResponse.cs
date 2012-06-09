﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ServiceStack.Text;

//Copyright (c) 2012, Flumeware
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

//Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//Neither the name of the <ORGANIZATION> nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
namespace MVCAPIHelper
{
    public class MVCAPIHelperResponse:ActionResult
    {
        CommonResponseWrapper wrapper;
        string type;

        public MVCAPIHelperResponse(CommonResponseWrapper response,string responseType)
        {
            wrapper = response;

            if (string.IsNullOrWhiteSpace(responseType) == true)
            {
                type = "json";
            }
            else
            {

                type = responseType.Trim().ToLowerInvariant();
            }
        }

        public override void ExecuteResult(ControllerContext context)
        {
            string data = Serializer.SerializeToFormat<CommonResponseWrapper>(wrapper, context.HttpContext.Request.Params["format"]);

            context.HttpContext.Response.Write(data);
        }
    }
}