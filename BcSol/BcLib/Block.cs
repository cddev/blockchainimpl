using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace BcLib
{
    public class Block
    {

        public List<Transaction> TransactionList;


        public int Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public int Proof { get; set; }
        public string PreviousHash { get; set; }

        
        public Block(int index,DateTime timeStamp,List<Transaction> transactionList,int proof,string previousHash)
        {
            Index = index;
            TimeStamp = timeStamp;
            Proof = proof;
            PreviousHash = previousHash;

            this.TransactionList = new List<Transaction>();

            if (transactionList != null)
            {
                SetTransactionList(transactionList);
            }
        }

        

        public List<Transaction> GetTransactionList()
        {
            return TransactionList;
        }

        public void SetTransactionList(List<Transaction> value)
        {
            TransactionList = value;
        }

        
    }
}
