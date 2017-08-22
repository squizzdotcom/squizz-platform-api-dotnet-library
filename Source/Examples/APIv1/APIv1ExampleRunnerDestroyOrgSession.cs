/**
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

/// <summary>Shows an example of creating a organisation session with the SQUIZZ.com platform's API, then destroys the session</summary>
public class APIv1ExampleRunnerDestroyOrgSession 
{
    public static void runAPIv1ExampleRunnerDestroyOrgSession()
    {
        Console.WriteLine("Example - Destroying An Organisation API Session");
        Console.WriteLine("");

        //obtain or load in an organisation's API credentials, in this example from command line arguments
        Console.WriteLine("Enter Organisation ID:");
        string orgID = Console.ReadLine();
        Console.WriteLine("Enter Organisation API Key:");
        string orgAPIKey = Console.ReadLine();
        Console.WriteLine("Enter Organisation API Password:");
        string orgAPIPass = Console.ReadLine();
        
        int sessionTimeoutMilliseconds = 20000;
		
		//create an API session instance
		APIv1OrgSession apiOrgSession = new APIv1OrgSession(orgID, orgAPIKey, orgAPIPass, sessionTimeoutMilliseconds, APIv1Constants.SUPPORTED_LOCALES_EN_AU);
		
		//call the platform's API to request that a session is created
		APIv1EndpointResponse endpointResponse = apiOrgSession.createOrgSession();
		
		//check if the organisation's credentials were correct and that a session was created in the platform's API
		if(endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS)
		{
            //session has been created so now can call other API endpoints
            Console.WriteLine("SUCCESS - API session has successfully been created.");
		}
		else
		{
            //session failed to be created
            Console.WriteLine("FAIL - API session failed to be created. Reason: " + endpointResponse.result_message  + " Error Code: " + endpointResponse.result_code);
		}
		
		//next steps
		//call API endpoints...
		
		//destroy the session in the platform's API
        endpointResponse = apiOrgSession.destroyOrgSession();
		
		//check the result of destroying the session
		if(endpointResponse.result.ToUpper() == APIv1EndpointResponse.ENDPOINT_RESULT_SUCCESS){
            Console.WriteLine("SUCCESS - API session successfully destroyed.");
		}else{
            Console.WriteLine("FAIL - API session failed to be destroyed. Reason: " + endpointResponse.result_message  + " Error Code: " + endpointResponse.result_code);
		}

        Console.WriteLine("Example Finished.");
    }
}
