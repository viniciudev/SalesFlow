using Model.PerformedModels;
using System.Collections.Generic;

namespace Model.Moves
{
  public class BudgetPerformed : BaseEntity
  {
    public ICollection<NonExistentProducts> NonExistentProducts { get; set; }
    public ICollection<DifferentAmount> DifferentAmounts { get; set; }
    public ICollection<DifferentValues> DifferentValues { get; set; }

  }
}
