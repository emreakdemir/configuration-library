using System;
using HotConfig.Domain.Concrete.Base;
using System.Text.Json.Serialization;

namespace HotConfig.Domain.Concrete.Model
{
    public class OperationLogModel : BaseModel
    {

        public string RequestedController { get; set; }
        public string RequestedAction { get; set; }
        public string RequestMethod { get; set; }
        public string IpAddress { get; set; }
        public DateTime OperationDate { get; set; }
        public string RequestPath { get; set; }

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