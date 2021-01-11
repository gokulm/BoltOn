In order to run the Samples projects, SQL server and Redis should be configured. If you already have SQL server and Redis installed, please change the appsettings in the BoltOn.Samples.WebApi and BoltOn.Samples.Console projects. If you do not have them installed, you could use [docker-compose](#docker-compose), or run SQL Server and Redis separately in docker using the commands mentioned below and change the appsettings. Instead of using Redis, you could also configure in-memory cache.

docker-compose
--------------
Navigate to samples folder and execute any of the commands mentioned below.

To build and run:

`docker-compose up -d --build`

To stop the conainers and remove the images:

`docker-compose down --rmi local`

Here is the [docker-compose](https://github.com/gokulm/BoltOn/blob/master/samples/docker-compose.yml) file used. It launches the samples API (http://localhost:5000/). You could test the [StudentsController's](https://github.com/gokulm/BoltOn/blob/master/samples/BoltOn.Samples.WebApi/Controllers/StudentsController.cs) actions using Postman or some other API testing tool. 

To run SQL Server separately:

`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Password1' -p 6000:1433 -d microsoft/mssql-server-linux:latest`

To run Redis separately:

`docker run -d -p 6379:6379 --name redis-local redis`

To run RabbitMq separately:

`docker run -d --name bolton-rabbitmq -p 15672:15672 -p 5672:5672 rabbitmq:3-management`

**OR**

To run all the above containers:

`docker-compose -f docker-compose-local.yml up -d`

