using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TestFaucet3.Models;
using Algorand;
using Algorand.Client;
using Algorand.V2;

namespace TestFaucet3.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public static void AtomicTransaction()
        {
            //Performing Atomic Transactions

            //PureStake Configuration with AlgodApi
            AlgodApi algodApiInstance = new AlgodApi("https://testnet-algorand.api.purestake.io/ps2", "4oTwch2udI9i0mbVyGu522B6jRfbDxQV2mwj0wp4");
            //First Account Address - acc1
            var acc1 = "OVNC4RLSPDOVOIFZRFOEHJ77WJYMVG3JZGJUOJEVEN7OVZAWBDLNR32DDQ";
            //Second Account Address - acc2
            var acc2 = "6I5URCJLDYH36HR6PMZB2QCMC4LSD3LDEFDON7SWRD3OU7FZHMZYBGOBRM";
            //Key of the Main account that will fund other Accounts
            var key = "sell ring motion jungle advance embark spawn upper cotton pave cave someone fever proud fix until bronze crop leaf skill deliver text film absent truck";
            //Account that will fund other accounts
            var accountAddress = "ZPOY7WAK5GG5LA64S2XSJ6A3EDMXBSBB4HXDTJQ2VMHD76PNA3H2N7RXIU";
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

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
