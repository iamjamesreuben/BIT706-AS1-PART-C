using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIT706_AS1_PART_C
{
    public partial class Form1 : Form
    {        
        static List<string> everydaySummary = new List<string>();
        static List<string> omniSummary = new List<string>();
        static List<string> investmentSummary = new List<string>();

        public class Client
        {
            private string _firstName;
            private string _lastName;
            private string _phoneNumber;
            private string _address;

            public Client(string firstName, string lastName, string phoneNumber, string address)
            {
                _firstName = firstName;
                _lastName = lastName;
                _phoneNumber = phoneNumber;
                _address = address;
            }

            public string ClientInfo =>
                $"Client Info \n\nName: {_firstName} {_lastName}\nPhone Number: {_phoneNumber}\nAddress: {_address}";
        }

        public abstract class Account
        {
            private Client _client;

            public Account(Client client, int accountNumber, decimal opening)
            {
                _client = client;
                this.AccountNumber = accountNumber;
                this.Opening = opening;
            }

            public Account(Client client, decimal opening) : this(client, Account.GenerateAccountNumber(), opening)
            { }

            public int AccountNumber { get; private set; }

            public string AccountType => $"{this.GetType().Name} Account";

            public abstract decimal Overdraft { get; protected set; }

            public decimal Opening { get; protected set; }
            public decimal Deposits { get; protected set; }
            public decimal Withdrawals { get; protected set; }
            public decimal Interest { get; protected set; }
            public decimal Fees { get; protected set; }

            public decimal Balance => this.Opening + this.Deposits - this.Withdrawals + this.Interest - this.Fees;

            private static Random _random = new Random();
            public static int GenerateAccountNumber() => _random.Next(100000000, 1000000000);

            public (decimal Withdrawn, decimal Fee) Withdraw(decimal amount)
            {
                if (amount <= 0m)
                {
                    throw new System.InvalidOperationException("Withdrawal amount must be positive");
                }
                decimal fee = 0;
                decimal availableBalance = this.Balance + this.Overdraft;
                if (amount > this.Balance)
                {
                    if (AccountType == "Everyday Account")
                    {
                        fee = 0m;
                        amount = 0m;
                    }
                    else if (AccountType == "Omni Account")
                    {
                        if (amount > availableBalance)
                        {
                            amount = 0m;
                            fee = 10m;
                        }
                    }
                    else if (AccountType == "Investment Account")
                    {
                        fee = 10m;
                        amount = 0m;
                    }

                }

                else if (this.Balance < amount)
                {
                    amount = this.Balance;
                }
                this.Withdrawals += amount;
                this.Fees += fee;
                return (amount, fee);
            }

            public decimal Deposit(decimal amount)
            {
                if (amount <= 0m)
                {
                    throw new System.InvalidOperationException("Deposit amount must be positive");
                }
                this.Deposits += amount;
                return amount;
            }

        }

        public class Everyday : Account
        {
            public decimal MinBalance { get; private set; } = 0m;
            public decimal MaxBalance { get; private set; } = 1000000000000m;

            public decimal Fee { get; private set; } = 0m;

            public override decimal Overdraft { get; protected set; } = 0m;

            public decimal InterestRate { get; protected set; } = 0m;

            public Everyday(Client client, decimal opening) : base(client, opening)
            { }

            public Everyday(Client client, int accountNumber, decimal opening) : base(client, accountNumber, opening)
            { }
        }

        public class Investment : Account
        {
            public decimal Fee { get; private set; } = 10m;

            public override decimal Overdraft { get; protected set; } = 0m;

            public decimal InterestRate { get; protected set; } = 4m;

            public Investment(Client client, decimal opening) : base(client, opening)
            { }

            public Investment(Client client, int accountNumber, decimal opening) : base(client, accountNumber, opening)
            { }
        }

        public class Omni : Account
        {
            public decimal Fee { get; private set; } = 10m;

            public override decimal Overdraft { get; protected set; } = 1000m;

            public decimal InterestRate { get; protected set; } = 4m;

            public Omni(Client client, decimal opening) : base(client, opening)
            { }

            public Omni(Client client, int accountNumber, decimal opening) : base(client, accountNumber, opening)
            { }
        }

        private static void DisplayBalance(Account account)
        {
            Console.WriteLine($"{account.AccountType} Balance: {account.Balance:$#,##0.00}");
        }

        private static void DepositAmount(Account account)
        {

            Console.WriteLine("How much would you like to deposit?");
            decimal deposited = account.Deposit(decimal.Parse(Console.ReadLine()));
            Console.WriteLine($"\nYou deposited: {deposited:$#,##0.00} into your {account.AccountType}");

            var time = DateTime.Now;

            if (account.AccountType == "Everyday Account")
            {
                everydaySummary.Add($"\nDeposited: {deposited:$#,##0.00} {time.ToString()}");
            }
            if (account.AccountType == "Omni Account")
            {
                omniSummary.Add($"\nDeposited: {deposited:$#,##0.00} {time.ToString()}");
            }
            if (account.AccountType == "Investment Account")
            {
                investmentSummary.Add($"\nDeposited: {deposited:$#,##0.00} {time.ToString()}");
            }

        }

        private static void WithdrawAmount(Account account)
        {
            Console.WriteLine("How much would you like to withdraw?");
            var result = account.Withdraw(decimal.Parse(Console.ReadLine()));
            Console.WriteLine($"\nYou withdrew: {result.Withdrawn:$#,##0.00}");
            if (result.Fee != 0m)
            {
                Console.WriteLine($"With fee: {result.Fee:$#,##0.00}");
            }

            if (result.Withdrawn == 0m)
            {
                Console.WriteLine("Insufficient Funds");
            }

            var time = DateTime.Now;

            if (account.AccountType == "Everyday Account")
            {
                everydaySummary.Add($"\nWithdrew: {result.Withdrawn:$#,##0.00} {time.ToString()}");
            }
            if (account.AccountType == "Omni Account")
            {
                omniSummary.Add($"\nWithdrew: {result.Withdrawn:$#,##0.00} {time.ToString()} Failed Transaction Fee: {result.Fee:$#,##0.00}");
            }
            if (account.AccountType == "Investment Account")
            {
                investmentSummary.Add($"\nWithdrew: {result.Withdrawn:$#,##0.00} {time.ToString()} Failed Transaction Fee: {result.Fee:$#,##0.00}");
            }
        }

        private static void DisplayDetails(Everyday account)
        {
            Console.WriteLine("Everyday Banking Details\n");
            Console.WriteLine($"Account Number: {account.AccountNumber}");
            Console.WriteLine($"{account.AccountType} Balance: {account.Balance:$#,##0.00}");
            Console.WriteLine("Overdraft: " + account.Overdraft);
            Console.WriteLine("Interest Rate: " + account.InterestRate + "%");
            Console.WriteLine("Fee: " + account.Fee);
        }

        private static void DisplayDetails(Investment account)
        {
            Console.WriteLine("Investment Banking Details\n");
            Console.WriteLine($"Account Number: {account.AccountNumber}");
            Console.WriteLine($"{account.AccountType} Balance: {account.Balance:$#,##0.00}");
            Console.WriteLine("Overdraft: " + account.Overdraft);
            Console.WriteLine("Interest Rate: " + account.InterestRate + "%");
            Console.WriteLine("Fee: " + account.Fee);
        }

        private static void DisplayDetails(Omni account)
        {
            Console.WriteLine("Omni Banking Details\n");
            Console.WriteLine($"Account Number: {account.AccountNumber}");
            Console.WriteLine($"{account.AccountType} Balance: {account.Balance:$#,##0.00}");
            Console.WriteLine("Overdraft: " + account.Overdraft);
            Console.WriteLine("Interest Rate: " + account.InterestRate + "%");
            Console.WriteLine("Fee: " + account.Fee);
        }
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());



        }

        public Form1()
        {
            InitializeComponent();  
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView2 = "ASD";
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }
    }
}
