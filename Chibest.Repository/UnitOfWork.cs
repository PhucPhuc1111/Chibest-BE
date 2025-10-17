using Chibest.Repository.Interface;
using Chibest.Repository.Models;

namespace Chibest.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        protected readonly ChiBestDbContext _context;
        //private IUserRepository _userRepository;
        //private IFoodRepository _foodRepository;
        //private IGeneralHealthProfileRepository _healthProfileRepository;
        //private IMealPlanRepository _mealPlanRepository;
        //private IMealPlanDetailRepository _mealPlanDetailRepository;
        //private IAllergyRepository _allergyRepository;
        //private IDiseaseRepository _diseaseRepository;
        //private IRecipeSuggestionRepository _recipeSuggestionRepository;
        //private IPersonalGoalRepository _personalGoalRepository;
        //private ICuisineRepository _cuisineRepository;
        //private IHealthcareIndicatorRepository _healthcareIndicatorRepository;
        //private IMealLogRepository _mealLogRepository;
        //private IAIRecommendationMealPlanRepository _aIRecommendationMealPlanRepository;
        //private IIngredientRepository _ingredientRepository;
        //private IAIRecommendationMeallogRepository _aIRecommendationMeallogRepository;
        //private IUserIngredientPreferenceRepository _userIngredientPreferenceRepository;
        //private IPackageRepository _packageRepository;
        //private IUserPackageRepository _userPackageRepository;
        //private ISystemConfigurationRepository _systemConfigurationRepository;
        //private IUserFoodPreferenceRepository _userFoodPreferenceRepository;
        //private INotificationRepository _notificationRepository;

        public UnitOfWork(ChiBestDbContext context)
        {
            _context = context;
        }

        //public IUserRepository UserRepository => _userRepository ??= new UserRepository(_context);
        //public IFoodRepository FoodRepository => _foodRepository ??= new FoodRepository(_context);
        //public IGeneralHealthProfileRepository HealthProfileRepository => _healthProfileRepository ??= new GeneralHealthProfileRepository(_context);
        //public IMealPlanRepository MealPlanRepository => _mealPlanRepository ??= new MealPlanRepository(_context);
        //public IMealPlanDetailRepository MealPlanDetailRepository => _mealPlanDetailRepository ??= new MealPlanDetailRepository(_context);
        //public IAllergyRepository AllergyRepository => _allergyRepository ??= new AllergyRepository(_context);
        //public IDiseaseRepository DiseaseRepository => _diseaseRepository ??= new DiseaseRepository(_context);
        //public IRecipeSuggestionRepository RecipeSuggestionRepository => _recipeSuggestionRepository ??= new RecipeSuggestionRepository(_context);
        //public IPersonalGoalRepository PersonalGoalRepository => _personalGoalRepository ??= new PersonalGoalRepository(_context);
        //public ICuisineRepository CuisineRepository => _cuisineRepository ??= new CuisineRepository(_context);
        //public IHealthcareIndicatorRepository HealthcareIndicatorRepository => _healthcareIndicatorRepository ??= new HealthcareIndicatorRepository(_context);
        //public IMealLogRepository MealLogRepository => _mealLogRepository ??= new MealLogRepository(_context);
        //public IAIRecommendationMealPlanRepository AIRecommendationRepository => _aIRecommendationMealPlanRepository ??= new AIRecommendationMealplanRepository(_context);
        //public IIngredientRepository IngredientRepository => _ingredientRepository ??= new IngredientRepository(_context);
        //public IAIRecommendationMeallogRepository AIRecommendationMeallogRepository => _aIRecommendationMeallogRepository ??= new AIRecommendationMeallogRepository(_context);
        //public IUserIngredientPreferenceRepository UserIngredientPreferenceRepository => _userIngredientPreferenceRepository ??= new UserIngredientPreferenceRepository(_context);
        //public IPackageRepository PackageRepository => _packageRepository ??= new PackageRepository(_context);
        //public IUserPackageRepository UserPackageRepository => _userPackageRepository ??= new UserPackageRepository(_context);
        //public ISystemConfigurationRepository SystemConfigurationRepository => _systemConfigurationRepository ??= new SystemConfigurationRepository(_context);
        //public IUserFoodPreferenceRepository UserFoodPreferenceRepository => _userFoodPreferenceRepository ??= new UserFoodPreferenceRepository(_context);
        //public INotificationRepository NotificationRepository => _notificationRepository ??= new NotificationRepository(_context);

        public async Task BeginTransaction()
        {
            await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransaction()
        {
            await _context.Database.CommitTransactionAsync();
        }

        public async Task RollbackTransaction()
        {
            await _context.Database.RollbackTransactionAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
