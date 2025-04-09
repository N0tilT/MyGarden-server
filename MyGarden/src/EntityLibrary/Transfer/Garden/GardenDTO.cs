using EntitiesLibrary.Gardens;

namespace EntitiesLibrary.Transfer.Garden
{
    public record GardenDTO : IdentifiableEntityDTO
    {
        public required string UserId { get; init; }
        public List<Bed>? Beds { get; init; }
    }
}
