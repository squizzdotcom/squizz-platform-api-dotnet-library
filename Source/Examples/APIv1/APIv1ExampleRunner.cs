﻿/**
* Copyright (C) 2017 Squizz PTY LTD
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
                Console.WriteLine("1 - Create Organisation API Sesssion");
                Console.WriteLine("2 - Destroy Organisation API Sesssion");
                Console.WriteLine("3 - Validate Organisation API Sesssion");
                Console.WriteLine("4 - Validate/Create Organisation API Sesssion");
                Console.WriteLine("5 - Quit");

                bool optionSelected = false;
                int optionNumber = 5;

                //get the selected option
                while (!optionSelected)
                {
                    Console.Write("Enter The Option Number: ");
                    try
                    {
                        optionNumber = Convert.ToInt32(Console.ReadLine());
                        optionSelected = (optionNumber >= 1 && optionNumber <= 5);
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
                    default:
                        continueRunning = false;
                        break;
                }
            }
        }
    }
}
