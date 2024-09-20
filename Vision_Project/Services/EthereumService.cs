using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using System.Threading.Tasks;
using System;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.TransactionReceipts;
using Nethereum.Util;
using System.Collections.Generic;
using Vision_Project.Models;
using System.Xml.Linq;
using Nethereum.ABI.Model;
using System.Numerics;


namespace Vision_Project.Services
{
    public class EthereumService 
    {
        private readonly string _contractABI;
        private readonly string _contractAddress;
        private readonly Web3 _web3;
        private readonly Contract _contract;


        public EthereumService(string privateKey)
        {
            var account = new Account(privateKey);
            _web3 = new Web3(account, "http://127.0.0.1:8545");
            _contractAddress = "0x9F24075c1D78b4A55F71F35d7BA4AB27E92A4134";

            _contractABI = @"[
  {
    ""inputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""constructor""
  },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": """",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""candidates"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""id"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""string"",
        ""name"": ""name"",
        ""type"": ""string""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""partyId"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""electionId"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""uint256"",
        ""name"": ""voteCount"",
        ""type"": ""uint256""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""owner"",
    ""outputs"": [
      {
        ""internalType"": ""address"",
        ""name"": """",
        ""type"": ""address""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
      ""inputs"": [
        {
          ""internalType"": ""string"",
          ""name"": ""_name"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""_partyId"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""uint256"",
          ""name"": ""_electionId"",
          ""type"": ""uint256""
        }
      ],
      ""name"": ""addCandidate"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
 {
      ""inputs"": [
        {
          ""internalType"": ""string"",
          ""name"": ""_name"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""_email"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""_shortcode"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""_leader"",
          ""type"": ""string""
        }
      ],
      ""name"": ""addParty"",
      ""outputs"": [],
      ""stateMutability"": ""nonpayable"",
      ""type"": ""function""
    },
  {
    ""inputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": """",
        ""type"": ""uint256""
      }
    ],
    ""name"": ""parties"",
    ""outputs"": [
      {
        ""internalType"": ""uint256"",
        ""name"": ""id"",
        ""type"": ""uint256""
      },
      {
        ""internalType"": ""string"",
        ""name"": ""name"",
        ""type"": ""string""
      },
      {
        ""internalType"": ""string"",
        ""name"": ""email"",
        ""type"": ""string""
      },
      {
        ""internalType"": ""string"",
        ""name"": ""shortcode"",
        ""type"": ""string""
      },
      {
        ""internalType"": ""string"",
        ""name"": ""leader"",
        ""type"": ""string""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""getAllPartyIds"",
    ""outputs"": [
      {
        ""internalType"": ""uint256[]"",
        ""name"": """",
        ""type"": ""uint256[]""
      }
    ],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
{
  ""inputs"": [
    {
      ""internalType"": ""uint256"",
      ""name"": ""_partyId"",
      ""type"": ""uint256""
    }
  ],
  ""name"": ""getParty"",
  ""outputs"": [
    {
      ""components"": [
        {
          ""internalType"": ""uint256"",
          ""name"": ""id"",
          ""type"": ""uint256""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""name"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""email"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""shortcode"",
          ""type"": ""string""
        },
        {
          ""internalType"": ""string"",
          ""name"": ""leader"",
          ""type"": ""string""
        }
      ],
      ""internalType"": ""struct Election.Party"",
      ""name"": """",
      ""type"": ""tuple""
    }
  ],
  ""stateMutability"": ""view"",
  ""type"": ""function"",
  ""constant"": true
},
{
  ""inputs"": [],
  ""name"": ""getTotalParties"",
  ""outputs"": [
    {
      ""internalType"": ""uint256"",
      ""name"": """",
      ""type"": ""uint256""
    }
  ],
  ""stateMutability"": ""view"",
  ""type"": ""function"",
  ""constant"": true
}

]";
            // remove if works 
            _contract = _web3.Eth.GetContract(_contractABI, _contractAddress);
        }
       //interact with contract method
        public async Task<Contract> GetContractInstanceAsync()
        {
            return _web3.Eth.GetContract(_contractABI, _contractAddress);
        }
        //add party to blockchain 
        public async Task<string> AddPartyAsync(string partyName, string partyEmail, string partyShortcode, string partyLeader)
        {
            var contract = await GetContractInstanceAsync();
            var addPartyFunction = contract.GetFunction("addParty");
            var gasPrice = new HexBigInteger(Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei));
            var gasLimit = new HexBigInteger(900000);

            var transactionHash = await addPartyFunction.SendTransactionAsync(
                _web3.TransactionManager.Account.Address,
                gasLimit,
                gasPrice,
                null,
                partyName, partyEmail, partyShortcode, partyLeader);

            var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            Console.WriteLine($"Transaction successful: {receipt.Status.Value}");

            return transactionHash;
        }
        //add candidates to blockchain
        public async Task<string> AddCandidateAsync(string name, int partyId, int electionId)
        {
            var contract = await GetContractInstanceAsync();
            var addCandidateFunction = contract.GetFunction("addCandidate");

            var gasPrice = new HexBigInteger(Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei));
            var gasLimit = new HexBigInteger(900000);

            var transactionHash = await addCandidateFunction.SendTransactionAsync(
                _web3.TransactionManager.Account.Address,
                gasLimit,
                gasPrice,
                null,
                name, partyId, electionId);

            var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            Console.WriteLine($"Transaction successful: {receipt.Status.Value}");

            return transactionHash;
        }
        //add voter to blockchain
        public async Task<TransactionReceipt> AddVoterAsync(int userId, int electionId)
        {
            var contract = await GetContractInstanceAsync();
            var addVoterFunction = contract.GetFunction("addVoter");

            var gasPrice = new HexBigInteger(Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei));
            var gasLimit = new HexBigInteger(900000);

            try
            {
                var transactionHash = await addVoterFunction.SendTransactionAsync(
                    _web3.TransactionManager.Account.Address,
                    gasLimit,
                    gasPrice,
                    null,
                    userId, electionId);

                var receiptService = new TransactionReceiptPollingService(_web3.TransactionManager);
                var receipt = await receiptService.PollForReceiptAsync(transactionHash);

                if (receipt.Status.Value == 1)
                {
                    Console.WriteLine("Transaction was successful!");
                }
                else
                {
                    Console.WriteLine("Transaction failed!");
                }

                return receipt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while processing blockchain transaction: " + ex.Message);
                throw;
            }
        }
        //add votes to blockchain
        public async Task VoteAsync(int voterId, int candidateId)
        {
            var contract = await GetContractInstanceAsync();
            var voteFunction = contract.GetFunction("vote");

            var gasLimit = new HexBigInteger(900000);
            var gasPrice = new HexBigInteger(Web3.Convert.ToWei(20, UnitConversion.EthUnit.Gwei));

            try
            {
                var transactionHash = await voteFunction.SendTransactionAsync(
                    _web3.TransactionManager.Account.Address, gasLimit, gasPrice, null, voterId, candidateId);

                var receiptService = new TransactionReceiptPollingService(_web3.TransactionManager);
                var receipt = await receiptService.PollForReceiptAsync(transactionHash);

                if (receipt.Status.Value == 1)
                {
                    Console.WriteLine($"Transaction successful with hash: {transactionHash}");
                    Console.WriteLine($"Block number: {receipt.BlockNumber.Value}");
                    Console.WriteLine($"Gas used: {receipt.GasUsed.Value}");
                    Console.WriteLine("Vote recorded successfully!");
                }
                else
                {
                    throw new Exception("Transaction failed! Please try again.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while processing the transaction: " + ex.Message);
            }
        }

        public async Task<uint> GetTotalCandidatesAsync()
        {
            var contract = await GetContractInstanceAsync();
            var getTotalCandidatesFunction = contract.GetFunction("getTotalCandidates");
            return await getTotalCandidatesFunction.CallAsync<uint>();
        }

        public async Task<uint> GetTotalVotersAsync()
        {
            var contract = await GetContractInstanceAsync();
            var getTotalVotersFunction = contract.GetFunction("getTotalVoters");
            return await getTotalVotersFunction.CallAsync<uint>();
        }

        public async Task<uint> GetTotalVotesAsync()
        {
            var contract = await GetContractInstanceAsync();
            var getTotalVotesFunction = contract.GetFunction("getTotalVotes");
            return await getTotalVotesFunction.CallAsync<uint>();
        }
        
        public async Task<BigInteger> GetTotalPartiesAsync()
        {
            var contract = _web3.Eth.GetContract(_contractABI, _contractAddress);
            var getTotalPartiesFunction = contract.GetFunction("getTotalParties");

            var totalParties = await getTotalPartiesFunction.CallAsync<BigInteger>();

            return totalParties;
        }

    }
}
  