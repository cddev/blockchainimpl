using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.IO;
using System.Diagnostics;
using System.Net;

namespace BcLib
{
    public class BlockChain
    {

        #region props
        public List<Transaction> _currentTransaction { get; set; }
        public List<Block> _chain { get; set; }
        public List<Node> _nodes { get; set; }

        private Block _lastBock => _chain.Last();

        public string NodeId { get; private set; }

        #endregion

        #region Ctor
        public BlockChain()
        {
            NodeId = Guid.NewGuid().ToString().Replace("-", "");
            _currentTransaction = new List<Transaction>();
            _chain = new List<Block>();

            //Initial Block
            CreateBlock(100, "1");
        }
        #endregion

        #region Block
        #region public
        public Block CreateBlock(int proof, string previous_hash = null)
        {

            int index = _chain.Count;

            Block CurrentBlock = new Block(index, DateTime.UtcNow, _currentTransaction, proof, string.IsNullOrEmpty(previous_hash) ? HashBlock(_lastBock) : previous_hash);


            _chain.Add(CurrentBlock);

            _currentTransaction = new List<Transaction>();

            return CurrentBlock;
        }

        #endregion
        #region private
        private string HashBlock(Block block)
        {
            string jsonBlock = JsonConvert.SerializeObject(block);

            return GetSha256(jsonBlock);
        }
        #endregion
        #endregion

        #region POW
        public int CreateProofOfWork(int last_proof, string previousHash)
        {
            int proof = 0;

            while (!ProofIsValid(last_proof, proof, previousHash))
            {
                proof += 1;
            }

            return proof;

        }
        private bool ProofIsValid(int last_proof, int proof, string previousHash)
        {

            string guess = $"{last_proof}{proof}{previousHash}";

            string guessHash = GetSha256(guess);

            return guessHash.StartsWith("0000");
        }
        private bool IsValidChain(List<Block> chain)
        {
            Block block = null;
            Block lastBlock = chain.First();
            int currentIndex = 1;

            while (currentIndex < chain.Count)
            {
                block = chain.ElementAt(currentIndex);

                Debug.WriteLine($"{lastBlock}");
                Debug.WriteLine($"{block}");
                Debug.WriteLine("----------------------------");

                if (block.PreviousHash != HashBlock(lastBlock))
                    return false;

                //Check that the Proof of Work is correct
                if (!ProofIsValid(lastBlock.Proof, block.Proof, lastBlock.PreviousHash))
                    return false;

                lastBlock = block;
                currentIndex++;
            }

            return true;
        }
        #endregion

        #region utils
        private string GetSha256(string data)
        {
            var sha256 = new SHA256Managed();
            var hashBuilder = new StringBuilder();

            byte[] bytes = Encoding.Unicode.GetBytes(data);
            byte[] hash = sha256.ComputeHash(bytes);

            foreach (byte x in hash)
                hashBuilder.Append($"{x:x2}");

            return hashBuilder.ToString();
        }
        public static string ByteArrayToString(byte[] array)
        {
            StringBuilder sBuilder = new StringBuilder();


            for (int i = 0; i < array.Length; i++)
            {
                sBuilder.Append(String.Format("{0:X2}", array[i]));

            }

            return sBuilder.ToString();
        }
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
        #endregion

        #region Nodes
        private void RegisterNode(string address)
        {
            _nodes.Add(new Node { Adress = new Uri(address) });
        }
        private bool ResolveConflicts()
        {
            List<Block> newChain = null;
            int maxLength = _chain.Count;

            foreach (Node node in _nodes)
            {
                var url = new Uri(node.Adress, "/chain");
                var request = (HttpWebRequest)WebRequest.Create(url);
                var response = (HttpWebResponse)request.GetResponse();


                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var model = new
                    {
                        chain = new List<Block>(),
                        length = 0

                    };

                    string json = new StreamReader(response.GetResponseStream()).ReadToEnd();

                    var data = JsonConvert.DeserializeAnonymousType(json, model);

                    if (data.chain.Count > _chain.Count && IsValidChain(data.chain))
                    {
                        maxLength = data.chain.Count;
                        newChain = data.chain;
                    }
                }
            }

            if (newChain != null)
            {
                _chain = newChain;
                return true;

            }
            return false;


        }
        #endregion

        #region API

        internal string Mine()
        {
            int proof = CreateProofOfWork(_lastBock.Proof, _lastBock.PreviousHash);

            CreateTransaction("0", NodeId, 1);

            Block block = CreateBlock(proof);


            var response = new
            {
                Message = "New Block Forged",
                Index = block.Index,
                Transactions = block.TransactionList.ToArray(),
                Proof = block.Proof,
                PreviousHash = block.PreviousHash
            };

            return JsonConvert.SerializeObject(response);
        }
        internal string GetFullChain()
        {
            var response = new
            {
                chain = _chain.ToArray(),
                length = _chain.Count

            };

            return JsonConvert.SerializeObject(response);

        }
        internal string RegisterNodes(string[] nodes)
        {
            StringBuilder builder = new StringBuilder();

            foreach (string node in nodes)
            {
                string url = $"http://{node}";
                RegisterNode(url);
                builder.Append($"{url}, ");
            }

            builder.Insert(0, $"{nodes.Count()} new nodes have been added: ");
            string result = builder.ToString();

            return result.Substring(0, result.Length - 2);
        }
        internal string Consensus()
        {
            bool replaced = ResolveConflicts();
            string message = replaced ? "was replaced" : "is authoritive";

            var response = new
            {
                Message = $"Our chain {message}",
                Chain = _chain

            };

            return JsonConvert.SerializeObject(response);
        }
        internal int CreateTransaction(string sender, string recipient, int amount)
        {
            _currentTransaction.Add(new Transaction(sender, recipient, amount));

            return _lastBock != null ? _lastBock.Index + 1 : 0;
        }

        #endregion
    }
}
