using System.Threading.Tasks;

namespace Chibest.Repository.Interface
{
    public interface IUnitOfWork
    {
        Task BeginTransaction();
        Task CommitTransaction();
        Task RollbackTransaction();
        Task SaveChangesAsync();

        //IUserRepository UserRepository { get; }
        //IFoodRepository FoodRepository { get; }
        //IGeneralHealthProfileRepository HealthProfileRepository { get; }
        //IMealPlanRepository MealPlanRepository { get; }
        //IMealPlanDetailRepository MealPlanDetailRepository { get; }
        //IAllergyRepository AllergyRepository { get; }
        //IDiseaseRepository DiseaseRepository { get; }
        //IRecipeSuggestionRepository RecipeSuggestionRepository { get; }
        //IPersonalGoalRepository PersonalGoalRepository { get; }
        //ICuisineRepository CuisineRepository { get; }
        //IHealthcareIndicatorRepository HealthcareIndicatorRepository { get; }
        //IMealLogRepository MealLogRepository { get; }
        //IAIRecommendationMealPlanRepository AIRecommendationRepository { get; }
        //IIngredientRepository IngredientRepository { get; }
        //IAIRecommendationMeallogRepository AIRecommendationMeallogRepository { get; }
        //IUserIngredientPreferenceRepository UserIngredientPreferenceRepository { get; }
        //IPackageRepository PackageRepository { get; }
        //IUserPackageRepository UserPackageRepository { get; }
        //ISystemConfigurationRepository SystemConfigurationRepository { get; }
        //IUserFoodPreferenceRepository UserFoodPreferenceRepository { get; }
        //INotificationRepository NotificationRepository { get; }
    }
}