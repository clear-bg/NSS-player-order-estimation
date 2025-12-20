using System;

namespace NssOrderTool.Models.Interfaces
{
  public interface ITimestamp
  {
    DateTime CreatedAt { get; set; }
    DateTime UpdatedAt { get; set; }
  }
}
