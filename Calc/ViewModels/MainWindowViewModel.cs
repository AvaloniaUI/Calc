﻿using System;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using Calc.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Calc.ViewModels
{
    public sealed partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _shownString = string.Empty;
        
        [ObservableProperty]
        private string _shownResult = string.Empty;
        private int _numberOfOpeningParentheses;
        private int _numberOfClosingParentheses;
        
        // Commands
        public ICommand AddDecimalSeparatorCommand { get; }
        public ICommand AddNumberCommand { get; }
        public ICommand AddOperatorCommand { get; }
        public ICommand AddParenthesisCommand { get; }
        public ICommand AlternateNegativePositiveCommand { get; }
        public ICommand ClearScreenCommand { get; }
        public ICommand DeleteLastCommand { get; }
        public ICommand PickResultCommand { get; }

        public MainWindowViewModel()
        {
            AddDecimalSeparatorCommand = new RelayCommand(AddDecimalSeparator);
            AddNumberCommand = new RelayCommand<int>(AddNumber);
            AddOperatorCommand = new RelayCommand<Operator>(AddOperator);
            AddParenthesisCommand = new RelayCommand(AddParenthesis);
            AlternateNegativePositiveCommand = new RelayCommand(AlternateNegativePositive);
            ClearScreenCommand = new RelayCommand(ClearScreen);
            DeleteLastCommand = new RelayCommand(DeleteLast);
            PickResultCommand = new RelayCommand(PickResult);
        }

        private void AddDecimalSeparator()
        {
            if (CanDecimalSeparatorBePlaced())
                ShownString += CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
        }

        private void AddNumber(int value)
        {
            ShownString += value;
            Calculate(ShownString);
        }

        internal void ProcessText(string? text)
        {
            if (text?.Length == 1)
            {
                switch (text)
                {
                    case "+":
                        AddOperator(Operator.Add);
                        break;
                    
                    case "-":
                        AddOperator(Operator.Subtract);
                        break;

                    case "*":
                        AddOperator(Operator.Multiply);
                        break;
                    
                    case "/":
                        AddOperator(Operator.Divide);
                        break;
                    
                    case "=":
                        PickResult();
                        break;
                    
                    case ".":
                        AddDecimalSeparator();
                        break;
                    
                    case "(":
                    case ")":
                        AddParenthesis();
                        break;
                    
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                    case "0":
                        AddNumber(int.Parse(text));
                        break;
                }
            }
        }

        private void AddOperator(Operator @operator)
        {
            if (ShownString[^1].Equals('('))
                return;
            
            if (IsLastInputAnOperator())
                ShownString = ShownString[..^1];

            ShownString += @operator switch
            {
                Operator.Add => OperatorChar.Add,
                Operator.Subtract => OperatorChar.Subtract,
                Operator.Multiply => OperatorChar.Multiply,
                Operator.Divide => OperatorChar.Divide,
                _ => throw new InvalidDataException("Operator not allowed")
            };
        }

        private void AddParenthesis()
        {
            if (ShownString.Length == 0 || IsLastInputAnOperator() || ShownString[^1].Equals('('))
            {
                ShownString += "(";
                _numberOfOpeningParentheses++;
            }
            else if (_numberOfClosingParentheses < _numberOfOpeningParentheses)
            {
                ShownString += ")";
                _numberOfClosingParentheses++;
                Calculate(ShownString);
            }
        }

        private void AlternateNegativePositive()
        {
            var indexWhereSetOrUnsetSign = SetIndexWhereToSetOrUnsetSign();

            if (ShownString.Length == 0 || ShownString[^1].Equals('('))
                ShownString += OperatorChar.Subtract;
            else
            {
                switch (ShownString[indexWhereSetOrUnsetSign])
                {
                    case OperatorChar.Subtract:
                        if (indexWhereSetOrUnsetSign == 0 ||
                            ShownString[indexWhereSetOrUnsetSign - 1].Equals('(') ||
                            OperatorChar.IsAnOperator(ShownString[indexWhereSetOrUnsetSign - 1]))
                            
                            ShownString = ShownString.Remove(indexWhereSetOrUnsetSign, 1);
                        else
                            // Add -
                            ShownString = ShownString[..indexWhereSetOrUnsetSign] +
                                          OperatorChar.Subtract +
                                          ShownString[indexWhereSetOrUnsetSign..];
                        break;
                    default:
                        // Add -
                        ShownString = ShownString[..indexWhereSetOrUnsetSign] +
                                      OperatorChar.Subtract +
                                      ShownString[indexWhereSetOrUnsetSign..];
                        break;
                }
                
                Calculate(ShownString);
            }
        }

        private void Calculate(string calc)
        {
            ShownResult = Calculator.Calculate(calc);
        }
        
        private bool CanDecimalSeparatorBePlaced()
        {
            var indexLastDecimalSeparator = ShownString.LastIndexOf(
                CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator,
                StringComparison.Ordinal);
            var indexLastOperator = ShownString.LastIndexOfAny(OperatorChar.Operators);

            if (indexLastDecimalSeparator == -1 && indexLastOperator == -1)
                return true;
            
            return indexLastOperator > indexLastDecimalSeparator;
        }

        private void ClearScreen()
        {
            ShownString = string.Empty;
            ShownResult = string.Empty;
            _numberOfOpeningParentheses = 0;
            _numberOfClosingParentheses = 0;
        }

        private void DeleteLast()
        {
            if (ShownString.Length == 1)
            {
                ClearScreen();
                return;
            }

            // Update number of parentheses
            switch (ShownString[^1])
            {
                case '(':
                    _numberOfOpeningParentheses--;
                    break;
                case ')':
                    _numberOfClosingParentheses--;
                    break;
            }
            
            ShownString = ShownString[..^1];
            Calculate(IsLastInputAnOperator() ? ShownString[..^1] : ShownString);
        }

        private bool IsLastInputAnOperator()
        {
            return OperatorChar.IsAnOperator(ShownString[^1]);
        }
        
        private static int MaxOf(int number1, int number2)
        {
            return number1 > number2 ? number1 : number2;
        }
        
        private void PickResult()
        {
            ShownString = ShownResult;
            ShownResult = string.Empty;
        }

        private int SetIndexWhereToSetOrUnsetSign()
        {
            char[] nonSubstractOperators = { OperatorChar.Add, OperatorChar.Multiply, OperatorChar.Divide };
            
            var indexAfterLastNonSubstractOperator = ShownString.LastIndexOfAny(nonSubstractOperators) + 1;
            var indexOfLastSubstractOperator = ShownString.LastIndexOf(OperatorChar.Subtract);
            var indexAfterLastParenthesis = ShownString.LastIndexOf('(') + 1;
            var indexLastOperator = MaxOf(indexAfterLastNonSubstractOperator, indexOfLastSubstractOperator);

            return MaxOf(indexAfterLastParenthesis, indexLastOperator);
        }
    }
}