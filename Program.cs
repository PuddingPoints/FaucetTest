using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Algorand;
using Algorand.Client;
using Algorand.V2;

namespace TestFaucet1
{
    class Program
    {
        static void Main(string[] args)
        {
            Account acccount = new Account();
            var address = acccount.Address;
            var key = acccount.ToMnemonic();
            Console.WriteLine($"Account Address: {address}\nKey: {key}");
            //Account 1
            //Address: DZLN2QPBURBXHGHYY2Z5B3BOBZQS4BXIHJMOVXXDBDF66ZFWLFOXZG4OWE
            //Key: hood industry indicate catch erode return wage gas wrong museum lesson renew acid erupt victory swallow distance spy orchard tomorrow organ traffic hen ability drift

            //Account 2
            //Address: QQNTMX62E2UZF3264V3OB3O3F6CURWBBHR3MQZPRM26CFVMXUYWOXIKG6E
            //Key: salt priority million dignity cage roast enemy visual solid impose vocal cost quiz list result festival brother rain elder crisp jewel husband trim abandon brisk

            //Acount 3 
            //Address: RP2HQNAY4F4FWJ6OGOBSLCB73PXHVAQ36R6RAQTYPUEIPR4C7M2YJS4SSE
            //Key: race sure wagon prevent about powder rely enable erosion almost lens moment rain mechanic carry sense permit venue parent between permit upset lens able circle
            AtomicTransaction();

        }
        //Atomic Transaction Method
        public static void AtomicTransaction()
        {
            //Performing Atomic Transactions

            //PureStake Configuration with AlgodApi
            AlgodApi algodApiInstance = new AlgodApi("https://testnet-algorand.api.purestake.io/ps2", "B3SU4KcVKi94Jap2VXkK83xx38bsv95K5UZm2lab");
            //First Account Address - acc1
            var acc1 = "QQNTMX62E2UZF3264V3OB3O3F6CURWBBHR3MQZPRM26CFVMXUYWOXIKG6E";
            //Second Account Address - acc2
            var acc2 = "RP2HQNAY4F4FWJ6OGOBSLCB73PXHVAQ36R6RAQTYPUEIPR4C7M2YJS4SSE";
            //Key of the Main account that will fund other Accounts
            var key = "hood industry indicate catch erode return wage gas wrong museum lesson renew acid erupt victory swallow distance spy orchard tomorrow organ traffic hen ability drift";
            //Account that will fund other accounts
            var accountAddress = "DZLN2QPBURBXHGHYY2Z5B3BOBZQS4BXIHJMOVXXDBDF66ZFWLFOXZG4OWE";
            Account src = new Account(key);
            //Creating a transactionParams on the V2
            Algorand.V2.Model.TransactionParametersResponse transParams;
            try
            {
                transParams = algodApiInstance.TransactionParams();
            }
            catch (ApiException e)
            {
                throw new Exception("Could not get params", e);
            }

            // let's create a transaction group

            //amount to fund other accounts
            var amount = Utils.AlgosToMicroalgos(1);
            //First Transaction
            var tx = Utils.GetPaymentTransaction(new Address(accountAddress), new Address(acc1), amount, "pay message", transParams);
            //Second Transaction
            var tx2 = Utils.GetPaymentTransaction(new Address(accountAddress), new Address(acc2), amount, "pay message", transParams);
            //Grouping the Transactions (Atomic Transactions)
            Digest gid = TxGroup.ComputeGroupID(new Transaction[] { tx, tx2 });
            tx.AssignGroupID(gid);
            tx2.AssignGroupID(gid);
            // already updated the groupid, sign
            var signedTx = src.SignTransaction(tx);
            var signedTx2 = src.SignTransaction(tx2);
            try
            {
                //contact the signed msgpack
                List<byte> byteList = new List<byte>(Algorand.Encoder.EncodeToMsgPack(signedTx));
                byteList.AddRange(Algorand.Encoder.EncodeToMsgPack(signedTx2));
                var id = algodApiInstance.RawTransaction(byteList.ToArray());
                Console.WriteLine($"Successfully sent tx group with first tx id: {id}");
                Console.WriteLine(Utils.WaitTransactionToComplete(algodApiInstance, id.TxId));
            }
            catch (ApiException e)
            {
                // This is generally expected, but should give us an informative error message.
                Console.WriteLine("Exception when calling algod#rawTransaction: " + e.Message);
            }
            Console.WriteLine("You have successefully arrived the end of this test, please press and key to exit.");

        }

    }
}





