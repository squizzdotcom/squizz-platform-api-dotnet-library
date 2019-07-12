/**
* Copyright (C) 2019 Squizz PTY LTD
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

/// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then validates if the user's session is still valid or creates a new session</summary>
namespace Squizz.Platform.API.Examples.APIv1
{
    public class APIv1ExampleRunnerValidateCreateOrgSession
    {
        public static void runAPIv1ExampleRunnerValidateCreateOrgSession()
        {
            Console.WriteLine("Example - Validating/Creating An Organisation API Session");
            Console.WriteLine("");

            //obtain or load in an organisation's API credentials, in this example from command line arguments
            Console.WriteLine("Enter Organisation ID:");
            string orgID = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Key:");
            string orgAPIKey = Console.ReadLine();
            Console.WriteLine("Enter Organisation API Password:");
            string orgAPIPass = Console.ReadLine();

            //create an API session instance
            int sessionTimeoutMilliseconds = 20000;
            APIv1OrgSession apiOrgSession = new APIv1OrgSession(orgID, orgAPIKey, orgAPIPass, sessionTimeoutMilliseconds, APIv1Constants.SUPPORTED_LOCALES_EN_AU);

            //call the platform's API to request that a session is created
            APIv1EndpointResponse endpointResponse = apiOrgSession.createOrgSession();

            //check if the organisation's credentials were correct and that a session was created in the platform's API
            if (endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
            {
                //session has been created so now can call other API endpoints
                Console.WriteLine("SUCCESS - API session has successfully been created.");
            }
            else
            {
                //session failed to be created
                Console.WriteLine("FAIL - API session failed to be created. Reason: " + endpointResponse.result_message + " Error Code: " + endpointResponse.result_code);
            }

            //next steps
            //call API endpoints...

            //check if the session still is valid, if not have a new session created with the same organisation API credentials
            endpointResponse = apiOrgSession.validateCreateOrgSession();

            //check the result of validating or creating a new session
            if (endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
            {
                Console.WriteLine("SUCCESS - API session successfully validated/created.");
            }
            else {
                Console.WriteLine("FAIL - API session failed to be validated or created. Reason: " + endpointResponse.result_message + " Error Code: " + endpointResponse.result_code);
            }

            //destroy API session when done...
            apiOrgSession.destroyOrgSession();

            Console.WriteLine("Example Finished.");
        }
    }
}