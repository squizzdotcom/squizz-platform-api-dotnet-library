/**
* Copyright (C) Squizz PTY LTD
* This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
* This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
* You should have received a copy of the GNU General Public License along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squizz.Platform.API.v1;
using Squizz.Platform.API.v1.endpoint;

namespace Squizz.Platform.API.Examples.APIv1
{
    /// <summary>Class that runs a console application that shows different examples on how use the SQUIZZ.com API for organisations</summary>
    public class APIv1ExampleRunner
    {
        public const string CONSOLE_LINE = "===========================================================";

        /// <summary>Main entry point to run examples for calling the SQUIZZ.com API</summary>
        public static void Main()
        {
            string[] menuOptions = new string[]{
                "  - Create Organisation API Sesssion",
                "  - Destroy Organisation API Sesssion",
                "  - Validate Organisation API Sesssion",
                "  - Validate/Create Organisation API Sesssion",
                "  - Procure Purchase Order From Supplier",
                "  - Create Organisation Notification",
                "  - Retrieve Organisation (Supplier/Customer/Own) Data",
                "  - Import Organisation Data",
                "  - Import Organisation Sales Order",
                "  - Search Supplier Organisation Customer Account Records",
                "  - Retrieve Supplier Organisation Customer Account Record",
                " - Search and Retrieve Supplier Organisation Invoice Records For Purchase Order",
                " - Send Customer Invoice To Customer",
                " - Send Delivery Notice To Customer",
                " - Quit"
            };

            string[] dataRetrieveMenuOptions = new string[]{
                "  - Retrieve Attributes",
                "  - Retrieve Categories",
                "  - Retrieve Makers",
                "  - Retrieve Maker Models",
                "  - Retrieve Maker Model Mappings",
                "  - Retrieve Products",
                "  - Retrieve Product Stock Availability",
                "  - Retrieve Product Pricing"
            };

            string[] dataImportMenuOptions = new string[]{
                "  - Import Attributes",
                "  - Import Categories",
                "  - Import Makers",
                "  - Import Maker Models",
                "  - Import Maker Model Mappings",
                "  - Import Products",
                "  - Import Taxcodes"
            };

            Console.WriteLine(CONSOLE_LINE);
            Console.WriteLine("   _____ ____  __  ________________    __________  __  ___");
            Console.WriteLine("  / ___// __ \\/ / / /  _/__  /__  /   / ____/ __ \\/  |/  /");
            Console.WriteLine("  \\__ \\/ / / / / / // /   / /  / /   / /   / / / / /|_/ /");
            Console.WriteLine(" ___/ / /_/ / /_/ // /   / /__/ /___/ /___/ /_/ / /  / /");
            Console.WriteLine("/____/\\___\\_\\____/___/  /____/____(_)____/\\____/_/  /_/");
            Console.WriteLine(CONSOLE_LINE);
            Console.WriteLine("          SQUIZZ.com - The Connected Marketplace");
            Console.WriteLine(CONSOLE_LINE);
            Console.WriteLine("                   SQUIZZ Pty Ltd");
            Console.WriteLine(CONSOLE_LINE);
            Console.WriteLine("Testing SQUIZZ.com API C# Library: version 1");

            //display menu to allow the user to choose from an example to run
            bool continueRunning = true;
            while(continueRunning)
            {
                Console.WriteLine(CONSOLE_LINE);
                Console.WriteLine("Select from one of the API examples to run: ");
                Console.WriteLine(CONSOLE_LINE);

                //display menu options
                for(int i=0; i < menuOptions.Length; i++)
                {
                    Console.WriteLine((i+1).ToString()+ menuOptions[i]);
                }

                bool optionSelected = false;
                int optionNumber = menuOptions.Length;

                //get the selected option
                while (!optionSelected)
                {
                    Console.Write("Enter Menu Option Number: ");
                    try
                    {
                        //get the option that the user entered
                        string optionEntered = Console.ReadLine().Trim();

                        //if the user hit the q key then quit the application
                        if(optionEntered.ToLower() == "q"){
                            return;
                        }
                        
                        optionNumber = Convert.ToInt32(optionEntered);
                        optionSelected = (optionNumber >= 1 && optionNumber <= menuOptions.Length);
                    }
                    catch(Exception ex){}
                }

                //run the selected example
                switch (optionNumber)
                {
                    case 1:
                        APIv1ExampleRunnerCreateSession.runAPIv1ExampleRunnerCreateSession();
                        break;
                    case 2:
                        APIv1ExampleRunnerDestroyOrgSession.runAPIv1ExampleRunnerDestroyOrgSession();
                        break;
                    case 3:
                        APIv1ExampleRunnerValidateOrgSession.runAPIv1ExampleRunnerValidateOrgSession();
                        break;
                    case 4:
                        APIv1ExampleRunnerValidateCreateOrgSession.runAPIv1ExampleRunnerValidateCreateOrgSession();
                        break;
                    case 5:
                        APIv1ExampleRunnerProcurePurchaseOrderFromSupplier.runAPIv1ExampleRunnerProcurePurchaseOrderFromSupplier();
                        break;
                    case 6:
                        APIv1ExampleRunnerCreateOrgNotification.runAPIv1ExampleRunnerCreateOrgNotification();
                        break;
                    case 7:

                        Console.WriteLine(CONSOLE_LINE);
                        Console.WriteLine("Select from one of the data imports to run: ");
                        Console.WriteLine(CONSOLE_LINE);

                        //display menu options
                        for (int i = 0; i < dataRetrieveMenuOptions.Length; i++)
                        {
                            Console.WriteLine((i + 1).ToString() + dataRetrieveMenuOptions[i]);
                        }

                        bool dataRetrieveOptionSelected = false;
                        int dataRetrieveOptionNumber = dataRetrieveMenuOptions.Length;

                        //get the selected option
                        while (!dataRetrieveOptionSelected)
                        {
                            Console.Write("Enter Menu Option Number: ");
                            try
                            {
                                //get the option that the user entered
                                string optionEntered = Console.ReadLine().Trim();

                                //if the user hit the q key then quit the application
                                if (optionEntered.ToLower() == "q")
                                {
                                    return;
                                }

                                dataRetrieveOptionNumber = Convert.ToInt32(optionEntered);
                                dataRetrieveOptionSelected = (dataRetrieveOptionNumber >= 1 && dataRetrieveOptionNumber <= dataRetrieveMenuOptions.Length);
                            }
                            catch (Exception ex) { }
                        }

                        //run the selected example
                        switch (dataRetrieveOptionNumber)
                        {
                            case 1:
                                APIv1ExampleRunnerRetrieveOrgESDDataAttributes.runAPIv1ExampleRunnerRetrieveOrgESDDataAttribute();
                                break;
                            case 2:
                                APIv1ExampleRunnerRetrieveOrgESDDataCategories.runAPIv1ExampleRunnerRetrieveOrgESDDataCategories();
                                break;
                            case 3:
                                APIv1ExampleRunnerRetrieveOrgESDDataMakers.runAPIv1ExampleRunnerRetrieveOrgESDDataMakers();
                                break;
                            case 4:
                                APIv1ExampleRunnerRetrieveOrgESDDataMakerModels.runAPIv1ExampleRunnerRetrieveOrgESDDataMakerModels();
                                break;
                            case 5:
                                APIv1ExampleRunnerRetrieveOrgESDDataMakerModelMappings.runAPIv1ExampleRunnerRetrieveOrgESDDataMakerModelMappings();
                                break;
                            case 6:
                                APIv1ExampleRunnerRetrieveOrgESDDataProduct.runAPIv1ExampleRunnerRetrieveOrgESDDataProduct();
                                break;
                            case 7:
                                APIv1ExampleRunnerRetrieveOrgESDDataProductStock.runAPIv1ExampleRunnerRetrieveOrgESDDataProductStock();
                                break;
                            case 8:
                                APIv1ExampleRunnerRetrieveOrgESDDataPrice.runAPIv1ExampleRunnerRetrieveOrgESDDataPrice();
                                break;
                        }

                        break;
                    case 8:

                        Console.WriteLine(CONSOLE_LINE);
                        Console.WriteLine("Select from one of the data imports to run: ");
                        Console.WriteLine(CONSOLE_LINE);

                        //display menu options
                        for (int i = 0; i < dataImportMenuOptions.Length; i++)
                        {
                            Console.WriteLine((i + 1).ToString() + dataImportMenuOptions[i]);
                        }

                        bool dataImportOptionSelected = false;
                        int dataImportOptionNumber = dataImportMenuOptions.Length;

                        //get the selected option
                        while (!dataImportOptionSelected)
                        {
                            Console.Write("Enter Menu Option Number: ");
                            try
                            {
                                //get the option that the user entered
                                string optionEntered = Console.ReadLine().Trim();

                                //if the user hit the q key then quit the application
                                if (optionEntered.ToLower() == "q")
                                {
                                    return;
                                }

                                dataImportOptionNumber = Convert.ToInt32(optionEntered);
                                dataImportOptionSelected = (dataImportOptionNumber >= 1 && dataImportOptionNumber <= dataImportMenuOptions.Length);
                            }
                            catch (Exception ex) { }
                        }

                        //run the selected example
                        switch (dataImportOptionNumber)
                        {
                            case 1:
                                APIv1ExampleRunnerImportOrgESDDataAttributes.runAPIv1ExampleRunnerImportOrgESDAttributeData();
                                break;
                            case 2:
                                APIv1ExampleRunnerImportOrgESDDataCategories.runAPIv1ExampleRunnerImportOrgESDCategoryData();
                                break;
                            case 3:
                                APIv1ExampleRunnerImportOrgESDDataMakers.runAPIv1ExampleRunnerImportOrgESDMakerData();
                                break;
                            case 4:
                                APIv1ExampleRunnerImportOrgESDDataMakerModels.runAPIv1ExampleRunnerImportOrgESDMakerModelData();
                                break;
                            case 5:
                                APIv1ExampleRunnerImportOrgESDDataMakerModelMappings.runAPIv1ExampleRunnerImportOrgESDMakerModelMappingData();
                                break;
                            case 6:
                                APIv1ExampleRunnerImportOrgESDDataProducts.runAPIv1ExampleRunnerImportOrgESDProductData();
                                break;
                            case 7:
                                APIv1ExampleRunnerImportOrgESDDataTaxcodes.runAPIv1ExampleRunnerImportOrgESDTaxcodeData();
                                break;
                        }

                        break;
                    case 9:
                        APIv1ExampleRunnerImportOrgESDDataOrderSales.runAPIv1ExampleRunnerImportOrgESDDataOrderSales();
                        break;
                    case 10:
                        APIv1ExampleRunnerSearchCustomerAccountRecords.runAPIv1ExampleRunnerSearchCustomerAccountRecords();
                        break;
                    case 11:
                        APIv1ExampleRunnerRetrieveCustomerAccountRecord.runAPIv1ExampleRunnerRetrieveCustomerAccountRecord();
                        break;
                    case 12:
                        APIv1ExampleRunnerSearchRetrieveSupplierInvoiceRecordsForPurchaseOrder.runAPIv1ExampleRunnerSearchCustomerAccountRecords();
                        break;
                    case 13:
                        APIv1ExampleRunnerSendCustomerInvoiceToCustomer.runAPIv1ExampleRunnerSendCustomerInvoiceToCustomer();
                        break;
                    case 14:
                        APIv1ExampleRunnerSendDeliveryNoticeToCustomer.runAPIv1ExampleRunnerSendDeliveryNoticeToCustomer();
                        break;
                    default:
                        continueRunning = false;
                        break;
                }
            }
        }
    }
}
