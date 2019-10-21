In order to run the Samples projects, SQL server and RabbitMq should be configured. 

SQL Server
----------
You could run SQL in Docker using this command:

`docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=Password1' -p 1433:1433 -d microsoft/mssql-server-linux:latest`

Please modify the port if SQL is already installed. After launching SQL server, connect to the SQL Server and create a database using this command - `Create Database BoltOnSamples`

Configure the DbContext in the RegistrationTask like this:

    container.AddDbContext<SchoolDbContext>(options =>
    {
        options.UseSqlServer("Data Source=127.0.0.1;initial catalog=BoltOnSamples;persist security info=True;User ID=sa;Password=Password1;");
    });

RabbitMq
--------
You could run RabbitMq in Docker using this command:

`docker run -d --name bolton-rabbitmq -p 15672:15672 -p 5672:5672 rabbitmq:3-management`

If RabbitMq is running, you should be able to monitor from http://localhost:15672 (guest/guest is the default password)

Configure the RabbitMq in the RegistrationTask like this:

    container.AddMassTransit(x =>
    {
        x.AddBus(provider => MassTransit.Bus.Factory.CreateUsingRabbitMq(cfg =>
        {
            var host = cfg.Host(new Uri("rabbitmq://localhost:5672"), hostConfigurator =>
            {
                hostConfigurator.Username("guest");
                hostConfigurator.Password("guest");
            });
        }));
    });