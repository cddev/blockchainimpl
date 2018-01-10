using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcLib
{
    public class Transaction
    {
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public int Amount { get; set; }

        public Transaction(string sender,string recipient,int amount)
        {
            Sender = sender;
            Recipient = recipient;
            Amount = amount;
        }
    }
}
