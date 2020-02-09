In order to run the Samples projects, SQL server and RabbitMq should be configured. If you already have SQL server and RabbitMq installed, please change the appsettings in the BoltOn.Samples.WebApi and BoltOn.Samples.Console projects. If you do not have them installed, you could use [docker-compose](#docker-compose), or run SQL Server and RabbitMq separately in docker using the commands mentioned below and change the appsettings.

docker-compose
--------------
Navigate to samples folder and execute any of the commands mentioned below.

To build and run:

`docker-compose up -d --build`

To stop the conainers and remove the images:

`docker-compose down --rmi local`

Here is the [docker-compose](https://github.com/gokulm/BoltOn/blob/master/samples/docker-compose.yml) file used. It launches the samples API, console app (which acts MassTransit event consumer), RabbitMq and SQL Server. You could test the [StudentsController's](https://github.com/gokulm/BoltOn/blob/master/samples/BoltOn.Samples.WebApi/Controllers/StudentsController.cs) actions. Go over [CQRS](../cqrs/#implementation) documentation to understand the flow.

To run SQL Server separately:

`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Password1' -p 6000:1433 -d microsoft/mssql-server-linux:latest`

To run RabbitMq separately:

`docker run -d --name bolton-rabbitmq -p 15672:15672 -p 5672:5672 rabbitmq:3-management`