using System;
using HotConfig.Domain.Abstract.Base;

namespace HotConfig.Domain.Abstract.Model
{
    public interface ICacheModel
    {
        IModel Model { get; set; }
        string Key { get; set; }
        bool Renewable { get; set; }
        DateTime? ExpireDate { get; set; }
        bool IsExpired { get; }
    }
}