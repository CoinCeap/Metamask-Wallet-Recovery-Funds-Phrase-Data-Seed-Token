using System.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using metamask_mini_api.Shared;

namespace metamask_mini_api.Services
{
    public class SimpleBlockChain
    {
        private SimpleBlockChain(SimpleBlock genesisBlock)
        {
            Blocks = new Dictionary<string, SimpleBlock> {
                { genesisBlock.Hash, genesisBlock }
            };
            TransactionPool = new Dictionary<string, SimpleTransaction>();
            Transactions = new Dictionary<string, SimpleTransaction>();
        }
        public static SimpleBlockChain CreateSimpleBlockchain()
        {
            var genesisBlock = GenesisBlock();
            var result = new SimpleBlockChain(genesisBlock);
            return result;
        }

        public static SimpleBlockChain Initialize()
        {
            var result = CreateSimpleBlockchain();
            var blockBuilder = new System.Timers.Timer(1000);
            blockBuilder.Elapsed += result.AddBlock;
            blockBuilder.Start();
            return result;
        }

        private void AddBlock(object? sender, ElapsedEventArgs evt)
        {
            try
            {
                if (TransactionPool.Any())
                {
                    var transactions = TransactionPool.Keys.ToDictionary(x => x, x => string.Empty);
                    if (transactions != null)
                    {
                        var lastBlock = Blocks.Last().Value;
                        var blockNumber = lastBlock.BlockNumber.HexStringToBigInt() + 1;
                        var balances = lastBlock.Balances.ToDictionary(x => x.Key, y => y.Value);
                        foreach (var txKey in transactions.Keys)
                        {
                            var transaction = TransactionPool[txKey];
                            Transactions.Add(txKey, transaction);
                            TransactionPool.Remove(txKey);
                            if (balances[transaction.From] >= transaction.Amount)
                            {
                                balances[transaction.From] -= transaction.Amount;
                                if (balances.ContainsKey(transaction.To))
                                {
                                    balances[transaction.To] += transaction.Amount;
                                }
                                else
                                {
                                    balances.Add(transaction.To, transaction.Amount);
                                }
                            }
                            else
                            {
                                transactions[txKey] = $"sorry dude you're broke - trying to send {transaction.Amount} - available {balances[transaction.From]}";
                            }
                        }
                        var block = CreateBlock(blockNumber.ToHexString(), lastBlock.Hash, transactions, balances);
                        Blocks.Add(block.Hash, block);
                        Serilog.Log.Debug("added block {@block}", block);
                    }
                }
            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, "Exception while Adding block");
            }
        }



        public static SimpleBlock GenesisBlock()
        {
            var blockNumber = Constants.Zero;
            var previousHash = Constants.Zero;
            var transactions = new Dictionary<string, string>();
            var balances = new Dictionary<string, BigInteger>(){
                {Constants.GenesisWallet1, Nethereum.Util.UnitConversion.Convert.ToWei(42)},
                {Constants.GenesisWallet2, Nethereum.Util.UnitConversion.Convert.ToWei(1337)}
            };
            var block = CreateBlock(blockNumber, previousHash, transactions, balances);
            return block;
        }

        public static SimpleBlock CreateBlock(string blockNumber, string previousHash, Dictionary<string, string> transactions, Dictionary<string, BigInteger> balances)
        {
            var timestamp = NowTimestamp;
            var hash = Hash(blockNumber, previousHash, timestamp, string.Join(',', transactions.Keys), string.Join(',', transactions.Values));
            var block = new SimpleBlock(blockNumber, previousHash, timestamp, transactions, balances, hash);
            return block;
        }
        public string AddTransaction(string from, string to, BigInteger amount, string data)
        {
            var timestamp = NowTimestamp;
            var hash = Hash(from, to, amount.ToHexString(), data);
            var transaction = new SimpleTransaction(timestamp, from, to, amount, data, hash);
            if (TransactionPool.ContainsKey(hash) || Transactions.ContainsKey(hash))
            {
                Serilog.Log.Debug("eth_ eth_ transaction already exists..." + hash);
            }
            else
            {
                TransactionPool.Add(transaction.Hash, transaction);
            }
            return hash;
        }
        private static string Hash(params string[] inputs)
        {
            return Hash(string.Join(';', inputs));
        }

        private static string Hash(string input)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var binaryHash = System.Security.Cryptography.SHA256.HashData(bytes);
            var result = binaryHash.ToHexString();
            while (result.Length < 66)
            {
                result = result + "0";
            }
            if (result.Length > 66)
            {
                result = result.Substring(0, 66);
            }
            return result;
        }

        public static string NowTimestamp => DateTimeOffset.Now.ToUnixTimeSeconds().ToHexString();

        public Dictionary<string, SimpleBlock> Blocks { get; }
        public Dictionary<string, SimpleTransaction> Transactions { get; }
        public Dictionary<string, SimpleTransaction> TransactionPool { get; }
    }
}