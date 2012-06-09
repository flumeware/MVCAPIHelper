using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

//Copyright (c) 2012, Flumeware
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

//Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//Neither the name of the <ORGANIZATION> nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
namespace MVCAPIHelper.Quota
{
    public class QuotaService
    {
        IQuotaStore store;

        public double MaxIPQuota { get; set; }
        public double MaxTokenQuota { get; set; }
        public double MaxUserTokenQuota { get; set; }

        public QuotaService()
        {
            store = Current.QuotaStore;
            MaxIPQuota = Current.MaxRequestPerIpPerMin;
            MaxTokenQuota = Current.MaxRequestPerTokenPerMin;
            MaxUserTokenQuota = Current.MaxRequestPerUserPerMin;
        }

        public bool ExceedingQuota()
        {
            //get the ip
            string ip;

            ip = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrWhiteSpace(HttpContext.Current.Request.Headers["X-Forwarded-For"]) == false)
            {
                ip = HttpContext.Current.Request.Headers["X-Forwarded-For"];
            }

            return ExceedingQuotaInternal(ip,MaxIPQuota);
        }

        public bool ExceedingQuota(string token,bool hasUser)
        {
            if (hasUser == true)
            {
                token = token + ":user";
                return ExceedingQuotaInternal(token, MaxUserTokenQuota);
            }
            else
            {
                return ExceedingQuotaInternal(token,MaxTokenQuota);
            }
        }

        private bool ExceedingQuotaInternal(string key,double maxQuota)
        {
            Quota q = store.GetQuota(key);

            if (q == null)
            {
                CreateNewQuota(key, maxQuota - 1);
                return false;
            }

            if (q.Expires < DateTime.UtcNow)
            {
                store.DeleteQuota(q.Key);
                CreateNewQuota(key, maxQuota - 1);
                return false;
            }

            if (q.QuotaRemaing <= 0)
            {
                return true;
            }

            return false;
        }

        private double QuotaRemainingInternal(string key,double maxQuota)
        {
            Quota q = store.GetQuota(key);

            if (q == null)
            {
                CreateNewQuota(key, maxQuota - 1);
            }

            if (q.Expires < DateTime.UtcNow)
            {
                store.DeleteQuota(q.Key);
                CreateNewQuota(key, maxQuota - 1);
            }

            return maxQuota - 1;
        }

        public double QuotaRemaining()
        {
            //get the ip
            string ip;

            ip = HttpContext.Current.Request.UserHostAddress;

            if (string.IsNullOrWhiteSpace(HttpContext.Current.Request.Headers["X-Forwarded-For"]) == false)
            {
                ip = HttpContext.Current.Request.Headers["X-Forwarded-For"];
            }

            return QuotaRemainingInternal(ip, MaxIPQuota);
        }

        public double QuotaRemaining(string token, bool hasUser)
        {
            if (hasUser == true)
            {
                token = token + ":user";
                return QuotaRemainingInternal(token, MaxUserTokenQuota);
            }
            else
            {
                return QuotaRemainingInternal(token, MaxTokenQuota);
            }
        }

        private void CreateNewQuota(string key, double maxQuota)
        {
            Quota q = new Quota();

            q.Key = key;
            q.QuotaRemaing = maxQuota;

            DateTime zeroMin = DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(DateTime.UtcNow.Second));

            q.Expires = zeroMin.AddMinutes(1);

            store.AddQuota(q);
        }
    }
}
