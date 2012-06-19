using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

//Copyright (c) 2012, Flumeware
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

//Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//Neither the name of the MVCAPIHelper nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
namespace MVCAPIHelper
{
    public class APIController:Controller
    {
        [NonAction]
        public ActionResult API<T>(T data)
        {
            return API<T>(data, "*");
        }

        [NonAction]
        public ActionResult API<T>(T data,string requiredUser)
        {
            CommonResponseWrapper wrapper = new CommonResponseWrapper();
            Quota.QuotaService quotaSvc = new Quota.QuotaService();

            wrapper.Type = typeof(T).Name;
            wrapper.Status = CommonResponseWrapper.StatusEnum.Success;

            wrapper.Data = Serializer.SerializeToFormat<T>(data, Request.Params["format"]);
            bool hasUser;

            bool.TryParse(HttpContext.Items["token_has_user"] as string, out hasUser);
            string token = HttpContext.Items["api_token"] as string;
            string tokenUser = HttpContext.Items["api_token_user"] as string;

            if (requiredUser != "*")
            {
                //validate the user
                if (requiredUser != tokenUser)
                {
                    //not authed
                    //return error
                    wrapper.Status = CommonResponseWrapper.StatusEnum.AccessDenied;
                    wrapper.Type = "";
                    wrapper.Data = "";
                }
            }

            if (string.IsNullOrWhiteSpace(token) == true)
            {
                wrapper.QuotaRemaining = quotaSvc.QuotaRemaining();
                wrapper.TotalQuota = quotaSvc.MaxIPQuota;
            }
            else
            {
                wrapper.QuotaRemaining = quotaSvc.QuotaRemaining(token, hasUser);

                wrapper.TotalQuota = quotaSvc.MaxTokenQuota;

                if (hasUser == true)
                {
                    wrapper.TotalQuota = quotaSvc.MaxUserTokenQuota;
                }
            }



            return new APIResponse(wrapper, Request.Params["format"]);
        }

        public string AuthenticatedUser
        {
            get
            {
                bool hasUser;

                bool.TryParse(HttpContext.Items["token_has_user"] as string, out hasUser);
                string tokenUser = HttpContext.Items["api_token_user"] as string;

                if (hasUser == false)
                {
                    return "";
                }
                else
                {
                    return tokenUser;
                }
            }
        }

        public string APIToken
        {
            get
            {
                return HttpContext.Items["api_token"] as string;
            }
        }

        public bool IsAPIAuthenticated
        {
            get
            {
                bool isAuthed = false;

                bool.TryParse(HttpContext.Items["is_authed"] as string, out isAuthed);

                return isAuthed;
            }
        }
    }
}
