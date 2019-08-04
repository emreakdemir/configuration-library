using System;
using CodeSide.Domain.Concrete.Base;
using Newtonsoft.Json;

namespace CodeSide.Domain.Concrete.Model
{
    public class OperationLogModel : BaseModel
    {

        public string RequestedController { get; set; }
        public string RequestedAction { get; set; }
        public string RequestMethod { get; set; }
        public string IpAddress { get; set; }
        public DateTime OperationDate { get; set; }
        public string RequestPath { get; set; }

        [JsonIgnore]
        private string ComputedCacheKey { get; set; } = "";
        [JsonIgnore]
        public string CacheKey
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.ComputedCacheKey))
                {
                    this.ComputedCacheKey = $"{Guid.NewGuid().ToString()}-{this.RequestedController}-{this.RequestedAction}";
                }

                return this.ComputedCacheKey;
            }
        }

    }
}