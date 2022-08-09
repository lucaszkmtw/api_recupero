using System;

namespace CentralOperativa.Domain.Projects
{
    [Flags]
    public enum ProjectStatus : byte
    {
        Proposed,
        Executing,
        Finished,
        Cancelled
    }
}
