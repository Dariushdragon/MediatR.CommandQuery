using System;
using System.Collections.Generic;

namespace EntityFrameworkCore.CommandQuery.SqlServer.Tests.Domain.Models
{
    public partial class TenantUpdateModel
        : EntityUpdateModel
    {
        #region Generated Properties
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsActive { get; set; }

        #endregion

    }
}
