In order to create project templates and other BoltOn components from CLI, execute:

    dotnet new -i BoltOn.Templates

WebAPI
------
To create a .NET Core WebAPI project with BoltOn package bolted:

    dotnet new bowebapi -n <project_name>

**-n:** Specify the project name. If not specified, the folder name will be used as the project name

Handler
-------
To create a [Requestor](../requestor) handler without response:

    dotnet new bohandler -n <request_name>

**-n:** Specify the request name without Request suffix. If not specified, the folder name will be used as the handler name

eg.,

    dotnet new bohandler -n GetStudent 

The request and the handler will be added in **GetStudentHandler.cs** file inside Handlers folder (Handlers folder will be created if it doesn't exist).

    namespace Handlers
    {
        public class GetStudentRequest : IRequest
        {
        }

        public class GetStudentHandler : IHandler<GetStudentRequest>
        {
            public async Task HandleAsync(GetStudentRequest request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }

Handler (with response)
-----------------------
To create a [Requestor](../requestor) handler with response:

    dotnet new bohanlder -n <request_name> -r <response_type>

**-n:** Specify the request name without Request suffix. If not specified, the folder name will be used as the handler name
<br />
**-r OR --response:** Specifiy the type of the response like int, Guid etc. If it's a class type, specify the class name, but you have to  create the class

eg.,

    dotnet new bohandler -n GetStudentCount -r int 

The request and the handler will be added in **GetStudentsCountHandler.cs** file inside Handlers folder (Handlers folder will be created if it doesn't exist).

    namespace Handlers
    {
        public class GetStudentsCountRequest : IRequest<int>
        {
        }

        public class GetStudentsCountHandler : IHandler<GetStudentsCountRequest, int>
        {
            public async Task<int> HandleAsync(GetStudentsCountRequest request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }





