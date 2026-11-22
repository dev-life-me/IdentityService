using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dtos
{
    public record UpdateProfileRequest(string FullName, uint Version);
}
