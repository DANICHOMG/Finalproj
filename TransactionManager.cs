using UnityEngine;
using UnityEngine.UI;
using System;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using TMPro;

public class TransactionManager : MonoBehaviour
{
    public InputField amountInput;
    public InputField descriptionInput;
    public InputField dateInput;
    public InputField categoryInput;
    public GameObject menu;
    public GameObject input1;
    public GameObject input2;
    public GameObject input3;
    public GameObject input4;
    public GameObject kredit;
    public Transform transactionsParent;
    public Text transactionsText;
    public GameObject transactionPrefab;
    public Text balanceText;
    private string connectionString;
    public StatisticsUI statisticsUI;
    public GraphController graphController;
    private List<Transaction> transactions;
    private Credit currentCredit;
    public InputField termInput;
    public InputField interestRateInput;
    private bool hasCredit;
    private float balance = 0f;
    private float budget = 0f; 
    private float monthlyPayment = 0f;
    public TMP_Text budgetText;
    public TMP_Text paymentText;
    public InputField newBudgetInput;
    public InputField newPaymentInput;
    public GameObject settings;
    public GameObject razvvod;
    public GameObject razvvod2;
    public AccountsPageManager AccountsPageManager;

    public class Transaction
    {
        public float Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
    }

    public class Credit
    {
        public float InterestRate { get; private set; }
        public int LoanTermMonths { get; private set; }

        public Credit(float interestRate, int loanTermMonths)
        {
            InterestRate = interestRate;
            LoanTermMonths = loanTermMonths;
        }
    }


    public class Statistics
    {
        public int TotalTransactions { get; set; }
        public bool HasCredit { get; set; }
        public float TotalAmountSpent { get; set; }
        public float LargestTransactionAmount { get; set; }
        public string LargestTransactionDescription { get; set; }
    }

    public void UpdateStatistics()
    {
        Statistics stats = GetStatistics();

       
        if (statisticsUI != null)
        {
            statisticsUI.UpdateStatisticsUI(stats);
        }
    }

    public void TakeCredit()
    {
      
        Debug.Log("Before TakeCredit");

        float interestRate;
        if (!float.TryParse(interestRateInput.text, out interestRate))
        {
          
            Debug.LogError("Invalid interest rate");
            return;
        }

        int loanTermMonths;
        if (!int.TryParse(termInput.text, out loanTermMonths))
        {
            
            Debug.LogError("Invalid loan term");
            return;
        }

        
        TakeCredit(interestRate, loanTermMonths);
        hasCredit = true;
        kredit.SetActive(false);
        Debug.Log("After TakeCredit");
    }





    public void TakeCredit(float interestRate, int loanTermMonths)
    {
        currentCredit = new Credit(interestRate, loanTermMonths);
        balance += CalculateCreditAmount(interestRate, loanTermMonths);
        hasCredit = true;
        UpdateBalance();
        UpdateStatistics();
       
        StartCoroutine(AutoRepayCredit(currentCredit.LoanTermMonths));
    }


    private IEnumerator AutoRepayCredit(int loanTermMonths)
    {
        yield return new WaitForSeconds(loanTermMonths * 30 * 24 * 60 * 60); 

        if (currentCredit != null)
        {
        }
    }

    public void RepayCredit(float amount)
    {
        if (currentCredit != null)
        {

            balance -= amount;

            currentCredit = null;
            hasCredit = false;
        }
    }




    public List<string> FilterTransactionsByDate(DateTime selectedDate)
    {
        List<string> filteredTransactions = new List<string>();

        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Transactions";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DateTime transactionDate = DateTime.Parse(reader.GetString(3));

                        if (transactionDate.Date == selectedDate.Date)
                        {
                            
                            string transactionString = $"{reader.GetString(2)}, {reader.GetFloat(1)} грн, {reader.GetString(3)}. Категорія: {reader.GetString(4)}.";
                            filteredTransactions.Add(transactionString);
                        }
                    }
                }
            }

            dbConnection.Close();
        }

        return filteredTransactions;
    }



    public List<string> GetAllTransactions()
    {
        List<string> transactions = new List<string>();

        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT * FROM Transactions";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string transactionInfo = $"{reader["Description"]}, {reader["Amount"]} грн, {reader["Date"]}. Категорія: {reader["Category"]}.";
                        transactions.Add(transactionInfo);
                    }
                }
            }

            dbConnection.Close();
        }

        return transactions;
    }

    public Statistics GetStatistics()
    {
        Statistics stats = new Statistics
        {
            TotalTransactions = GetTotalTransactionCount(),
            HasCredit = hasCredit,
            TotalAmountSpent = GetTotalAmountSpent(),
            LargestTransactionAmount = GetLargestTransactionAmount(out string description),
            LargestTransactionDescription = description
        };

        return stats;
    }

    private int GetTotalTransactionCount()
    {
        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT COUNT(*) FROM Transactions";
                dbCmd.CommandText = sqlQuery;

                return Convert.ToInt32(dbCmd.ExecuteScalar());
            }
        }
    }

    private float GetTotalAmountSpent()
    {
        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT SUM(Amount) FROM Transactions WHERE Amount < 0";
                dbCmd.CommandText = sqlQuery;

                object result = dbCmd.ExecuteScalar();

        
                return result == DBNull.Value ? 0f : Convert.ToSingle(result);
            }
        }
    }


    private float GetLargestTransactionAmount(out string description)
    {
        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = "SELECT MAX(Amount), Description FROM Transactions";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        description = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        return reader.IsDBNull(0) ? 0f : Convert.ToSingle(reader.GetValue(0));
                    }
                }
            }
        }

      
        description = string.Empty;
        return 0f;
    }


    
    public List<string> SearchTransactions(string searchTerm)
    {
        List<string> searchResult = new List<string>();

        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                
                string sqlQuery = $"SELECT * FROM Transactions WHERE Description LIKE '%{searchTerm}%' OR Category LIKE '%{searchTerm}%'";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string transactionInfo = $"{reader["Amount"]} - {reader["Description"]} - {reader["Date"]} - {reader["Category"]}";
                        searchResult.Add(transactionInfo);
                    }
                }
            }

            dbConnection.Close();
        }

        return searchResult;
    }
    void Start()
    {
        connectionString = "URI=file:" + Application.dataPath + "/FinancesDB.sqlite";
        menu.SetActive(false);
        input1.SetActive(false);
        input2.SetActive(false);
        input3.SetActive(false);
        input4.SetActive(false);
        kredit.SetActive(false);
        settings.SetActive(false);
        DisplayTransactions(SortCriteria.Relevance);
        CalculateBalance();
        UpdateBalance();
        UpdateStatistics();
        LoadBudgetAndPayment();
        UpdateGraph();
    }
    public void SetBudget()
    {
        float budg;
        if (float.TryParse(newBudgetInput.text, out budg))
        {
            SaveBudgetAndPayment1(budg);
            UpdateStatistics();
            UpdateBalance();

        }
        else
        {
            Debug.LogError("Invalid budget input");
        }
    }

    public void SetMonthlyPayment()
    {
        float payment;
        if (float.TryParse(newPaymentInput.text, out payment))
        {
            SaveBudgetAndPayment2(payment);
            UpdateStatistics();
            UpdateBalance();
        }
        else
        {
            Debug.LogError("Invalid monthly payment input");
        }
    }


    private void LoadBudgetAndPayment()
    {
        budget = PlayerPrefs.GetFloat("Budget");
        monthlyPayment = PlayerPrefs.GetFloat("MonthlyPayment");
    }

    private void SaveBudgetAndPayment1(float budg)
    {
        PlayerPrefs.SetFloat("Budget", budg);
        PlayerPrefs.Save();
    }

    public void SaveBalance(float balance)
    {
        PlayerPrefs.SetFloat("Balance", balance);
        PlayerPrefs.Save();
    }

    public float LoadBalance()
    {
        return PlayerPrefs.GetFloat("Balance", 0f);
    }



    private void SaveBudgetAndPayment2(float monthlyPayment)
    {
        PlayerPrefs.SetFloat("MonthlyPayment", monthlyPayment);
        PlayerPrefs.Save();
    }


    public void OpenMenu()
    {
        menu.SetActive(true);
        input1.SetActive(true);
        input2.SetActive(true);
        input3.SetActive(true);
        input4.SetActive(true);
    }
    public void OpenKredit()
    {
        kredit.SetActive(true);
    }

    public void Loadrazvod()
    {
        razvvod.SetActive(true);
        razvvod2.SetActive(false);
        AccountsPageManager.UpdateTransactionList();
    }
    public void LoadMain()
    {
        razvvod.SetActive(false);
        razvvod2.SetActive(true);
    }

    public void AddTransaction()
    {
        Transaction newTransaction = new Transaction
        {
            Amount = categoryInput.text.ToLower() == "expense" ? -float.Parse(amountInput.text) : float.Parse(amountInput.text),
            Description = descriptionInput.text,
            Date = DateTime.Parse(dateInput.text),
            Category = categoryInput.text
        };
        transactions.Add(newTransaction);
        LoadBudgetAndPayment();
       
        if (CheckBudget(newTransaction.Amount))
        {
            balance -= newTransaction.Amount;
            PlayerPrefs.SetFloat("money", balance);
            PlayerPrefs.Save();
            SaveBalance(balance);
            
            using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
            {
                dbConnection.Open();

                using (IDbCommand dbCmd = dbConnection.CreateCommand())
                {
                    try
                    {
                        string sqlQuery = "INSERT INTO Transactions (Amount, Description, Date, Category) VALUES (@Amount, @Description, @Date, @Category)";
                        dbCmd.CommandText = sqlQuery;

                        var amountParam = dbCmd.CreateParameter();
                        amountParam.ParameterName = "@Amount";
                        amountParam.Value = categoryInput.text.ToLower() == "expense" ? -float.Parse(amountInput.text) : float.Parse(amountInput.text);
                        dbCmd.Parameters.Add(amountParam);

                        var descriptionParam = dbCmd.CreateParameter();
                        descriptionParam.ParameterName = "@Description";
                        descriptionParam.Value = descriptionInput.text;
                        dbCmd.Parameters.Add(descriptionParam);

                        var dateParam = dbCmd.CreateParameter();
                        dateParam.ParameterName = "@Date";
                        dateParam.Value = DateTime.Parse(dateInput.text).ToString("yyyy-MM-dd");
                        dbCmd.Parameters.Add(dateParam);

                        var categoryParam = dbCmd.CreateParameter();
                        categoryParam.ParameterName = "@Category";
                        categoryParam.Value = categoryInput.text;
                        dbCmd.Parameters.Add(categoryParam);

                        dbCmd.ExecuteNonQuery();
                        Debug.Log("Transaction added successfully.");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error while adding transaction: {ex.Message}");
                    }
                }


                dbConnection.Close();
                amountInput.text = "";
                descriptionInput.text = "";
                dateInput.text = "";
                categoryInput.text = "";
                menu.SetActive(false);
                input1.SetActive(false);
                input2.SetActive(false);
                input3.SetActive(false);
                input4.SetActive(false);
                UpdateTransactionList();
                DisplayTransactions();
                CalculateBalance();
                UpdateBalance();
                LoadSavedBalance();
                UpdateGraph();
                UpdateStatistics();
            }
        }
        else
        {
            Debug.Log("Превишено сумму ліміту. Введіть сумму нижче, або змініть ліміт.");
        }
        
        
    }
    private bool CheckBudget(float transactionAmount)
    {
        return transactionAmount + GetTotalAmountSpent() <= budget;
    }
    private void SaveBalance()
    {
        float currentBalance = CalculateBalance();
        SaveBalance(currentBalance);
    }

    private void LoadSavedBalance()
    {
        float savedBalance = LoadBalance();
        balance = savedBalance;
        UpdateBalance();
    }

    void UpdateGraph()
    {
        List<float> transactionData = GetTransactionData();
        graphController.UpdateGraph(transactionData);
    }
    public List<float> GetTransactionData()
    {
        List<float> transactionData = new List<float> { 10f, 20f, 15f, 25f, 18f };

        return transactionData;
    }

    float CalculateCreditAmount(float interestRate, int loanTermMonths)
    {
        return interestRate;
    }

    float CalculateBalance()
    {
        float totalIncome = 0;
        float totalExpense = 0;
        float balance = totalIncome - Math.Abs(totalExpense);
        SaveBalance(balance);  
        return balance;

    }

    private void UpdateBalance()
    {
        balanceText.text = $"{balance:F2}";
        SaveBalance(balance); 
        budgetText.text = $"Бюджет: {budget:F2}";
        paymentText.text = $"Рег. платіж: {monthlyPayment:F2}";
    }
    private void UpdateBalance2()
    {
        balanceText.text = $"{PlayerPrefs.GetFloat("money")}";
    }

    public void OpenSettings()
    {
        float balance = CalculateBalance();
        LoadBudgetAndPayment();
        UpdateBalance();
        SaveBalance(balance);
        settings.SetActive(true);
    }
    public void CloseSettings()
    {
        settings.SetActive(false);
    }

    private void UpdateTransactionList()
    {
        List<Transaction> transactions = LoadRecentTransactions(10); 
        foreach (Transform child in transactionsParent)
        {
            Destroy(child.gameObject);
        }
        foreach (Transaction transaction in transactions)
        {
            GameObject transactionText = Instantiate(transactionPrefab, transactionsParent);
            Text textComponent = transactionText.GetComponent<Text>();
            textComponent.text = $"{transaction.Date.ToShortDateString()} - {transaction.Amount} - {transaction.Description} - {transaction.Category}";
        }
    }

    public enum SortCriteria
    {
        Alphabetical,
        Relevance,
        Price
    }

    public List<Transaction> GetSortedTransactions(SortCriteria sortCriteria)
    {
        transactions = LoadRecentTransactions(10);

        List<Transaction> sortedTransactions = new List<Transaction>();
        switch (sortCriteria)
        {
            case SortCriteria.Alphabetical:
                sortedTransactions = transactions.OrderBy(t => t.Description).ToList();
                break;

            case SortCriteria.Relevance:
                sortedTransactions = transactions.OrderByDescending(t => t.Date).ToList();
                break;

            case SortCriteria.Price:
                sortedTransactions = transactions.OrderByDescending(t => t.Amount).ToList();
                break;
        }

        return sortedTransactions;
    }



    public void DisplayTransactions(SortCriteria sortCriteria = SortCriteria.Relevance)
    {
        transactions = GetSortedTransactions(sortCriteria);

        foreach (Transform child in transactionsParent)
        {
            Destroy(child.gameObject);
        }

        foreach (Transaction transaction in transactions)
        {
            GameObject transactionText = Instantiate(transactionPrefab, transactionsParent);
            Text textComponent = transactionText.GetComponent<Text>();
            textComponent.text = $"{transaction.Date.ToShortDateString()} - {transaction.Amount} - {transaction.Description} - {transaction.Category}";
        }
    }

    private List<Transaction> LoadRecentTransactions(int count)
    {
        using (IDbConnection dbConnection = new SQLiteConnection(connectionString))
        {
            dbConnection.Open();

            using (IDbCommand dbCmd = dbConnection.CreateCommand())
            {
                string sqlQuery = $"SELECT * FROM Transactions ORDER BY Date DESC LIMIT {count}";
                dbCmd.CommandText = sqlQuery;

                using (IDataReader reader = dbCmd.ExecuteReader())
                {
                    List<Transaction> transactions = new List<Transaction>();

                    while (reader.Read())
                    {
                        Transaction transaction = new Transaction
                        {
                            Amount = reader.GetFloat(1),
                            Description = reader.GetString(2),
                            Date = DateTime.Parse(reader.GetString(3)),
                            Category = reader.GetString(4)
                        };

                        transactions.Add(transaction);
                    }

                    return transactions;
                }
            }
        }
    }

}
