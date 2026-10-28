using System;
using System.Text.Json.Serialization;
using HotConfig.Domain.Abstract.Base;
using HotConfig.Domain.Abstract.Model;

namespace HotConfig.Domain.Concrete.Model
{
    public class CacheModel : ICacheModel
    {
        [JsonConverter(typeof(RuntimeTypeModelConverter))]
        public virtual IModel Model { get; set; }
        public string Key { get; set; }
        public bool Renewable { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsExpired => this.Renewable && this.ExpireDate.HasValue && this.ExpireDate.Value < DateTime.Now;

        public CacheModel()
        {
            this.Renewable = true;
        }
    }
}