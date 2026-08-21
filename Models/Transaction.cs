using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Expense_Tracker.Models;

public class Transaction
{
    public int TransactionId { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }
    
    public Category? Category { get; set; }
    
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
    public int Amount { get; set; }
    
    [Column(TypeName = "nvarchar(75)")]
    public string? Note { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Now;
    
    [NotMapped]
    public string? categoryTitleWithIcon
    {
        get
        {
            return Category == null ? "" : Category.Icon + " " + Category.Title;
        }
    }
    
    public string? FormattedAmount
    {
        get
        {
            return ((Category == null || Category.Type == "Expense") ? "- " : "+ ") + Amount.ToString("C0");
        }
    }
}