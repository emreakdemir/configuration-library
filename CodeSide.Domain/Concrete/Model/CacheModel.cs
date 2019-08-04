using System;
using CodeSide.Domain.Abstract.Base;
using CodeSide.Domain.Abstract.Model;

namespace CodeSide.Domain.Concrete.Model
{
    public class CacheModel : ICacheModel
    {
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