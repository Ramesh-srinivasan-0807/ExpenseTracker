using System.Globalization;
using Expense_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Controllers
{
    public class DashboardController : Controller
    {
        
        private readonly ApplicationDbContext _context;
        
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async Task<ActionResult> Index()
        {
            
            //Last 7 Days
            DateTime StartDate = DateTime.Now.AddDays(-6);
            DateTime EndDate = DateTime.Today;

            List<Transaction> SelectedTransactions = await _context.Transactions.Include(t => t.Category)
                .Where(t => t.Date >= StartDate && t.Date <= EndDate)
                .ToListAsync();
            
            //Total Income
            int TotalIncome = SelectedTransactions.Where(t => t.Category.Type == "Income").Sum(t => t.Amount);
            ViewBag.TotalIncome = TotalIncome.ToString("C0");
            
            //Total Expense
            int TotalExpense = SelectedTransactions.Where(t => t.Category.Type == "Expense").Sum(t => t.Amount);
            ViewBag.TotalExpense = TotalExpense.ToString("C0");
            
            //Total Balance
            int TotalBalance = TotalIncome - TotalExpense;
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            culture.NumberFormat.CurrencyNegativePattern = 1; // Set negative currency pattern to "-$n"
            ViewBag.TotalBalance = TotalBalance.ToString("C0", culture);
            
            //Doughnut Chart - Expense By Category
            ViewBag.DoughnutChartData = SelectedTransactions
                .Where(i => i.Category.Type == "Expense")
                .GroupBy(i => i.Category.CategoryId)
                .Select(k => new
                {
                    categoryTitleWithIcon = k.First().Category.Icon+ " " + k.First().Category.Title,
                    TotalAmount = k.Sum(x => x.Amount),
                    formattedAmount = k.Sum(x => x.Amount).ToString("C0")
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();
            
            
            //Spline Chart - Income vs Expense
            
            //Income
            List<SplineChartData> IncomeSummary = SelectedTransactions
                .Where(I => I.Category.Type == "Income")
                .GroupBy(I => I.Date)
                .Select(k => new SplineChartData
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    income = k.Sum(x => x.Amount)
                })
                .ToList();
            
            //Expense
            List<SplineChartData> ExpenseSummary = SelectedTransactions
                .Where(I => I.Category.Type == "Expense")
                .GroupBy(I => I.Date)
                .Select(k => new SplineChartData
                {
                    day = k.First().Date.ToString("dd-MMM"),
                    expense = k.Sum(x => x.Amount)
                })
                .ToList();
            
            //Combine Income and Expense Summary
            string[] Last7Days = Enumerable.Range(0, 7)
                .Select(i => StartDate.AddDays(i).ToString("dd-MMM"))
                .ToArray();
            
            ViewBag.SplineChartData = from day in Last7Days
                join income in IncomeSummary on day equals income.day into dayIncomeJoined
                from income in dayIncomeJoined.DefaultIfEmpty()
                join expense in ExpenseSummary on day equals expense.day into expenseJoined
                from expense in expenseJoined.DefaultIfEmpty()
                select new 
                {
                    day = day,
                    income = income == null ? 0 : income.income,
                    expense = expense == null ? 0 : expense.expense
                };
                
            //Recent Transactions
            ViewBag.RecentTransactions = await _context.Transactions
                .Include(i => i.Category)
                .OrderByDescending(j => j.Date)
                .Take(5)
                .ToListAsync();
            
            
            return View();
        }

    }
    
    public class SplineChartData
    {
        public string day;
        public int income;
        public int expense;
    }
}
