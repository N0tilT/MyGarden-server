namespace EntitiesLibrary.Transfer.Garden
{
    public static class GardenMapper
    {
        public static Gardens.Garden ToEntity(this RequestGardenDTO requestGarden)
        {
            return new Gardens.Garden
            {
                Id = requestGarden.Id,
                UserId = requestGarden.UserId,
                Beds = requestGarden.Beds ?? []
            };
        }


        public static GardenDTO ToDTO(this Gardens.Garden garden)
        {
            return new GardenDTO
            {
                Id = garden.Id,
                UserId = garden.UserId,
                Beds = garden.Beds,
                CreatedAt = garden.CreatedAt,
                UpdatedAt = garden.UpdatedAt
            };
        }
    }
}
