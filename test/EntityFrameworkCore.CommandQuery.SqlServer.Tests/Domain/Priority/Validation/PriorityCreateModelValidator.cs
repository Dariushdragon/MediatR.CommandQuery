using System;
using FluentValidation;
using EntityFrameworkCore.CommandQuery.SqlServer.Tests.Domain.Models;

namespace EntityFrameworkCore.CommandQuery.SqlServer.Tests.Domain.Validation
{
    public partial class PriorityCreateModelValidator
        : AbstractValidator<PriorityCreateModel>
    {
        public PriorityCreateModelValidator()
        {
            #region Generated Constructor
            RuleFor(p => p.Name).NotEmpty();
            RuleFor(p => p.Name).MaximumLength(100);
            RuleFor(p => p.Description).MaximumLength(255);
            #endregion
        }

    }
}
