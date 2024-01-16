using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UI.Dates;
using System;
using static TransactionManager;

public class AccountsPageManager : MonoBehaviour
{
    public InputField searchInput;
    public Text resultText;
    public DatePicker datePicker;

    public TransactionManager transactionManager;

    void Start()
    {
        UpdateTransactionList();
    }

    public void OnSortCriteriaChanged(int index)
    {
        SortCriteria sortCriteria = (SortCriteria)index;
        transactionManager.DisplayTransactions(sortCriteria);
    }

    public void SearchTransactions()
    {
        string searchTerm = searchInput.text;

        List<string> searchResult = PerformSearch(searchTerm);
        resultText.text = string.Join("\n", searchResult);
    }
    public void FilterTransactionsByDate()
    {
        DateTime selectedDate = datePicker.SelectedDate;
        List<string> filteredTransactions = transactionManager.FilterTransactionsByDate(selectedDate);
        resultText.text = string.Join("\n", filteredTransactions);
    }
    public void UpdateTransactionList()
    {
        List<string> allTransactions = transactionManager.GetAllTransactions();
        resultText.text = string.Join("\n", allTransactions);
    }
    List<string> PerformSearch(string searchTerm)
    {
        List<string> searchResult = transactionManager.SearchTransactions(searchTerm);

        return searchResult;
    }
}
