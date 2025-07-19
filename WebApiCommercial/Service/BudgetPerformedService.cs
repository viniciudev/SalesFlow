using Model;
using Model.Moves;
using Model.PerformedModels;
using Model.Registrations;
using Repository;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
  public class BudgetPerformedService : BaseService<BudgetPerformed>, IBudgetPerformedService
  {
    private IBudgetService budgetService;
    private IServicesProvisionService servicesProvisionService;
    private IServicesProvisionItemsService servicesProvisionItemsService;
    public BudgetPerformedService(IGenericRepository<BudgetPerformed> repository,
      IBudgetService budgetService, IServicesProvisionService servicesProvisionService,
      IServicesProvisionItemsService servicesProvisionItemsService) : base(repository)
    {
      this.budgetService = budgetService;
      this.servicesProvisionService = servicesProvisionService;
      this.servicesProvisionItemsService = servicesProvisionItemsService;
    }

    public async Task<BudgetPerformed> GetByComparation(Filters filters)
    {
    
      BudgetPerformed budgetPerformed = new BudgetPerformed();
      budgetPerformed.DifferentAmounts = new List<DifferentAmount>();
      budgetPerformed.NonExistentProducts = new List<NonExistentProducts>();
      budgetPerformed.DifferentValues = new List<DifferentValues>();
     
      Budget budget = await budgetService.GetByIdBudget(filters.idBudget);
      List<ServicesProvisionItems> ListServicesProvisionItems =
        await servicesProvisionItemsService.GedByIdServiceProvision(filters);  //buscar itens devido nome 
     
  
      foreach (ServicesProvisionItems servicesProvisionItems in ListServicesProvisionItems)
      {
        var item = budget.BudgetItems.FirstOrDefault( x =>
          x.IdItem == servicesProvisionItems.IdItem
        && (int)x.TypeItem==(int)servicesProvisionItems.TypeItem);

        var differentQuantity = budget.BudgetItems.FirstOrDefault(x =>
         x.IdItem == servicesProvisionItems.IdItem
         && (int)x.TypeItem == (int)servicesProvisionItems.TypeItem
         && x.Amount != servicesProvisionItems.Amount);

        var differentValue= budget.BudgetItems.FirstOrDefault(x =>
          x.IdItem == servicesProvisionItems.IdItem
          && (int)x.TypeItem == (int)servicesProvisionItems.TypeItem
          && x.Value != servicesProvisionItems.Value);

        if (item == null)
        {
          NonExistentProducts nonExistentProducts = new NonExistentProducts();
          nonExistentProducts.Description = "Produto inexistente no orçamento.";
          nonExistentProducts.NameProduct = servicesProvisionItems.NameItem;
          budgetPerformed.NonExistentProducts.Add(nonExistentProducts);
        }
        if (differentQuantity != null)
        {
          DifferentAmount differentAmount = new DifferentAmount();
          differentAmount.Description = "Produto com quantidade diferente a do orçamento!";
          differentAmount.NameProduct = servicesProvisionItems.NameItem;
          differentAmount.AmountBudget = differentQuantity.Amount;
          differentAmount.AmountService = servicesProvisionItems.Amount;
          budgetPerformed.DifferentAmounts.Add(differentAmount);
        }
        if (differentValue != null)
        {
          DifferentValues differentValues = new DifferentValues();
          differentValues.Description = "Produto com Valor diferente a do orçamento!";
          differentValues.NameProduct = servicesProvisionItems.NameItem;
          differentValues.ValueBudget = differentQuantity.Value;
          differentValues.ValueService = servicesProvisionItems.Value;
          budgetPerformed.DifferentValues.Add(differentValues);
        }
      }
      return budgetPerformed;
      
    }

  }
  public interface IBudgetPerformedService : IBaseService<BudgetPerformed>
  {
    Task<BudgetPerformed> GetByComparation(Filters filters);
  }
}
