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
//Neither the name of the Flumeware nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
namespace MVCAPIHelper
{
    public class MVCAPIHelperMethodAttribute:ActionFilterAttribute
    {
        public bool RequireAuth { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //check the authentication status
            string apiToken = filterContext.HttpContext.Request.Params["api_token"];
            bool isAuthed = false;
            if (string.IsNullOrWhiteSpace(apiToken) & RequireAuth == true)
            {
                //return error
                filterContext.Result = ErrorResponse(CommonResponseWrapper.StatusEnum.AccessTokenRequired);
                return;
            }

            //check the authentication
            Authentication.AuthenticationService authSvc = new Authentication.AuthenticationService();

            //try to validate the token
            string user = "";
            bool hasUser = false;

            if (string.IsNullOrWhiteSpace(apiToken) == false)
            {
                if (authSvc.ValidateToken(apiToken, out user) == false)
                {
                    //return error
                    filterContext.Result = ErrorResponse(CommonResponseWrapper.StatusEnum.InvalidAccessToken);
                    return;
                }
                else
                {
                    isAuthed = true;



                    if (string.IsNullOrWhiteSpace(user) == false)
                    {
                        hasUser = true;
                        filterContext.HttpContext.Items["api_token_user"] = user;
                    }


                }
            }

            if (RequireAuth == true & isAuthed == false)
            {
                //return error
                filterContext.Result = ErrorResponse(CommonResponseWrapper.StatusEnum.AccessTokenRequired);
                return;
            }

            //check for a throttle
            Quota.QuotaService quotaSvc = new Quota.QuotaService();
            if (isAuthed == true)
            {
                
                //try using the user id
                if (quotaSvc.ExceedingQuota(apiToken, hasUser) == true)
                {
                    filterContext.Result = ErrorResponse(CommonResponseWrapper.StatusEnum.ThrottleExceeded);
                    return;
                }

            }
            else
            {
                if (quotaSvc.ExceedingQuota() == true)
                {
                    filterContext.Result = ErrorResponse(CommonResponseWrapper.StatusEnum.ThrottleExceeded);
                    return;
                }
            }

            filterContext.HttpContext.Items["token_has_user"] = hasUser;
            filterContext.HttpContext.Items["api_token"] = apiToken;
        }

        private ActionResult ErrorResponse(CommonResponseWrapper.StatusEnum error)
        {
            CommonResponseWrapper wrapper = new CommonResponseWrapper();

            wrapper.Type = "";
            wrapper.Status = error;

            return new MVCAPIHelperResponse(wrapper, HttpContext.Current.Request.Params["format"]);
        }
    }
}
