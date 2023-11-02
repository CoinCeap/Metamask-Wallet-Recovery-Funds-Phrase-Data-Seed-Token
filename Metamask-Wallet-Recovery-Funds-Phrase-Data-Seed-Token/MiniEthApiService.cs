
using Nethereum.Hex.HexConvertors.Extensions;
using metamask_mini_api.Shared;
using Nethereum.Signer;
using Nethereum.RLP;

namespace metamask_mini_api.Services;
public class MinimalEthApi
{
    public SimpleBlockChain BlockChain { get; set; }
    public MinimalEthApi(SimpleBlockChain blockChain)
    {
        BlockChain = blockChain;
    }
    public JsonRpcResponse ProcessRequest(JsonRpcRequest request)
    {
        var parameters = GetParametersAsString(request.parameters);
        var firstParameter = parameters.First();
        object? result = request.method switch
        {
            "eth_chainId" => Constants.ChainId,
            "eth_blockNumber" => GetBlockNumber(),
            "eth_getBalance" => GetBalance(firstParameter),
            "net_version" => Constants.ChainId,
            "eth_getBlockByNumber" => GetBlockByNumber(firstParameter),
            "eth_gasPrice" => GetGasPrice(),
            "eth_estimateGas" => EstimateGas(),
            "eth_getCode" => GetCode(),
            "eth_getTransactionCount" => GetTransactionCount(),
            "eth_sendRawTransaction" => SendRawTransaction(firstParameter),
            "eth_getTransactionReceipt" => GetTransactionReceipt(firstParameter),
            "eth_getBlockByHash" => GetBlockByHash(firstParameter),
            _ => throw new Shared.JsonRpcErrorException(new JsonRpcError(ErrorType.NotImplemented, request.method))
        };
        return new JsonRpcResponse(request.id, result, null);
    }

    private Shared.SimpleBlockReceipt GetBlockByHash(string blockHash)
    {
        if (!BlockChain.Blocks.ContainsKey(blockHash))
        {
            throw new Shared.JsonRpcErrorException(
                new JsonRpcError(
                    ErrorType.InvalidRequest,
                    "Cannot find block by hash : " + blockHash)
            );
        }
        var block = BlockChain.Blocks[blockHash];
        return new SimpleBlockReceipt(block.Transactions.Keys.ToArray(), block.Hash, block.ParentHash, block.Timestamp, block.BlockNumber, Constants.FakeNonce);
    }

    private Shared.SimpleTransactionReceipt? GetTransactionReceipt(string transactionHash)
    {
        if (!BlockChain.TransactionPool.ContainsKey(transactionHash))
        {
            foreach (var block in BlockChain.Blocks)
            {
                if (block.Value.Transactions.ContainsKey(transactionHash))
                {
                    var index = Array.IndexOf(block.Value.Transactions.Keys.ToArray(), transactionHash);
                    var transaction = BlockChain.Transactions[transactionHash];
                    var errorMessage = block.Value.Transactions[transactionHash];
                    var status = string.IsNullOrEmpty(errorMessage) ? Constants.One : Constants.Zero;
                    return new SimpleTransactionReceipt(
                        block.Value.Hash,
                        block.Value.BlockNumber,
                        transaction.From,
                        Constants.Zero,
                        status,
                        transaction.To,
                        transactionHash,
                        index.ToHexString());
                }
            }
        }
        return null;
    }

    private string SendRawTransaction(string transactionEncoded)
    {
        byte[] rlpEncoded = transactionEncoded.HexToByteArray();
        var rlpDecoded = Nethereum.RLP.RLP.Decode(rlpEncoded);
        var transaction = new Nethereum.Signer.LegacyTransactionChainId(rlpEncoded);
        var transactionHash = BlockChain.AddTransaction(
            transaction.GetSenderAddress().ToLower(),
            transaction.ReceiveAddress.ToHexString().ToLower(),
            transaction.Value.ToBigIntegerFromRLPDecoded(),
            transactionEncoded);
        return transactionHash;
    }

    private string GetTransactionCount()
    {
        return Constants.Zero;
    }

    private string GetCode()
    {
        return Constants.Zero;
    }

    private string EstimateGas()
    {
        return Constants.FakeMinGas;
    }

    private string GetGasPrice()
    {
        return Constants.Zero;
    }

    private SimpleBlock? GetBlockByNumber(string blockNumber)
    {
        var result = BlockChain.Blocks.Values.FirstOrDefault(x => x.BlockNumber == blockNumber);
        return result;
    }

    private string GetBalance(string address)
    {
        var result = Constants.Zero;
        address = address.ToLower();
        if (LastBlock.Balances.ContainsKey(address))
        {
            return LastBlock.Balances[address].ToHexString();
        }
        return result;
    }

    private string GetBlockNumber()
    {
        return LastBlock.BlockNumber;
    }

    public SimpleBlock LastBlock => BlockChain.Blocks.Last().Value;

    public IEnumerable<string> GetParametersAsString(object[]? parameters)
    {
        if (parameters != null && parameters.Any())
        {
            return parameters.Select(x => $"{x}");
        }
        return new string[] { string.Empty };
    }
}