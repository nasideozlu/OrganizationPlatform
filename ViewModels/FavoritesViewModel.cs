using OrganizationPlatform.Models;

namespace OrganizationPlatform.ViewModels;

public class FavoritesViewModel
{
    public List<BusinessFavorite> BusinessFavorites { get; set; } = new();

    public List<FavoriteVenue> VenueFavorites { get; set; } = new();
}