using System;
using CodeSide.Domain.Abstract.Base;

namespace CodeSide.Domain.Abstract.Model
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