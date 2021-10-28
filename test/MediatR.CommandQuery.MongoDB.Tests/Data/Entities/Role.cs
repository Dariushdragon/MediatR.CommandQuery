using MediatR.CommandQuery.Definitions;

using MongoDB.Abstracts;

namespace MediatR.CommandQuery.MongoDB.Tests.Data.Entities
{
    public partial class Role : MongoEntity, IHaveIdentifier<string>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }
    }
}
